using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public interface ILocationMapProvider
    {
        MapData GetMap();
        Location GetCurrentLocation();
    }
}