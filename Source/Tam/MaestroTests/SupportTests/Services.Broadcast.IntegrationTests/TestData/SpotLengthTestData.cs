using System.Collections.Generic;

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
	}
}