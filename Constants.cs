using System;
using System.Collections.Generic;
using System.Linq;
using HoMM;

namespace Homm.Client
{
    public static class Constants
    {
        public const double GoldMilitiaCounterConst = 1d; //TODO: Настроить константы
        public const double MineCoefficent = 5817.815439391435;
        public const double ResourceRarityCoefficent = 10 / 1e3;
        public const double ArmyEfficencyCoefficent = 10 / 1e3;
        public const double OneScoreWeight = 85222.69299239293;
        public const double BattleCoefficient = 5682;
        public const double DecreaseByLevel = 8;

        public static readonly Dictionary<UnitType, UnitType> UnitCounters = new Dictionary<UnitType, UnitType>
        {
            {UnitType.Infantry, UnitType.Cavalry},
            {UnitType.Cavalry, UnitType.Ranged},
            {UnitType.Ranged, UnitType.Infantry}
        };

        public static readonly Dictionary<UnitType, Resource> UnitToResource = new Dictionary<UnitType, Resource>
        {
            {UnitType.Militia, Resource.Gold},
            {UnitType.Cavalry, Resource.Ebony},
            {UnitType.Ranged, Resource.Glass},
            {UnitType.Infantry, Resource.Iron}
        };

        public static readonly Dictionary<Resource, UnitType> ResourceToUnit = new Dictionary<Resource, UnitType>
        {
            {Resource.Gold, UnitType.Militia},
            {Resource.Ebony, UnitType.Cavalry},
            {Resource.Glass, UnitType.Ranged},
            {Resource.Iron, UnitType.Infantry}
        };

        public static readonly Direction[] Directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToArray();
    }
}
