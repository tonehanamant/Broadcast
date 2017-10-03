using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Validators
{
    public interface IRatesFileValidator
    {
        RatesFileValidatorResult ValidateRatesFile(RatesFile ratesFile);
    }

    public class RatesFileValidator : IRatesFileValidator
    {
        public RatesFileValidatorResult ValidateRatesFile(RatesFile ratesFile)
        {
            var results = new RatesFileValidatorResult();
            // so far, nothing to validate.
            //Todo: if this method and class remains empty upon completion of the new inventory structure, consider removal.
            return results;
        }
    }
}
