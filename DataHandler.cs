using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class DataHandler : ILocationMapProvider, IPlayerInfoProvider, ITypesCoefficientsCalculator
    {
        public HommSensorData CurrentData;
        private EnemyArmyData enemyArmyData;
        private ResourcesData resourcesData;

        public DataHandler(HommSensorData initialData)
        {
            UpdateData(initialData);
        }

        public void UpdateData(HommSensorData data)
        {
            CurrentData = data;
            enemyArmyData = EnemyArmyData.Parse(this, this);
            resourcesData = ResourcesData.Parse(this, this);
        }

        public double GetDegreeOfNeed(UnitType unitType)
        {
            return GetDegreeOfNeed(Constants.UnitToResource[unitType]);
        }

        public double GetDegreeOfNeed(Resource resourceType)
        {
            var coeff = GetCounterMeetingPropability(Constants.ResourceToUnit[resourceType]) *
                   Constants.ArmyEfficencyCoefficent
                   + resourcesData.GetRarity(resourceType) * Constants.ResourceRarityCoefficent;
            return resourceType == Resource.Gold ? 2 * coeff : coeff;
        }

        public double GetCounterMeetingPropability(UnitType type)
        {
            return type == UnitType.Militia
                ? Constants.GoldMilitiaCounterConst
                : enemyArmyData.GetPart(Constants.UnitCounters[type]);
        }

        public MapData Map => CurrentData.Map;

        public Location CurrentLocation => CurrentData.Location.ToLocation();

        public bool IsDead => CurrentData.IsDead;

        public Dictionary<Resource, int> MyTreasury => CurrentData.MyTreasury;

        public Dictionary<UnitType, int> MyArmy => CurrentData.MyArmy;

        public string MyRespawnSide => CurrentData.MyRespawnSide;
    }
}