using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Validators
{
    public interface IInventoryFileValidator
    {
        List<InventoryFileProblem> ValidateInventoryFile(InventoryFile inventoryFile);

        InventoryFileProblem DuplicateRecordValidation(string station);
    }

    public class InventoryFileValidator : IInventoryFileValidator
    {
        private IInventoryRepository _inventoryRepository;

        public InventoryFileValidator(IDataRepositoryFactory dataRepositoryFactory)
        {
            _inventoryRepository = dataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        public List<InventoryFileProblem> ValidateInventoryFile(InventoryFile inventoryFile)
        {
            var results = new List<InventoryFileProblem>();
            results.AddRange(_CheckForDuplicateRecords(inventoryFile));
            return results;
        }

        private List<InventoryFileProblem> _CheckForDuplicateRecords(InventoryFile inventoryFile)
        {
            var validationProblems = new List<InventoryFileProblem>();
            foreach (var inventoryGroup in inventoryFile.InventoryGroups)
            {
                var spotLengthStationGroups = inventoryGroup.Manifests.GroupBy(
                    m => new
                    {
                        m.SpotLengthId,
                        m.Station.LegacyCallLetters
                    }).Select(g => g).ToList();
                foreach (var spotLengthStationGroup in spotLengthStationGroups)
                {
                    var duplicateProblems =
                        spotLengthStationGroup.SelectMany(g => g.ManifestDayparts)
                            .GroupBy(d => d.Daypart.ToLongString())
                            .Where(g => g.Count() > 1)
                            .Select(d => DuplicateRecordValidation(spotLengthStationGroup.Key.LegacyCallLetters)).ToList();

                    if (duplicateProblems.Count > 0)
                    {
                        validationProblems.AddRange(duplicateProblems);
                    }
                }
            }
            return validationProblems;
        }

        public InventoryFileProblem DuplicateRecordValidation(string station)
        {
            return
                new InventoryFileProblem(
                    string.Format("Invalid data for Station {0}, duplicate entry for same spot length", station));
        }
    }
}
