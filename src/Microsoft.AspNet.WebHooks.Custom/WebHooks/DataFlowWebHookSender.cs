// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookSender"/> for sending HTTP requests to 
    /// registered <see cref="WebHook"/> instances using a default <see cref="WebHook"/> wire format
    /// and retry mechanism.
    /// </summary>
    public class DataflowWebHookSender : WebHookSender
    {
        private const int DefaultMaxConcurrencyLevel = 8;

        private static readonly Collection<TimeSpan> DefaultRetries = new Collection<TimeSpan> { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(4) };

        private readonly HttpClient _httpClient;
        private readonly ActionBlock<WebHookWorkItem>[] _launchers;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataflowWebHookSender"/> class with a default retry policy.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/>.</param>
        public DataflowWebHookSender(ILogger logger)
            : this(logger, retryDelays: null, options: null, httpClient: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataflowWebHookSender"/> class with a given collection of <paramref name="retryDelays"/> and
        /// <paramref name="options"/> for how to manage the queuing policy for each transmission. The transmission model is as follows: each try
        /// and subsequent retries is managed by a separate <see cref="ActionBlock{T}"/> which controls the level of concurrency used to 
        /// send out WebHooks. The <paramref name="options"/> parameter can be used to control all <see cref="ActionBlock{T}"/> instances 
        /// by setting the maximum level of concurrency, length of queue, and more.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/>.</param>
        /// <param name="retryDelays">A collection of <see cref="TimeSpan"/> instances indicating the delay between each retry. If <c>null</c>,
        /// then a default retry policy is used of one retry after one 1 minute and then again after 4 minutes. A retry is attempted if the 
        /// delivery fails or does not result in a 2xx HTTP status code. If the status code is 410 then no retry is attempted. If the collection
        /// is empty then no retries are attempted.</param>
        /// <param name="options">An <see cref="ExecutionDataflowBlockOptions"/> used to control the <see cref="ActionBlock{T}"/> instances.
        /// The default setting uses a maximum of 8 concurrent transmitters for each try or retry.</param>
        public DataflowWebHookSender(ILogger logger, IEnumerable<TimeSpan> retryDelays, ExecutionDataflowBlockOptions options)
            : this(logger, retryDelays, options, httpClient: null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="DataflowWebHookSender"/> with the given retry policy, <paramref name="options"/>,
        /// and <paramref name="httpClient"/>. This constructor is intended for unit testing purposes.
        /// </summary>
        internal DataflowWebHookSender(
            ILogger logger,
            IEnumerable<TimeSpan> retryDelays,
            ExecutionDataflowBlockOptions options,
            HttpClient httpClient)
            : base(logger)
        {
            retryDelays = retryDelays ?? DefaultRetries;

            options = options ?? new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DefaultMaxConcurrencyLevel
            };

            _httpClient = httpClient ?? new HttpClient();

            // Create the launch processors with the given retry delays
            _launchers = new ActionBlock<WebHookWorkItem>[1 + retryDelays.Count()];

            int offset = 0;
            _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await LaunchWebHook(item), options);
            foreach (TimeSpan delay in retryDelays)
            {
                _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await DelayedLaunchWebHook(item, delay));
            }

            string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_Started, typeof(DataflowWebHookSender).Name, _launchers.Length);
            Logger.Info(msg);
        }

        /// <inheritdoc />
        public override Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
        {
            if (workItems == null)
            {
                throw new ArgumentNullException("workItems");
            }

            foreach (WebHookWorkItem workItem in workItems)
            {
                _launchers[0].Post(workItem);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    if (_launchers != null)
                    {
                        try
                        {
                            // Start shutting down launchers
                            Task[] completionTasks = new Task[_launchers.Length];
                            for (int cnt = 0; cnt < _launchers.Length; cnt++)
                            {
                                ActionBlock<WebHookWorkItem> launcher = _launchers[cnt];
                                launcher.Complete();
                                completionTasks[cnt] = launcher.Completion;
                            }

                            // Cancel any outstanding HTTP requests
                            if (_httpClient != null)
                            {
                                _httpClient.CancelPendingRequests();
                                _httpClient.Dispose();
                            }

                            // Wait for launchers to complete
                            Task.WaitAll(completionTasks);
                        }
                        catch (Exception ex)
                        {
                            ex = ex.GetBaseException();
                            string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_CompletionFailure, ex.Message);
                            Logger.Error(msg, ex);
                        }
                    }
                }
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// If delivery of a WebHook is not successful, i.e. something other than a 2xx or 410 Gone 
        /// HTTP status code is received and the request is to be retried, then <see cref="OnWebHookRetry"/> 
        /// is called enabling additional post-processing of a retry request. 
        /// </summary>
        /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
        protected virtual Task OnWebHookRetry(WebHookWorkItem workItem)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// If delivery of a WebHook is successful, i.e. a 2xx HTTP status code is received,
        /// then <see cref="OnWebHookSuccess"/> is called enabling additional post-processing. 
        /// </summary>
        /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
        protected virtual Task OnWebHookSuccess(WebHookWorkItem workItem)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// If delivery of a WebHook is not successful, i.e. something other than a 2xx or 410 Gone 
        /// HTTP status code is received after having retried the request according to the retry-policy, 
        /// then <see cref="OnWebHookFailure"/> is called enabling additional post-processing. 
        /// </summary>
        /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
        protected virtual Task OnWebHookFailure(WebHookWorkItem workItem)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// If delivery of a WebHook results in a 410 Gone HTTP status code, then <see cref="OnWebHookGone"/> 
        /// is called enabling additional post-processing. 
        /// </summary>
        /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
        protected virtual Task OnWebHookGone(WebHookWorkItem workItem)
        {
            return Task.FromResult(true);
        }

        private async Task DelayedLaunchWebHook(WebHookWorkItem item, TimeSpan delay)
        {
            await Task.Delay(delay);
            await LaunchWebHook(item);
        }

        /// <summary>
        /// Launch a <see cref="WebHook"/>.
        /// </summary>
        /// <remarks>We don't let exceptions propagate out from this method as it is used by the launchers
        /// and if they see an exception they shut down.</remarks>
        private async Task LaunchWebHook(WebHookWorkItem workItem)
        {
            try
            {
                // Setting up and send WebHook request 
                HttpRequestMessage request = CreateWebHookRequest(workItem);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_Result, workItem.WebHook.Id, response.StatusCode, workItem.Offset);
                Logger.Info(msg);

                if (response.IsSuccessStatusCode)
                {
                    // If we get a successful response then we are done.
                    await OnWebHookSuccess(workItem);
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Gone)
                {
                    // If we get a 410 Gone then we are also done.
                    await OnWebHookGone(workItem);
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_WebHookFailure, workItem.Offset, workItem.WebHook.Id, ex.Message);
                Logger.Error(msg, ex);
            }

            try
            {
                // See if we should retry the request with delay or give up
                workItem.Offset++;
                if (workItem.Offset < _launchers.Length)
                {
                    // If we are to retry then we submit the request again after a delay.
                    await OnWebHookRetry(workItem);
                    _launchers[workItem.Offset].Post(workItem);
                }
                else
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_GivingUp, workItem.WebHook.Id, workItem.Offset);
                    Logger.Error(msg);
                    await OnWebHookFailure(workItem);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_WebHookFailure, workItem.Offset, workItem.WebHook.Id, ex.Message);
                Logger.Error(msg, ex);
            }
        }
    }
}
