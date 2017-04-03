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
        private const int radius = 10; //c этим еще определимся
        public readonly LocationValueCalculator LocCalc;
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
                var howManyCanHireHere = hireHelper.HowManyICanHire(GetObjectAtMe().Dwelling);
                if (howManyCanHireHere > 0)
                    OnDataUpdated(client.HireUnits(howManyCanHireHere));
                var levels = LocationValueCalculator.GroupByRange(radius, CurrentData);
                var suitableLocations = new Dictionary<Location, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                {
                    var level = levels[i];
                    suitableLocations[i] = new Dictionary<Location, double>();
                    foreach (var keyValuePair in level)
                    {
                        suitableLocations[i].Add(keyValuePair.Key, LocCalc.GetMapObjectWeight(keyValuePair.Value));
                        if (i != lastLevel)
                            suitableLocations[i][keyValuePair.Key] +=
                                LocCalc.AddNeighboursWeight(suitableLocations[i + 1],
                                    keyValuePair.Value);
                    }
                }
                OnDataUpdated(client.Act(LocCalc.TakeMovementDecision(suitableLocations[1])));
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

        private void OnDataUpdated(HommSensorData data)
        {
            CurrentData = data; // Не совсем уверен, что тут еще что-то может быть, ну да ладно
            UpdateData();
        }
    }
}