using System.Collections.Generic;
using HoMM;

namespace Homm.Client
{
    public interface IPlayerInfoProvider
    {
        bool IsDead();
        Dictionary<Resource, int> GetMyTreasury();
        Dictionary<UnitType, int> GetMyArmy();
        string GetMyRespawnSide();
    }
}