﻿using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class BattleCalculator
    {
        private readonly IPlayerInfoProvider playerInfo;
        private readonly ITypesCoefficientsCalculator coeffsCalc;

        public BattleCalculator(IPlayerInfoProvider playerInfo, ITypesCoefficientsCalculator calc)
        {
            this.playerInfo = playerInfo;
            coeffsCalc = calc;
        }

        public bool WouldWinAttackAgainst(Dictionary<UnitType, int> enemy)
        {
            return Combat.Resolve(new ArmiesPair(playerInfo.MyArmy, enemy)).IsAttackerWin;
        }

        public double GetProfitFromAttack(Dictionary<UnitType, int> enemyArmy)
        {
            return GetBattleProfit(new ArmiesPair(playerInfo.MyArmy, enemyArmy));
        }

        private double GetBattleProfit(ArmiesPair initialState, bool isAttackerProfit = true)
        {
            return GetBattleProfit(initialState, Combat.Resolve(initialState), isAttackerProfit);
        }

        private double GetBattleProfit(ArmiesPair initialState, Combat.CombatResult result,
            bool isAttackerProfit = true)
        {
            if (result.IsDefenderWin)
                return -1;
            var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
            return
            (from type in unitTypes
                let attackerLoss =
                initialState.AttackingArmy.GetOrDefault(type) - result.AttackingArmy.GetOrDefault(type)
                let defenderLoss =
                initialState.DefendingArmy.GetOrDefault(type) - result.DefendingArmy.GetOrDefault(type)
                select defenderLoss * UnitsConstants.Current.Scores[type] -
                       attackerLoss * coeffsCalc.GetDegreeOfNeed(type)
            ).Sum();
        }
    }
}