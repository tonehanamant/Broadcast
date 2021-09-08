using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttachmentMicroServiceApiTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var tests = new List<ITest>
            {
                new AttachmentServiceApiClientTest()
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
