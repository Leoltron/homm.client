using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using Dwelling = HoMM.ClientClasses.Dwelling;

namespace Homm.Client
{
    public static class HireHelper
    {
        public static int HowManyCanHire(Dwelling dwelling, Dictionary<Resource, int> resourcesAvailable)
        {
            if (dwelling == null)
                return 0;
            var hireCost = UnitsConstants.Current.UnitCost[dwelling.UnitType];
            var canHireWithResource = (from resource in hireCost.Keys
                where
                hireCost[resource] > 0 &&
                resourcesAvailable.ContainsKey(resource) &&
                resourcesAvailable[resource] > 0
                select resourcesAvailable[resource] / hireCost[resource]).ToList();
            canHireWithResource.Add(dwelling.AvailableToBuyCount);
            return canHireWithResource.Min();
        }
    }
}