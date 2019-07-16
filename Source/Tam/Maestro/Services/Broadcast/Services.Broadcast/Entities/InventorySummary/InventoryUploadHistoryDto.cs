using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventoryUploadHistoryDto
    {
        public int FileId { get; set; }
        public DateTime UploadDateTime { get; set; }
        public String Username { get; set; }
        public String Filename { get; set; }
        public String DaypartCode { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public MediaMonthDto HutBook { get; set; }
        public MediaMonthDto ShareBook { get; set; }
        public int Rows { get; set; }
        public String Status { get; set; }
        public FileStatusEnum FileLoadStatus { get; set; }
        public BackgroundJobProcessingStatus FileProcessingStatus { get; set; }


    }
}
