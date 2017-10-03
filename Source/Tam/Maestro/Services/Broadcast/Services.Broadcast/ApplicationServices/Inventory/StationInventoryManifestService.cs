
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Systems.LockTokens;
using Services.Broadcast.Entities;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices.Inventory
{
    public interface IStationInventoryManifestService : IApplicationService
    {
        void SaveStationInventoryManifest(StationInventoryManifest manifest);
    }

    public class StationInventoryManifestService : IStationInventoryManifestService
    {
        public StationInventoryManifestService()
        {
        }


        public void SaveStationInventoryManifest(StationInventoryManifest manifest)
        {
            // do we want to lock?
            // using (manifest.Id.HasValue ? new BomsLockManager(_SmsClient, new ProposalToken(manifest.Id.Value)) : null)
            
            // check for matches

        }
    }
}