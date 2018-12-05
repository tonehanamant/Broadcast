using System;

namespace Services.Broadcast.Entities.Nti
{
    /// <summary>
    /// Dto class that contains PDF Base64 data
    /// </summary>
    public class NtiPdfDto
    {
        private string base64String;
        public string Base64String
        {
            get => base64String;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Unable to parse an empty document");
                }
                if (!value.StartsWith("data:application/pdf;base64,"))
                {
                    value = $"data:application/pdf;base64,{value}";
                }
                base64String = value;
            }
        }
    }
}
