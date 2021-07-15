using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanIsciService : IApplicationService
    {
        /// <summary>
        /// list of Iscis based on search key.
        /// </summary>
        /// <param name="isciSearch">Isci search input</param>       
        List<IsciListItemDto> GetAvailableIscis(IsciSearchDto isciSearch);
    }
    /// <summary>
    /// Operations related to the PlanIsci domain.
    /// </summary>
    /// <seealso cref="IPlanIsciService" />
    public class PlanIsciService : BroadcastBaseClass, IPlanIsciService
    {

        private readonly IPlanIsciRepository _PlanIsciRepository;
        public PlanIsciService(IDataRepositoryFactory dataRepositoryFactory)
        {
            _PlanIsciRepository = dataRepositoryFactory.GetDataRepository<IPlanIsciRepository>();

        }
        /// <inheritdoc />
        public List<IsciListItemDto> GetAvailableIscis(IsciSearchDto isciSearch)
        {

            List<IsciListItemDto> isciListDto = new List<IsciListItemDto>();
            IsciListItemDto isciListItemDto = new IsciListItemDto();
            IsciDto IsciItemDto = new IsciDto();
            var result = _PlanIsciRepository.GetAvailableIscis(isciSearch);
            var resultlamba = result.GroupBy(stu => stu.AdvertiserName).OrderBy(stu => stu.Key);
            foreach (var group in resultlamba)
            {
                isciListItemDto.AdvertiserName = group.Key;
                foreach (var item in group)
                {
                    IsciItemDto.Isci = item.Isci;
                    IsciItemDto.SpotLengthsString = Convert.ToString(item.SpotLengthsString);
                    IsciItemDto.ProductName = item.ProductName;
                    isciListItemDto.Iscis.Add(IsciItemDto);

                }
                isciListDto.Add(isciListItemDto);
            }
            return isciListDto;


        }
    }

}
