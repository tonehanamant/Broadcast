using Common.Services.ApplicationServices;
using Services.Broadcast.Entities.StationInventory;

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