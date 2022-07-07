namespace Services.Broadcast.Clients
{
    /// <summary>
    /// This class provides the config keys to access the locking apis
    /// </summary>
    public static class GeneralLockingApiClientConfigKeys
    {
        /// <summary>
        /// Defines the locking api url key which can be read from appsettings.json file
        /// </summary>
        public static readonly string ApiBaseUrl = "LockingConfig:ApiBaseUrl";
        /// <summary>
        /// Defines the locking key which can be read from appsettings.json file
        /// </summary>
        public static readonly string EncryptedApiKey = "LockingConfig:EncryptedApiKey";
        /// <summary>
        /// Defines the appname which can be read from appsettings.json file
        /// </summary>
        public static readonly string AppName = "LockingConfig:AppName";
        /// <summary>
        /// Defines the cadent applicationid which can be read from appsettings.json file
        /// </summary>
        public static readonly string ApplicationId = "LockingConfig:ApplicationId";
    }
}
