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

        public Dictionary<Location, MapObjectData> GetMapObjects() => map.Map;

        public static IEnumerable<Location> GetUnlookedNotStinkingNeighbs(
            Location location,
            HashSet<Location> looked,
            Dictionary<Location, double> smells)
        {
            return location.Neighborhood
                .Where(neighb => !looked.Contains(neighb) &&
                                 smells.ContainsKey(neighb) &&
                                 smells[neighb] >= 0);
        }

        public bool CanStandThere(Location location)
        {
            if (!IsInsideMap(location, mapProvider.Map))
                return false;
            var obj = map[location];
            return obj?.Wall == null;
        }

        public Direction GetFirstAvailableDirection()
        {
            var currentLocation = mapProvider.CurrentLocation;
            return Constants.Directions.FirstOrDefault(
                direction => CanStandThere(currentLocation.NeighborAt(direction)));
        }

        public static bool IsInsideMap(Location location, MapData map)
        {
            return location.X >= 0 && location.Y >= 0 && location.X < map.Width && location.Y < map.Height;
        }

        public void UpdateData(ILocationMapProvider locationMapProvider)
        {
            map.UpdateMap(locationMapProvider);
        }
    }
}