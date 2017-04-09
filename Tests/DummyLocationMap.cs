using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client.Tests
{
    internal class DummyLocationMap : ILocationMapProvider
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

        public static DummyLocationMap GetExampleMap()
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
                mapObjects.Add(new MapObjectData { Location = location.ToLocationInfo() });
            }
            var mapData = new MapData
            {
                Objects = mapObjects,
                Width = mapObjects.Max(m => m.Location.X) + 1,
                Height = mapObjects.Max(m => m.Location.Y) + 1
            };

            var map = new DummyLocationMap { Map = mapData, CurrentLocation = new Location(1, 1) };
            return map;
        }
    }
}