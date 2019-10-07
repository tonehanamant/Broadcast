using Services.Broadcast.Entities;
using System.Linq;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IBaseInventoryFileImporter
    {
        void PopulateInventoryFileDateRange(InventoryFileBase inventoryFile);
    }

    public class BaseInventoryFileImporter : IBaseInventoryFileImporter
    {
        public void PopulateInventoryFileDateRange(InventoryFileBase inventoryFile)
        {
            var weeks = inventoryFile.GetAllManifests().SelectMany(x => x.ManifestWeeks).Select(x => x.MediaWeek);

            if (weeks.Any())
            {
                inventoryFile.EffectiveDate = weeks.Min(x => x.StartDate);
                inventoryFile.EndDate = weeks.Max(x => x.EndDate);
            }
        }
    }
}
