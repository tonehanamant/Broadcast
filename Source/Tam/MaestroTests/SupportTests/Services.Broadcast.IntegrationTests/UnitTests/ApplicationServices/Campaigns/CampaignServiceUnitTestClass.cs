using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Campaigns
{

    public class CampaignServiceUnitTestClass : CampaignService
    {
        public ICampaignValidator CampaignValidator { get; set; }

        public ICampaignServiceData CampaignServiceData { get; set; }

        public CampaignServiceUnitTestClass(IDataRepositoryFactory dataRepositoryFactory, 
                                            ISMSClient smsClient, 
                                            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, 
                                            IQuarterCalculationEngine quarterCalculationEngine)
            : base(dataRepositoryFactory, smsClient, mediaMonthAndWeekAggregateCache, quarterCalculationEngine)
        {
        }

        protected override ICampaignServiceData GetCampaignServiceData()
        {
            return CampaignServiceData;
        }

        protected override ICampaignValidator GetCampaignValidator()
        {
            return CampaignValidator;
        }
    }
}