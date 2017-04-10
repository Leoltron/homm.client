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

        public AI(HommClient client, HommSensorData initialData, bool debugMode = false)
        {
            this.client = client;
            DataHandler= new DataHandler(initialData);
            this.client.OnSensorDataReceived += OnDataUpdated;
            locHelper = new LocationHelper(DataHandler);
            locWeightCalc = new LocationWeightCalculator(this, locHelper);
            BattleCalc = new BattleCalculator(this);
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
                if (CurrentData.Location.X == 8 && CurrentData.Location.Y == 7)
                    ;
                var suitableLocations = locWeightCalc.GetSpreadWeights(CurrentData.Map);
                var firstLevel = GetFirstLevel(suitableLocations, CurrentData.Location.ToLocation());
                Debug(suitableLocations); //смотрю коэффициенты на поле
                OnDataUpdated(client.Act(TakeMovementDecision(firstLevel)));
            }
        }

        private Dictionary<Location, double> GetFirstLevel(Dictionary<Location, double> suitableLocations, Location ourLocation)
        {
            return ourLocation.Neighborhood
                .Where(suitableLocations.ContainsKey)
                .ToDictionary(location => location, location => suitableLocations[location]);
        }

        private HommCommand TakeMovementDecision(Dictionary<Location, double> firstLevel)
        {
            var maxs = firstLevel
                .Where(pair => LocationHelper.CanStandThere(pair.Key, CurrentData.Map))
                .OrderByDescending(pair => pair.Value)
                .ToArray();
            var ourLocation = CurrentData.Location.ToLocation();
            foreach (var max in maxs)
            {
                foreach (var direction in Constants.Directions)
                {
                    var neighbor = ourLocation.NeighborAt(direction);
                    if (neighbor != max.Key)
                        continue;
                    return CommandGenerator.GetMoveCommand(direction);
                }
            }
            var dir = locHelper.GetFirstAvailableDirection();
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
        private void Debug(Dictionary<Location, double> weights)
        {
            var width = CurrentData.Map.Width;
            var height = CurrentData.Map.Height;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var loc = new Location(i, j);
                    Console.Write(weights.ContainsKey(loc) ? $" {weights[loc]:000.00}" : "   0   ");
                }
                Console.WriteLine();
            }
        }
    }
}