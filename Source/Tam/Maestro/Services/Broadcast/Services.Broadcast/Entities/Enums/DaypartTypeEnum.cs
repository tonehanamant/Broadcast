using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums
{
    /// <summary>
    /// Types of Dayparts.
    /// </summary>
    public enum DaypartTypeEnum
    {
        /// <summary>
        /// The type for News.
        /// </summary>
        [Description("News")]
        News = 1,

        /// <summary>
        /// The type for Entertainment \ Non-News
        /// </summary>
        [Description("Entertainment/Non-News")]
        EntertainmentNonNews = 2,

        /// <summary>
        /// The type for ROS
        /// </summary>
        [Description("ROS")]
        ROS = 3
    }
}
