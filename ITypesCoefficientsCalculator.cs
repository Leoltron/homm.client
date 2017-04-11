using HoMM;

namespace Homm.Client
{
    public interface ITypesCoefficientsCalculator
    {
        double GetDegreeOfNeed(UnitType unitType);
        double GetDegreeOfNeed(Resource resourceType);
        double GetCounterMeetingPropability(UnitType type);
    }
}