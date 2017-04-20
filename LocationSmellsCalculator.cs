using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class LocationSmellsCalculator
    {
        private readonly SmellsCalculator smellsCalc;
        private readonly LocationHelper locHelper;
        private readonly AI ai;

        public LocationSmellsCalculator(AI ai, LocationHelper locHelper)
        {
            this.ai = ai;
            this.locHelper = locHelper;
            smellsCalc = new SmellsCalculator(ai);
        }

        public Dictionary<Location, double> GetMapSmells()
        {
            return locHelper.GetMapObjects()
                .ToDictionary(pair => pair.Key, pair => GetMapObjectOwnSmell(pair.Value));
        }

        private double GetMapObjectOwnSmell(MapObjectData mapObject)
        {
            if (mapObject == null)
                return 0;
            var weight = 0d;
            weight += smellsCalc.GetPileSmell(mapObject.ResourcePile);
            weight += smellsCalc.GetMineSmell(mapObject.Mine, ai.CurrentData.MyRespawnSide);
            weight += smellsCalc.GetDwellingSmell(mapObject.Dwelling);
            weight += SmellsCalculator.GetTerrainSmell(mapObject.Terrain);
            var enemyArmy = FindEnemyArmy(mapObject);
            if (enemyArmy != null)
            {
                var battleProfit = ai.BattleCalc.GetProfitFromAttack(enemyArmy);
                weight = battleProfit <= 0 ? -2 : weight + Constants.BattleCoefficient * battleProfit;
            }
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