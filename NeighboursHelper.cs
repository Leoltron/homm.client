using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class NeighboursHelper
    {
        private readonly LocationHelper locHelper;

        public NeighboursHelper(LocationHelper locHelper)
        {
            this.locHelper = locHelper;
        }

        private double AddNeighboursWeight(
            Dictionary<Location, double> previousLevel,
            MapData map, 
            Location currentLoc)
        {
            return LocationHelper.CanStandThere(currentLoc, map) ||  locHelper.GetObjectAt(currentLoc) == null && previousLevel.Count > 0
                ? currentLoc.Neighborhood.Where(previousLevel.ContainsKey).Sum(neighb => previousLevel[neighb] / 8) 
                : 0;
        }

        public void SpreadSmellFromPrevLevel(
            Dictionary<Location, MapObjectData> level,
            Dictionary<Location, double>[] suitableLocations,
            MapData map,
            int stage)
        {
            foreach (var location in level.Keys)
            {
                if (suitableLocations[stage][location] >= 0)
                    suitableLocations[stage][location] += AddNeighboursWeight(
                                                             suitableLocations[stage + 1],
                                                             map,
                                                             location);
            }
        }

        public static Dictionary<Location, double> SpreadSmellAlongLevel(Dictionary<Location, double> level, MapData map)
        {
            var renew = new Dictionary<Location, double>(level);
            foreach (var location in level.Keys)
            {
                foreach (var neighb in GetLevelNeighbours(level, location))
                {
                    if (LocationHelper.CanStandThere(neighb, map) && level[neighb] >= 0)
                        renew[neighb] += level[location] / 8;
                }
            }
            return renew;
        }

        private static IEnumerable<Location> GetLevelNeighbours(Dictionary<Location, double> level, Location current)
        {
            var neighbs = current.Neighborhood;
            return neighbs.Where(level.ContainsKey);
        }

        public static List<Location>[] GroupByRange(HommSensorData data)
        {
            var levels =  new List<List<Location>>();
            var visited = new HashSet<Location>();
            var looked = new HashSet<Location>();
            var queue = new Queue<Tuple<Location, int>>();
            var start = data.Location.ToLocation();
            queue.Enqueue(Tuple.Create(start, 0));
            levels.Add(new List<Location>());
            levels[0].Add(start);
            looked.Add(start);
            while (queue.Any())
            {
                var current = queue.Peek().Item1;
                var deep = queue.Dequeue().Item2;
                if (levels.Count == deep + 1)
                    levels.Add(new List<Location>());
                if (visited.Contains(current)) continue;
                visited.Add(current);
                var neighbs = current.Neighborhood;
                foreach (var neighb in neighbs)
                    if (LocationHelper.IsInsideMap(neighb, data.Map) && !looked.Contains(neighb))
                    {
                        looked.Add(neighb);
                        levels[deep + 1].Add(neighb);
                        queue.Enqueue(Tuple.Create(neighb, deep + 1));
                    }
            }
            return levels.ToArray();
        }
    }
}
