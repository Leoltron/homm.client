using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    internal struct ResourcesData
    {
        private readonly Dictionary<Resource, int> resources;
        public readonly int Total;

        public int this[Resource key] => resources[key];

        public ResourcesData(Dictionary<Resource, int> resources) : this()
        {
            this.resources = resources;
            Total = resources.Sum(pair => pair.Value);
        }

        public double GetRarity(Resource type)
        {
            if (!resources.ContainsKey(type) || this[type] == 0)
                return double.MaxValue;
            return (double) Total / this[type];
        }

        private const double MineCoefficent = 1; //TODO: Настроить коэффицент

        public static ResourcesData Parse(HommSensorData data)
        {
            var resources = new Dictionary<Resource, int>();
            foreach (var mapObject in data.Map.Objects)
            {
                if (mapObject.ResourcePile != null)
                    resources.AddOrSum(mapObject.ResourcePile.Resource, mapObject.ResourcePile.Amount);
                if (mapObject.Mine != null)
                    resources.AddOrSum(mapObject.Mine.Resource,
                        (int) (MineCoefficent * HommRules.Current.MineDailyResourceYield));
            }
            foreach (var resource in data.MyTreasury)
                resources.AddOrSum(resource);
            return new ResourcesData(resources);
        }
    }
}