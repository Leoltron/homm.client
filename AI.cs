using System;
using System.Collections.Generic;
using System.Linq;
using CVARC.V2;
using HoMM;
using HoMM.ClientClasses;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

namespace Homm.Client
{
    public class AI
    {
        private HommClient client;
        private HommSensorData currentData;

        private EnemyData enemyData;

        public AI(HommClient client, HommSensorData initialData)
        {
            this.client = client;
            currentData = initialData;
            client.OnSensorDataReceived += OnDataUpdated;
            UpdateData();

            while (true)
            {
                NextMove();
            }
        }

        private void UpdateData()
        {
            var enemyUnitsData = new Dictionary<UnitType,int>();
            foreach (var mapObject in currentData.Map.Objects)
            {
                //Не учитываю гарнизоны врага и врага, ибо хз как отличать от своих
                if(mapObject.NeutralArmy != null)
                    foreach (var army in mapObject.NeutralArmy.Army)
                        enemyUnitsData.AddOrSum(army);
            }
            enemyData = new EnemyData(enemyUnitsData);
        }

        private Boolean wouldWinAttackAgainst(Dictionary<UnitType, int> enemy)
        {
            return Combat.Resolve(new ArmiesPair(currentData.MyArmy, enemy)).IsAttackerWin;
        }

        private void NextMove()
        {
            throw new NotImplementedException();
        }

        private double GetPileValue(ResourcePile pile)
        {
            throw new NotImplementedException();
        }

        private void OnDataUpdated(HommSensorData data)
        {
            currentData = data;// Не совсем увыерен, что тут еще что-то может быть, ну да ладно
        }

        public struct EnemyData
        {
            private readonly Dictionary<UnitType, int> UnitAmount;
            private readonly int amountOverall;

            public double GetPart(UnitType type)
            {
                if (!UnitAmount.ContainsKey(type) || amountOverall == 0)
                    return 0;
                return (double) UnitAmount[type] / amountOverall;
            }

            public EnemyData(Dictionary<UnitType, int> unitAmount) : this()
            {
                UnitAmount = unitAmount;
                amountOverall = UnitAmount.Sum(u => u.Value);
            }
        }
    }
}