using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationHelper
    {
        private readonly ILocationMapProvider mapProvider;

        public LocationHelper(ILocationMapProvider mapProvider)
        {
            this.mapProvider = mapProvider;
        }

        public MapObjectData GetObjectAt(Location location)
        {
            var map = mapProvider.GetMap();
            if (location.X < 0 || location.X >= map.Width || location.Y < 0 ||
                location.Y >= map.Height)
                return null;
            return map.Objects.FirstOrDefault(
                x => x.Location.X == location.X && x.Location.Y == location.Y);
        }

        public static bool CanStandThere(Location location, MapData map)
        {
            if (!IsInsideMap(location,map))
                return false;
            var obj = map.Objects.FirstOrDefault(x => x.Location.X == location.X && x.Location.Y == location.Y);
            return obj != null && obj.Wall == null;
        }

        public Direction GetFirstAvailableDirection()
        {
            var curLocation = mapProvider.GetCurrentLocation();
            return Constants.Directions.FirstOrDefault(
                direction => CanStandThere(curLocation.NeighborAt(direction), mapProvider.GetMap()));
        }

        public static bool IsInsideMap(Location location, MapData map)
        {
            return location.X >= 0 && location.Y >= 0 && location.X < map.Width && location.Y < map.Height;
        }
    }
}