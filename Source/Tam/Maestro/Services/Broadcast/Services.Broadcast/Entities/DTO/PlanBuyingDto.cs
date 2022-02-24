using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class BuyingJobSubmitResponse
    {
        public string request_id { get; set; }
        public string task_id { get; set; }
        public BuyingApiErrorDto error { get; set; }
    }

    public class BuyingApiErrorDto
    {
        public List<string> Messages { get; set; } = new List<string>();
        public string Name { get; set; }
    }

    public class BuyingJobFetchRequest
    {
        public string task_id { get; set; }
    }

    public class BuyingJobFetchResponse<T>
    {
        public string request_id { get; set; }
        public string task_id { get; set; }
        public string task_status { get; set; }
        public List<T> results { get; set; }
        public BuyingApiErrorDto error { get; set; }
    }
}
