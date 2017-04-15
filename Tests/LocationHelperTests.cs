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
        public void TestIsMapObjectInsideMap()
        {
            var map = GetExampleMap();
            foreach (var mapObject in map.Map.Objects)
                Assert.IsTrue(LocationHelper.IsInsideMap(mapObject.Location.ToLocation(), map.Map));
        }

        [TestCase(-1, 0, ExpectedResult = false)]
        [TestCase(0, -1, ExpectedResult = false)]
        [TestCase(-1, -1, ExpectedResult = false)]
        [TestCase(3, 0, ExpectedResult = false)]
        [TestCase(0, 3, ExpectedResult = false)]
        [TestCase(3, 3, ExpectedResult = false)]
        [TestCase(100, 0, ExpectedResult = false)]
        [TestCase(0, 100, ExpectedResult = false)]
        [TestCase(100, 100, ExpectedResult = false)]
        public bool IsInsideMap(int x, int y)
        {
            return LocationHelper.IsInsideMap(new Location(x, y), GetExampleMap().Map);
        }

        [Test]
        public void TestCanStandInMapObjects()
        {
            var map = GetExampleMap();
            var locHelper = new LocationHelper(map);
            foreach (var mapObject in map.Map.Objects)
                Assert.IsTrue(locHelper.CanStandThere(mapObject.Location.ToLocation()),
                    $"Can't stand in ({mapObject.Location.X},{mapObject.Location.Y})");
        }

        [Test]
        public void CanStandOutside()
        {
            var locHelper = new LocationHelper(GetExampleMap());
            Assert.IsFalse(locHelper.CanStandThere(new Location(-1, -1)));
            Assert.IsFalse(locHelper.CanStandThere(new Location(20, 20)));
        }

        [TestCase(Direction.Up)]
        [TestCase(Direction.RightUp)]
        [TestCase(Direction.LeftUp)]
        [TestCase(Direction.Down)]
        [TestCase(Direction.RightDown)]
        [TestCase(Direction.LeftDown)]
        public void TestGoThroughWallCircleWithHole(Direction holeDirection)
        {
            var map = GetExampleMap();
            foreach (var direction in Constants.Directions)
            {
                if (direction == holeDirection) continue;
                var neighbourLoc = map.CurrentLocation.NeighborAt(direction);
                foreach (var mapObject in map.Map.Objects)
                {
                    if (mapObject.Location.ToLocation() != neighbourLoc) continue;
                    mapObject.Wall = new Wall();
                    break;
                }
            }
            var locHelper = new LocationHelper(map);
            Assert.AreEqual(holeDirection,locHelper.GetFirstAvailableDirection());
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