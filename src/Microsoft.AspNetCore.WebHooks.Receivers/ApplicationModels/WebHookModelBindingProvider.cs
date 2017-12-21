// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// An <see cref="IApplicationModelProvider"/> implementation that adds <see cref="IBindingSourceMetadata"/> and
    /// <see cref="IModelNameProvider"/> information to <see cref="ParameterModel"/>s of WebHook actions.
    /// </summary>
    public class WebHookModelBindingProvider : IApplicationModelProvider
    {
        /// <inheritdoc />
        public int Order => WebHookMetadataProvider.Order + 20;

        /// <inheritdoc />
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (var i = 0; i < context.Result.Controllers.Count; i++)
            {
                var controller = context.Result.Controllers[i];
                for (var j = 0; j < controller.Actions.Count; j++)
                {
                    var action = controller.Actions[j];
                    var attribute = action.Attributes.OfType<WebHookAttribute>().FirstOrDefault();
                    if (attribute == null)
                    {
                        // Not a WebHook handler.
                        continue;
                    }

                    action.Properties.TryGetValue(typeof(IWebHookBindingMetadata), out var bindingMetadata);
                    action.Properties.TryGetValue(typeof(IWebHookRequestMetadata), out var requestMetadata);
                    for (var k = 0; k < action.Parameters.Count; k++)
                    {
                        var parameter = action.Parameters[k];
                        Apply(
                            (IWebHookBindingMetadata)bindingMetadata,
                            (IWebHookRequestMetadata)requestMetadata,
                            parameter);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            // No-op
        }

        private void Apply(
            IWebHookBindingMetadata bindingMetadata,
            IWebHookRequestMetadata requestMetadata,
            ParameterModel parameter)
        {
            var bindingInfo = parameter.BindingInfo;
            if (bindingInfo?.BinderModelName != null ||
                bindingInfo?.BinderType != null ||
                bindingInfo?.BindingSource != null)
            {
                // User was explicit. Nothing to do.
                return;
            }

            if (bindingInfo == null)
            {
                bindingInfo = parameter.BindingInfo = new BindingInfo();
            }

            var parameterName = parameter.ParameterName;
            var parameterType = parameter.ParameterInfo.ParameterType;
            switch (parameterName.ToUpperInvariant())
            {
                case "ACTION":
                case "ACTIONS":
                case "ACTIONNAME":
                case "ACTIONNAMES":
                    SourceEvent(bindingInfo, parameterType);
                    break;

                case "DATA":
                    SourceData(bindingInfo, requestMetadata);
                    break;

                case "EVENT":
                case "EVENTS":
                case "EVENTNAME":
                case "EVENTNAMES":
                    SourceEvent(bindingInfo, parameterType);
                    break;

                case "ID":
                    SourceId(bindingInfo, parameterType);
                    break;

                case "RECEIVER":
                case "RECEIVERNAME":
                    SourceReceiver(bindingInfo, parameterType);
                    break;

                case "RECEIVERID":
                    SourceId(bindingInfo, parameterType);
                    break;

                case "WEBHOOKRECEIVER":
                    SourceReceiver(bindingInfo, parameterType);
                    break;

                default:
                    // If additional parameters are configured and match, map them. If not, treat IFormCollection,
                    // JContainer and XElement parameters as data. IsAssignableFrom(...) looks reversed because this
                    // check is about model binding system support, not an actual assignment to the parameter.
                    if (!TrySourceAdditionalParameter(bindingInfo, bindingMetadata, parameterName) &&
                        (typeof(IFormCollection).IsAssignableFrom(parameterType) ||
                         // ??? Any need to support simple JToken's? JContainer is the base for JArray and JObject.
                         typeof(JContainer).IsAssignableFrom(parameterType) ||
                         typeof(XElement).IsAssignableFrom(parameterType)))
                    {
                        SourceData(bindingInfo, requestMetadata);
                    }
                    break;
            }
        }

        private void SourceData(BindingInfo bindingInfo, IWebHookRequestMetadata requestMetadata)
        {
            if (requestMetadata == null)
            {
                return;
            }

            if (requestMetadata.BodyType == WebHookBodyType.Form)
            {
                bindingInfo.BinderModelName = WebHookConstants.ModelStateBodyModelName;
                bindingInfo.BindingSource = BindingSource.Form;
                return;
            }

            bindingInfo.BinderModelName = WebHookConstants.ModelStateBodyModelName;
            bindingInfo.BindingSource = BindingSource.Body;
        }

        private static void SourceEvent(BindingInfo bindingInfo, Type parameterType)
        {
            // IsAssignableFrom(...) looks reversed because this check is about model binding system support, not an
            // actual assignment to the parameter.
            if (typeof(string) != parameterType &&
                !typeof(IEnumerable<string>).IsAssignableFrom(parameterType))
            {
                // ??? Do we need logging about these strange cases?
                // Unexpected / unsupported type. Do nothing.
                return;
            }

            bindingInfo.BinderModelName = WebHookConstants.EventKeyName;
            bindingInfo.BindingSource = BindingSource.Path;
        }

        private static void SourceId(BindingInfo bindingInfo, Type parameterType)
        {
            if (typeof(string) != parameterType)
            {
                // Unexpected / unsupported type. Do nothing.
                return;
            }

            bindingInfo.BinderModelName = WebHookConstants.IdKeyName;
            bindingInfo.BindingSource = BindingSource.Path;
        }

        private static void SourceReceiver(BindingInfo bindingInfo, Type parameterType)
        {
            if (typeof(string) != parameterType)
            {
                // Unexpected / unsupported type. Do nothing.
                return;
            }

            bindingInfo.BinderModelName = WebHookConstants.ReceiverKeyName;
            bindingInfo.BindingSource = BindingSource.Path;
        }

        private static bool TrySourceAdditionalParameter(
            BindingInfo bindingInfo,
            IWebHookBindingMetadata bindingMetadata,
            string parameterName)
        {
            var parameter = bindingMetadata?.Parameters
                .FirstOrDefault(item => string.Equals(parameterName, item.Name, StringComparison.OrdinalIgnoreCase));
            if (parameter == null)
            {
                return false;
            }

            bindingInfo.BinderModelName = parameter.SourceName;
            switch (parameter.ParameterType)
            {
                case WebHookParameterType.Header:
                    bindingInfo.BindingSource = BindingSource.Header;
                    break;

                case WebHookParameterType.RouteValue:
                    bindingInfo.BindingSource = BindingSource.Path;
                    break;

                case WebHookParameterType.QueryParameter:
                    bindingInfo.BindingSource = BindingSource.Query;
                    break;

                default:
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.General_InvalidEnumValue,
                        nameof(WebHookParameterType),
                        parameter.ParameterType);
                    throw new InvalidOperationException(message);
            }

            return true;
        }
    }
}
