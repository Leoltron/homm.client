using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using NUnit.Framework;
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
            var helper = new LocationHelper(GetMap());
        }

        private static DummyMap GetMap()
        {
            var mapObjects = new List<MapObjectData>
            {
                new MapObjectData
                {
                    Location = new Location(0, 0)
                        .NeighborAt(Direction.RightDown)
                        .NeighborAt(Direction.Down)
                        .ToLocationInfo()
                }
            };
            foreach (var location in mapObjects[0].Location.ToLocation().Neighborhood)
            {
                mapObjects.Add(new MapObjectData {Location = location.ToLocationInfo()});
            }
            var mapData = new MapData
            {
                Objects = mapObjects,
                Width = mapObjects.Max(m => m.Location.X) + 1,
                Height = mapObjects.Max(m => m.Location.Y) + 1
            };

            var map = new DummyMap {Map = mapData, CurrentLocation = new Location(1, 1)};
            return map;
        }

        [Test]
        public void TestInsideMap()
        {
            var map = GetMap();
            foreach (var mapObject in map.Map.Objects)
                Assert.IsTrue(LocationHelper.IsInsideMap(mapObject.Location.ToLocation(), map.Map));
        }

        [Test]
        public void TestOutsideMap()
        {
            var map = GetMap();
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
            var map = GetMap();
            foreach (var mapObject in map.Map.Objects)
                Assert.IsTrue(LocationHelper.CanStandThere(mapObject.Location.ToLocation(), map.Map),
                    $"Can't stand in ({mapObject.Location.X},{mapObject.Location.Y})");
        }

        [Test]
        public void TestAvailableDirections1()
        {
            var map = GetMap();
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
            var map = GetMap();
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
            var map = GetMap();
            var helper = new LocationHelper(map);
            var objects = map.Map.Objects.ToDictionary(obj => obj.Location.ToLocation());
            objects.Add(new Location(-1, -1), null);
            objects.Add(new Location(100, 100), null);
            foreach (var location in objects.Keys)
                Assert.AreEqual(objects[location], helper.GetObjectAt(location));
        }
    }

    internal class DummyMap : ILocationMapProvider
    {
        public MapData Map;
        public Location CurrentLocation;

        public MapData GetMap()
        {
            return Map;
        }

        public Location GetCurrentLocation()
        {
            return CurrentLocation;
        }
    }
}