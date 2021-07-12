using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Entities.ProprietaryInventory;
using System.Collections.Generic;
using System.Linq;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities;
using Common.Services.Extensions;
using ConfigurationService.Client;
using static Services.Broadcast.Entities.Enums.ProposalEnums;
using System.Data.Entity;
using System;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories
{
    public interface IProprietaryRepository : IDataRepository
    {
        /// <summary>
        /// Save a proprietary file object
        /// </summary>
        /// <param name="proprietaryFile">Proprietary file object</param>
        void SaveProprietaryInventoryFile(ProprietaryInventoryFile proprietaryFile);

        ProprietaryInventoryFile GetInventoryFileWithHeaderById(int fileId);

        /// <summary>
        /// Gets the header information for an inventory file id
        /// </summary>
        /// <param name="inventoryFileId">Inventory file id to get the data for</param>
        /// <returns>ProprietaryInventoryHeader object containing the header data</returns>
        ProprietaryInventoryHeader GetInventoryFileHeader(int inventoryFileId);
    }

    public class ProprietaryInventoryRepository : BroadcastRepositoryBase, IProprietaryRepository
    {
        public ProprietaryInventoryRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public ProprietaryInventoryFile GetInventoryFileWithHeaderById(int fileId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var file = context.inventory_files.Single(x => x.id == fileId, $"Could not find existing file with id={fileId}");
                var header = file.inventory_file_proprietary_header.First();
                var audience = header.audience;

                var result = new ProprietaryInventoryFile
                {
                    Id = file.id,
                    FileName = file.name,
                    FileStatus = (FileStatusEnum)file.status,
                    Hash = file.file_hash,
                    UniqueIdentifier = file.identifier,
                    CreatedBy = file.created_by,
                    CreatedDate = file.created_date,
                    RowsProcessed = file.rows_processed,
                    EffectiveDate = file.effective_date,
                    EndDate = file.end_date,
                    InventorySource = new InventorySource
                    {
                        Id = file.inventory_sources.id,
                        InventoryType = (InventorySourceTypeEnum)file.inventory_sources.inventory_source_type,
                        IsActive = file.inventory_sources.is_active,
                        Name = file.inventory_sources.name
                    },
                    Header = new ProprietaryInventoryHeader
                    {
                        ContractedDaypartId = header.contracted_daypart_id,
                        Cpm = header.cpm,
                        DaypartCode = header.standard_dayparts?.code,
                        EffectiveDate = header.effective_date,
                        EndDate = header.end_date,
                        HutBookId = header.hut_projection_book_id,
                        ShareBookId = header.share_projection_book_id,
                        PlaybackType = (ProposalPlaybackType?)header.playback_type,
                        NtiToNsiIncrease = header.nti_to_nsi_increase
                    }
                };

                if (audience != null)
                {
                    result.Header.Audience = new BroadcastAudience
                    {
                        Id = audience.id,
                        CategoryCode = (EBroadcastAudienceCategoryCode)audience.category_code,
                        SubCategoryCode = audience.sub_category_code,
                        RangeStart = audience.range_start,
                        RangeEnd = audience.range_end,
                        Custom = audience.custom,
                        Code = audience.code,
                        Name = audience.name
                    };
                }

                return result;
            });
        }

        /// <summary>
        /// Gets the header information for an inventory file id
        /// </summary>
        /// <param name="inventoryFileId">Inventory file id to get the data for</param>
        /// <returns>ProprietaryInventoryHeader object containing the header data</returns>
        public ProprietaryInventoryHeader GetInventoryFileHeader(int inventoryFileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var queryResult = context.inventory_file_proprietary_header
                        .Include(x => x.audience)
                        .Where(x => x.inventory_file_id == inventoryFileId)
                        .Single();

                    var audience = queryResult.audience;
                    var header = new ProprietaryInventoryHeader
                    {
                        ContractedDaypartId = queryResult.contracted_daypart_id,
                        Cpm = queryResult.cpm,
                        DaypartCode = queryResult.standard_dayparts.code,
                        EffectiveDate = queryResult.effective_date,
                        EndDate = queryResult.end_date,
                        HutBookId = queryResult.hut_projection_book_id,
                        PlaybackType = (ProposalPlaybackType)queryResult.playback_type,
                        ShareBookId = queryResult.share_projection_book_id,
                        NtiToNsiIncrease = queryResult.nti_to_nsi_increase
                    };

                    if (audience != null)
                    {
                        header.Audience = new BroadcastAudience
                        {
                            Id = audience.id,
                            CategoryCode = (EBroadcastAudienceCategoryCode)audience.category_code,
                            SubCategoryCode = audience.sub_category_code,
                            RangeStart = audience.range_start,
                            RangeEnd = audience.range_end,
                            Custom = audience.custom,
                            Code = audience.code,
                            Name = audience.name
                        };
                    }

                    return header;
                });
        }

        /// <summary>
        /// Save a proprietary file object
        /// </summary>
        /// <param name="proprietaryFile">Proprietary file object</param>
        public void SaveProprietaryInventoryFile(ProprietaryInventoryFile proprietaryFile)
        {
            _InReadUncommitedTransaction(context =>
            {
                var inventoryFile = context.inventory_files.Single(x => x.id == proprietaryFile.Id);

                inventoryFile.status = (byte)proprietaryFile.FileStatus;
                inventoryFile.rows_processed = proprietaryFile.RowsProcessed;
                inventoryFile.effective_date = proprietaryFile.EffectiveDate;
                inventoryFile.end_date = proprietaryFile.EndDate;
                inventoryFile.inventory_file_proprietary_header = new List<inventory_file_proprietary_header>{
                            new inventory_file_proprietary_header
                            {
                                audience_id = proprietaryFile.Header.Audience?.Id,
                                contracted_daypart_id = proprietaryFile.Header.ContractedDaypartId,
                                cpm = proprietaryFile.Header.Cpm,
                                standard_daypart_id = proprietaryFile.Header.DaypartCode == null ?
                                    (int?)null : 
                                    context.standard_dayparts.Single(x => x.code.Equals(proprietaryFile.Header.DaypartCode, StringComparison.InvariantCultureIgnoreCase)).id,
                                effective_date = proprietaryFile.Header.EffectiveDate,
                                end_date = proprietaryFile.Header.EndDate,
                                hut_projection_book_id = proprietaryFile.Header.HutBookId,
                                playback_type = (int?)proprietaryFile.Header.PlaybackType,
                                share_projection_book_id = proprietaryFile.Header.ShareBookId,
                                nti_to_nsi_increase = proprietaryFile.Header.NtiToNsiIncrease
                            }
                        };
                inventoryFile.inventory_file_problems = proprietaryFile.ValidationProblems.Select(
                            x => new inventory_file_problems
                            {
                                problem_description = x
                            }).ToList();
                context.SaveChanges();
            });
        }
    }
}
