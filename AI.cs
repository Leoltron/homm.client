using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using HoMM.Robot;

namespace Homm.Client
{
    // ReSharper disable once InconsistentNaming
    public class AI
    {
        private readonly HommClient client;

        private readonly LocationWeightCalculator locWeightCalc;
        public readonly BattleCalculator BattleCalc;
        private readonly LocationHelper locHelper;
        public readonly DataHandler DataHandler;

        public AI(HommClient client, HommSensorData initialData)
        {
            this.client = client;
            DataHandler = new DataHandler(initialData);
            this.client.OnSensorDataReceived += OnDataUpdated;
            locHelper = new LocationHelper(DataHandler);
            locWeightCalc = new LocationWeightCalculator(this, locHelper);
            BattleCalc = new BattleCalculator(DataHandler, DataHandler);
        }

        public void Run()
        {
            while (true)
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
                var suitableLocations = locWeightCalc.GetSpreadedWeights();
                var firstLevel = GetFirstLayer(suitableLocations);
                Debug(suitableLocations); //смотрю коэффициенты на поле
                OnDataUpdated(client.Act(TakeMovementDecision(firstLevel)));
            }
        }

        private Dictionary<Location, double> GetFirstLayer(
            Dictionary<Location, double> suitableLocations)
        {
            return CurrentData.Location.ToLocation().Neighborhood
                .Where(suitableLocations.ContainsKey)
                .ToDictionary(location => location, location => suitableLocations[location]);
        }

        private HommCommand TakeMovementDecision(Dictionary<Location, double> neighbourhood)
        {
            var availableNeighbours = neighbourhood
                .Where(pair => locHelper.CanStandThere(pair.Key))
                .ToArray();

            Direction resultDirection;
            if (availableNeighbours.Length == 0)
                resultDirection = locHelper.GetFirstAvailableDirection();
            else
            {
                var ourLocation = CurrentData.Location.ToLocation();
                resultDirection = ourLocation.GetDirectionTo(
                    availableNeighbours.OrderByDescending(a => a.Value).First().Key);
            }

            return CommandGenerator.GetMoveCommand(resultDirection);
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
            locHelper.UpdateData(DataHandler);
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