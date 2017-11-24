// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Storage;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstract implementation of <see cref="IWebHookStore"/> targeting SQL using a parameterized <see cref="DbContext"/>.
    /// The <see cref="DbContext"/> must contain an entity of type <see cref="IRegistration"/> as this is used to access the data
    /// in the DB.
    /// </summary>
    /// <typeparam name="TContext">The type of <see cref="DbContext"/> to be used.</typeparam>
    /// <typeparam name="TRegistration">The type of <see cref="IRegistration"/> to be used.</typeparam>
    [CLSCompliant(false)]
    public abstract class DbWebHookStore<TContext, TRegistration> : WebHookStore
        where TContext : DbContext, new()
        where TRegistration : class, IRegistration, new()
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { Formatting = Formatting.None };
        private readonly IDataProtector _protector;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbWebHookStore{TContext,TRegistration}"/> class with the given <paramref name="logger"/>.
        /// Using this constructor, the data will not be encrypted while persisted to the database.
        /// </summary>
        protected DbWebHookStore(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbWebHookStore{TContext,TRegistration}"/> class with the given <paramref name="protector"/>
        /// and <paramref name="logger"/>. 
        /// Using this constructor, the data will be encrypted using the provided <paramref name="protector"/>.
        /// </summary>
        protected DbWebHookStore(IDataProtector protector, ILogger logger)
            : this(logger)
        {
            if (protector == null)
            {
                throw new ArgumentNullException(nameof(protector));
            }

            _protector = protector;
        }

        /// <inheritdoc />
        public override async Task<ICollection<WebHook>> GetAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = GetContext())
                {
                    var registrations = await context.Set<TRegistration>().Where(r => r.User == user).ToArrayAsync();
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
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            predicate = predicate ?? DefaultPredicate;

            try
            {
                using (var context = GetContext())
                {
                    var registrations = await context.Set<TRegistration>().Where(r => r.User == user).ToArrayAsync();
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
                throw new ArgumentNullException(nameof(user));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            user = NormalizeKey(user);
            id = NormalizeKey(id);

            try
            {
                using (var context = GetContext())
                {
                    var registration = await context.Set<TRegistration>().Where(r => r.User == user && r.Id == id).FirstOrDefaultAsync();
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
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = GetContext())
                {
                    var registration = ConvertFromWebHook(user, webHook);
                    context.Set<TRegistration>().Attach(registration);
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
                _logger.Error(msg, ocex);
                return StoreResult.Conflict;
            }
            catch (SqlException sqlex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Insert", sqlex.Message);
                _logger.Error(msg, sqlex);
                return StoreResult.OperationError;
            }
            catch (DbException dbex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Insert", dbex.Message);
                _logger.Error(msg, dbex);
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
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = GetContext())
                {
                    var registration = await context.Set<TRegistration>().Where(r => r.User == user && r.Id == webHook.Id).FirstOrDefaultAsync();
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
            catch (DbException dbex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Update", dbex.Message);
                _logger.Error(msg, dbex);
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
                throw new ArgumentNullException(nameof(user));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = GetContext())
                {
                    var match = await context.Set<TRegistration>().Where(r => r.User == user && r.Id == id).FirstOrDefaultAsync();
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
            catch (DbException dbex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SqlStorageResources.SqlStore_SqlOperationFailed, "Delete", dbex.Message);
                _logger.Error(msg, dbex);
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
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            try
            {
                using (var context = GetContext())
                {
                    var matches = await context.Set<TRegistration>().Where(r => r.User == user).ToArrayAsync();
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
                throw new ArgumentNullException(nameof(actions));
            }

            predicate = predicate ?? DefaultPredicate;

            try
            {
                using (var context = GetContext())
                {
                    var registrations = await context.Set<TRegistration>().ToArrayAsync();
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

        /// <summary>
        /// Converts the provided <paramref name="registration"/> to a <see cref="WebHook"/> instance
        /// which is returned from this <see cref="IWebHookStore"/> implementation.
        /// </summary>
        /// <param name="registration">The instance to convert.</param>
        /// <returns>An initialized <see cref="WebHook"/> instance.</returns>
        protected virtual WebHook ConvertToWebHook(TRegistration registration)
        {
            if (registration == null)
            {
                return null;
            }

            try
            {
                string content = _protector != null ? _protector.Unprotect(registration.ProtectedData) : registration.ProtectedData;
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

        /// <summary>
        /// Converts the provided <paramref name="webHook"/> associated with the given
        /// <paramref name="user"/> to an <typeparamref name="TRegistration"/> instance
        /// which is used by this <see cref="IWebHookStore"/> implementation.
        /// </summary>
        /// <param name="user">The user associated with this <see cref="WebHook"/>.</param>
        /// <param name="webHook">The <see cref="WebHook"/> to convert.</param>
        /// <returns>An initialized <typeparamref name="TRegistration"/> instance.</returns>
        protected virtual TRegistration ConvertFromWebHook(string user, WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            string content = JsonConvert.SerializeObject(webHook, _serializerSettings);
            string protectedData = _protector != null ? _protector.Protect(content) : content;
            var registration = new TRegistration
            {
                User = user,
                Id = webHook.Id,
                ProtectedData = protectedData
            };
            return registration;
        }

        /// <summary>
        /// Updates an existing <typeparamref name="TRegistration"/> instance with data provided
        /// by the given <paramref name="webHook"/>.
        /// </summary>
        /// <param name="user">The user associated with this <see cref="WebHook"/>.</param>
        /// <param name="webHook">The <paramref name="webHook"/> to update the existing <paramref name="registration"/> with.</param>
        /// <param name="registration">The <typeparamref name="TRegistration"/> instance to update.</param>
        protected virtual void UpdateRegistrationFromWebHook(string user, WebHook webHook, TRegistration registration)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.User = user;
            registration.Id = webHook.Id;
            string content = JsonConvert.SerializeObject(webHook, _serializerSettings);
            string protectedData = _protector != null ? _protector.Protect(content) : content;
            registration.ProtectedData = protectedData;
        }

        /// <summary>
        /// Constructs a new context instance
        /// </summary>
        protected virtual TContext GetContext()
        {
            return new TContext();
        }

        private static bool DefaultPredicate(WebHook webHook, string user)
        {
            return true;
        }
    }
}
