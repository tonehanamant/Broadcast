﻿namespace Services.Broadcast.Entities.DTO
{
    public class EnvironmentDto
    {
        public string Environment { get; set; }
        public bool DisplayCampaignLink { get; set; }

        public bool DisplayBuyingLink { get; set; }

        public bool AllowMultipleCreativeLengths { get; set; }

        public bool EnablePricingInEdit { get; set; }
    }
}
