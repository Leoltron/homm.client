using HoMM;
using HoMM.ClientClasses;
using Dwelling = HoMM.ClientClasses.Dwelling;
using Mine = HoMM.ClientClasses.Mine;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

namespace Homm.Client
{
    public class CoefficientsCalculator
    {
        private readonly AI ai;

        public CoefficientsCalculator(AI ai)
        {
            this.ai = ai;
        }

        public static double GetTerrainValue(Terrain terrain) => 0; //-Constants.CostOfMove[terrain] / 7;

        public static double GetDwellingValue(Dwelling dwelling, string me) => dwelling != null && dwelling.Owner != me
            ? Constants.OneScoreWeight
            : 0;



        public double GetPileValue(ResourcePile pile)
        {
            return pile == null
                ? 0
                : (HommRules.Current.ResourcesGainScores + pile.Amount * GetDegreeOfNeed(pile.Resource)) * Constants.OneScoreWeight;
        }

        private double GetDegreeOfNeed(Resource resource)
        {
            return ai.DataHandler.GetDegreeOfNeed(resource);
        }

        public double GetMineValue(Mine mine)
        {
            var result = mine == null
                ? 0
                : (HommRules.Current.MineOwningDailyScores +
                   GetDegreeOfNeed(mine.Resource) * HommRules.Current.MineDailyResourceYield) *
                  Constants.MineCoefficent;
            if (mine == null)
                return 0;
            return mine.Owner != ai.CurrentData.MyRespawnSide ? result : 0;
        }
    }
}