using System;
using System.Collections.Generic;
using System.Linq;
using CVARC.V2;
using HoMM;
using HoMM.ClientClasses;
using Dwelling = HoMM.ClientClasses.Dwelling;
using Mine = HoMM.ClientClasses.Mine;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

namespace Homm.Client
{
    public class AI
    {
        private HommClient client;
        private HommSensorData currentData;

        private static EnemyArmyData enemyArmyData;
        private static ResourcesData resourcesData;
        private int radius; //c этим еще определимся

        public AI(HommClient client, HommSensorData initialData)
        {
            this.client = client;
            currentData = initialData;
            this.client.OnSensorDataReceived += OnDataUpdated;
            UpdateData();

            while (true)
            {
                NextMove();
            }
        }

        private void UpdateData()
        {
            enemyArmyData = EnemyArmyData.Parse(currentData);
            resourcesData = ResourcesData.Parse(currentData);
        }

        private bool WouldWinAttackAgainst(Dictionary<UnitType, int> enemy)
        {
            return Combat.Resolve(new ArmiesPair(currentData.MyArmy, enemy)).IsAttackerWin;
        }

        private Direction TakeDecision(Dictionary<Tuple<int, int>, double> neighbours)
        {
            var x = currentData.Location.X;
            var y = currentData.Location.Y;
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
            return BackBrain.Compas[bestDirection];
        }

        private void NextMove()
        {
            if (currentData.IsDead)
                client.Wait(HommRules.Current.RespawnInterval);
            else
            {
                var levels = BackBrain.DivideByFar(radius, currentData);
                var godnota = new Dictionary<Tuple<int, int>, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                {
                    var level = levels[i];
                    godnota[i] = new Dictionary<Tuple<int, int>, double>();
                    foreach (var keyValuePair in level)
                    {
                        godnota[i].Add(keyValuePair.Key, BackBrain.GetWeight(keyValuePair.Value));
                        if (i != lastLevel)
                            godnota[i][keyValuePair.Key] += BackBrain.AddNeighboursWeight(godnota[i + 1], keyValuePair.Value);
                    }
                }
            }
        }

        //TODO: Настроить коэффиценты
        private const double ResourceRarityCoefficent = 1;
        private const double ArmyEfficencyCoefficent = 1;

        public static double GetPileValue(ResourcePile pile)
        {
            return HommRules.Current.ResourcesGainScores + pile.Amount * GetDegreeOfNeed(pile.Resource);
        }

        private static double GetDegreeOfNeed(Resource resourceType)
        {
            return GetCounterMeetingPropability(UnitRelation[resourceType]) * ArmyEfficencyCoefficent
                   + resourcesData.GetRarity(resourceType) * ResourceRarityCoefficent;
        }

        //TODO: Настроить коэффицент
        private const double MineCoefficent = 1;

        public static double GetMineValue(Mine mine)
        {
            return (HommRules.Current.MineOwningDailyScores +
                    GetDegreeOfNeed(mine.Resource) * HommRules.Current.MineDailyResourceYield) * MineCoefficent;
        }

        public static double GetDwellingValue(Dwelling dwelling)
        {
            throw new NotImplementedException();
        }

        private void OnDataUpdated(HommSensorData data)
        {
            currentData = data; // Не совсем уверен, что тут еще что-то может быть, ну да ладно
        }

        private static readonly Dictionary<Resource, UnitType> UnitRelation = new Dictionary<Resource, UnitType>()
        {
            {Resource.Gold, UnitType.Militia},
            {Resource.Ebony, UnitType.Cavalry},
            {Resource.Glass, UnitType.Ranged},
            {Resource.Iron, UnitType.Infantry}
        };

        private static readonly Dictionary<UnitType, UnitType> UnitCounters = new Dictionary<UnitType, UnitType>()
        {
            {UnitType.Infantry, UnitType.Cavalry},
            {UnitType.Cavalry, UnitType.Ranged},
            {UnitType.Ranged, UnitType.Infantry}
        };

        private const double GoldMilitiaCounterConst = 1d; //TODO: Настроить константу

        private static double GetCounterMeetingPropability(UnitType type)
        {
            return type == UnitType.Militia ? GoldMilitiaCounterConst : enemyArmyData.GetPart(UnitCounters[type]);
        }
    }
}