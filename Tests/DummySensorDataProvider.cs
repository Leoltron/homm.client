using System.Collections.Generic;
using HoMM;

namespace Homm.Client.Tests
{
    internal class DummyPlayerInfo : IPlayerInfoProvider
    {
        public bool IsPlayerDead;
        public Dictionary<Resource, int> MyTreasury;
        public Dictionary<UnitType, int> MyArmy;
        public string MyRespawnSide;

        public bool IsDead()
        {
            return IsPlayerDead;
        }

        public Dictionary<Resource, int> GetMyTreasury()
        {
            return MyTreasury;
        }

        public Dictionary<UnitType, int> GetMyArmy()
        {
            return MyArmy;
        }

        public string GetMyRespawnSide()
        {
            return MyRespawnSide;
        }

        public static DummyPlayerInfo GetExampleInfo()
        {
            return new DummyPlayerInfo
            {
                IsPlayerDead = false,
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