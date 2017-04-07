using System;
using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationWeightCalculator
    {
        private readonly AI ai;
        private readonly CoefficientsCalculator coefsCalc;

        public LocationWeightCalculator(AI ai)
        {
            this.ai = ai;
            coefsCalc = new CoefficientsCalculator(ai);
        }

        public void CalculateLevelWeights(
            Dictionary<Location, double>[] suitableLocations,
            List<MapObjectData>[] levels,
            int stage,
            int lastStage)
        {
            var level = levels[stage];
            suitableLocations[stage] = new Dictionary<Location, double>();
            foreach (var mapObject in level)
            {
                var location = mapObject.Location.ToLocation();
                suitableLocations[stage].Add(
                                            location,
                                            GetMapObjectWeight(mapObject));
                if (stage == lastStage) continue;
                var current = mapObject;
                if (suitableLocations[stage][location] >= 0)
                    suitableLocations[stage][location] += NeighboursHelper.AddNeighboursWeight(
                                                            suitableLocations[stage + 1],
                                                            ai.CurrentData.Map, 
                                                            current.Location.ToLocation());
            }
        }
        
        private double GetMapObjectWeight(MapObjectData mapObject)
        {
            var weight = 0d;
            weight += coefsCalc.GetPileValue(mapObject.ResourcePile);
            weight += coefsCalc.GetMineValue(mapObject.Mine);
            weight += CoefficientsCalculator.GetDwellingValue(mapObject.Dwelling, ai.CurrentData.MyRespawnSide);
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