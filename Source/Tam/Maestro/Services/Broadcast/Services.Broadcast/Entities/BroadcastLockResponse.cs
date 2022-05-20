namespace Services.Broadcast.Entities
{
    public class BroadcastLockResponse
    {
        public string Error { get; set; }
        public string Key { get; set; }
        public string LockedUserId { get; set; }
        public string LockedUserName { get; set; }
        public int LockTimeoutInSeconds { get; set; }
        public bool Success { get; set; }
    }
}
