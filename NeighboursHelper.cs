using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public static class NeighboursHelper
    {
        public static double AddNeighboursWeight(
            Dictionary<Location, double> previousLevel,
            MapData map, Location currentLoc)
        {
            return AI.CanStandThere(map, currentLoc) ?
                currentLoc.Neighborhood.Where(previousLevel.ContainsKey).Sum(neighb => previousLevel[neighb] / 2) :
                0;
        }

        public static Dictionary<Location, MapObjectData>[] GroupByRange(HommSensorData data)
        {
            var levels = new Dictionary<int, List<MapObjectData>>();
            foreach (var mapObject in data.Map.Objects)
            {
                var dx = mapObject.Location.X - data.Location.X;
                var dy = mapObject.Location.Y - data.Location.Y;
                var distance = (int)Math.Sqrt(dx * dx + dy * dy);
                //Посмотри, нельзя ли использовать вместо этого EuclideanDistance(), а то там не совсем тривиальная формула какая-то
                if (distance > Constants.Radius)
                    continue;
                if (!levels.ContainsKey(distance))
                    levels.Add(distance, new List<MapObjectData>());
                levels[distance].Add(mapObject);
            }
            return levels
                .OrderBy(record => record.Key)
                .Select(record => record.Value)
                .Select(collection =>
                    collection.ToDictionary(point => point.Location.ToLocation()))
                .ToArray();
        }
    }
}
