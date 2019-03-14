using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Entities.BarterInventory;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities;
using Common.Services.Extensions;
using static Services.Broadcast.Entities.Enums.ProposalEnums;

namespace Services.Broadcast.Repositories
{
    public interface IBarterRepository : IDataRepository
    {
        /// <summary>
        /// Save a barter file object
        /// </summary>
        /// <param name="barterFile">Barter file object</param>
        void SaveBarterInventoryFile(BarterInventoryFile barterFile);

        /// <summary>
        /// Adds validation problems for a barter inventory file to DB
        /// </summary>
        /// <param name="barterFile">Barter inventory file</param>
        void AddValidationProblems(BarterInventoryFile barterFile);

        BarterInventoryFile GetBarterInventoryFileById(int fileId);
    }

    public class BarterInventoryRepository : BroadcastRepositoryBase, IBarterRepository
    {
        public BarterInventoryRepository(ISMSClient pSmsClient,
           IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory
           , ITransactionHelper pTransactionHelper)
           : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        /// <summary>
        /// Adds validation problems for a barter inventory file to DB
        /// </summary>
        /// <param name="barterFile">Barter inventory file</param>
        public void AddValidationProblems(BarterInventoryFile barterFile)
        {
            _InReadUncommitedTransaction(context =>
            {
                var inventoryFile = context.inventory_files.Single(x => x.id == barterFile.Id);
                inventoryFile.status = (byte)barterFile.FileStatus;
                inventoryFile.inventory_file_problems = barterFile.ValidationProblems.Select(
                            x => new inventory_file_problems
                            {
                                problem_description = x
                            }).ToList();
                context.SaveChanges();
            });
        }

        public BarterInventoryFile GetBarterInventoryFileById(int fileId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var file = context.inventory_files.Single(x => x.id == fileId, $"Could not find existing file with id={fileId}");

                return new BarterInventoryFile
                {
                    Id = file.id,
                    FileName = file.name,
                    FileStatus = (FileStatusEnum)file.status,
                    Hash = file.file_hash,
                    UniqueIdentifier = file.identifier,
                    CreatedBy = file.created_by,
                    CreatedDate = file.created_date,
                    InventorySource = new InventorySource
                    {
                        Id = file.inventory_sources.id,
                        InventoryType = (InventorySourceTypeEnum)file.inventory_sources.inventory_source_type,
                        IsActive = file.inventory_sources.is_active,
                        Name = file.inventory_sources.name
                    },
                    Header = new BarterInventoryHeader
                    {
                        AudienceId = file.inventory_file_barter_header.First().audience_id,
                        ContractedDaypartId = file.inventory_file_barter_header.First().contracted_daypart_id,
                        Cpm = file.inventory_file_barter_header.First().cpm,
                        DaypartCode = file.inventory_file_barter_header.First().daypart_code,
                        EffectiveDate = file.inventory_file_barter_header.First().effective_date,
                        EndDate = file.inventory_file_barter_header.First().end_date,
                        HutBookId = file.inventory_file_barter_header.First().hut_projection_book_id,
                        ShareBookId = file.inventory_file_barter_header.First().share_projection_book_id,
                        PlaybackType = (ProposalPlaybackType)file.inventory_file_barter_header.First().playback_type
                    }
                };
            });
        }

        /// <summary>
        /// Save a barter file object
        /// </summary>
        /// <param name="barterFile">Barter file object</param>
        public void SaveBarterInventoryFile(BarterInventoryFile barterFile)
        {
            _InReadUncommitedTransaction(context =>
            {
                var inventoryFile = context.inventory_files.Single(x => x.id == barterFile.Id);

                inventoryFile.status = (byte)barterFile.FileStatus;
                inventoryFile.inventory_file_barter_header = new List<inventory_file_barter_header>{
                            new inventory_file_barter_header
                            {
                                audience_id = barterFile.Header.AudienceId,
                                contracted_daypart_id = barterFile.Header.ContractedDaypartId,
                                cpm = barterFile.Header.Cpm,
                                daypart_code = barterFile.Header.DaypartCode,
                                effective_date = barterFile.Header.EffectiveDate,
                                end_date = barterFile.Header.EndDate,
                                hut_projection_book_id = barterFile.Header.HutBookId,
                                playback_type = (int)barterFile.Header.PlaybackType,
                                share_projection_book_id = barterFile.Header.ShareBookId
                            }
                        };
                inventoryFile.inventory_file_problems = barterFile.ValidationProblems.Select(
                            x => new inventory_file_problems
                            {
                                problem_description = x
                            }).ToList();
                context.SaveChanges();
            });
        }
    }
}
