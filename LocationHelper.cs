using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationHelper
    {
        private readonly ILocationMapProvider mapProvider;
        private readonly WarFogMap map;

        public LocationHelper(ILocationMapProvider mapProvider)
        {
            this.mapProvider = mapProvider;
            map = new WarFogMap();
            UpdateData(mapProvider);
        }

        public MapObjectData GetObjectAt(Location location) => map[location];

        public bool CanStandThere(Location location)
        {
            if (!IsInsideMap(location, mapProvider.GetMap()))
                return false;
            var obj = map[location];
            return obj != null && obj.Wall == null;
        }

        public Direction GetFirstAvailableDirection()
        {
            var curLocation = mapProvider.GetCurrentLocation();
            return Constants.Directions.FirstOrDefault(
                direction => CanStandThere(curLocation.NeighborAt(direction)));
        }

        public static bool IsInsideMap(Location location, MapData map)
        {
            return location.X >= 0 && location.Y >= 0 && location.X < map.Width && location.Y < map.Height;
        }

        public void UpdateData(ILocationMapProvider locationMapProvider)
        {
            map.UpdateMap(locationMapProvider);
        }

        public Dictionary<Location, MapObjectData> GetMapObjects()
        {
            return map.Map;
        }
    }
}