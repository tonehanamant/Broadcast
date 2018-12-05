using System;

namespace Services.Broadcast.Entities.Nti
{
    /// <summary>
    /// DTO contains Header Information for the rating document
    /// </summary>
    public class NtiRatingHeaderDto
    {
        /// <summary>
        /// Document Date
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Advertiser Name
        /// </summary>
        public string Advertiser { get; set; }
        /// <summary>
        /// Report's Name
        /// </summary>
        public string ReportName { get; set; }
        /// <summary>
        /// Program Id
        /// </summary>
        public string ProgramId { get; set; }
        /// <summary>
        /// Stream
        /// </summary>
        public string Stream { get; set; }
        /// <summary>
        /// Program Type
        /// </summary>
        public string ProgramType { get; set; }
        /// <summary>
        /// Duration of the Program
        /// </summary>
        public string ProgramDuration { get; set; }
        /// <summary>
        /// Station's Name
        /// </summary>
        public string Stations { get; set; }
        /// <summary>
        /// CVG value
        /// </summary>
        public string CVG { get; set; }
        /// <summary>
        /// TbyC value
        /// </summary>
        public string TbyC { get; set; }
        /// <summary>
        /// TA value
        /// </summary>
        public string TA { get; set; }
        /// <summary>
        /// Week Ending
        /// </summary>
        public DateTime WeekEnding { get; set; }
    }
}
