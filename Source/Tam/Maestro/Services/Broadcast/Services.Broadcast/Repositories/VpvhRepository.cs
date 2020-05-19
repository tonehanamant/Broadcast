using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Vpvh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IVpvhRepository : IDataRepository
    {
        bool HasFile(string fileHash);

        void SaveFile(VpvhFile vpvhFile);
    }

    public class VpvhRepository : BroadcastRepositoryBase, IVpvhRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        public VpvhRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public bool HasFile(string fileHash) =>
            _InReadUncommitedTransaction(
                context => context.vpvh_files.Any(x => x.file_hash == fileHash));

        public void SaveFile(VpvhFile vpvhFile)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.vpvh_files.Add(new vpvh_files
                {
                    created_date = vpvhFile.CreatedDate,
                    created_by = vpvhFile.CreatedBy,
                    error_message = vpvhFile.ErrorMessage,
                    file_hash = vpvhFile.FileHash,
                    file_name = vpvhFile.FileName,
                    success = vpvhFile.Success,
                    vpvhs = vpvhFile.Items.Select(v => new vpvh
                    {
                        am_news = v.AMNews,
                        audience_id = v.Audience.Id,
                        pm_news = v.PMNews,
                        quarter = v.Quarter,
                        syn_all = v.SynAll,
                        year = v.Year
                    }).ToList()
                });

                context.SaveChanges();
            });
        }
    }
}
