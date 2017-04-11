using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Homm.Client.Tests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    internal class WarFogMapTests
    {
        [Test]
        public void CreateTest()
        {
            var map = new WarFogMap();
        }

        [Test]
        public void UpdateTest()
        {
            var map = new WarFogMap();
            Assert.IsTrue(map.Map.Count == 0);
            var mapProvider = DummyLocationMap.GetExampleMap();
            map.UpdateMap(mapProvider);
            foreach (var mapObject in mapProvider.Map.Objects)
                Assert.IsTrue(map[mapObject.Location.ToLocation()] == mapObject);
        }
    }
}