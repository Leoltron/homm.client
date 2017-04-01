using System.Collections.Generic;

namespace Homm.Client
{
    static class DictExtensions
    {
        public static void AddOrSum<T>(this IDictionary<T, int> dict, T key, int value)
        {
            if (dict.ContainsKey(key))
                dict[key] += value;
            else
                dict[key] += value;
        }

        public static void AddOrSum<T>(this IDictionary<T, int> dict, KeyValuePair<T,int> pair)
        {
            AddOrSum(dict,pair.Key,pair.Value);
        }
    }
}