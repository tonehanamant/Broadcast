using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
	public  class HealthServiceUnitTestClass: HealthService
	{
		protected  override string _GetExecutingAssemblyLocalPath(Assembly executingAssembly)
		{
			return Path.GetDirectoryName($@".\Files\api_build.txt");
		}

	}
}
