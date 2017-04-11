using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HoMM;
using HoMM.ClientClasses;
using NUnit.Framework;

namespace Homm.Client.Tests
{
    [TestFixture]
    internal class BattleCalculatorTests
    {
        private static readonly Random Rand = new Random();

        private static Dictionary<UnitType, int> GetRandomArmy(int totalAmount)
        {
            var result = new Dictionary<UnitType, int>();
            foreach (var unitType in Enum.GetValues(typeof(UnitType)).Cast<UnitType>())
            {
                var amount = Rand.Next(totalAmount + 1);
                result[unitType] = amount;
                totalAmount -= amount;
            }
            return result;
        }

        private BattleCalculator getDefaultBattleCalculator()
        {
            return new BattleCalculator(DummyPlayerInfo.GetExampleInfo(), new DummyCoefficients());
        }

        [Test]
        public void TestInternalRandomArmy()
        {
            Combat.Resolve(new ArmiesPair(GetRandomArmy(500), GetRandomArmy(500)));
        }

        [Test]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        public void TestCreate()
        {
            getDefaultBattleCalculator();
        }

        [Test]
        public void TestWinAgainstNoone()
        {
            var battlehelper = getDefaultBattleCalculator();
            Assert.IsTrue(battlehelper.WouldWinAttackAgainst(GetRandomArmy(0)));
        }

        [Test]
        public void TestWinAgainstOne()
        {
            var battlehelper = getDefaultBattleCalculator();
            Assert.IsTrue(battlehelper.WouldWinAttackAgainst(GetRandomArmy(1)));
        }

        [Test]
        public void TestLoseAgainstHorde()
        {
            var battlehelper = getDefaultBattleCalculator();
            Assert.IsFalse(battlehelper.WouldWinAttackAgainst(GetRandomArmy(1000)));
        }

        [Test]
        public void TestProfitAgainstNoone()
        {
            var battlehelper = getDefaultBattleCalculator();
            var profit = battlehelper.GetProfitFromAttack(GetRandomArmy(0));
            Assert.IsTrue(Math.Abs(profit) < 1e-5,$"Expected profit to be zero, got {profit}");
        }

        [Test]
        public void TestProfitAgainstOne()
        {
            var battlehelper = getDefaultBattleCalculator();
            var profit = battlehelper.GetProfitFromAttack(GetRandomArmy(1));
            Assert.IsTrue(profit >= 0, $"Got profit {profit} less than zero");
        }

        [Test]
        public void TestProfitAgainstHorde()
        {
            var battlehelper = getDefaultBattleCalculator();
            var profit = battlehelper.GetProfitFromAttack(GetRandomArmy(1000));
            Assert.IsTrue(profit <= 0, $"Expected profit to be more than zero, expected {profit}");
        }
    }

    internal class DummyCoefficients : ITypesCoefficientsCalculator
    {
        private readonly Dictionary<UnitType, double> degreeOfUnitNeed;
        private readonly Dictionary<Resource, double> degreeOfResourceNeed;
        private readonly Dictionary<UnitType, double> counterMeetingPropability;

        public DummyCoefficients(
            double degreeOfUnitNeed = 1d,
            double degreeOfResourceNeed = 1d,
            double counterMeetingPropability = 1d) : this(
            new Dictionary<UnitType, double>
            {
                {UnitType.Cavalry, degreeOfUnitNeed},
                {UnitType.Infantry, degreeOfUnitNeed},
                {UnitType.Militia, degreeOfUnitNeed},
                {UnitType.Ranged, degreeOfUnitNeed}
            },
            new Dictionary<Resource, double>
            {
                {Resource.Ebony, degreeOfResourceNeed},
                {Resource.Iron, degreeOfResourceNeed},
                {Resource.Gold, degreeOfResourceNeed},
                {Resource.Glass, degreeOfResourceNeed}
            },
            new Dictionary<UnitType, double>
            {
                {UnitType.Cavalry, counterMeetingPropability},
                {UnitType.Infantry, counterMeetingPropability},
                {UnitType.Militia, counterMeetingPropability},
                {UnitType.Ranged, counterMeetingPropability}
            })
        {
        }

        public DummyCoefficients(
            Dictionary<UnitType, double> degreeOfUnitNeed,
            Dictionary<Resource, double> degreeOfResourceNeed,
            Dictionary<UnitType, double> counterMeetingPropability)
        {
            this.degreeOfUnitNeed = degreeOfUnitNeed;
            this.degreeOfResourceNeed = degreeOfResourceNeed;
            this.counterMeetingPropability = counterMeetingPropability;
        }

        public double GetDegreeOfNeed(UnitType unitType) => degreeOfUnitNeed[unitType];
        public double GetDegreeOfNeed(Resource resourceType) => degreeOfResourceNeed[resourceType];
        public double GetCounterMeetingPropability(UnitType type) => counterMeetingPropability[type];
    }
}