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
        private readonly LocationHelper locHelper;
        private readonly BridgeLocator bridgeLocator;

        public SimpleWeights(AI ai, LocationHelper locHelper)
        {
            coefsCalc = new CoefficientsCalculator(ai);
            this.ai = ai;
            this.locHelper = locHelper;
            bridgeLocator = new BridgeLocator(locHelper);
        }

        public Dictionary<Location, double> GetMapSimpleWeights()
        {
            return locHelper.GetMapObjects()
                .ToDictionary(pair => pair.Key, pair => GetMapObjectOwnWeight(pair.Value));
        }

        private double GetMapObjectOwnWeight(MapObjectData mapObject)
        {
            if (mapObject == null)
                return 0;
            var weight = 0d;
            weight += coefsCalc.GetPileCoefficient(mapObject.ResourcePile);
            weight += coefsCalc.GetMineCoefficient(mapObject.Mine, ai.CurrentData.MyRespawnSide);
            weight += coefsCalc.GetDwellingCoefficient(mapObject.Dwelling);
            weight += CoefficientsCalculator.GetTerrainCoefficient(mapObject.Terrain);
            var enemyArmy = FindEnemyArmy(mapObject);
            if (enemyArmy != null)
            {
                var battleProfit = ai.BattleCalc.GetProfitFromAttack(enemyArmy);
                weight = battleProfit <= 0 ? -2 : weight + Constants.BattleCoefficient * battleProfit;
            }
            //... и тут видимо для каждого поля нужно так сделать(
            if (bridgeLocator.IsBridge(mapObject.Location.ToLocation()))
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