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
    }

    public class InventoryFileValidator : IInventoryFileValidator
    {
        public InventoryFileValidatorResult ValidateInventoryFile(InventoryFile inventoryFile)
        {
            var results = new InventoryFileValidatorResult();
            // so far, nothing to validate.
            //Todo: if this method and class remains empty upon completion of the new inventory structure, consider removal.
            return results;
        }
    }
}
