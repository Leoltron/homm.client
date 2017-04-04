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
        private readonly LocationValueCalculator LocCalc;
        public readonly BattleCalculator BattleCalc;
        private readonly HireHelper hireHelper;

        public AI(HommClient client, HommSensorData initialData)
        {
            this.client = client;
            CurrentData = initialData;
            this.client.OnSensorDataReceived += OnDataUpdated;
            UpdateData();
            LocCalc = new LocationValueCalculator(this);
            BattleCalc = new BattleCalculator(this);
            hireHelper = new HireHelper(this);
            while (true)
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
                var levels = NeighbsHelper.GroupByRange(CurrentData);
                var suitableLocations = new Dictionary<Location, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                {
                    var level = levels[i];
                    suitableLocations[i] = new Dictionary<Location, double>();
                    foreach (var keyValuePair in level)
                    {
                        suitableLocations[i].Add(
                            keyValuePair.Key, 
                            LocCalc.GetMapObjectWeight(keyValuePair.Value));
                        if (i != lastLevel)
                            suitableLocations[i][keyValuePair.Key] +=
                                NeighbsHelper.AddNeighboursWeight(
                                    suitableLocations[i + 1],
                                    keyValuePair.Value,
                                    this);
                    }
                }
                //debug(suitableLocations); //смотрю коэффициенты на поле
                OnDataUpdated(client.Act(LocCalc.TakeMovementDecision(suitableLocations[1])));
            }
        }

        private void TryHire()
        {
            var howManyCanHireHere = hireHelper.HowManyICanHire(GetObjectAtMe().Dwelling);
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
            return CurrentData.Map.Objects.FirstOrDefault(x => x.Location.X == location.X && x.Location.Y == location.Y);
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
            CurrentData = data; // Не совсем уверен, что тут еще что-то может быть, ну да ладно
            UpdateData();
        }


        //вот тут их смотрю
        void debug(Dictionary<Location, double>[] weights)
        {
            int width = CurrentData.Map.Width;
            int height = CurrentData.Map.Height;
            var w = weights
                .Skip(1)
                .SelectMany(pair => pair)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            Location loc;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    loc = new Location(i, j);
                    if (w.ContainsKey(loc))
                        Console.Write(String.Format(" {0:000.00}", w[loc]));
                    else
                        Console.Write("   0   ");
                }
                Console.WriteLine();
            }
        }
    }
}