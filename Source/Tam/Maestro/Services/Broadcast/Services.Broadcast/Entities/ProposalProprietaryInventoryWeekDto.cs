using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Services.Broadcast.Entities.OpenMarketInventory;

namespace Services.Broadcast.Entities
{
    public class ProposalProprietaryInventoryWeekDto : BaseProposalInventoryWeekDto
    {
        public ProposalProprietaryInventoryWeekDto()
        {
            DaypartGroups = new List<KeyValuePair<string, InventoryDaypartGroupDto>>();
            DaypartGroupsDict = new Dictionary<string, InventoryDaypartGroupDto>();
        }
        public List<KeyValuePair<string, InventoryDaypartGroupDto>> DaypartGroups { get; set; }

        [JsonIgnore]
        public Dictionary<string, InventoryDaypartGroupDto> DaypartGroupsDict { get; set; }

        public int MaxSlotSpotLength { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }

        public class InventoryDaypartGroupDto
        {
            public InventoryDaypartGroupDto()
            {
                DaypartSlots = new List<InventoryDaypartSlotDto>();
            }

            public List<InventoryDaypartSlotDto> DaypartSlots { get; set; }
        }

        public class InventoryDaypartSlotDto
        {
            public InventoryDaypartSlotDto(int spotLength, int inventoryDetailSlotId, int inventoryDetailId, int detailLevel)
            {
                ProposalsAllocations = new List<ProposalAllocationDto>();
                SpotLength = spotLength;
                InventoryDetailSlotId = inventoryDetailSlotId;
                InventoryDetailId = inventoryDetailId;
                DetailLevel = detailLevel;
            }

            public int InventoryDetailSlotId { get; set; }
            public int InventoryDetailId { get; private set; }
            public int DetailLevel { get; private set; }
            public int SpotLength { get; set; }
            public List<ProposalAllocationDto> ProposalsAllocations { get; set; }

            public double EFF { get; set; }
            public double Impressions { get; set; }
            public decimal CPM { get; set; }
            public decimal Cost { get; set; }
            public bool HasWastage { get; set; }

            public override string ToString()
            {
                return InventoryDetailId + "-" + DetailLevel;
            }
        }

        public class ProposalAllocationDto
        {
            public ProposalAllocationDto(string proposalName, int detailSpotLength, int order, bool isCurrentProposal, int proposalVersionDetailQuarterWeekId, string userName, DateTime weekStartDate)
            {
                ProposalName = proposalName;
                SpotLength = detailSpotLength;
                Order = order;
                IsCurrentProposal = isCurrentProposal;
                ProposalVersionDetailQuarterWeekId = proposalVersionDetailQuarterWeekId;
                UserName = userName;
                WeekStartDate = weekStartDate;
            }

            public bool IsCurrentProposal { get; set; }
            public int ProposalVersionDetailQuarterWeekId { get; private set; }
            public string UserName { get; private set; }
            public DateTime WeekStartDate { get; private set; }
            public int Order { get; set; }
            public int SpotLength { get; set; }
            public string ProposalName { get; set; }
        }
    }
}