using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IIsciService : IApplicationService
    {
        /// <summary>
        /// Finds all the valid iscis based on the filter
        /// </summary>
        /// <param name="isciFilter">Isci filter</param>
        /// <returns>List of valid iscis</returns>
        List<string> FindValidIscis(string isciFilter);

        /// <summary>
        /// Loads isci mapping into the file detail object
        /// </summary>
        /// <param name="fileDetails">File details to load isci mappings to</param>
        void LoadIsciMappings(List<ScrubbingFileDetail> fileDetails);

        /// <summary>
        /// Ads a new mapping
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object containing the effective and the original isci</param>
        /// <param name="username">Username requesting the mapping</param>
        /// <returns>True or false</returns>
        bool AddIsciMapping(MapIsciDto mapIsciDto, string username);

        /// <summary>
        /// Removes iscis from blacklist table
        /// </summary>
        /// <param name="iscisToRemove">Isci list to remove</param>
        /// <returns>True or false</returns>
        bool RemoveIscisFromBlacklistTable(List<string> iscisToRemove);

        /// <summary>
        /// Adds a new record in isci_blacklist
        /// </summary>
        /// <param name="iscis">List of Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false</returns>
        bool BlacklistIscis(List<string> iscis, string username);
    }

    public class IsciService : IIsciService
    {
        private IIsciRepository _IsciRepository;

        public IsciService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _IsciRepository = broadcastDataRepositoryFactory.GetDataRepository<IIsciRepository>();
        }

        /// <summary>
        /// Ads a new mapping
        /// </summary>
        /// <param name="mapIsciDto">MapIsciDto object containing the effective and the original isci</param>
        /// <param name="username">Username requesting the mapping</param>
        /// <returns>True or false</returns>
        public bool AddIsciMapping(MapIsciDto mapIsciDto, string username)
        {
            return _IsciRepository.AddIsciMapping(mapIsciDto.OriginalIsci, mapIsciDto.EffectiveIsci, username);
        }

        /// <summary>
        /// Finds all the valid iscis based on the filter
        /// </summary>
        /// <param name="isciFilter">Isci filter</param>
        /// <returns>List of valid iscis</returns>
        public List<string> FindValidIscis(string isciFilter)
        {
            var iscis = _IsciRepository.FindValidIscis(isciFilter);
            var groupedIscis = iscis.GroupBy(x => new { x.HouseIsci, x.ProposalId });
            var distinctIscis = iscis.Select(x => x.HouseIsci).Distinct().ToList();
            foreach (var isci in distinctIscis)
            {
                if (groupedIscis.Where(x => x.Key.HouseIsci.Equals(isci)).Count() > 1
                    && iscis.Any(x => x.HouseIsci.Equals(isci) && x.Married == false))
                {
                    iscis.RemoveAll(x => x.HouseIsci.Equals(isci));
                }
            }

            return iscis.Select(x => x.HouseIsci).ToList();
        }

        /// <summary>
        /// Loads isci mapping into the file detail object
        /// </summary>
        /// <param name="fileDetails">File details to load isci mappings to</param>
        public void LoadIsciMappings(List<ScrubbingFileDetail> fileDetails)
        {
            Dictionary<string, string> isciMappings = _IsciRepository.LoadIsciMappings(fileDetails.Select(x => x.Isci).ToList());
            if (isciMappings.Count > 0)
            {
                fileDetails.ForEach(x =>
                {
                    if (isciMappings.TryGetValue(x.Isci, out string value))
                    {
                        x.MappedIsci = value;
                    }
                });
            }
        }

        /// <summary>
        /// Removes iscis from blacklist table
        /// </summary>
        /// <param name="iscisToRemove">Isci list to remove</param>
        /// <returns>True or false</returns>
        public bool RemoveIscisFromBlacklistTable(List<string> iscisToRemove)
        {
            return _IsciRepository.RemoveIscisFromBlacklistTable(iscisToRemove);
        }

        /// <summary>
        /// Adds a new record in isci_blacklist
        /// </summary>
        /// <param name="iscis">List of Iscis to archive</param>
        /// <param name="username">User requesting the change</param>
        /// <returns>True or false</returns>
        public bool BlacklistIscis(List<string> iscis, string username)
        {
            return _IsciRepository.ArchiveIscis(iscis, username);
        }
    }
}
