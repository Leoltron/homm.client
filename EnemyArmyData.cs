using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public struct EnemyArmyData
    {
        private readonly Dictionary<UnitType, int> unitAmount;
        internal readonly int amountOverall;

        public double GetPart(UnitType type)
        {
            if (!unitAmount.ContainsKey(type) || amountOverall == 0)
                return 0;
            return (double)unitAmount[type] / amountOverall;
        }

        internal EnemyArmyData(Dictionary<UnitType, int> unitAmount)
        {
            this.unitAmount = unitAmount;
            if (unitAmount.Values.Any(amount => amount < 0))
                throw new ArgumentException("Unit amount cannot be less than zero!");
            amountOverall = this.unitAmount.Sum(u => u.Value);
        }

        public static EnemyArmyData Parse(HommSensorData data)
        {
            var enemyUnitsData = new Dictionary<UnitType, int>();
            foreach (var mapObject in data.Map.Objects)
            {
                //Не учитываю гарнизоны врага и врага, ибо хз как отличать от своих
                if (mapObject.NeutralArmy == null) continue;
                foreach (var army in mapObject.NeutralArmy.Army)
                    enemyUnitsData.AddOrSum(army);
            }
            return new EnemyArmyData(enemyUnitsData);
        }

    }
}