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

        public static Dictionary<Location, MapObjectData>[] Refresh(List<Location>[] levels, LocationHelper locHelper)
        {
            RefreshVisited(levels, locHelper);
            return levels
                .Select(level => level
                    .ToDictionary(location => location, ValueOrDefault))
                .ToArray();
        }

        private static MapObjectData ValueOrDefault(Location location) => visited.ContainsKey(location) ? visited[location] : null;

        private static void AddOrRefresh(Location location, LocationHelper locHelper)
        {
            var mapObject = locHelper.GetObjectAt(location);
            if (mapObject == null)
                return;
            if (!visited.ContainsKey(location))
                visited.Add(location, mapObject);
            else
                visited[location] = mapObject;
        }

        private static void RefreshVisited(List<Location>[] levels, LocationHelper locHelper)
        {
            foreach (var level in levels)
            foreach (var mapObject in level)
                AddOrRefresh(mapObject, locHelper);
        }
    }
}
