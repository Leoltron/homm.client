using System.Collections.Generic;
using HoMM;

namespace Homm.Client.Tests
{
    internal class DummyPlayerInfo : IPlayerInfoProvider
    {
        public bool IsDead { get; private set; }
        public Dictionary<Resource, int> MyTreasury { get; private set; }
        public Dictionary<UnitType, int> MyArmy { get; private set; }
        public string MyRespawnSide { get; private set; }

        public static DummyPlayerInfo GetExampleInfo()
        {
            return new DummyPlayerInfo
            {
                IsDead = false,
                MyTreasury = new Dictionary<Resource, int>
                {
                    {Resource.Gold, 87}
                },
                MyArmy = new Dictionary<UnitType, int>
                {
                    {UnitType.Militia, 100},
                    {UnitType.Cavalry, 1},
                    {UnitType.Infantry, 1},
                    {UnitType.Ranged, 1}
                },
                MyRespawnSide = "Name???"
            };
        }
    }
}