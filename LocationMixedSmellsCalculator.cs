using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationMixedSmellsCalculator
    {
        private readonly LocationSmellsCalculator locSmellsCalc;
        private readonly LocationHelper locHelper;

        public LocationMixedSmellsCalculator(AI ai, LocationHelper locHelper)
        {
            this.locHelper = locHelper;
            locSmellsCalc = new LocationSmellsCalculator(ai, locHelper);
        }

        public Dictionary<Location, double> GetMixedSmells(MapData map)
        {
            var smells = locSmellsCalc.GetMapSmells();
            AddEmptyWeights(map, smells);
            var mixedSmells = smells.ToDictionary(pair => pair.Key, pair => 0d);
            return smells.Keys
                .Aggregate(mixedSmells, (current, key) => 
                    MixSmells(key, smells[key], current, smells));
        }

        private Dictionary<Location, double> MixSmells(
            Location location,
            double smellToAdd,
            Dictionary<Location, double> mixedSmells,
            Dictionary<Location, double> initialSmells)
        {
            
            if (smellToAdd < 0)
            {
                mixedSmells[location] = smellToAdd;
                return mixedSmells;
            }
            Algorithms<double>.BFS(initialSmells, mixedSmells, Mix, locHelper.GetNeighbsNextLevel, location, smellToAdd);
            return mixedSmells;
        }

        private static void Mix(
            Dictionary<Location, double> smells,
            Dictionary<Location, double> mixedSmells,
            Location location,
            double smell,
            int deep)
        {
            var spreadSmell = smell / Math.Pow(Constants.DecreaseByLevel, deep);
            if (smells[location] >= 0)
                mixedSmells[location] += spreadSmell;
        }

        private static void AddEmptyWeights(MapData map, Dictionary<Location, double> simpleWeights)
        {
            for (var i = 0; i < map.Height; i++)
                for (var j = 0; j < map.Width; j++)
                    if (!simpleWeights.ContainsKey(new Location(i, j)))
                        simpleWeights.Add(new Location(i, j), 0d);
        }
    }
}