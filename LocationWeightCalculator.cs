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

        public Dictionary<Location, double> GetSpreadedWeights()
        {
            var simpleWeights = simple.GetMapSimpleWeights();
            var spreadedWeights = simpleWeights.ToDictionary(pair => pair.Key, pair => 0d);
            return simpleWeights.Keys
                .Aggregate(spreadedWeights, (currentWeight, location) => 
                    SpreadWeights(location, simpleWeights[location], currentWeight));
        }

        private Dictionary<Location, double> SpreadWeights(
            Location location,
            double weight,
            Dictionary<Location, double> spreadedWeights)
        {
            if (weight < 0)
            {
                spreadedWeights[location] = weight;
                return spreadedWeights;
            }
            var queue = new Queue<Tuple<Location, int>>();
            var looked = new HashSet<Location> {location};
            queue.Enqueue(Tuple.Create(location, 0));
            while (queue.Count != 0)
            {
                var loc = queue.Peek().Item1;
                var deep = queue.Dequeue().Item2;
                var smell = weight / Math.Pow(Constants.DecreaseByLevel, deep);
                
                if (spreadedWeights[loc] >= 0)
                    spreadedWeights[loc] += smell;
                var neighbs = GetNeighbsNextLevel(loc, looked);
                foreach (var neighb in neighbs)
                {
                    looked.Add(neighb);
                    queue.Enqueue(Tuple.Create(neighb, deep + 1));
                }
            }
            return spreadedWeights;
        }

        private IEnumerable<Location> GetNeighbsNextLevel(Location location, ICollection<Location> looked)
        {
            return location.Neighborhood
                .Where(neighb => !looked.Contains(neighb) && locHelper.CanStandThere(neighb));
        }
    }
}