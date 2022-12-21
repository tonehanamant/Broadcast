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
                    var client = test.CreateHttpClient();
                    await test.RunPricingTrue(client);
                    await test.RunBuyingTrue(client);
                    await test.RunPricingFalse(client);
                    await test.RunBuyingFalse(client);
                    await test.RunPricingTrue(client);
                    await test.RunBuyingTrue(client);
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
