using HoMM;
using HoMM.ClientClasses;
using NUnit.Framework;

namespace Homm.Client.Tests
{
    [TestFixture]
    internal class DataHandlerTests
    {
        private static HommSensorData GetExampleData()
        {
            return GetExampleData(DummyPlayerInfo.GetExampleInfo(), DummyLocationMap.GetExampleMap());
        }

        private static HommSensorData GetExampleData(IPlayerInfoProvider playerData, ILocationMapProvider mapData)
        {
            return new HommSensorData
            {
                IsDead = playerData.IsDead,
                MyArmy = playerData.MyArmy,
                MyRespawnSide = playerData.MyRespawnSide,
                MyTreasury = playerData.MyTreasury,
                Map = mapData.Map,
                Location = mapData.CurrentLocation.ToLocationInfo(),
                WorldCurrentTime = 0
            };
        }

        [Test]
        public void TestCreate()
        {
            var playerData = DummyPlayerInfo.GetExampleInfo();
            var mapData = DummyLocationMap.GetExampleMap();
            var data = new DataHandler(GetExampleData(playerData,mapData));
            Assert.AreEqual(data.IsDead,playerData.IsDead);
            Assert.AreEqual(data.MyArmy,playerData.MyArmy);
            Assert.AreEqual(data.MyRespawnSide, playerData.MyRespawnSide);
            Assert.AreEqual(data.MyTreasury, playerData.MyTreasury);
            Assert.AreEqual(data.Map, mapData.Map);
            Assert.AreEqual(data.CurrentLocation, mapData.CurrentLocation);
        }

        [TestCase(UnitType.Militia, Constants.GoldMilitiaCounterConst)]
        [TestCase(UnitType.Infantry, 0)]
        [TestCase(UnitType.Ranged, 0)]
        [TestCase(UnitType.Cavalry, 0)]
        public void TestCounterMeetingPropability(UnitType type, double result)
        {
            var data = new DataHandler(GetExampleData());
            Assert.AreEqual(data.GetCounterMeetingPropability(type), result);
        }

        [Test]
        public void TestDegreeOfNeed()
        {
            var data = new DataHandler(GetExampleData());
            Assert.IsTrue(data.GetDegreeOfNeed(UnitType.Militia) > 0);
            Assert.IsTrue(data.GetDegreeOfNeed(UnitType.Infantry) > 0);
            Assert.IsTrue(data.GetDegreeOfNeed(UnitType.Ranged) > 0);
            Assert.IsTrue(data.GetDegreeOfNeed(UnitType.Cavalry) > 0);
        }
    }
}