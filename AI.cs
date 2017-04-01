﻿using System;
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

        private EnemyArmyData enemyArmyData;
        private ResourcesData resourcesData;

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

        private double GetBattleProfit(Combat.CombatResult result, bool isAttackerProfit=true)
        {
            if (isAttackerProfit && result.IsAttackerWin || !isAttackerProfit && result.IsDefenderWin)
                return 0;
            var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
            
        }

        private Combat.CombatResult GetAttackResultAgainst(Dictionary<UnitType, int> enemy)
        {
            return Combat.Resolve(new ArmiesPair(currentData.MyArmy, enemy));
        }

        private void NextMove()
        {
            if (currentData.IsDead)
                client.Wait(HommRules.Current.RespawnInterval);
            else
            {
                throw new NotImplementedException();
            }
        }

        //TODO: Настроить коэффиценты
        private const double ResourceRarityCoefficent = 1;
        private const double ArmyEfficencyCoefficent = 1;

        private double GetPileValue(ResourcePile pile)
        {
            return HommRules.Current.ResourcesGainScores + pile.Amount * GetDegreeOfNeed(pile.Resource);
        }

        private double GetDegreeOfNeed(Resource resourceType)
        {
            return GetCounterMeetingPropability(UnitRelation[resourceType]) * ArmyEfficencyCoefficent
                   + resourcesData.GetRarity(resourceType) * ResourceRarityCoefficent;
        }

        //TODO: Настроить коэффицент
        private const double MineCoefficent = 1;

        private double GetMineValue(Mine mine)
        {
            return (HommRules.Current.MineOwningDailyScores +
                    GetDegreeOfNeed(mine.Resource) * HommRules.Current.MineDailyResourceYield) * MineCoefficent;
        }

        private double GetDwellingValue(Dwelling dwelling)
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

        private double GetCounterMeetingPropability(UnitType type)
        {
            return type == UnitType.Militia ? GoldMilitiaCounterConst : enemyArmyData.GetPart(UnitCounters[type]);
        }
    }
}