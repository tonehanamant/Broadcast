using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
public	class HealthResponseDto
	{
		public string ExecutingAssemblyVersion { get; set; }
		//No need for file version
		public string ExecutingAssemblyName { get; set; }
		public string ExecutingAssemblyCreationTime { get; set; }
		public string ApiBuildContent { get; set; }
		// currently we are looking for one dependent assembly Services.Broadcast. If we add up more in it. Following will be replaced by List
		public string DependentAssemblyName { get; set; }
		public string DependentAssemblyVersion { get; set; }
		public string DependentCreationTime { get; set; }

	}
}
