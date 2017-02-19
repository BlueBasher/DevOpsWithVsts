namespace DevOpsWithVsts.Web.Authentication
{
    using System;

    /// <summary>
    /// User Token
    /// </summary>
    public class UserToken
    {
        /// <summary>
        /// Gets or sets the web user unique identifier.
        /// </summary>
        public string WebUserUniqueId { get; set; }

        /// <summary>
        /// Gets or sets the cache bits.
        /// </summary>
        public byte[] CacheBits { get; set; }

        /// <summary>
        /// Gets or sets the last write.
        /// </summary>
        public DateTimeOffset LastWrite { get; set; }
    }
}