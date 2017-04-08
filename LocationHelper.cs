using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationHelper
    {
        private readonly AI ai;

        public LocationHelper(AI ai)
        {
            this.ai = ai;
        }

        public MapObjectData GetObjectAt(Location location)
        {
            if (location.X < 0 || location.X >= ai.CurrentData.Map.Width || location.Y < 0 ||
                location.Y >= ai.CurrentData.Map.Height)
                return null;
            var mapObject = ai.CurrentData.Map.Objects.FirstOrDefault(
                x => x.Location.X == location.X && x.Location.Y == location.Y);
            var emptyObject = new MapObjectData {Location = new LocationInfo(location.X, location.Y)};
            return mapObject ?? emptyObject;
        }

        public static bool CanStandThere(MapData map, Location location)
        {
            if (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height)
                return false;
            var obj = map.Objects.FirstOrDefault(x => x.Location.X == location.X && x.Location.Y == location.Y);
            return obj != null && obj.Wall == null;
        }

        public Direction GetFirstAvailableDirection()
        {
            var curLocation = ai.CurrentData.Location.ToLocation();
            return Constants.Directions.FirstOrDefault(
                direction => CanStandThere(ai.CurrentData.Map, curLocation.NeighborAt(direction)));
        }

        public bool IsInsideMap(Location location, MapData map)
        {
            return location.X >= 0 && location.Y >= 0 && location.X < map.Width && location.Y < map.Height;
        }
    }
}