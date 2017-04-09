using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using HoMM.Robot;

namespace Homm.Client
{
    public class AI
    {
        private readonly HommClient client;

        private readonly LocationWeightCalculator locWeightCalc;
        public readonly BattleCalculator BattleCalc;
        private readonly LocationHelper locHelper;
        public readonly DataHandler DataHandler;
        private readonly NeighboursHelper neighbsHelper;

        public AI(HommClient client, HommSensorData initialData, bool debugMode = false)
        {
            this.client = client;
            DataHandler= new DataHandler(initialData);
            this.client.OnSensorDataReceived += OnDataUpdated;
            locHelper = new LocationHelper(this);
            locWeightCalc = new LocationWeightCalculator(this, locHelper);
            BattleCalc = new BattleCalculator(this);
            neighbsHelper = new NeighboursHelper(locHelper);
            while (!debugMode)
            {
                NextMove();
            }
        }

        public HommSensorData CurrentData => DataHandler.CurrentData;

        private void NextMove()
        {
            if (CurrentData.IsDead)
                client.Wait(HommRules.Current.RespawnInterval);
            else
            {
                TryHire();
                var levelsLocations = neighbsHelper.GroupByRange(CurrentData);
                var levels = OutsideVisibility.Refresh(levelsLocations, locHelper);
                var suitableLocations = new Dictionary<Location, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                    locWeightCalc.CalculateLevelWeights(suitableLocations, levels, i, lastLevel);
                Debug(suitableLocations); //смотрю коэффициенты на поле
                OnDataUpdated(client.Act(TakeMovementDecision(suitableLocations[1])));
            }
        }

        //private Location prevLocation = new Location(-1, -1);

        private HommCommand TakeMovementDecision(Dictionary<Location, double> firstLevel)
        {
            var maxs = firstLevel
                .Where(pair => LocationHelper.CanStandThere(CurrentData.Map, pair.Key))
                .OrderByDescending(pair => pair.Value)
                .ToArray();
            var ourLocation = CurrentData.Location.ToLocation();
            foreach (var max in maxs)
            {
                foreach (var direction in Constants.Directions)
                {
                    var neighbor = ourLocation.NeighborAt(direction);
                    if (neighbor != max.Key)// ||
                      //  prevLocation.X == neighbor.X && prevLocation.Y == neighbor.Y)
                        continue;
                  //  prevLocation = ourLocation;
                    return CommandGenerator.GetMoveCommand(direction);
                }
            }
            var dir = locHelper.GetFirstAvailableDirection();
           // prevLocation = ourLocation;
            return CommandGenerator.GetMoveCommand(dir);
        }

        private void TryHire()
        {
            var howManyCanHireHere = HireHelper.HowManyCanHire(GetObjectAtMe().Dwelling, CurrentData.MyTreasury);
            if (howManyCanHireHere > 0)
                OnDataUpdated(client.HireUnits(howManyCanHireHere));
        }

        private MapObjectData GetObjectAtMe()
        {
            return locHelper.GetObjectAt(CurrentData.Location.ToLocation());
        }

        private void OnDataUpdated(HommSensorData data)
        {
            DataHandler.UpdateData(data);
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