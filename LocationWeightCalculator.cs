using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;

namespace Homm.Client
{
    public class LocationWeightCalculator
    {
        private readonly SimpleWeights simple;
        private readonly LocationHelper locHelper;

        public LocationWeightCalculator(AI ai, LocationHelper locHelper)
        {
            this.locHelper = locHelper;
            simple = new SimpleWeights(ai, locHelper);
        }

        public Dictionary<Location, double> GetSpreadWeights(MapData map)
        {
            var simpleWeights = simple.GetMapSimpleWeights();
            AddEmptyWeights(map, simpleWeights);
            var spreadWeights = simpleWeights.ToDictionary(pair => pair.Key, pair => 0d);
            return simpleWeights.Keys
                .Aggregate(spreadWeights, (current, key) => 
                    SpreadWeights(map, key, simpleWeights[key], current, simpleWeights));
        }

        private Dictionary<Location, double> SpreadWeights(
            MapData map,
            Location location,
            double weight,
            Dictionary<Location, double> spreadWeights,
            Dictionary<Location, double> simpleWeights)
        {
            
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
                if (simpleWeights[loc] >= 0)
                    spreadWeights[loc] += smell;
                var neighbs = GetNeighbsNextLevel(loc, looked, map, simpleWeights);
                foreach (var neighb in neighbs)
                {
                    looked.Add(neighb);
                    queue.Enqueue(Tuple.Create(neighb, deep + 1));
                }
            }
            return spreadWeights;
        }

        private IEnumerable<Location> GetNeighbsNextLevel(Location location, HashSet<Location> looked, MapData map, Dictionary<Location, double> simpleWeights)
        {
            return location.Neighborhood
                .Where(neighb => !looked.Contains(neighb) &&
                                 locHelper.CanStandThere(neighb) &&
                                 simpleWeights[neighb] >= 0);
        }

        private void AddEmptyWeights(MapData map, Dictionary<Location, double> simpleWeights)
        {
            for (var i = 0; i < map.Height; i++)
                for (var j = 0; j < map.Width; j++)
                    if (!simpleWeights.ContainsKey(new Location(i, j)))
                        simpleWeights.Add(new Location(i, j), 0d);
        }
    }
}