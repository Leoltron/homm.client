using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;

namespace Homm.Client
{
    public struct EnemyArmyData
    {
        private readonly Dictionary<UnitType, int> unitAmount;
        internal readonly int AmountOverall;

        public double GetPart(UnitType type)
        {
            if (!unitAmount.ContainsKey(type) || AmountOverall == 0)
                return 0;
            return (double) unitAmount[type] / AmountOverall;
        }

        internal EnemyArmyData(Dictionary<UnitType, int> unitAmount)
        {
            this.unitAmount = unitAmount;
            if (unitAmount.Values.Any(amount => amount < 0))
                throw new ArgumentException("Unit amount cannot be less than zero!");
            AmountOverall = this.unitAmount.Sum(u => u.Value);
        }

        public static EnemyArmyData Parse(IPlayerInfoProvider playerInfo, ILocationMapProvider data)
        {
            var enemyUnitsData = new Dictionary<UnitType, int>();
            foreach (var mapObject in data.GetMap().Objects)
            {
                if (mapObject.NeutralArmy != null)
                    foreach (var army in mapObject.NeutralArmy.Army)
                        enemyUnitsData.AddOrSum(army);
                else if (mapObject.Garrison != null && mapObject.Garrison.Owner != playerInfo.GetMyRespawnSide())
                    foreach (var army in mapObject.Garrison.Army)
                        enemyUnitsData.AddOrSum(army);
                else if (mapObject.Hero != null && mapObject.Hero.Name != playerInfo.GetMyRespawnSide())
                    foreach (var army in mapObject.Hero.Army)
                        enemyUnitsData.AddOrSum(army);
            }
            return new EnemyArmyData(enemyUnitsData);
        }
    }
}