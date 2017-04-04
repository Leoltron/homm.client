using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public static class NeighbsHelper
    {
        public static double AddNeighboursWeight(
            Dictionary<Location, double> previousLevel,
            MapObjectData current,
            AI ai)
        {
            var ourLocation = current.Location.ToLocation();
            if (!AI.CanStandThere(ai.CurrentData.Map, ourLocation))
                return 0;
            var neighbs = ourLocation.Neighborhood;
            return neighbs.Where(previousLevel.ContainsKey).Sum(neighb => previousLevel[neighb] / 2);
        }

        public static Dictionary<Location, MapObjectData>[] GroupByRange(HommSensorData data)
        {
            var levels = new Dictionary<int, List<MapObjectData>>();
            foreach (var mapObject in data.Map.Objects)
            {
                var dx = mapObject.Location.X - data.Location.X;
                var dy = mapObject.Location.Y - data.Location.Y;
                var distance = (int)Math.Sqrt(dx * dx + dy * dy);
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
                    collection.ToDictionary(point => new Location(point.Location.Y, point.Location.X)))
                .ToArray();
        }
    }
}
