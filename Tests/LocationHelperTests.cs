using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HoMM;
using NUnit.Framework;
using static Homm.Client.Tests.DummyLocationMap;
using Wall = HoMM.ClientClasses.Wall;

namespace Homm.Client.Tests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    internal class LocationHelperTests
    {
        [Test]
        public void TestCreate()
        {
            var helper = new LocationHelper(GetExampleMap());
        }

        [Test]
        public void TestInsideMap()
        {
            var map = GetExampleMap();
            foreach (var mapObject in map.Map.Objects)
                Assert.IsTrue(LocationHelper.IsInsideMap(mapObject.Location.ToLocation(), map.Map));
        }

        [Test]
        public void TestOutsideMap()
        {
            var map = GetExampleMap();
            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(-1, 0), map.Map));
            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(0, -1), map.Map));
            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(-1, -1), map.Map));

            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(3, 0), map.Map));
            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(0, 3), map.Map));
            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(3, 3), map.Map));

            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(100, 0), map.Map));
            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(0, 100), map.Map));
            Assert.IsFalse(LocationHelper.IsInsideMap(new Location(100, 100), map.Map));
        }


        [Test]
        public void TestCanStandEmpty()
        {
            var map = GetExampleMap();
            var locHelper = new LocationHelper(map);
            foreach (var mapObject in map.Map.Objects)
                Assert.IsTrue(locHelper.CanStandThere(mapObject.Location.ToLocation()),
                    $"Can't stand in ({mapObject.Location.X},{mapObject.Location.Y})");

            Assert.IsFalse(locHelper.CanStandThere(new Location(-1, -1)));
            Assert.IsFalse(locHelper.CanStandThere(new Location(20, 20)));
        }

        [Test]
        public void TestAvailableDirections1()
        {
            var map = GetExampleMap();
            foreach (var mapObject in map.Map.Objects)
            {
                if (!(mapObject.Location.X == 1 && mapObject.Location.Y == 1) &&
                    !(mapObject.Location.X == 1 && mapObject.Location.Y == 0))
                    mapObject.Wall = new Wall();
            }
            var helper = new LocationHelper(map);
            Assert.AreEqual(Direction.Up, helper.GetFirstAvailableDirection());
        }

        [Test]
        public void TestAvailableDirections2()
        {
            var map = GetExampleMap();
            foreach (var mapObject in map.Map.Objects)
            {
                if (!(mapObject.Location.X == 1 && mapObject.Location.Y == 1) &&
                    !(mapObject.Location.X == 2 && mapObject.Location.Y == 1))
                    mapObject.Wall = new Wall();
            }
            var helper = new LocationHelper(map);
            Assert.AreEqual(Direction.RightUp, helper.GetFirstAvailableDirection());
        }

        [Test]
        public void TestObjectAt()
        {
            var map = GetExampleMap();
            var helper = new LocationHelper(map);
            var objects = map.Map.Objects.ToDictionary(obj => obj.Location.ToLocation());
            objects.Add(new Location(-1, -1), null);
            objects.Add(new Location(100, 100), null);
            foreach (var location in objects.Keys)
                Assert.AreEqual(objects[location], helper.GetObjectAt(location));
        }
    }
}