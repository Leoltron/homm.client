using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

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
                let attackerLoss = initialState.AttackingArmy.GetOrDefault(type) - result.AttackingArmy.GetOrDefault(type)
                let defenderLoss = initialState.DefendingArmy.GetOrDefault(type) - result.DefendingArmy.GetOrDefault(type)
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
                var levels = calculator.DivideByFar(radius, CurrentData);
                var godnota = new Dictionary<Location, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                {
                    var level = levels[i];
                    godnota[i] = new Dictionary<Location, double>();
                    foreach (var keyValuePair in level)
                    {
                        godnota[i].Add(keyValuePair.Key, calculator.GetWeight(keyValuePair.Value));
                        if (i != lastLevel)
                            godnota[i][keyValuePair.Key] += calculator.AddNeighboursWeight(godnota[i + 1],
                                keyValuePair.Value);
                    }
                }
                CurrentData = Client.Act(calculator.TakeDecision(godnota[1]));
            }
        }


        private void OnDataUpdated(HommSensorData data)
        {
            CurrentData = data; // Не совсем уверен, что тут еще что-то может быть, ну да ладно
            UpdateData();
        }
    }
}