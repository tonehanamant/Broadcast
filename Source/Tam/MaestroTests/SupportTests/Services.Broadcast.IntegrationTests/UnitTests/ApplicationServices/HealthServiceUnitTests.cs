using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices

{
	[TestFixture]
	public class HealthServiceUnitTests
	{
		[Test]
		public void GetInfoTest()
		{
			// Arrange
			var healthService = new HealthServiceUnitTestClass();
			var assembly = Assembly.GetExecutingAssembly();

			// Act
			var  result=	healthService.GetInfo(assembly);

			// Assert
			Assert.IsNotNull(result.ApiBuildContent);
			Assert.AreEqual("Services.Broadcast.dll",result.DependentAssemblyName );
			Assert.IsNotNull(result.DependentCreationTime);
			Assert.IsNotNull(result.DependentAssemblyVersion);
			Assert.IsNotNull(result.ExecutingAssemblyCreationTime);
			Assert.IsNotNull(result.ExecutingAssemblyName);
			Assert.IsNotNull(result.ExecutingAssemblyVersion);

		}
	}
}
