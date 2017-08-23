using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.Entities;

namespace Services.Broadcast.Validators
{
    public interface IRatesFileValidator
    {
        void ValidateRatesFile(RatesFile ratesFile);
    }

    public class RatesFileValidator : IRatesFileValidator
    {

        public void ValidateRatesFile(RatesFile ratesFile)
        {
            throw new NotImplementedException();
        }
    }
}
