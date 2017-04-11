using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class WarFogMap
    {
        public readonly Dictionary<Location, MapObjectData> Map;
        public MapObjectData this[Location loc] => Map.ContainsKey(loc) ? Map[loc] : null;

        public WarFogMap()
        {
            Map = new Dictionary<Location, MapObjectData>();
        }

        public void UpdateMap(ILocationMapProvider map)
        {
            foreach (var mapObjectData in map.Map.Objects)
                Map[mapObjectData.Location.ToLocation()] = mapObjectData;
        }
    }
}