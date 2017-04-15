using System;
using System.Collections.Generic;
using HoMM;
using NUnit.Framework;
using static Homm.Client.ResourcesData;
using Mine = HoMM.ClientClasses.Mine;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

// ReSharper disable UnusedVariable

namespace Homm.Client.Tests
{
    [TestFixture]
    internal class ResourcesDataTests
    {
        [Test]
        public void TestCreate()
        {
            var resData1 = new ResourcesData(new Dictionary<Resource, int>());
            var resData2 = new ResourcesData(new Dictionary<Resource, int> {{Resource.Ebony, 5}, {Resource.Glass, 20}});
        }

        [Test]
        public void TestInvalidResourceAmount()
        {
            Assert.Catch(typeof(ArgumentException), CreateDataWithNegatveAmount);
        }

        private static void CreateDataWithNegatveAmount()
        {
            var resData1 = new ResourcesData(new Dictionary<Resource, int> {{Resource.Iron, -10}});
        }

        [TestCase(5, 20, 10, 100)]
        [TestCase(250, 0, 0, 100)]
        public void TestTotal(int ebonyAmount, int glassAmount, int ironAmount, int goldAmount)
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, ebonyAmount},
                {Resource.Glass, glassAmount},
                {Resource.Iron, ironAmount},
                {Resource.Gold, goldAmount}
            });
            Assert.AreEqual(ebonyAmount + glassAmount + ironAmount + goldAmount, resData.Total);
        }

        [TestCase(5, 20, 10, 100,
            27,6.75,13.5,1.35)]
        [TestCase(250, 0, 0, 100,
            1.4,MaxRarity,MaxRarity,3.5)]
        public void TestRarity(
            int ebonyAmount, int glassAmount, int ironAmount, int goldAmount,
            double ebonyRarity, double glassRarity, double ironRarity, double goldRarity)
        {
            var resData = new ResourcesData(new Dictionary<Resource, int>
            {
                {Resource.Ebony, ebonyAmount},
                {Resource.Glass, glassAmount},
                {Resource.Iron, ironAmount},
                {Resource.Gold, goldAmount}
            });
            Assert.AreEqual(ebonyRarity, resData.GetRarity(Resource.Ebony), 1e-5);
            Assert.AreEqual(glassRarity, resData.GetRarity(Resource.Glass), 1e-5);
            Assert.AreEqual(ironRarity, resData.GetRarity(Resource.Iron), 1e-5);
            Assert.AreEqual(goldRarity, resData.GetRarity(Resource.Gold), 1e-5);
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

        [Test]
        public void TestParse()
        {
            var map = DummyLocationMap.GetExampleMap();
            var info = DummyPlayerInfo.GetExampleInfo();
            map.Map.Objects[1].ResourcePile = new ResourcePile(Resource.Gold, 10);
            map.Map.Objects[2].Mine = new Mine(Resource.Iron, "OwnerDoesntMatter");
            var data = Parse(info, map);
            Assert.IsTrue(data.Total > 0);
        }
    }
}