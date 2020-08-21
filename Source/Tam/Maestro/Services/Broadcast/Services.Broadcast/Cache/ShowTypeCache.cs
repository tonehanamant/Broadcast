using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Cache
{
    public interface IShowTypeCache
    {
        LookupDto GetMaestroShowTypeLookupDtoByName(string name);
        LookupDto GetMasterShowTypeLookupDtoByName(string name);
        ShowTypeDto GetMaestroShowTypeByName(string name);
        ShowTypeDto GetMasterShowTypeByName(string name);
        ShowTypeDto GetMaestroShowTypeByMasterShowType(ShowTypeDto masterShowType);
        ShowTypeDto GetMasterShowTypeByMaestroShowType(ShowTypeDto maestroShowType);
    }

    public class ShowTypeCache : IShowTypeCache
    {
        private readonly Dictionary<string, LookupDto> _MaestroShowTypesLookupDtoByName;
        private readonly Dictionary<string, LookupDto> _MasterShowTypesLookupDtoByName;
        private readonly Dictionary<int, LookupDto> _MaestroShowTypesLookupDtoById;
        private readonly Dictionary<int, LookupDto> _MasterShowTypesLookupDtoById;
        private readonly Dictionary<string, ShowTypeDto> _MaestroShowTypesByName;
        private readonly Dictionary<string, ShowTypeDto> _MasterShowTypesByName;
        private readonly Dictionary<ShowTypeDto, ShowTypeDto> _MaestroShowTypesByMasterShowTypes;
        private readonly Dictionary<ShowTypeDto, ShowTypeDto> _MasterShowTypesByMaestroShowTypes;

        public ShowTypeCache(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;

            var showTypeRepository = broadcastDataRepositoryFactory.GetDataRepository<IShowTypeRepository>();

            var maestroShowTypes = showTypeRepository.GetMaestroShowTypes();
            var masterShowTypes = showTypeRepository.GetMasterShowTypes();

            var maestroShowTypeLookupDtos = maestroShowTypes.Select(s => s.ToLookupDto()).ToList();
            var masterShowTypeLookupDtos = masterShowTypes.Select(s => s.ToLookupDto()).ToList();

            _MaestroShowTypesByName = maestroShowTypes.ToDictionary(x => x.Name, x => x, comparer);
            _MasterShowTypesByName = masterShowTypes.ToDictionary(x => x.Name, x => x, comparer);

            _MasterShowTypesByMaestroShowTypes = showTypeRepository.GetShowTypeMappings();
            _MaestroShowTypesByMasterShowTypes = _MasterShowTypesByMaestroShowTypes.ToDictionary(x => x.Value, x => x.Key);

            _MaestroShowTypesLookupDtoByName = maestroShowTypeLookupDtos.ToDictionary(x => x.Display, x => x, comparer);
            _MasterShowTypesLookupDtoByName = masterShowTypeLookupDtos.ToDictionary(x => x.Display, x => x, comparer);

            _MaestroShowTypesLookupDtoById = maestroShowTypeLookupDtos.ToDictionary(x => x.Id, x => x);
            _MasterShowTypesLookupDtoById = masterShowTypeLookupDtos.ToDictionary(x => x.Id, x => x);
        }

        public LookupDto GetMaestroShowTypeLookupDtoByName(string name)
        {
            if (_MaestroShowTypesLookupDtoByName.TryGetValue(name, out var showType))
                return showType;
            else
                throw new Exception($"No show type was found by name : {name}");
        }

        public LookupDto GetMasterShowTypeLookupDtoByName(string name)
        {
            if (_MasterShowTypesLookupDtoByName.TryGetValue(name, out var showType))
                return showType;
            else
                throw new Exception($"No show type was found by name : {name}");
        }

        public ShowTypeDto GetMaestroShowTypeByMasterShowType(ShowTypeDto masterShowType)
        {
            if (_MaestroShowTypesByMasterShowTypes.TryGetValue(masterShowType, out var maestroShowType))
                return maestroShowType;
            else
                throw new Exception($"No show type was found by master show type: {masterShowType}");
        }

        public ShowTypeDto GetMasterShowTypeByMaestroShowType(ShowTypeDto maestroShowType)
        {
            if (_MasterShowTypesByMaestroShowTypes.TryGetValue(maestroShowType, out var masterShowType))
                return masterShowType;
            else
                throw new Exception($"No show type was found by maestro show type: {maestroShowType}");
        }

        public ShowTypeDto GetMaestroShowTypeByName(string name)
        {
            if (_MaestroShowTypesByName.TryGetValue(name, out var showType))
                return showType;
            else
                throw new Exception($"No show type was found by name : {name}");
        }

        public ShowTypeDto GetMasterShowTypeByName(string name)
        {
            if (_MasterShowTypesByName.TryGetValue(name, out var showType))
                return showType;
            else
                throw new Exception($"No show type was found by name : {name}");
        }
    }
}
