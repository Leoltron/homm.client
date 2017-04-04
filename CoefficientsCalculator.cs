using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;
using Dwelling = HoMM.ClientClasses.Dwelling;
using Mine = HoMM.ClientClasses.Mine;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

namespace Homm.Client
{
    public static class CoefficientsCalculator
    {
        public static double GetTerrainValue(Terrain terrain) => 0;//-Constants.CostOfMove[terrain] / 7;

        public static double GetDwellingValue(Dwelling dwelling, string me) => dwelling != null && dwelling.Owner != me ? 1 : 0;

        public static double GetPileValue(ResourcePile pile, AI ai)
        {
            return pile == null
                ? 0
                : HommRules.Current.ResourcesGainScores + (pile.Amount * CoefficientsCalculator.GetDegreeOfNeed(pile.Resource, ai)) / 4e3;
        }

        public static double GetMineValue(Mine mine, AI ai)
        {
            var result = mine == null
                ? 0
                : (HommRules.Current.MineOwningDailyScores +
                   GetDegreeOfNeed(mine.Resource, ai) * HommRules.Current.MineDailyResourceYield) * Constants.MineCoefficent;
            if (mine == null)
                return 0;
            return mine.Owner != ai.CurrentData.MyRespawnSide ? result : 0;
        }

        public static double GetDegreeOfNeed(UnitType unitType, AI ai)
        {
            return GetDegreeOfNeed(Constants.UnitToResource[unitType], ai);
        }

        private static double GetDegreeOfNeed(Resource resourceType, AI ai)
        {
            //TODO: Вынести в переменную, чтобы считалась лишь раз между двумя обновлениями данных?
            return GetCounterMeetingPropability(Constants.ResourceToUnit[resourceType], ai) * Constants.ArmyEfficencyCoefficent
                   + ai.ResourcesData.GetRarity(resourceType) * Constants.ResourceRarityCoefficent;
        }

        private static double GetCounterMeetingPropability(UnitType type, AI ai)
        {
            return type == UnitType.Militia ? Constants.GoldMilitiaCounterConst : ai.EnemyArmyData.GetPart(Constants.UnitCounters[type]);
        }
    }
}
