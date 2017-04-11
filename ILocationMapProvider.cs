using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public interface ILocationMapProvider
    {
        MapData Map { get; }
        Location CurrentLocation { get; }
    }
}