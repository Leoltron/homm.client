using System;
using System.Collections.Generic;
using HoMM;

namespace Homm.Client
{
    public static class Algorithms<TValue>
    {
        public static void BFS(
            Dictionary<Location, TValue> baseOfCompute,
            Dictionary<Location, TValue> resultOfCompute,
            Action<Dictionary<Location, TValue>, Dictionary<Location, TValue>, Location, TValue, int> compute,
            Func<Location, HashSet<Location>, Dictionary<Location, TValue>, IEnumerable<Location>> getNeighbs,
            Location startLocation, 
            TValue startValue)
        {
            var queue = new Queue<Tuple<Location, int>>();
            var looked = new HashSet<Location> { startLocation };
            queue.Enqueue(Tuple.Create(startLocation, 0));
            while (queue.Count != 0)
            {
                var loc = queue.Peek().Item1;
                var deep = queue.Dequeue().Item2;
                compute(baseOfCompute, resultOfCompute, loc, startValue, deep);
                var neighbs = getNeighbs(loc, looked, baseOfCompute);
                foreach (var neighb in neighbs)
                {
                    looked.Add(neighb);
                    queue.Enqueue(Tuple.Create(neighb, deep + 1));
                }
            }
        }
    }
}
