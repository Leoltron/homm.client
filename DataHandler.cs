using System.Collections.Generic;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class DataHandler : ILocationMapProvider, IPlayerInfoProvider
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
            //TODO: Вынести в переменную, чтобы считалась лишь раз между двумя обновлениями данных?
            var coeff = GetCounterMeetingPropability(Constants.ResourceToUnit[resourceType]) *
                   Constants.ArmyEfficencyCoefficent
                   + resourcesData.GetRarity(resourceType) * Constants.ResourceRarityCoefficent;
            return resourceType == Resource.Gold ? 2 * coeff : coeff;
        }

        private double GetCounterMeetingPropability(UnitType type)
        {
            return type == UnitType.Militia
                ? Constants.GoldMilitiaCounterConst
                : enemyArmyData.GetPart(Constants.UnitCounters[type]);
        }

        public MapData GetMap()
        {
            return CurrentData.Map;
        }

        public Location GetCurrentLocation()
        {
            return CurrentData.Location.ToLocation();
        }

        public bool IsDead()
        {
            return CurrentData.IsDead;
        }

        public Dictionary<Resource, int> GetMyTreasury()
        {
            return CurrentData.MyTreasury;
        }

        public Dictionary<UnitType, int> GetMyArmy()
        {
            return CurrentData.MyArmy;
        }

        public string GetMyRespawnSide()
        {
            return CurrentData.MyRespawnSide;
        }
    }
}