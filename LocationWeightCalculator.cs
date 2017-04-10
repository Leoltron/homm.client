using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationWeightCalculator
    {
        private readonly SimpleWeights simple;
        private readonly LocationHelper locHelper;

        public LocationWeightCalculator(AI ai, LocationHelper locHelper)
        {
            simple = new SimpleWeights(ai, locHelper);
            this.locHelper = locHelper;
        }

        public Dictionary<Location, double> GetSpreadWeights(MapData map)
        {
            var visited = OutsideVisibility.RefreshVisited(map, locHelper);
            var simpleWeights = simple.GetMapSimpleWeights(visited);
            var spreadWeights = simpleWeights.ToDictionary(pair => pair.Key, pair => 0d);
            return simpleWeights.Keys
                .Aggregate(spreadWeights, (current, key) => 
                    SpreadWeights(map, key, simpleWeights[key], current));
        }

        private Dictionary<Location, double> SpreadWeights(
            MapData map,
            Location location,
            double weight,
            Dictionary<Location, double> spreadWeights)
        {
            if (location.X == 4 && location.Y == 13)
                ;
            if (weight < 0)
            {
                spreadWeights[location] = weight;
                return spreadWeights;
            }
            var queue = new Queue<Tuple<Location, int>>();
            var looked = new HashSet<Location> {location};
            queue.Enqueue(Tuple.Create(location, 0));
            while (queue.Count != 0)
            {
                var loc = queue.Peek().Item1;
                var deep = queue.Dequeue().Item2;
                var smell = weight / Math.Pow(Constants.DecreaseByLevel, deep);
                
                if (spreadWeights[loc] >= 0)
                    spreadWeights[loc] += smell;
                var neighbs = GetNeighbsNextLevel(loc, looked, map);
                foreach (var neighb in neighbs)
                {
                    looked.Add(neighb);
                    queue.Enqueue(Tuple.Create(neighb, deep + 1));
                }
            }
            return spreadWeights;
        }

        private IEnumerable<Location> GetNeighbsNextLevel(Location location, HashSet<Location> looked, MapData map)
        {
            return location.Neighborhood
                .Where(neighb => !looked.Contains(neighb) &&
                                 LocationHelper.CanStandThere(neighb, map));
        }
    }
}