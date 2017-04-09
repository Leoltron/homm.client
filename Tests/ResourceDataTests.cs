using System;
using System.Collections.Generic;
using HoMM;
using NUnit.Framework;
using static Homm.Client.ResourcesData;

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
            Assert.AreEqual(135, resData.Total);

            resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 250},
                {Resource.Glass, 0},
                {Resource.Iron, 0},
                {Resource.Gold, 100}
            });
            Assert.AreEqual(350, resData.Total);
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
            Assert.AreEqual(27, resData.GetRarity(Resource.Ebony), 1e-5);
            Assert.AreEqual(6.75, resData.GetRarity(Resource.Glass), 1e-5);
            Assert.AreEqual(13.5, resData.GetRarity(Resource.Iron), 1e-5);
            Assert.AreEqual(1.35, resData.GetRarity(Resource.Gold), 1e-5);

            resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 250},
                {Resource.Glass, 0},
                {Resource.Iron, 0},
                {Resource.Gold, 100}
            });
            Assert.AreEqual(1.4, resData.GetRarity(Resource.Ebony), 1e-5);
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Glass), 1e-5);
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Iron), 1e-5);
            Assert.AreEqual(3.5, resData.GetRarity(Resource.Gold), 1e-5);
        }

        [Test]
        public void TestRarityMissingResources()
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, 5},
                {Resource.Glass, 20}
            });
            Assert.AreEqual(5, resData.GetRarity(Resource.Ebony), 1e-5);
            Assert.AreEqual(1.25, resData.GetRarity(Resource.Glass), 1e-5);
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Iron), 1e-5);
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Gold), 1e-5);
        }

        [Test]
        public void TestRarityNoResources()
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>());
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Ebony), 1e-5);
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Glass), 1e-5);
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Iron), 1e-5);
            Assert.AreEqual(MaxRarity, resData.GetRarity(Resource.Gold), 1e-5);
        }
    }
}