using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class SpotLengthTestData
	{
        public static List<LookupDto> GetAllSpotLengths()
        {
            return _SpotLengths;
        }

        public static Dictionary<int, int> GetSpotLengthIdsByDuration()
		{
            // length, id
	        return _SpotLengths.ToDictionary(k => int.Parse(k.Display), v => v.Id);
        }

        public static Dictionary<int, int> GetSpotLengthDurationsById()
        {
            // length, id
            return _SpotLengths.ToDictionary(k => k.Id, v => int.Parse(v.Display));
        }

        public static int GetSpotLengthValueById(int id)
        {
            var dict = _SpotLengths.ToDictionary(k => k.Id, v => int.Parse(v.Display));
            var result = dict[id];
            return result;
        }

        public static int GetSpotLengthIdByDuration(int duration)
        {
            var dict = GetSpotLengthIdsByDuration();
            var result = dict[duration];
            return result;
        }

        private static List<LookupDto> _SpotLengths = new List<LookupDto>
        {
            new LookupDto { Id = 1, Display = "30" },
            new LookupDto { Id = 2, Display = "60" },
            new LookupDto { Id = 3, Display = "15" },
            new LookupDto { Id = 4, Display = "120" },
            new LookupDto { Id = 5, Display = "180" },
            new LookupDto { Id = 6, Display = "300" },
            new LookupDto { Id = 7, Display = "90" },
            new LookupDto { Id = 8, Display = "45" },
            new LookupDto { Id = 9, Display = "10" },
            new LookupDto { Id = 10, Display = "150" },
            new LookupDto { Id = 11, Display = "75" },
            new LookupDto { Id = 12, Display = "5" },
        };

        public static Dictionary<int, double> GetDeliveryMultipliersBySpotLengthId()
        {
            var deliveryMultipliersBySpotLengthId = new Dictionary<int, double>
            {
                {1,1},
                {2,2},
                {3,0.5},
                {4,4},
                {5,6},
                {6,10},
                {7,3},
                {8,1.5},
                {9,0.333333},
                {10,5},
                {11,2.5},
                {12,1.66667},
            };

            return deliveryMultipliersBySpotLengthId;
        }

        public static Dictionary<int, decimal> GetCostMultipliersBySpotLengthId(bool applyInventoryPremium = true)
        {
            var deliveryMultipliersBySpotLengthId = new Dictionary<int, decimal>
            {
                {1,1},
                {2,2},
                {3,  (.5m + (applyInventoryPremium ? 0.15m : 0m))},
                {4,4},
                {7,3},
                {8,1.5m},
                {5,6},
                {6,10},
                {9,0.333333m},
                {10,5},
                {11,2.5m},
                {12,1.66667m},
            };

            return deliveryMultipliersBySpotLengthId;
        }
    }
}