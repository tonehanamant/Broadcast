using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PricingModelEndpointTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var tests = new List<ITest>
            {
                new PricingJobQueueApiClientTest()
            };

            try
            {
                //tests.ForEach(t => await t.Run());
                foreach(var test in tests)
                {
                    await test.Run();
                }
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
