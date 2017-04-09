using System;
using System.Collections.Generic;
using HoMM;
using NUnit.Framework;
using NUnit.Framework.Internal;

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

        [Test]
        public void TestTotal()
        {
            var armyData = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Militia, 25},
                {UnitType.Cavalry, 50},
                {UnitType.Infantry, 125},
                {UnitType.Ranged, 300}
            });
            Assert.AreEqual(500, armyData.amountOverall);

            armyData = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Militia, 0},
                {UnitType.Cavalry, 89},
                {UnitType.Infantry, 11},
                {UnitType.Ranged, 1205}
            });
            Assert.AreEqual(1305, armyData.amountOverall);
        }

        [Test]
        public void TestPart()
        {
            var armyData = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Militia, 25},
                {UnitType.Cavalry, 50},
                {UnitType.Infantry, 125},
                {UnitType.Ranged, 300}
            });
            Assert.AreEqual(0.05, armyData.GetPart(UnitType.Militia), 1e-5);
            Assert.AreEqual(0.1, armyData.GetPart(UnitType.Cavalry), 1e-5);
            Assert.AreEqual(0.25, armyData.GetPart(UnitType.Infantry), 1e-5);
            Assert.AreEqual(0.6, armyData.GetPart(UnitType.Ranged), 1e-5);

            armyData = new EnemyArmyData(new Dictionary<UnitType, int>
            {
                {UnitType.Militia, 0},
                {UnitType.Cavalry, 89},
                {UnitType.Infantry, 11},
                {UnitType.Ranged, 1205}
            });
            Assert.AreEqual(0, armyData.GetPart(UnitType.Militia), 1e-5);
            Assert.AreEqual(0.06819, armyData.GetPart(UnitType.Cavalry), 1e-5);
            Assert.AreEqual(0.00842, armyData.GetPart(UnitType.Infantry), 1e-5);
            Assert.AreEqual(0.92337, armyData.GetPart(UnitType.Ranged), 1e-5);
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
    }
}