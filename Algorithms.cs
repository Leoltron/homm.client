using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HoMM;

namespace Homm.Client
{
    public static class Algorithms<TValue>
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static IEnumerable<Tuple<Location,int>> BFS(
            Location startLocation,
            Dictionary<Location, TValue> locations,
            Func<Location, HashSet<Location>, Dictionary<Location, TValue>, IEnumerable<Location>> getNeighbs)
        {
            var queue = new Queue<Tuple<Location, int>>();
            var looked = new HashSet<Location> { startLocation };
            queue.Enqueue(Tuple.Create(startLocation, 0));
            while (queue.Count != 0)
            {
                var locDepth = queue.Dequeue();
                yield return locDepth;
                foreach (var neighb in getNeighbs(locDepth.Item1, looked, locations))
                {
                    looked.Add(neighb);
                    queue.Enqueue(Tuple.Create(neighb, locDepth.Item2 + 1));
                }
            }
        }
    }
}
