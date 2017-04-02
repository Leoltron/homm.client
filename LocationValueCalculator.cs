using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using HoMM.Robot;
using Dwelling = HoMM.ClientClasses.Dwelling;
using Mine = HoMM.ClientClasses.Mine;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

namespace Homm.Client
{
    internal class LocationValueCalculator
    {
        private readonly AI ai;

        public static readonly Dictionary<Tuple<int, int>, Direction> Compass = new Dictionary
            <Tuple<int, int>, Direction>
            {
                {Tuple.Create(-1, -1), Direction.LeftDown},
                {Tuple.Create(-1, 1), Direction.LeftUp},
                {Tuple.Create(0, -1), Direction.Down},
                {Tuple.Create(0, 1), Direction.Up},
                {Tuple.Create(1, -1), Direction.RightDown},
                {Tuple.Create(1, 1), Direction.RightUp}
            };

        public LocationValueCalculator(AI ai)
        {
            this.ai = ai;
        }

        public Dictionary<Tuple<int, int>, MapObjectData>[] DivideByFar(int radius, HommSensorData data)
        {
            var levels = new Dictionary<int, List<MapObjectData>>();
            foreach (var mapObject in data.Map.Objects)
            {
                var dx = mapObject.Location.X - data.Location.X;
                var dy = mapObject.Location.Y - data.Location.Y;
                var distance = dx * dx + dy * dy;
                if (Math.Sqrt(distance) > radius)
                    continue;
                if (!levels.ContainsKey(distance))
                    levels.Add(distance, new List<MapObjectData>());
                levels[distance].Add(mapObject);
            }
            return levels
                .OrderBy(record => record.Key)
                .Select(record => record.Value)
                .Select(collection =>
                    collection.ToDictionary(point => Tuple.Create(point.Location.X, point.Location.Y)))
                .ToArray();
        }

        public double GetWeight(MapObjectData mapObject)
        {
            var weight = 0.0;
            weight += GetPileValue(mapObject.ResourcePile);
            weight += GetMineValue(mapObject.Mine);
            //weight += GetDwellingValue(mapObject.Dwelling);
            weight += GetTerrainValue(mapObject.Terrain);
            var enemyArmy = GetEnemyArmy(mapObject);
            if (enemyArmy != null)
                weight += ai.GetProfitFromAttack(enemyArmy);
            //... и тут видимо для каждого поля нужно так сделать(
            return weight;
        }

        private Dictionary<UnitType, int> GetEnemyArmy(MapObjectData cell)
        {
            if (cell.NeutralArmy != null)
                return cell.NeutralArmy.Army;
            if (cell.Hero != null)
                return cell.Hero.Army;
            if (cell.Garrison != null && cell.Garrison.Owner != "Видимо имя нашего героя")
                return cell.Garrison.Army;
            return null;
        }

        public double AddNeighboursWeight(Dictionary<Tuple<int, int>, double> previousLevel,
            MapObjectData current)
        {
            var sum = 0.0;
            for (var i = -1; i <= 1; i++)
            for (var j = -1; j <= 1; j++)
            {
                var x = current.Location.X + i;
                var y = current.Location.Y + j;
                var point = Tuple.Create(x, y);
                if (i != 0 || j != 0 && previousLevel.ContainsKey(point))
                    sum += previousLevel[point];
            }
            return sum;
        }

        public HommCommand TakeDecision(Dictionary<Tuple<int, int>, double> neighbours)
        {
            var x = ai.CurrentData.Location.X;
            var y = ai.CurrentData.Location.Y;
            var bestDirection = new Tuple<int, int>(0, 0);
            double max = -1;
            for (var i = -1; i <= 1; i++)
            for (var j = -1; j <= 1; j++)
            {
                if (i != 0 || j != 0)
                {
                    var neighb = Tuple.Create(x + i, y + j);
                    if (neighbours.ContainsKey(neighb) && neighbours[neighb] > max)
                    {
                        max = neighbours[neighb];
                        bestDirection = Tuple.Create(i, j);
                    }
                }
            }
            return CommandGenerator.GetMoveCommand(Compass[bestDirection]);
        }

        //TODO: Настроить коэффиценты
        private const double ResourceRarityCoefficent = 1;
        private const double ArmyEfficencyCoefficent = 1;

        public double GetPileValue(ResourcePile pile)
        {
            return pile == null
                ? 0
                : HommRules.Current.ResourcesGainScores + pile.Amount * GetDegreeOfNeed(pile.Resource);
        }

        public double GetDegreeOfNeed(UnitType unitType)
        {
            return GetDegreeOfNeed(UnitToResource[unitType]);
        }

        public double GetDegreeOfNeed(Resource resourceType)
        {
            //TODO: Вынести в переменную, чтобы считалась лишь раз между двумя обновлениями данных?
            return GetCounterMeetingPropability(ResourceToUnit[resourceType]) * ArmyEfficencyCoefficent
                   + ai.ResourcesData.GetRarity(resourceType) * ResourceRarityCoefficent;
        }

        //TODO: Настроить коэффицент
        private const double MineCoefficent = 1;

        public double GetMineValue(Mine mine)
        {
            return mine == null
                ? 0
                : (HommRules.Current.MineOwningDailyScores +
                   GetDegreeOfNeed(mine.Resource) * HommRules.Current.MineDailyResourceYield) * MineCoefficent;
        }

        public double GetDwellingValue(Dwelling dwelling)
        {
            throw new NotImplementedException();
        }

        private double GetTerrainValue(Terrain terrain) => 1 / costOfMove[terrain];

        private static readonly Dictionary<Terrain, double> costOfMove = new Dictionary<Terrain, double>
        {
            {Terrain.Desert, TileTerrain.Desert.TravelCost},
            {Terrain.Grass, TileTerrain.Grass.TravelCost},
            {Terrain.Marsh, TileTerrain.Marsh.TravelCost},
            {Terrain.Road, TileTerrain.Road.TravelCost},
            {Terrain.Snow, TileTerrain.Snow.TravelCost}
        };

        public static readonly Dictionary<Resource, UnitType> ResourceToUnit = new Dictionary<Resource, UnitType>()
        {
            {Resource.Gold, UnitType.Militia},
            {Resource.Ebony, UnitType.Cavalry},
            {Resource.Glass, UnitType.Ranged},
            {Resource.Iron, UnitType.Infantry}
        };

        public static readonly Dictionary<UnitType, Resource> UnitToResource = new Dictionary<UnitType, Resource>()
        {
            {UnitType.Militia, Resource.Gold},
            {UnitType.Cavalry, Resource.Ebony},
            {UnitType.Ranged, Resource.Glass},
            {UnitType.Infantry, Resource.Iron}
        };

        public static readonly Dictionary<UnitType, UnitType> UnitCounters = new Dictionary<UnitType, UnitType>()
        {
            {UnitType.Infantry, UnitType.Cavalry},
            {UnitType.Cavalry, UnitType.Ranged},
            {UnitType.Ranged, UnitType.Infantry}
        };

        private const double GoldMilitiaCounterConst = 1d; //TODO: Настроить константу

        private double GetCounterMeetingPropability(UnitType type)
        {
            return type == UnitType.Militia ? GoldMilitiaCounterConst : ai.EnemyArmyData.GetPart(UnitCounters[type]);
        }
    }
}