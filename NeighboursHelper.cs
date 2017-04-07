using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class NeighboursHelper
    {
        private LocationHelper locHelper;

        public NeighboursHelper(LocationHelper locHelper)
        {
            this.locHelper = locHelper;
        }

        public static double AddNeighboursWeight(
            Dictionary<Location, double> previousLevel,
            MapData map, 
            Location currentLoc)
        {
            return LocationHelper.CanStandThere(map, currentLoc) 
                ? currentLoc.Neighborhood.Where(previousLevel.ContainsKey).Sum(neighb => previousLevel[neighb] / 1.5) 
                : 0;
        }

        public List<MapObjectData>[] GroupByRange(HommSensorData data)
        {
            var levels =  new List<List<MapObjectData>>();
            var visited = new HashSet<Location>();
            var looked = new HashSet<Location>();
            var queue = new Queue<Tuple<Location, int>>();
            var start = data.Location.ToLocation();
            queue.Enqueue(Tuple.Create(start, 0));
            levels.Add(new List<MapObjectData>());
            levels[0].Add(locHelper.GetObjectAt(start));
            looked.Add(start);
            while (queue.Any())
            {
                var current = queue.Peek().Item1;
                var deep = queue.Dequeue().Item2;
                if (levels.Count == deep + 1)
                    levels.Add(new List<MapObjectData>());
                if (visited.Contains(current)) continue;
                visited.Add(current);
                var neighbs = current.Neighborhood;
                foreach (var neighb in neighbs)
                    if (locHelper.IsInsideMap(neighb, data.Map) && !looked.Contains(neighb))
                    {
                        looked.Add(neighb);
                        levels[deep + 1].Add(locHelper.GetObjectAt(neighb));
                        queue.Enqueue(Tuple.Create(neighb, deep + 1));
                    }
            }
            return levels.ToArray();
        }
    }
}
