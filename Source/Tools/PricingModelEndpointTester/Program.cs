using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PricingModelEndpointTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var tests = new List<ITest>
            {
                new PricingApiClientTest()
                , new PricingJobQueueApiClientTest()
            };

            try
            {
                tests.ForEach(t => t.Run());
            }
            catch (Exception e)
            {
                Console.WriteLine("****************************");
                Console.WriteLine("Error caught !!!!! : ");
                Console.WriteLine(e);
                Console.WriteLine("****************************");
            }

            Console.WriteLine("All done!  Hit <ENTER> to end...");
            Console.ReadLine();
        }
    }
}
