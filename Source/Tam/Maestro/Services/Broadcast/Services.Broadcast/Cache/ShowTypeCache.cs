using Common.Services.Repositories;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Cache
{
    public interface IShowTypeCache
    {
        LookupDto GetShowTypeByName(string name);
    }

    public class ShowTypeCache : IShowTypeCache
    {
        private readonly Dictionary<string, LookupDto> _ShowTypeByName;

        public ShowTypeCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            var showTypeRepository = broadcastDataRepositoryFactory.GetDataRepository<IShowTypeRepository>();
            var showTypes = showTypeRepository.GetShowTypes();

            _ShowTypeByName = showTypes.ToDictionary(x => x.Display, x => x);
        }

        public LookupDto GetShowTypeByName(string name)
        {
            if (_ShowTypeByName.TryGetValue(name, out var showType))
                return showType;
            else
                throw new Exception($"No show type was found by name : {name}");
        }
    }
}
