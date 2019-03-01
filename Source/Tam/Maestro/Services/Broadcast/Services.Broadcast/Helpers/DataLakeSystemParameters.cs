using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IDataLakeSystemParameters
    {
        string GetSharedFolder();
        string GetUserName();
        string GetPassword();
        string GetNotificationEmail();
    }

    public class DataLakeSystemParameters : IDataLakeSystemParameters
    {
        public string GetSharedFolder()
        {
            return BroadcastServiceSystemParameter.DataLake_SharedFolder;
        }

        public string GetUserName()
        {
            return BroadcastServiceSystemParameter.DataLake_SharedFolder_UserName;
        }

        public string GetPassword()
        {
            return BroadcastServiceSystemParameter.DataLake_SharedFolder_Password;
        }

        public string GetNotificationEmail()
        {
            return BroadcastServiceSystemParameter.DataLake_NotificationEmail;
        }
    }
}