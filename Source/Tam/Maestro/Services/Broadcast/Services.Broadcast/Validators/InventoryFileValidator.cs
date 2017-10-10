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
        public InventoryFileValidatorResult ValidateInventoryFile(InventoryFile inventoryFile)
        {
            var results = new InventoryFileValidatorResult();
            results.InventoryFileProblems = _CheckForDuplicateRecords(inventoryFile);
            return results;
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
                    });
                foreach (var spotLenghtStationGroup in spotLenghtStationGroups)
                {
                    var duplicateProblems =
                        spotLenghtStationGroup.SelectMany(g => g.Dayparts)
                            .GroupBy(d => d.ToLongString())
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
