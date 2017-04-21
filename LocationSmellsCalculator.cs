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
            var smell = 0d;
            smell += smellsCalc.GetPileSmell(mapObject.ResourcePile);
            smell += smellsCalc.GetMineSmell(mapObject.Mine);
            smell += smellsCalc.GetDwellingSmell(mapObject.Dwelling);
            smell += SmellsCalculator.GetTerrainSmell();
            var enemyArmy = FindEnemyArmy(mapObject);
            if (enemyArmy != null)
            {
                var battleProfit = ai.BattleCalc.GetProfitFromAttack(enemyArmy);
                smell = battleProfit <= 0 ? -2 : smell + Constants.BattleCoefficient * battleProfit;
            }
            return smell;
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