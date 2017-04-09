using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationWeightCalculator
    {
        private readonly AI ai;
        private readonly CoefficientsCalculator coefsCalc;
        private readonly NeighboursHelper neighbsHelper;

        public LocationWeightCalculator(AI ai, LocationHelper locHelper)
        {
            this.ai = ai;
            coefsCalc = new CoefficientsCalculator(ai);
            neighbsHelper = new NeighboursHelper(locHelper);
        }

        public void CalculateLevelWeights(
            Dictionary<Location, double>[] suitableLocations,
            Dictionary<Location, MapObjectData>[] levels,
            int stage,
            int lastStage)
        {
            var level = levels[stage];
            suitableLocations[stage] = GetBaseLevelCoefficients(level, stage);
            if (stage != lastStage)
                neighbsHelper.SpreadSmellFromPrevLevel(level, suitableLocations, ai.CurrentData.Map, stage);
            suitableLocations[stage] = NeighboursHelper.SpreadSmellAlongLevel(suitableLocations[stage], ai.CurrentData.Map);

        }

        private Dictionary<Location, double> GetBaseLevelCoefficients(Dictionary<Location, MapObjectData> level, int stage)
        {
            var suitableLocations = new Dictionary<Location, double>();
            foreach (var location in level.Keys)
            {
                var weight = level[location] != null ? GetMapObjectWeight(level[location]) : 0;
                suitableLocations.Add(location, weight);
            }
            return suitableLocations;
        }
        
        private double GetMapObjectWeight(MapObjectData mapObject)
        {
            var weight = 0d;
            weight += coefsCalc.GetPileValue(mapObject.ResourcePile);
            weight += coefsCalc.GetMineValue(mapObject.Mine, ai.CurrentData.MyRespawnSide);
            weight += coefsCalc.GetDwellingValue(mapObject.Dwelling, ai.CurrentData.MyRespawnSide);
            weight += CoefficientsCalculator.GetTerrainValue(mapObject.Terrain);
            var enemyArmy = FindEnemyArmy(mapObject);
            if (enemyArmy != null)
            {
                var battleProfit = ai.BattleCalc.GetProfitFromAttack(enemyArmy);
                weight = battleProfit <= 0 ? -2 : weight + Constants.BattleCoefficient * battleProfit;
            }
            //... и тут видимо для каждого поля нужно так сделать(
            return weight;
        }

        private Dictionary<UnitType, int> FindEnemyArmy(MapObjectData cell)
        {
            if (cell.NeutralArmy != null)
                return cell.NeutralArmy.Army;
            if (cell.Hero != null && cell.Hero.Name != ai.CurrentData.MyRespawnSide)
                return cell.Hero.Army;
            if (cell.Garrison != null && cell.Garrison.Owner != ai.CurrentData.MyRespawnSide) //TODO: Так какое же?
                return cell.Garrison.Army;                                                    //Оп вот здесь кстати объяснение какого черта он на гарнизоны нападает) Как я понял это сторона с которой начинаем
            return null;
        }
    }
}