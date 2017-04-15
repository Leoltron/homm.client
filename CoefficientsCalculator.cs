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

        public static double GetTerrainCoefficient(Terrain terrain) => 0; //-Constants.CostOfMove[terrain] / 7;

        public double GetDwellingCoefficient(Dwelling dwelling)
        {
            var canHire = HireHelper.HowManyCanHire(dwelling, ai.CurrentData.MyTreasury);
            return dwelling != null && canHire >= 1
                ? Constants.OneScoreWeight
                : 0;
        }


        public double GetPileCoefficient(ResourcePile pile)
        {
            return pile == null
                ? 0
                : (HommRules.Current.ResourcesGainScores + pile.Amount * 
                GetDegreeOfNeed(pile.Resource)) * 
                Constants.OneScoreWeight;
        }

        private double GetDegreeOfNeed(Resource resource)
        {
            return ai.DataHandler.GetDegreeOfNeed(resource);
        }

        public double GetMineCoefficient(Mine mine, string me)
        {
            var result = mine == null
                ? 0
                : (HommRules.Current.MineOwningDailyScores +
                   GetDegreeOfNeed(mine.Resource) * HommRules.Current.MineDailyResourceYield) *
                  Constants.MineCoefficent;
            if (mine == null || mine.Owner == me)
                return 0;
            return mine.Owner != ai.CurrentData.MyRespawnSide ? result : 0;
        }
    }
}