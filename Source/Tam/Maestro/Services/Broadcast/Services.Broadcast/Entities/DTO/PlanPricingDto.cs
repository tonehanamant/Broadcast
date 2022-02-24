using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class PricingJobSubmitResponse
    {
        public string request_id { get; set; }
        public string task_id { get; set; }
        public PricingApiErrorDto error { get; set; }
    }

    public class PricingApiErrorDto
    {
        public List<string> Messages { get; set; } = new List<string>();
        public string Name { get; set; }
    }

    public class PricingJobFetchRequest
    {
        public string task_id { get; set; }
    }

    public class PricingJobFetchResponse<T>
    {
        public string request_id { get; set; }
        public string task_id { get; set; }
        public string task_status { get; set; }
        public List<T> results { get; set; }
        public PricingApiErrorDto error { get; set; }
    }
}
