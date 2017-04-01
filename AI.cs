using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class AI
    {
        public readonly HommClient client;
        public HommSensorData currentData;

        public EnemyArmyData EnemyArmyData;
        public ResourcesData ResourcesData;
        private int radius; //c этим еще определимся
        private readonly LocationValueCalculator calculator;


        public AI(HommClient client, HommSensorData initialData)
        {
            this.client = client;
            currentData = initialData;
            this.client.OnSensorDataReceived += OnDataUpdated;
            UpdateData();
            calculator = new LocationValueCalculator(this);

            while (true)
            {
                NextMove();
            }
        }

        private double GetBattleProfit(ArmiesPair initialState, Combat.CombatResult result, bool isAttackerProfit = true)
        {
            if (isAttackerProfit && result.IsAttackerWin || !isAttackerProfit && result.IsDefenderWin)
                return 0;

            var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
            return
            (from type in unitTypes
                let attackerLoss = initialState.AttackingArmy[type] - result.AttackingArmy[type]
                let defenderLoss = initialState.DefendingArmy[type] - result.DefendingArmy[type]
                select isAttackerProfit
                    ? defenderLoss * UnitsConstants.Current.Scores[type] -
                      attackerLoss * calculator.GetDegreeOfNeed(type)
                    : //TODO: Сделать расчет войск -> ресурса (и насколько нам все же нужно ополчение?)
                    attackerLoss * UnitsConstants.Current.Scores[type] -
                    defenderLoss * calculator.GetDegreeOfNeed(type)
            ).Sum();
        }

        private void UpdateData()
        {
            EnemyArmyData = EnemyArmyData.Parse(currentData);
            ResourcesData = ResourcesData.Parse(currentData);
        }

        private bool WouldWinAttackAgainst(Dictionary<UnitType, int> enemy)
        {
            return Combat.Resolve(new ArmiesPair(currentData.MyArmy, enemy)).IsAttackerWin;
        }

        private void NextMove()
        {
            if (currentData.IsDead)
                client.Wait(HommRules.Current.RespawnInterval);
            else
            {
                var levels = calculator.DivideByFar(radius, currentData);
                var godnota = new Dictionary<Tuple<int, int>, double>[levels.Length];
                var lastLevel = levels.Length - 1;
                for (var i = lastLevel; i > 0; i--)
                {
                    var level = levels[i];
                    godnota[i] = new Dictionary<Tuple<int, int>, double>();
                    foreach (var keyValuePair in level)
                    {
                        godnota[i].Add(keyValuePair.Key, calculator.GetWeight(keyValuePair.Value));
                        if (i != lastLevel)
                            godnota[i][keyValuePair.Key] += calculator.AddNeighboursWeight(godnota[i + 1],
                                keyValuePair.Value);
                    }
                }
            }
        }


        private void OnDataUpdated(HommSensorData data)
        {
            currentData = data; // Не совсем уверен, что тут еще что-то может быть, ну да ладно
            UpdateData();
        }
    }
}