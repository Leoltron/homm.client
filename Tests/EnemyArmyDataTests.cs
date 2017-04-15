using System;
using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;
using NUnit.Framework;
using Garrison = HoMM.ClientClasses.Garrison;
using NeutralArmy = HoMM.ClientClasses.NeutralArmy;

// ReSharper disable UnusedVariable

namespace Homm.Client.Tests
{
    [TestFixture]
    internal class EnemyArmyDataTests
    {
        [Test]
        public void TestCreate()
        {
            var enemyArmyData1 = new EnemyArmyData(new Dictionary<UnitType, int>());
            var enemyArmyData2 = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Cavalry, 5},
                {UnitType.Militia, 20}
            });
        }

        private static void CreateWithNegativeAmount()
        {
            var enemyArmyData = new EnemyArmyData(new Dictionary<UnitType, int> {{UnitType.Militia, -10}});
        }

        [Test]
        public void TestInvalidUnitAmount()
        {
            Assert.Catch(typeof(ArgumentException), CreateWithNegativeAmount);
        }

        [TestCase(25, 50, 125, 300)]
        [TestCase(0, 89, 11, 1205)]
        [TestCase(0, 0, 0, 0)]
        public void TestAmountOverall(int militiaAmount, int cavalryAmount, int infantryAmount, int rangedAmount)
        {
            var armyData = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Militia, militiaAmount},
                {UnitType.Cavalry, cavalryAmount},
                {UnitType.Infantry, infantryAmount},
                {UnitType.Ranged, rangedAmount}
            });
            Assert.AreEqual(militiaAmount + cavalryAmount + infantryAmount + rangedAmount, armyData.AmountOverall);
        }

        [TestCase(25, 50, 125, 300,
            0.05, 0.1, 0.25, 0.6)]
        [TestCase(0,89,11,1205,
            0,0.06819,0.00842,0.92337)]
        public void TestPart(
            int militiaAmount, int cavalryAmount, int infantryAmount, int rangedAmount,
            double militiaPart, double cavalryPart, double infantryPart, double rangedPart)
        {
            var armyData = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Militia, militiaAmount},
                {UnitType.Cavalry, cavalryAmount},
                {UnitType.Infantry, infantryAmount},
                {UnitType.Ranged, rangedAmount}
            });
            Assert.AreEqual(militiaPart, armyData.GetPart(UnitType.Militia), 1e-5);
            Assert.AreEqual(cavalryPart, armyData.GetPart(UnitType.Cavalry), 1e-5);
            Assert.AreEqual(infantryPart, armyData.GetPart(UnitType.Infantry), 1e-5);
            Assert.AreEqual(rangedPart, armyData.GetPart(UnitType.Ranged), 1e-5);
        }

        [Test]
        public void TestPartMissingUnits()
        {
            var armyData = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Militia, 25},
                {UnitType.Ranged, 300}
            });
            Assert.AreEqual(0.07692, armyData.GetPart(UnitType.Militia), 1e-5);
            Assert.AreEqual(0, armyData.GetPart(UnitType.Cavalry), 1e-5);
            Assert.AreEqual(0, armyData.GetPart(UnitType.Infantry), 1e-5);
            Assert.AreEqual(0.92307, armyData.GetPart(UnitType.Ranged), 1e-5);
        }

        [Test]
        public void TestPartNoUnits()
        {
            var armyData = new EnemyArmyData(new Dictionary<UnitType, int>());
            Assert.AreEqual(0, armyData.GetPart(UnitType.Militia), 1e-5);
            Assert.AreEqual(0, armyData.GetPart(UnitType.Cavalry), 1e-5);
            Assert.AreEqual(0, armyData.GetPart(UnitType.Infantry), 1e-5);
            Assert.AreEqual(0, armyData.GetPart(UnitType.Ranged), 1e-5);
        }

        [Test]
        public void TestParse()
        {
            var map = DummyLocationMap.GetExampleMap();
            var info = DummyPlayerInfo.GetExampleInfo();
            map.Map.Objects[1].NeutralArmy = new NeutralArmy(new Dictionary<UnitType, int> {{UnitType.Ranged, 10}});
            map.Map.Objects[2].Garrison = new Garrison("Enemy", new Dictionary<UnitType, int> {{UnitType.Ranged, 10}});
            map.Map.Objects[3].Hero = new Hero("Enemy", new Dictionary<UnitType, int> {{UnitType.Ranged, 10}});
            var data = EnemyArmyData.Parse(info, map);
            Assert.IsTrue(data.AmountOverall > 0);
        }
    }
}