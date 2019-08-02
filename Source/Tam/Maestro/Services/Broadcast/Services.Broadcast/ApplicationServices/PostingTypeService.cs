using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostingTypeService : IApplicationService
    {
        /// <summary>
        /// Gets the posting types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPostingTypes();
    }

    public class PostingTypeService : IPostingTypeService
    {
        ///<inheritdoc/>
        public List<LookupDto> GetPostingTypes()
        {
            return EnumExtensions.ToLookupDtoList<PostingTypeEnum>().OrderByDescending(x=>x.Id == (int)PostingTypeEnum.NTI).ToList();
        }
    }
}
