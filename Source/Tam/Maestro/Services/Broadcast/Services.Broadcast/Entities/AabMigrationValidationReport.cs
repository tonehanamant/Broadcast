using System.Collections.Generic;
using System.Text;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore.Internal;
using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.Entities
{

    public class AabMigrationValidationReport
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is safe to migrate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is safe to migrate; otherwise, <c>false</c>.
        /// </value>
        public bool IsSafeToMigrate
        {
            get
            {
                if (MissingAgencies.Any()
                    || MissingAdvertisers.Any()
                    || MissingProducts.Any())
                {
                    return false;
                }

                return true;
            }
        }
        
        public int CampaignCount { get; set; }

        public int NullAgencyMasterCount { get; set; }

        public int NullAdvertiserMasterCount { get; set; }

        public int AgencyMasterAabMismatchCount { get; set; }

        public int AdvertiserMasterAabMismatchCount { get; set; }

        public int AgencyIdAabMismatchCount { get; set; }

        public int AdvertiserIdAabMismatchCount { get; set; }

        public int PlanCount { get; set; }

        public int NullProductMasterCount { get; set; }

        public int ProductMasterAabMismatch { get; set; }

        public int ProductIdAabMismatch { get; set; }

        public List<AgencyDto> MissingAgencies { get; set; } = new List<AgencyDto>();

        public List<AdvertiserDto> MissingAdvertisers { get; set; } = new List<AdvertiserDto>();

        public List<ProductDto> MissingProducts { get; set; } = new List<ProductDto>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Validation Result");
            sb.AppendLine($"\t{nameof(IsSafeToMigrate)} : {IsSafeToMigrate}");
            sb.AppendLine($"\t{nameof(MissingAgencies)} : {MissingAgencies.Count}");
            MissingAgencies.ForEach(a => sb.AppendLine($"\t\tID : {a.Id}; Name : '{a.Name}'; MasterId : '{a.MasterId}';"));
            sb.AppendLine($"\t{nameof(MissingAdvertisers)} : {MissingAdvertisers.Count}");
            MissingAdvertisers.ForEach(a => sb.AppendLine($"\t\tID : {a.Id}; Name : '{a.Name}'; MasterId : '{a.MasterId}';"));
            sb.AppendLine($"\t{nameof(MissingProducts)} : {MissingProducts.Count}");
            MissingProducts.ForEach(a => sb.AppendLine($"\t\tID : {a.Id}; Name : '{a.Name}'; MasterId : '{a.MasterId}';"));

            sb.AppendLine("");
            sb.AppendLine("Counts");
            sb.AppendLine($"\t{nameof(CampaignCount)} : {CampaignCount}");
            sb.AppendLine($"\t{nameof(NullAgencyMasterCount)} : {NullAgencyMasterCount}");
            sb.AppendLine($"\t{nameof(NullAdvertiserMasterCount)} : {NullAdvertiserMasterCount}");
            sb.AppendLine($"\t{nameof(AgencyMasterAabMismatchCount)} : {AgencyMasterAabMismatchCount}");
            sb.AppendLine($"\t{nameof(AdvertiserMasterAabMismatchCount)} : {AdvertiserMasterAabMismatchCount}");
            sb.AppendLine($"\t{nameof(AgencyIdAabMismatchCount)} : {AgencyIdAabMismatchCount}");
            sb.AppendLine($"\t{nameof(AdvertiserIdAabMismatchCount)} : {AdvertiserIdAabMismatchCount}");

            sb.AppendLine($"\t{nameof(PlanCount)} : {PlanCount}");
            sb.AppendLine($"\t{nameof(NullProductMasterCount)} : {NullProductMasterCount}");
            sb.AppendLine($"\t{nameof(ProductMasterAabMismatch)} : {ProductMasterAabMismatch}");
            sb.AppendLine($"\t{nameof(ProductIdAabMismatch)} : {ProductIdAabMismatch}");
         
            return sb.ToString();
        }
    }
}