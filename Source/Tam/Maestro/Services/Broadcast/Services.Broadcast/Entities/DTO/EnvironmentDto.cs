namespace Services.Broadcast.Entities.DTO
{
    public class EnvironmentDto
    {
        public string Environment { get; set; }

        public string HostName { get; set; }      
      
        public bool EnableExportPreBuy { get; set; }
       
        public bool EnableAabNavigation { get; set; }

        // Keep These : these are referenced by the NavBar.cshtml       
        public bool DisplayBuyingLink { get; set; }     
        public bool DisplaySpotExceptionsLink { get; set; }
    }
}
