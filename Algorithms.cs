using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;

namespace Homm.Client
{
    public static class Algorithms<TValue>
    {
        public static void BFS(
            Dictionary<Location, TValue> baseOfCompute,
            Dictionary<Location, TValue> computable,
            Action<Dictionary<Location, TValue>, Dictionary<Location, TValue>, Location, TValue, int> Change,
            Func<Location, HashSet<Location>, Dictionary<Location, TValue>, IEnumerable<Location>> GetNeighbs,
            Location startKey, 
            TValue startValue)
        {
            var queue = new Queue<Tuple<Location, int>>();
            var looked = new HashSet<Location> { startKey };
            queue.Enqueue(Tuple.Create(startKey, 0));
            while (queue.Count != 0)
            {
                var loc = queue.Peek().Item1;
                var deep = queue.Dequeue().Item2;
                Change(baseOfCompute, computable, loc, startValue, deep);
                var neighbs = GetNeighbs(loc, looked, baseOfCompute);
                foreach (var neighb in neighbs)
                {
                    looked.Add(neighb);
                    queue.Enqueue(Tuple.Create(neighb, deep + 1));
                }
            }
        }
    }
}
