using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using HoMM.Robot;

namespace Homm.Client
{
    public class LocationValueCalculator
    {
        private readonly AI ai;

        public LocationValueCalculator(AI ai)
        {
            this.ai = ai;
        }

        public double GetMapObjectWeight(MapObjectData mapObject)
        {
            var weight = 0d;
            weight += CoefficientsCalculator.GetPileValue(mapObject.ResourcePile, ai);
            weight += CoefficientsCalculator.GetMineValue(mapObject.Mine, ai);
            weight += CoefficientsCalculator.GetDwellingValue(mapObject.Dwelling, ai.CurrentData.MyRespawnSide);
            weight += CoefficientsCalculator.GetTerrainValue(mapObject.Terrain);
            var enemyArmy = FindEnemyArmy(mapObject);
            if (enemyArmy != null)
            {
                var battleProfit = ai.BattleCalc.GetProfitFromAttack(enemyArmy);
                weight = battleProfit <= 0 ? -2 : weight + battleProfit;
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
        
        private Location prevLocation = new Location(-1,-1);
        public HommCommand TakeMovementDecision(Dictionary<Location, double> firstLevel)
        {
            var maxs = firstLevel
                .Where(pair => AI.CanStandThere(ai.CurrentData.Map, pair.Key))
                .OrderByDescending(pair => pair.Value)
                .ToArray();
            var ourLocation = ai.CurrentData.Location.ToLocation();
            foreach (var max in maxs)
            {
                foreach (var direction in Constants.Directions)
                {
                    var neighbor = ourLocation.NeighborAt(direction);
                    if (neighbor != max.Key ||
                        prevLocation.X == neighbor.X && prevLocation.Y == neighbor.Y)
                        continue;
                    prevLocation = ourLocation;
                    return CommandGenerator.GetMoveCommand(direction);
                }
            }
            var dir = GetFirstAvailableDirection();
            prevLocation = ourLocation;
            return CommandGenerator.GetMoveCommand(dir);
        }

        private Direction GetFirstAvailableDirection()
        {
            var curLocation = ai.CurrentData.Location.ToLocation();
            return Constants.Directions.FirstOrDefault(
                direction => AI.CanStandThere(ai.CurrentData.Map, curLocation.NeighborAt(direction)));
        }
    }
}