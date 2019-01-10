using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface ISpotLengthEngine : IApplicationService
    {
        int GetSpotLengthIdByValue(int spotLengthValue);

        int GetSpotLengthValueById(int spotLengthId);
    }

    public class SpotLengthEngine : ISpotLengthEngine
    {
        private readonly Dictionary<int, int> _SpotLengthsDict;

        public SpotLengthEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _SpotLengthsDict = broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
        }

        public int GetSpotLengthIdByValue(int spotLengthValue)
        {
            if (_SpotLengthsDict.TryGetValue(spotLengthValue, out var spotLengthId))
            {
                return spotLengthId;
            }

            throw new Exception($"Invalid spot length value: '{spotLengthValue}' found");
        }

        public int GetSpotLengthValueById(int spotLengthId)
        {
            var spotLenght = _SpotLengthsDict.Single(x => x.Value == spotLengthId, $"Invalid spot length id: '{spotLengthId}' found");
            return spotLenght.Key;
        }
    }
}
