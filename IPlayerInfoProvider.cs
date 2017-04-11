using System.Collections.Generic;
using HoMM;

namespace Homm.Client
{
    public interface IPlayerInfoProvider
    {
        bool IsDead { get; }
        Dictionary<Resource, int> MyTreasury { get; }
        Dictionary<UnitType, int> MyArmy { get; }
        string MyRespawnSide { get; }
    }
}