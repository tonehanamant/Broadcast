namespace Services.Broadcast.Entities.DTO
{
    public class EnvironmentDto
    {
        public string Environment { get; set; }

        public string HostName { get; set; }

        public bool DisplayCampaignLink { get; set; }

        public bool DisplayBuyingLink { get; set; }

        public bool AllowMultipleCreativeLengths { get; set; }

        public bool EnablePricingInEdit { get; set; }

        public bool EnableExportPreBuy { get; set; }

        public bool EnableRunPricingAutomaticaly { get; set; }

        public bool EnableAabNavigation { get; set; }
    }
}
