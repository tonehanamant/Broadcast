using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Entities.BarterInventory;
using System.Collections.Generic;
using System.Linq;

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
