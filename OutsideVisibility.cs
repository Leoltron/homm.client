using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    static class OutsideVisibility
    {
        private static readonly Dictionary<Location, MapObjectData> visited = new Dictionary<Location, MapObjectData>();

        private static void AddOrRefresh(Location location, LocationHelper locHelper)
        {
            var mapObject = locHelper.GetObjectAt(location);
            if (!visited.ContainsKey(location))
                visited.Add(location, mapObject);
            else
                visited[location] = mapObject ?? visited[location];
        }

        public static Dictionary<Location, MapObjectData> RefreshVisited(MapData map, LocationHelper locHelper)
        {
            for (var i = 0; i < map.Width; i++)
                for (var j = 0; j < map.Height; j++)
                    AddOrRefresh(new Location(i, j), locHelper);
            return visited;
        }
    }
}
