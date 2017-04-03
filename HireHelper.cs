using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using Dwelling = HoMM.ClientClasses.Dwelling;

namespace Homm.Client
{
    public class HireHelper
    {
        private readonly AI ai;

        public HireHelper(AI ai)
        {
            this.ai = ai;
        }

        public int HowManyICanHire(Dwelling dwelling)
        {
            return HowManyCanHire(dwelling, ai.CurrentData.MyTreasury);
        }

        public static int HowManyCanHire(Dwelling dwelling, Dictionary<Resource, int> resources)
        {
            if (dwelling == null)
                return 0;
            var hireCost = UnitsConstants.Current.UnitCost[dwelling.UnitType];
            var canHireWithResource = (from resource in hireCost.Keys
                where
                hireCost[resource] > 0 &&
                resources.ContainsKey(resource) &&
                resources[resource] > 0
                select resources[resource] / hireCost[resource]).ToList();
            canHireWithResource.Add(dwelling.AvailableToBuyCount);
            return canHireWithResource.Min();
        }
    }
}