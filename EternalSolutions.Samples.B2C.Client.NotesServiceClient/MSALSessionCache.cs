/*
 * Class based on the https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-v2 sample code.
 * (c) Vittorio Betrocci https://github.com/vibronet
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

namespace EternalSolutions.Samples.B2C.Client.NotesServiceClient
{
    public class MSALSessionCache
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string UserId = string.Empty;
        string CacheId = string.Empty;
        HttpContext httpContext = null;

        TokenCache cache = new TokenCache();

        public MSALSessionCache(string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            httpContext = httpcontext;
            Load();
        }

        public TokenCache GetMsalCacheInstance()
        {
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return cache;
        }

        public void Load()
        {
            SessionLock.EnterReadLock();
            byte[] bytes;
            if (httpContext.Session.TryGetValue(CacheId, out bytes))
                cache.Deserialize(bytes);
            SessionLock.ExitReadLock();
        }

        public void Persist()
        {
            SessionLock.EnterWriteLock();

            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            cache.HasStateChanged = false;

            // Reflect changes in the persistent store
            httpContext.Session.Set(CacheId, cache.Serialize());
            SessionLock.ExitWriteLock();
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (cache.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
