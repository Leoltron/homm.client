using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using Dwelling = HoMM.ClientClasses.Dwelling;

namespace Homm.Client
{
    public class AI
    {
        public readonly HommClient Client;
        public HommSensorData CurrentData;

        public EnemyArmyData EnemyArmyData;
        public ResourcesData ResourcesData;
        private const int radius = 10; //c этим еще определимся
        private readonly LocationValueCalculator calculator;


        public AI(HommClient client, HommSensorData initialData)
        {
            Client = client;
            CurrentData = initialData;
            Client.OnSensorDataReceived += OnDataUpdated;
            UpdateData();
            calculator = new LocationValueCalculator(this);

            while (true)
            {
                NextMove();
            }
        }

        public double GetProfitFromAttack(Dictionary<UnitType, int> enemyArmy)
        {
            return GetBattleProfit(new ArmiesPair(CurrentData.MyArmy, enemyArmy));
        }

        private double GetBattleProfit(ArmiesPair initialState, bool isAttackerProfit = true)
        {
            return GetBattleProfit(initialState, Combat.Resolve(initialState), isAttackerProfit);
        }

        private double GetBattleProfit(ArmiesPair initialState, Combat.CombatResult result, bool isAttackerProfit = true)
        {
            if (isAttackerProfit && result.IsAttackerWin || !isAttackerProfit && result.IsDefenderWin)
                return 0;

            var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
            return
            (from type in unitTypes
                let attackerLoss =
                initialState.AttackingArmy.GetOrDefault(type) - result.AttackingArmy.GetOrDefault(type)
                let defenderLoss =
                initialState.DefendingArmy.GetOrDefault(type) - result.DefendingArmy.GetOrDefault(type)
                select isAttackerProfit
                    ? defenderLoss * UnitsConstants.Current.Scores[type] -
                      attackerLoss * calculator.GetDegreeOfNeed(type)
                    : attackerLoss * UnitsConstants.Current.Scores[type] -
                      defenderLoss * calculator.GetDegreeOfNeed(type)
            ).Sum();
        }

        private void UpdateData()
        {
            EnemyArmyData = EnemyArmyData.Parse(CurrentData);
            ResourcesData = ResourcesData.Parse(CurrentData);
        }

        private bool WouldWinAttackAgainst(Dictionary<UnitType, int> enemy)
        {
            return Combat.Resolve(new ArmiesPair(CurrentData.MyArmy, enemy)).IsAttackerWin;
        }

        private void NextMove()
        {
            if (CurrentData.IsDead)
                Client.Wait(HommRules.Current.RespawnInterval);
            else
            {
                var howManyCanHireHere = HowManyICanHire(GetObjectAtMe().Dwelling);
                if (howManyCanHireHere > 0)
                    OnDataUpdated(Client.HireUnits(howManyCanHireHere));
                var levels = LocationValueCalculator.DivideByFar(radius, CurrentData);
                var suitableLocations = new Dictionary<Location, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                {
                    var level = levels[i];
                    suitableLocations[i] = new Dictionary<Location, double>();
                    foreach (var keyValuePair in level)
                    {
                        suitableLocations[i].Add(keyValuePair.Key, calculator.GetWeight(keyValuePair.Value));
                        if (i != lastLevel)
                            suitableLocations[i][keyValuePair.Key] +=
                                calculator.AddNeighboursWeight(suitableLocations[i + 1],
                                    keyValuePair.Value);
                    }
                }
                OnDataUpdated(Client.Act(calculator.TakeMovementDecision(suitableLocations[1])));
            }
        }

        private MapObjectData GetObjectAtMe()
        {
            return GetObjectAt(CurrentData.Location.ToLocation());
        }

        private MapObjectData GetObjectAt(Location location)
        {
            if (location.X < 0 || location.X >= CurrentData.Map.Width || location.Y < 0 ||
                location.Y >= CurrentData.Map.Height)
                return null;
            return CurrentData.Map.Objects.FirstOrDefault(x => x.Location.X == location.X && x.Location.Y == location.Y);
        }

        public static bool CanStandThere(MapData map, Location location)
        {
            if (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height)
                return false;
            var obj = map.Objects.FirstOrDefault(x => x.Location.X == location.X && x.Location.Y == location.Y);
            return obj != null && obj.Wall == null;
        }

        public int HowManyICanHire(Dwelling dwelling)
        {
            return HowManyCanHire(dwelling, CurrentData.MyTreasury);
        }

        public static int HowManyCanHire(Dwelling dwelling, Dictionary<Resource, int> resources)
        {
            if (dwelling == null)
                return 0;
            var hireCost = UnitsConstants.Current.UnitCost[dwelling.UnitType];
            var canHireWithResource = (from resource in hireCost.Keys
                where
                hireCost[resource] > 0 &&
                resources.ContainsKey(resource) &&
                resources[resource] > 0
                select resources[resource] / hireCost[resource]).ToList();
            canHireWithResource.Add(dwelling.AvailableToBuyCount);
            return canHireWithResource.Min();
        }

        private void OnDataUpdated(HommSensorData data)
        {
            CurrentData = data; // Не совсем уверен, что тут еще что-то может быть, ну да ладно
            UpdateData();
        }
    }
}