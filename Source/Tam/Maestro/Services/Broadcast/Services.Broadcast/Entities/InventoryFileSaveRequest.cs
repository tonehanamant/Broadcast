﻿using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class InventoryFileSaveRequest : FileRequest
    {
        public string InventorySource { get; set; }
        public int? RatingBook { get; set; }
        public ProposalEnums.ProposalPlaybackType? PlaybackType { get; set; }
        public List<AudiencePricingDto> AudiencePricing { get; set; }
    }

    public class AudiencePricingDto
    {
        public int AudienceId { get; set; }
        public decimal Price { get; set; }
    }
}
