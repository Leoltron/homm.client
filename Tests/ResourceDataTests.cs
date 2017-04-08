using System;
using System.Collections.Generic;
using HoMM;
using NUnit.Framework;

// ReSharper disable UnusedVariable

namespace Homm.Client.Tests
{
    [TestFixture]
    internal class ResourceDataTests
    {
        [Test]
        public void TestCreate()
        {
            var resData1 = new ResourcesData(new Dictionary<Resource, int>());
            var resData2 = new ResourcesData(new Dictionary<Resource, int> {{Resource.Ebony, 5}, {Resource.Glass, 20}});
        }

        private static void CreateWithNegativeAmount()
        {
            var resData1 = new ResourcesData(new Dictionary<Resource, int> {{Resource.Iron, -10}});
        }

        [Test]
        public void TestInvalidResourceAmount()
        {
            Assert.Catch(typeof(ArgumentException), CreateWithNegativeAmount);
        }

        [Test]
        public void TestTotal()
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 5},
                {Resource.Glass, 20},
                {Resource.Iron, 10},
                {Resource.Gold, 100}
            });
            Assert.AreEqual(resData.total, 135);

            resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 250},
                {Resource.Glass, 0},
                {Resource.Iron, 0},
                {Resource.Gold, 100}
            });
            Assert.AreEqual(resData.total, 350);
        }

        [Test]
        public void TestRarity()
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 5},
                {Resource.Glass, 20},
                {Resource.Iron, 10},
                {Resource.Gold, 100}
            });
            Assert.AreEqual(resData.GetRarity(Resource.Ebony), 27, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Glass), 6.75, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Iron), 13.5, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Gold), 1.35, 1e-5);

            resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 250},
                {Resource.Glass, 0},
                {Resource.Iron, 0},
                {Resource.Gold, 100}
            });
            Assert.AreEqual(resData.GetRarity(Resource.Ebony), 1.4, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Glass), double.MaxValue, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Iron), double.MaxValue, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Gold), 3.5, 1e-5);
        }

        [Test]
        public void TestRarityMissingResources()
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 5},
                {Resource.Glass, 20}
            });
            Assert.AreEqual(resData.GetRarity(Resource.Ebony), 5, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Glass), 1.25, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Iron), double.MaxValue, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Gold), double.MaxValue, 1e-5);
        }

        [Test]
        public void TestRarityNoResources()
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>());
            Assert.AreEqual(resData.GetRarity(Resource.Ebony), double.MaxValue, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Glass), double.MaxValue, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Iron), double.MaxValue, 1e-5);
            Assert.AreEqual(resData.GetRarity(Resource.Gold), double.MaxValue, 1e-5);
        }
    }
}