namespace BroadcastLogging
{
    /// <summary>
    /// A configuration for the logging implementation used by Broadcast.
    /// </summary>
    public interface IBroadcastLoggingConfiguration
    {
        /// <summary>
        /// Gets the setting determines if an attempt to log should be made when the package is used.  
        /// </summary>
        bool LoggingEnabled { get; }

        /// <summary>
        /// Get the settings path to be added to the service URL to make a secure logging call.  It MUST be the following text (at this time):  "api/v1/log/add"
        /// </summary>
        string LoggingSecureEndpoint { get; }

        /// <summary>
        /// Gets the settings URL to the server hosting the Central Logging Service.  This will be provided by DevOps or another party
        /// </summary>
        string LoggingServiceUrl { get; }

        /// <summary>
        /// Gets the settings path to be added to the service URL to make an unsecured logging call.   it MUST be the following text (at this time):  "api/v1/log/unsecure/add"
        /// </summary>
        string LoggingUnsecureEndpoint { get; }

        /// <summary>
        /// Gets the name of the log level to use for the application.  It MUST be one of the following:  TRACE, DEBUG, INFO, WARN, ERROR, or FATAL.
        /// </summary>
        string LogLevelSetting { get; }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// Gets the name of the environment the application is running in.  It MUST be one of the following:  DEV, QA, UAT, STG, PROD, or DMO
        /// </summary>
        string EnvironmentName { get; }

        /// <summary>
        /// Gets the path that the log file should be stored in.
        /// </summary>
        string LogFilePath { get; }

        /// <summary>
        /// Gets the maximum size a log file can be before a new file is generated.  The default if this is not provided is 64 MB.
        /// </summary>
        int MaxFileSizeMb { get; }

        /// <summary>
        /// Gets an unique identifier to the application.
        /// </summary>
        string ApplicationId { get; }
    }
}
