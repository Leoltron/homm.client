using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationSmellsMixer
    {
        private readonly LocationSmellsCalculator locSmellsCalc;
        private readonly LocationHelper locHelper;

        public LocationSmellsMixer(AI ai, LocationHelper locHelper)
        {
            this.locHelper = locHelper;
            locSmellsCalc = new LocationSmellsCalculator(ai, locHelper);
        }

        public Dictionary<Location, double> GetMixedSmells(MapData map)
        {
            var smells = locSmellsCalc.GetMapSmells();
            var mixedSmells = smells.ToDictionary(pair => pair.Key, pair => 0d);
            return smells.Keys
                .Aggregate(mixedSmells, (current, key) =>
                    MixSmells(key, smells[key], current, smells));
        }

        private Dictionary<Location, double> MixSmells(
            Location initialLocation,
            double smellToAdd,
            Dictionary<Location, double> mixedSmells,
            Dictionary<Location, double> initialSmells)
        {
            if (smellToAdd < 0)
            {
                mixedSmells[initialLocation] = smellToAdd;
                return mixedSmells;
            }
            foreach (var locDepth in Algorithms<double>.BFS(initialLocation,
                initialSmells, LocationHelper.GetUnlookedNotStinkingNeighbs)
                .Where(ld => locHelper.CanStandThere(ld.Item1)))
            {
                var location = locDepth.Item1;
                var depth = locDepth.Item2;
                var spreadSmell = smellToAdd / Math.Pow(Constants.DecreaseByLevel, depth);
                if (initialSmells[location] >= 0)
                    mixedSmells[location] += spreadSmell;
            }
            return mixedSmells;
        }
    }
}