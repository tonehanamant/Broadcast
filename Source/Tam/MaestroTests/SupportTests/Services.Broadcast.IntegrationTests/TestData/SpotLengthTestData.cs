using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.TestData
{
	public static class SpotLengthTestData
	{
		public static Dictionary<int, int> GetSpotLengthAndIds()
		{
			var spotLengthMap = new Dictionary<int, int>();
			spotLengthMap.Add(30, 1);
			spotLengthMap.Add(60, 2);
			spotLengthMap.Add(15, 3);
			spotLengthMap.Add(120, 4);
			spotLengthMap.Add(180, 5);
			spotLengthMap.Add(300, 6);
			spotLengthMap.Add(90, 7);
			spotLengthMap.Add(45, 8);

			return spotLengthMap;
		}

        public static List<LookupDto> GetAllSpotLengths()
        {
            return new List<LookupDto>
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
                new LookupDto { Id = 10, Display = "150" }
            };
        }

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
                {9,0.3},
                {10,5}
			};

            return deliveryMultipliersBySpotLengthId;
        }
	}
}