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

        public static List<MapObjectData>[] Refresh(List<MapObjectData>[] levels)
        {
            RefreshVisited(levels);
            return levels
                .Select(level => level
                    .Select(ValueOrDefault)
                    .ToList())
                .ToArray();
        }

        private static MapObjectData ValueOrDefault(MapObjectData mapObject)
        {
            var location = mapObject.Location.ToLocation();
            return visited[location];
        }

        private static void AddOrRefresh(MapObjectData mapObject)
        {
            var location = mapObject.Location.ToLocation();
            if (!visited.ContainsKey(location))
                visited.Add(location, mapObject);
            else
                visited[location] = mapObject;
        }

        private static void RefreshVisited(List<MapObjectData>[] levels)
        {
            foreach (var level in levels)
            foreach (var mapObject in level)
                AddOrRefresh(mapObject);
        }
    }
}
