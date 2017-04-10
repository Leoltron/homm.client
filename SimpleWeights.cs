using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class SimpleWeights
    {
        private readonly CoefficientsCalculator coefsCalc;
        private readonly AI ai;

        public SimpleWeights(AI ai, LocationHelper locHelper)
        {
            this.coefsCalc = new CoefficientsCalculator(ai);
            this.ai = ai;
        }

        public Dictionary<Location, double> GetMapSimpleWeights(Dictionary<Location, MapObjectData> visited)
        {
            return visited
                .Select(pair => new KeyValuePair<Location, double>(
                    pair.Key,
                    GetMapObjectWeight(pair.Value)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private double GetMapObjectWeight(MapObjectData mapObject)
        {
            if (mapObject == null)
                return 0;
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
            if (Bridges.IsBridge(mapObject.Location.ToLocation()))
                weight *= 5;
            return weight;
        }

        private Dictionary<UnitType, int> FindEnemyArmy(MapObjectData cell)
        {
            if (cell.NeutralArmy != null)
                return cell.NeutralArmy.Army;
            if (cell.Hero != null && cell.Hero.Name != ai.CurrentData.MyRespawnSide)
                return cell.Hero.Army;
            if (cell.Garrison != null && cell.Garrison.Owner != ai.CurrentData.MyRespawnSide)
                return cell.Garrison.Army;
            return null;
        }
    }
}
