namespace DevOpsWithVsts.Web.Authentication
{
    using System;
    using System.Web.Security;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Runtime.Caching;

    /// <summary>
    /// 
    /// </summary>
    public class AdalTokenCache : TokenCache
    {
        private string userId;
        private UserToken userToken;
        private ObjectCache cache = MemoryCache.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdalTokenCache"/> class.
        /// </summary>
        /// <param name="signedInUserId">The signed in user identifier.</param>
        public AdalTokenCache(string signedInUserId)
        {
            // associate the cache to the current user of the web app
            userId = signedInUserId;
            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            BeforeWrite = BeforeWriteNotification;
            // look up the entry in the cache
            userToken = cache.Get(userId) as UserToken;
            // place the entry in memory
            Deserialize((userToken == null) ? null : MachineKey.Unprotect(userToken.CacheBits, "ADALCache"));
        }

        // clean up the database
        /// <summary>
        /// Clears the cache by deleting all the items. Note that if the cache is the default shared cache, clearing it would
        /// impact all the instances of <see cref="T:Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext" /> which share that cache.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            var cacheEntry = cache.Get(userId) as UserToken;
            if (cacheEntry != null)
            {
                cache.Remove(userId);
            }
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (userToken == null)
            {
                // first time access
                userToken = cache.Get(userId) as UserToken;
            }
            else
            {
                // retrieve last write from the DB
                var cached = cache.Get(userId) as UserToken;

                // if the in-memory copy is older than the persistent copy
                if (cached == null
                    || cached.LastWrite > userToken.LastWrite)
                {
                    // read from from storage, update in-memory copy
                    userToken = cache.Get(userId) as UserToken;
                }
            }
            Deserialize((userToken == null) ? null : MachineKey.Unprotect(userToken.CacheBits, "ADALCache"));
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (HasStateChanged)
            {
                if (userToken == null)
                {
                    userToken = new UserToken
                    {
                        WebUserUniqueId = userId,
                        CacheBits = MachineKey.Protect(Serialize(), "ADALCache"),
                        LastWrite = DateTimeOffset.Now
                    };
                }
                else
                {
                    userToken.WebUserUniqueId = userId;
                    userToken.CacheBits = MachineKey.Protect(Serialize(), "ADALCache");
                    userToken.LastWrite = DateTimeOffset.Now;
                }
                // update the DB and the lastwrite 
                var policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                cache.Set(userToken.WebUserUniqueId, userToken, policy);
                HasStateChanged = false;
            }
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }

        /// <summary>
        /// Deletes an item from the cache.
        /// </summary>
        /// <param name="item">The item to delete from the cache</param>
        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
        }
    }
}
