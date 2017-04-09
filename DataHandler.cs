using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
    public class DataHandler : ILocationMapProvider
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
            enemyArmyData = EnemyArmyData.Parse(CurrentData);
            resourcesData = ResourcesData.Parse(CurrentData);
        }

        public double GetDegreeOfNeed(UnitType unitType)
        {
            return GetDegreeOfNeed(Constants.UnitToResource[unitType]);
        }

        public double GetDegreeOfNeed(Resource resourceType)
        {
            //TODO: Вынести в переменную, чтобы считалась лишь раз между двумя обновлениями данных?
            return GetCounterMeetingPropability(Constants.ResourceToUnit[resourceType]) *
                   Constants.ArmyEfficencyCoefficent
                   + resourcesData.GetRarity(resourceType) * Constants.ResourceRarityCoefficent;
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
    }
}