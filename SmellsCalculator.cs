using HoMM;
using HoMM.ClientClasses;
using Dwelling = HoMM.ClientClasses.Dwelling;
using Mine = HoMM.ClientClasses.Mine;
using ResourcePile = HoMM.ClientClasses.ResourcePile;

namespace Homm.Client
{
    public class SmellsCalculator
    {
        private readonly AI ai;

        public SmellsCalculator(AI ai)
        {
            this.ai = ai;
        }

        public static double GetTerrainSmell() => 0;

        public double GetDwellingSmell(Dwelling dwelling)
        {
            var canHire = HireHelper.HowManyCanHire(dwelling, ai.CurrentData.MyTreasury);
            return dwelling != null && canHire >= 1
                ? Constants.OneScoreCoefficient
                : 0;
        }


        public double GetPileSmell(ResourcePile pile)
        {
            return pile == null
                ? 0
                : (HommRules.Current.ResourcesGainScores 
                    + pile.Amount * GetDegreeOfNeed(pile.Resource)) 
                  * Constants.OneScoreCoefficient;
        }

        private double GetDegreeOfNeed(Resource resource) => ai.DataHandler.GetDegreeOfNeed(resource);

        public double GetMineSmell(Mine mine)
        {
            return mine == null || mine.Owner == ai.CurrentData.MyRespawnSide
                ? 0
                : (HommRules.Current.MineOwningDailyScores +
                   GetDegreeOfNeed(mine.Resource) * HommRules.Current.MineDailyResourceYield) *
                  Constants.MineCoefficent;
        }
    }
}