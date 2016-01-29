// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.AspNet.WebHooks.Storage;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookStore"/> storing registered WebHooks in Microsoft SQL Server.
    /// </summary>
    [CLSCompliant(false)]
    public class SqlWebHookStore : WebHookStore
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { Formatting = Formatting.None };
        private readonly IDataProtector _protector;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlWebHookStore"/> class with the given <paramref name="settings"/>,
        /// <paramref name="protector"/>, and <paramref name="logger"/>.
        /// </summary>
        public SqlWebHookStore(SettingsDictionary settings, IDataProtector protector, ILogger logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            if (protector == null)
            {
                throw new ArgumentNullException("protector");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            CheckSqlStorageConnectionString(settings);
            _protector = protector;
            _logger = logger;
        }

        /// <summary>
        /// Provides a static method for creating a standalone <see cref="SqlWebHookStore"/> instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use.</param>
        /// <returns>An initialized <see cref="SqlWebHookStore"/> instance.</returns>
        public static IWebHookStore CreateStore(ILogger logger)
        {
            SettingsDictionary settings = CommonServices.GetSettings();
            IDataProtector protector = DataSecurity.GetDataProtector();
            IWebHookStore store = new SqlWebHookStore(settings, protector, logger);
            return store;
        }

        /// <inheritdoc />
        public override async Task<ICollection<WebHook>> GetAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var registrations = await context.Registrations.Where(r => r.User == user).ToArrayAsync();
                    ICollection<WebHook> result = registrations.Select(r => ConvertToWebHook(r))
                        .Where(w => w != null)
                        .ToArray();
                    return result;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Get", ex.Message);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
        }

        /// <inheritdoc />
        public override async Task<ICollection<WebHook>> QueryWebHooksAsync(string user, IEnumerable<string> actions, Func<WebHook, string, bool> predicate)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user = NormalizeKey(user);

            predicate = predicate ?? DefaultPredicate;

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var registrations = await context.Registrations.Where(r => r.User == user).ToArrayAsync();
                    ICollection<WebHook> matches = registrations.Select(r => ConvertToWebHook(r))
                        .Where(w => MatchesAnyAction(w, actions) && predicate(w, user))
                        .ToArray();
                    return matches;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Get", ex.Message);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
        }

        /// <inheritdoc />
        public override async Task<WebHook> LookupWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            user = NormalizeKey(user);
            id = NormalizeKey(id);

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var registration = await context.Registrations.Where(r => r.User == user && r.Id == id).FirstOrDefaultAsync();
                    if (registration != null)
                    {
                        return ConvertToWebHook(registration);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Lookup", ex.Message);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
        }

        /// <inheritdoc />
        public override async Task<StoreResult> InsertWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var registration = ConvertFromWebHook(user, webHook);
                    context.Registrations.Attach(registration);
                    context.Entry(registration).State = EntityState.Added;
                    await context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException uex)
            {
                string error = uex.GetBaseException().Message;
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Insert", error);
                _logger.Error(msg, uex);
                return StoreResult.Conflict;
            }
            catch (OptimisticConcurrencyException ocex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_ConcurrencyError, "Insert", ocex.Message);
            }
            catch (SqlException sqlex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Insert", sqlex.Message);
                _logger.Error(msg, sqlex);
                return StoreResult.OperationError;
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Insert", ex.Message);
                _logger.Error(msg, ex);
                return StoreResult.InternalError;
            }
            return StoreResult.Success;
        }

        /// <inheritdoc />
        public override async Task<StoreResult> UpdateWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var registration = await context.Registrations.Where(r => r.User == user && r.Id == webHook.Id).FirstOrDefaultAsync();
                    if (registration == null)
                    {
                        return StoreResult.NotFound;
                    }
                    UpdateRegistrationFromWebHook(user, webHook, registration);
                    context.Entry(registration).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
            }
            catch (OptimisticConcurrencyException ocex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_ConcurrencyError, "Update", ocex.Message);
                _logger.Error(msg, ocex);
                return StoreResult.Conflict;
            }
            catch (SqlException sqlex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Update", sqlex.Message);
                _logger.Error(msg, sqlex);
                return StoreResult.OperationError;
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Update", ex.Message);
                _logger.Error(msg, ex);
                return StoreResult.InternalError;
            }
            return StoreResult.Success;
        }

        /// <inheritdoc />
        public override async Task<StoreResult> DeleteWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var match = await context.Registrations.Where(r => r.User == user && r.Id == id).FirstOrDefaultAsync();
                    if (match == null)
                    {
                        return StoreResult.NotFound;
                    }
                    context.Entry(match).State = EntityState.Deleted;
                    await context.SaveChangesAsync();
                }
            }
            catch (OptimisticConcurrencyException ocex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_ConcurrencyError, "Delete", ocex.Message);
                _logger.Error(msg, ocex);
                return StoreResult.Conflict;
            }
            catch (SqlException sqlex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Delete", sqlex.Message);
                _logger.Error(msg, sqlex);
                return StoreResult.OperationError;
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Delete", ex.Message);
                _logger.Error(msg, ex);
                return StoreResult.InternalError;
            }
            return StoreResult.Success;
        }

        /// <inheritdoc />
        public override async Task DeleteAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var matches = await context.Registrations.Where(r => r.User == user).ToArrayAsync();
                    foreach (var m in matches)
                    {
                        context.Entry(m).State = EntityState.Deleted;
                    }
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "DeleteAll", ex.Message);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
        }

        /// <inheritdoc />
        public override async Task<ICollection<WebHook>> QueryWebHooksAcrossAllUsersAsync(IEnumerable<string> actions, Func<WebHook, string, bool> predicate)
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            predicate = predicate ?? DefaultPredicate;

            try
            {
                using (var context = new WebHookStoreContext())
                {
                    var registrations = await context.Registrations.ToArrayAsync();
                    var matches = new List<WebHook>();
                    foreach (var registration in registrations)
                    {
                        WebHook webHook = ConvertToWebHook(registration);
                        if (MatchesAnyAction(webHook, actions) && predicate(webHook, registration.User))
                        {
                            matches.Add(webHook);
                        }
                    }
                    return matches;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_OperationFailed, "Get", ex.Message);
                _logger.Error(msg, ex);
                throw new InvalidOperationException(msg, ex);
            }
        }

        internal static string CheckSqlStorageConnectionString(SettingsDictionary settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ConnectionSettings connection;
            if (!settings.Connections.TryGetValue(WebHookStoreContext.ConnectionStringName, out connection) || connection == null || string.IsNullOrEmpty(connection.ConnectionString))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_NoConnectionString, WebHookStoreContext.ConnectionStringName);
                throw new InvalidOperationException(msg);
            }
            return connection.ConnectionString;
        }

        private static bool DefaultPredicate(WebHook webHook, string user)
        {
            return true;
        }

        private WebHook ConvertToWebHook(Registration registration)
        {
            if (registration == null)
            {
                return null;
            }

            try
            {
                string content = _protector.Unprotect(registration.ProtectedData);
                WebHook webHook = JsonConvert.DeserializeObject<WebHook>(content, _serializerSettings);
                return webHook;
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_BadWebHook, typeof(WebHook).Name, ex.Message);
                _logger.Error(msg, ex);
            }
            return null;
        }

        private Registration ConvertFromWebHook(string user, WebHook webHook)
        {
            string content = JsonConvert.SerializeObject(webHook, _serializerSettings);
            string protectedData = _protector.Protect(content);
            var registration = new Registration
            {
                User = user,
                Id = webHook.Id,
                ProtectedData = protectedData
            };
            return registration;
        }

        private void UpdateRegistrationFromWebHook(string user, WebHook webHook, Registration registration)
        {
            registration.User = user;
            registration.Id = webHook.Id;
            string content = JsonConvert.SerializeObject(webHook, _serializerSettings);
            string protectedData = _protector.Protect(content);
            registration.ProtectedData = protectedData;
        }
    }
}
