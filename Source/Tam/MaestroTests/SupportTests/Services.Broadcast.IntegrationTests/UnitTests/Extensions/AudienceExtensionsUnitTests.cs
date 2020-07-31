using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class AudienceExtensionsUnitTests
    {
        [Test]
        public void ToDisplayAudience()
        {
            var id = 13;
            var name = "W18+";
            var broadcastAudience = new BroadcastAudience() { Id = id, Name = name };

            var result = broadcastAudience.ToDisplayAudience();

            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(name, result.AudienceString);
        }
    }
}
