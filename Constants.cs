﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public static class Constants
    {
        public const double GoldMilitiaCounterConst = 1d; //TODO: Настроить константы
        public const double MineCoefficent = 8e-2;
        public const double ResourceRarityCoefficent = 10;
        public const double ArmyEfficencyCoefficent = 10;
        public const int Radius = 10;

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

        public static readonly Dictionary<Terrain, double> CostOfMove = new Dictionary<Terrain, double>
        {
            {Terrain.Desert, TileTerrain.Desert.TravelCost},
            {Terrain.Grass, TileTerrain.Grass.TravelCost},
            {Terrain.Marsh, TileTerrain.Marsh.TravelCost},
            {Terrain.Road, TileTerrain.Road.TravelCost},
            {Terrain.Snow, TileTerrain.Snow.TravelCost}
        };

        public static readonly Direction[] Directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToArray();
    }
}