using System;
using System.Collections.Generic;
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
            foreach (var mapObject in locHelper.GetMapObjects().Values)
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
            Assert.AreEqual(holeDirection, locHelper.GetFirstAvailableDirection());
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

        [Test]
        public void TestGetNotStinkingNeighbs()
        {
            var rand = new Random();
            var smells = new Dictionary<Location, double>();
            var expected = new HashSet<Location>();
            var start = new Location(2, 2);
            smells.Add(start, 0);
            var flag = true;
            foreach (var location in start.Neighborhood)
            {
                if (flag)
                {
                    smells.Add(location, rand.NextDouble() * 50 + 1e-5);
                    expected.Add(location);
                }
                else
                    smells.Add(location, -(rand.NextDouble() * 50 + 1e-5));
                flag = !flag;
            }
            var actual = new HashSet<Location>(
                LocationHelper.GetUnlookedNotStinkingNeighbs(start, new HashSet<Location>(), smells)
            );
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGetUnlookedNeighbs()
        {
            var rand = new Random();
            var smells = new Dictionary<Location, double>();
            var expected = new HashSet<Location>();
            var looked = new HashSet<Location>();
            var start = new Location(2, 2);
            smells.Add(start, 0);
            var flag = true;
            foreach (var location in start.Neighborhood)
            {
                smells.Add(location, rand.NextDouble() * 50 + 1e-5);
                if (flag)
                    expected.Add(location);
                else
                    looked.Add(location);
                flag = !flag;
            }
            var actual = new HashSet<Location>(LocationHelper.GetUnlookedNotStinkingNeighbs(start, looked, smells)
            );
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGetUnlookedNotStinkingNeighbs()
        {
            var rand = new Random();
            var smells = new Dictionary<Location, double>();
            var expected = new HashSet<Location>();
            var looked = new HashSet<Location>();
            var start = new Location(2, 2);
            smells.Add(start, 0);
            var flag = 0;
            foreach (var location in start.Neighborhood)
            {
                if (flag == 0)
                    expected.Add(location);
                else if (flag % 2 == 1)
                    looked.Add(location);

                smells.Add(location, (flag / 2 == 1 ? -1 : 1) * (rand.NextDouble() * 50 + 1e-5));
                flag = (flag + 1) % 4;
            }
            var actual = new HashSet<Location>(LocationHelper.GetUnlookedNotStinkingNeighbs(start, looked, smells)
            );
            Assert.AreEqual(expected, actual);
        }
    }
}