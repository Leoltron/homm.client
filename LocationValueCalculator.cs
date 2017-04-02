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
        private MapObjectData currentObject;

        private Direction[] ways =
        {
            Direction.Up, Direction.RightUp, Direction.RightDown, Direction.Down,
            Direction.LeftDown, Direction.LeftUp
        };

        public LocationValueCalculator(AI ai)
        {
            this.ai = ai;
        }

        public Dictionary<Location, MapObjectData>[] DivideByFar(int radius, HommSensorData data)
        {
            var levels = new Dictionary<int, List<MapObjectData>>();
            foreach (var mapObject in data.Map.Objects)
            {
                var dx = mapObject.Location.X - data.Location.X;
                var dy = mapObject.Location.Y - data.Location.Y;
                if (dx == 0 && dy == 0)
                    currentObject = mapObject;
                var distance = (int)Math.Sqrt(dx * dx + dy * dy);
                if (distance > radius)
                    continue;
                if (!levels.ContainsKey(distance))
                    levels.Add(distance, new List<MapObjectData>());
                levels[distance].Add(mapObject);
            }
            return levels
                .OrderBy(record => record.Key)
                .Select(record => record.Value)
                .Select(collection =>
                    collection.ToDictionary(point => new Location(point.Location.Y, point.Location.X)))
                .ToArray();
        }

        public double GetWeight(MapObjectData mapObject)
        {
            var weight = 0.0;
            weight += GetPileValue(mapObject.ResourcePile);
            weight += GetMineValue(mapObject.Mine);
            weight += GetDwellingValue(mapObject.Dwelling);
            weight += GetTerrainValue(mapObject.Terrain);
            var enemyArmy = GetEnemyArmy(mapObject);
            if (enemyArmy != null)
            {
                var profit = ai.GetProfitFromAttack(enemyArmy);
                if (profit <= 0)
                    weight = profit;
                else
                    weight += profit;
            }
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

        public double AddNeighboursWeight(Dictionary<Location, double> previousLevel,
            MapObjectData current)
        {
            var ourLocation = new Location(current.Location.Y, current.Location.X);
            if (Program.GetObjectAt(ai.CurrentData.Map, ourLocation) == "Wall")
                return 0;
            var neighbs = ourLocation.Neighborhood;
            return neighbs.Where(previousLevel.ContainsKey).Sum(neighb => previousLevel[neighb]);
        }

        public HommCommand TakeDecision(Dictionary<Location, double> firstLevel)
        {
            var badWays = new HashSet<string>{"Wall", "Nothing", "Outside"};
            var maxs = firstLevel
                .Where(pair => !badWays.Contains(Program.GetObjectAt(ai.CurrentData.Map, pair.Key)))
                .OrderByDescending(pair => pair.Value)
                .ToArray();
            var ourLocation = new Location(ai.CurrentData.Location.Y, ai.CurrentData.Location.X);
            foreach (var max in maxs)
            {
                foreach (var direction in ways)
                {
                    var p = ourLocation.NeighborAt(direction);
                    if (p == max.Key)
                        return CommandGenerator.GetMoveCommand(direction);
                }
            }
            return CommandGenerator.GetMoveCommand(Direction.Down);
        }

        private void TryDwell()
        {
            var ourLocation = new Location(ai.CurrentData.Location.Y, ai.CurrentData.Location.X);
            if (Program.GetObjectAt(ai.CurrentData.Map, ourLocation) != "Dwelling")
                return;
            var type = currentObject.Dwelling.UnitType;
            var resource = UnitToResource[type];
            var countOfResource = ai.CurrentData.MyTreasury[resource];
            var countOfGold = ai.CurrentData.MyTreasury[Resource.Gold];
            // здесь я хотел получить стоимость юнита которого можно купить и нанять максимальное количество
        }

        //TODO: Настроить коэффиценты
        private const double ResourceRarityCoefficent = 10;
        private const double ArmyEfficencyCoefficent = 10;

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
        private const double MineCoefficent = 20;

        public double GetMineValue(Mine mine)
        {
            return mine == null
                ? 0
                : (HommRules.Current.MineOwningDailyScores +
                   GetDegreeOfNeed(mine.Resource) * HommRules.Current.MineDailyResourceYield) * MineCoefficent;
        }

        public double GetDwellingValue(Dwelling dwelling)
        {
            return dwelling != null ? 10 : 0;
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