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
        private readonly HommClient client;
        public HommSensorData CurrentData;

        public EnemyArmyData EnemyArmyData;
        public ResourcesData ResourcesData;
        private readonly LocationValueCalculator locCalc;
        public readonly BattleCalculator BattleCalc;

        public AI(HommClient client, HommSensorData initialData, bool debugMode = false)
        {
            this.client = client;
            CurrentData = initialData;
            this.client.OnSensorDataReceived += OnDataUpdated;
            UpdateData();
            locCalc = new LocationValueCalculator(this);
            BattleCalc = new BattleCalculator(this);
            while (!debugMode)
            {
                NextMove();
            }
        }

        private void UpdateData()
        {
            EnemyArmyData = EnemyArmyData.Parse(CurrentData);
            ResourcesData = ResourcesData.Parse(CurrentData);
        }

        private void NextMove()
        {
            if (CurrentData.IsDead)
                client.Wait(HommRules.Current.RespawnInterval);
            else
            {
                TryHire();
                var levels = NeighboursHelper.GroupByRange(CurrentData);
                var suitableLocations = new Dictionary<Location, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                {
                    var level = levels[i];
                    suitableLocations[i] = new Dictionary<Location, double>();
                    foreach (var keyValuePair in level)
                    {
                        suitableLocations[i]
                            .Add(
                                keyValuePair.Key,
                                locCalc.GetMapObjectWeight(keyValuePair.Value));
                        if (i == lastLevel) continue;
                        var current = keyValuePair.Value;
                        suitableLocations[i][keyValuePair.Key] +=
                            NeighboursHelper.AddNeighboursWeight(
                                suitableLocations[i + 1],
                                CurrentData.Map, current.Location.ToLocation());
                    }
                }
                //debug(suitableLocations); //смотрю коэффициенты на поле
                OnDataUpdated(client.Act(locCalc.TakeMovementDecision(suitableLocations[1])));
            }
        }

        private void TryHire()
        {
            var howManyCanHireHere = HireHelper.HowManyCanHire(GetObjectAtMe().Dwelling, CurrentData.MyTreasury);
            if (howManyCanHireHere > 0)
                OnDataUpdated(client.HireUnits(howManyCanHireHere));
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
            return CurrentData.Map.Objects.FirstOrDefault(
                x => x.Location.X == location.X && x.Location.Y == location.Y);
        }

        public static bool CanStandThere(MapData map, Location location)
        {
            if (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height)
                return false;
            var obj = map.Objects.FirstOrDefault(x => x.Location.X == location.X && x.Location.Y == location.Y);
            return obj != null && obj.Wall == null;
        }

        private void OnDataUpdated(HommSensorData data)
        {
            CurrentData = data;
            UpdateData();
        }


        //вот тут их смотрю
        private void Debug(IEnumerable<Dictionary<Location, double>> weights)
        {
            var width = CurrentData.Map.Width;
            var height = CurrentData.Map.Height;
            var w = weights
                .Skip(1)
                .SelectMany(pair => pair)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var loc = new Location(i, j);
                    Console.Write(w.ContainsKey(loc) ? $" {w[loc]:000.00}" : "   0   ");
                }
                Console.WriteLine();
            }
        }
    }
}