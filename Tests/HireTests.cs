using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;
using NUnit.Framework;
using Dwelling = HoMM.ClientClasses.Dwelling;

namespace Homm.Client.Tests
{
    [TestFixture]
    class HireTests
    {
        [Test]
        public void TestHire()
        {
            var dwelling = new Dwelling(UnitType.Cavalry, 20, "");
            var resources = new Dictionary<Resource, int>();
            foreach (var key in UnitsConstants.Current.UnitCost[UnitType.Cavalry].Keys)
                resources[key] = UnitsConstants.Current.UnitCost[UnitType.Cavalry][key] * 5;
            Assert.AreEqual(5, HireHelper.HowManyCanHire(dwelling, resources));
        }

        [Test]
        public void TestNoResources()
        {
            var dwelling = new Dwelling(UnitType.Cavalry, 20, "");
            var resources = new Dictionary<Resource, int>();
            Assert.AreEqual(0, HireHelper.HowManyCanHire(dwelling, resources));
        }

        [Test]
        public void TestHireTooMuch()
        {
            var dwelling = new Dwelling(UnitType.Cavalry, 20, "");
            var resources = new Dictionary<Resource, int>();
            foreach (var key in UnitsConstants.Current.UnitCost[UnitType.Cavalry].Keys)
                resources[key] = UnitsConstants.Current.UnitCost[UnitType.Cavalry][key] * 100;
            Assert.AreEqual(20, HireHelper.HowManyCanHire(dwelling, resources));
        }

        [Test]
        public void TestHireEmptyDwelling()
        {
            var dwelling = new Dwelling(UnitType.Cavalry, 0, "");
            var resources = new Dictionary<Resource, int>();
            foreach (var key in UnitsConstants.Current.UnitCost[UnitType.Cavalry].Keys)
                resources[key] = UnitsConstants.Current.UnitCost[UnitType.Cavalry][key] * 100;
            Assert.AreEqual(0, HireHelper.HowManyCanHire(dwelling, resources));
        }

        [Test]
        public void TestWrongResources()
        {
            var dwelling = new Dwelling(UnitType.Cavalry, 20, "");
            var resources = new Dictionary<Resource, int>();
            foreach (var key in UnitsConstants.Current.UnitCost[UnitType.Militia].Keys)
                resources[key] = UnitsConstants.Current.UnitCost[UnitType.Militia][key] * 5;
            Assert.AreEqual(0, HireHelper.HowManyCanHire(dwelling, resources));
        }
    }
}