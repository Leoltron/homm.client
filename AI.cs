using System;
using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

namespace Homm.Client
{
    public class AI
    {
        private HommClient client;
        private HommSensorData currentData;

        public AI(HommClient client, HommSensorData initialData)
        {
            this.client = client;
            currentData = initialData;
            client.OnSensorDataReceived += OnDataUpdated;

            while (true)
            {
                NextMove();
            }
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


    }
}