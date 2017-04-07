using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class BattleCalculator
    {
        private readonly AI ai;

        public BattleCalculator(AI ai)
        {
            this.ai = ai;
        }

        public bool WouldWinAttackAgainst(Dictionary<UnitType, int> enemy)
        {
            return Combat.Resolve(new ArmiesPair(ai.CurrentData.MyArmy, enemy)).IsAttackerWin;
        }

        public double GetProfitFromAttack(Dictionary<UnitType, int> enemyArmy)
        {
            return GetBattleProfit(new ArmiesPair(ai.CurrentData.MyArmy, enemyArmy));
        }

        private double GetBattleProfit(ArmiesPair initialState, bool isAttackerProfit = true)
        {
            return GetBattleProfit(initialState, Combat.Resolve(initialState), isAttackerProfit);
        }

        private double GetBattleProfit(ArmiesPair initialState, Combat.CombatResult result, bool isAttackerProfit = true)
        {
            //if (isAttackerProfit && result.IsAttackerWin || !isAttackerProfit && result.IsDefenderWin)
            //    return 0;
            if (!result.IsAttackerWin)
                return 0;
            return 1;

            var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();
            return
            (from type in unitTypes
             let attackerLoss =
             initialState.AttackingArmy.GetOrDefault(type) - result.AttackingArmy.GetOrDefault(type)
             let defenderLoss =
             initialState.DefendingArmy.GetOrDefault(type) - result.DefendingArmy.GetOrDefault(type)
             select isAttackerProfit
                 ? defenderLoss * UnitsConstants.Current.Scores[type] -
                   attackerLoss * ai.DataHandler.GetDegreeOfNeed(type)
                 : attackerLoss * UnitsConstants.Current.Scores[type] -
                   defenderLoss * ai.DataHandler.GetDegreeOfNeed(type)
            ).Sum();
        }
    }
}
