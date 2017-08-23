namespace Common.Services
{
    public interface IConfiguration
    {
        string EnvironmentName { get; }

        string LogFilePath { get; }
    }
}
