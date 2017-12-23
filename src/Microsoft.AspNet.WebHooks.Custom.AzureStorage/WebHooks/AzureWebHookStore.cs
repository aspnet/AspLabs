// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookStore"/> storing registered WebHooks in Microsoft Azure Table Storage.
    /// </summary>
    [CLSCompliant(false)]
    public class AzureWebHookStore : WebHookStore
    {
        internal const string WebHookTable = "WebHooks";
        internal const string WebHookDataColumn = "Data";
        internal const int AzureStoreSecretKeyMinLength = 8;
        internal const int AzureStoreSecretKeyMaxLength = 64;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { Formatting = Formatting.None };
        private readonly IStorageManager _manager;
        private readonly IDataProtector _protector;
        private readonly ILogger _logger;
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureWebHookStore"/> class with the given <paramref name="manager"/>,
        /// <paramref name="settings"/>, and <paramref name="logger"/>.
        /// Using this constructor, the data will not be encrypted while persisted to Azure Storage.
        /// </summary>
        public AzureWebHookStore(IStorageManager manager, SettingsDictionary settings, ILogger logger)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _manager = manager;
            _connectionString = manager.GetAzureStorageConnectionString(settings);
            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureWebHookStore"/> class with the given <paramref name="manager"/>,
        /// <paramref name="settings"/>, <paramref name="protector"/>, and <paramref name="logger"/>.
        /// Using this constructor, the data will be encrypted using the provided <paramref name="protector"/>.
        /// </summary>
        public AzureWebHookStore(IStorageManager manager, SettingsDictionary settings, IDataProtector protector, ILogger logger)
            : this(manager, settings, logger)
        {
            if (protector == null)
            {
                throw new ArgumentNullException(nameof(protector));
            }
            _protector = protector;
        }

        /// <summary>
        /// Provides a static method for creating a standalone <see cref="AzureWebHookStore"/> instance which will
        /// encrypt the data to be stored using <see cref="IDataProtector"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use.</param>
        /// <returns>An initialized <see cref="AzureWebHookStore"/> instance.</returns>
        public static IWebHookStore CreateStore(ILogger logger)
        {
            return CreateStore(logger, encryptData: true);
        }

        /// <summary>
        /// Provides a static method for creating a standalone <see cref="AzureWebHookStore"/> instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use.</param>
        /// <param name="encryptData">Indicates whether the data should be encrypted using <see cref="IDataProtector"/> while persisted.</param>
        /// <returns>An initialized <see cref="AzureWebHookStore"/> instance.</returns>
        public static IWebHookStore CreateStore(ILogger logger, bool encryptData)
        {
            var settings = CommonServices.GetSettings();
            IWebHookStore store;
            var storageManager = StorageManager.GetInstance(logger);
            if (encryptData)
            {
                var protector = DataSecurity.GetDataProtector();
                store = new AzureWebHookStore(storageManager, settings, protector, logger);
            }
            else
            {
                store = new AzureWebHookStore(storageManager, settings, logger);
            }
            return store;
        }

        /// <inheritdoc />
        public override async Task<ICollection<WebHook>> GetAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            var query = new TableQuery();
            _manager.AddPartitionKeyConstraint(query, user);

            var entities = await _manager.ExecuteQueryAsync(table, query);
            ICollection<WebHook> result = entities.Select(e => ConvertToWebHook(e))
                .Where(w => w != null)
                .ToArray();
            return result;
        }

        /// <inheritdoc />
        public override async Task<ICollection<WebHook>> QueryWebHooksAsync(string user, IEnumerable<string> actions, Func<WebHook, string, bool> predicate)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            user = NormalizeKey(user);

            predicate = predicate ?? DefaultPredicate;

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            var query = new TableQuery();
            _manager.AddPartitionKeyConstraint(query, user);

            var entities = await _manager.ExecuteQueryAsync(table, query);
            ICollection<WebHook> matches = entities.Select(e => ConvertToWebHook(e))
                .Where(w => MatchesAnyAction(w, actions) && predicate(w, user))
                .ToArray();
            return matches;
        }

        /// <inheritdoc />
        public override async Task<WebHook> LookupWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            user = NormalizeKey(user);
            id = NormalizeKey(id);

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            var result = await _manager.ExecuteRetrievalAsync(table, user, id);
            if (!result.IsSuccess())
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.AzureStore_NotFound, user, id);
                _logger.Info(message);
                return null;
            }

            var entity = result.Result as DynamicTableEntity;
            return ConvertToWebHook(entity);
        }

        /// <inheritdoc />
        public override async Task<StoreResult> InsertWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);
            var id = NormalizeKey(webHook.Id);

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            var tableEntity = ConvertFromWebHook(user, id, webHook);
            var operation = TableOperation.Insert(tableEntity, echoContent: false);
            var tableResult = await _manager.ExecuteAsync(table, operation);

            var result = GetStoreResult(tableResult);
            if (result != StoreResult.Success)
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_CreateFailed, table.Name, tableResult.HttpStatusCode);
                _logger.Error(message);
            }
            return result;
        }

        /// <inheritdoc />
        public override async Task<StoreResult> UpdateWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);
            var id = NormalizeKey(webHook.Id);

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            var tableEntity = ConvertFromWebHook(user, id, webHook);
            var operation = TableOperation.Replace(tableEntity);
            var tableResult = await _manager.ExecuteAsync(table, operation);

            var result = GetStoreResult(tableResult);
            if (result != StoreResult.Success)
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, table.Name, tableResult.HttpStatusCode);
                _logger.Error(message);
            }
            return result;
        }

        /// <inheritdoc />
        public override async Task<StoreResult> DeleteWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            user = NormalizeKey(user);
            id = NormalizeKey(id);

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            var tableEntity = new TableEntity(user, id)
            {
                ETag = "*"
            };

            var operation = TableOperation.Delete(tableEntity);
            var tableResult = await _manager.ExecuteAsync(table, operation);

            var result = GetStoreResult(tableResult);
            if (result != StoreResult.Success)
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.StorageManager_OperationFailed, table.Name, tableResult.HttpStatusCode);
                _logger.Error(message);
            }
            return result;
        }

        /// <inheritdoc />
        public override async Task DeleteAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            await _manager.ExecuteDeleteAllAsync(table, user, filter: null);
        }

        /// <inheritdoc />
        public override async Task<ICollection<WebHook>> QueryWebHooksAcrossAllUsersAsync(IEnumerable<string> actions, Func<WebHook, string, bool> predicate)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            var table = _manager.GetCloudTable(_connectionString, WebHookTable);
            var query = new TableQuery();

            predicate = predicate ?? DefaultPredicate;

            var entities = await _manager.ExecuteQueryAsync(table, query);
            var matches = new List<WebHook>();
            foreach (var entity in entities)
            {
                var webHook = ConvertToWebHook(entity);
                if (MatchesAnyAction(webHook, actions) && predicate(webHook, entity.PartitionKey))
                {
                    matches.Add(webHook);
                }
            }
            return matches;
        }

        private static bool DefaultPredicate(WebHook webHook, string user)
        {
            return true;
        }

        private static StoreResult GetStoreResult(TableResult result)
        {
            if (result.IsSuccess())
            {
                return StoreResult.Success;
            }
            if (result.IsNotFound())
            {
                return StoreResult.NotFound;
            }
            if (result.IsConflict())
            {
                return StoreResult.Conflict;
            }
            if (result.IsServerError())
            {
                return StoreResult.InternalError;
            }
            return StoreResult.OperationError;
        }

        private WebHook ConvertToWebHook(DynamicTableEntity entity)
        {
            if (entity == null || !entity.Properties.TryGetValue(WebHookDataColumn, out var property))
            {
                return null;
            }

            try
            {
                var encryptedContent = property.StringValue;
                if (encryptedContent != null)
                {
                    var content = _protector != null ? _protector.Unprotect(encryptedContent) : encryptedContent;
                    var webHook = JsonConvert.DeserializeObject<WebHook>(content, _serializerSettings);
                    return webHook;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.CurrentCulture, AzureStorageResources.AzureStore_BadWebHook, typeof(WebHook).Name, ex.Message);
                _logger.Error(message, ex);
            }
            return null;
        }

        private DynamicTableEntity ConvertFromWebHook(string partitionKey, string rowKey, WebHook webHook)
        {
            var entity = new DynamicTableEntity(partitionKey, rowKey)
            {
                ETag = "*"
            };

            // Set data column with encrypted serialization of WebHook
            var content = JsonConvert.SerializeObject(webHook, _serializerSettings);
            var encryptedContent = _protector != null ? _protector.Protect(content) : content;
            var property = EntityProperty.GeneratePropertyForString(encryptedContent);
            entity.Properties.Add(WebHookDataColumn, property);

            return entity;
        }
    }
}
