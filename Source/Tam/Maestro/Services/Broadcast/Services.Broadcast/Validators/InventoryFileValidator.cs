using System.Text.RegularExpressions;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Validators
{
    public interface IInventoryFileValidator
    {
        InventoryFileValidatorResult ValidateInventoryFile(InventoryFile inventoryFile);

        InventoryFileProblem DuplicateRecordValidation(string station);
    }

    public class InventoryFileValidator : IInventoryFileValidator
    {
        private IInventoryRepository _inventoryRepository;

        public InventoryFileValidator(IDataRepositoryFactory dataRepositoryFactory)
        {
            _inventoryRepository = dataRepositoryFactory.GetDataRepository<IInventoryRepository>();
        }

        public InventoryFileValidatorResult ValidateInventoryFile(InventoryFile inventoryFile)
        {
            var results = new InventoryFileValidatorResult();
            results.InventoryFileProblems.AddRange(_CheckForDuplicateRecords(inventoryFile));
            _CheckForExistingRecords(inventoryFile);

            return results;
        }

        //Only checks for ungrouped manifest records (Open Market Only)
        private List<InventoryFileProblem> _CheckForExistingRecords(InventoryFile inventoryFile)
        {
            var result = new List<InventoryFileProblem>();

            foreach (var manifest in inventoryFile.InventoryManifests.ToList())
            {
                foreach (var manifestDaypart in manifest.ManifestDayparts.ToList())
                {
                     var exists = _inventoryRepository.CheckIfManifestByStationProgramFlightDaypartExists(
                        manifest.Station.Code,
                        manifestDaypart.ProgramName,
                        manifest.EffectiveDate,
                        manifest.EndDate.Value,
                        manifestDaypart.Daypart.Id);
                    if (exists)
                    {
                        manifest.ManifestDayparts.Remove(manifestDaypart);
                        if (!manifest.ManifestDayparts.Any())
                        {
                            inventoryFile.InventoryManifests.Remove(manifest);
                        }
                        result.Add(new InventoryFileProblem()
                        {
                            ProblemDescription = "There is already an existing program with the same flight and airtime.",
                            ProgramName = manifestDaypart.ProgramName,
                            StationLetters = manifest.Station.LegacyCallLetters
                        });
                    }
                }
                
            }

            return result;
        }

        private List<InventoryFileProblem> _CheckForDuplicateRecords(InventoryFile inventoryFile)
        {
            var validationProblems = new List<InventoryFileProblem>();
            foreach (var inventoryGroup in inventoryFile.InventoryGroups)
            {
                var spotLenghtStationGroups = inventoryGroup.Manifests.GroupBy(
                    m => new
                    {
                        m.SpotLengthId,
                        m.Station.LegacyCallLetters
                    }).Select(g => g).ToList();
                foreach (var spotLenghtStationGroup in spotLenghtStationGroups)
                {
                    var duplicateProblems =
                        spotLenghtStationGroup.SelectMany(g => g.ManifestDayparts)
                            .GroupBy(d => d.Daypart.ToLongString())
                            .Where(g => g.Count() > 1)
                            .Select(d => DuplicateRecordValidation(spotLenghtStationGroup.Key.LegacyCallLetters)).ToList();

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
