using System;

namespace WebScraper.Scrapers
{
    public class Elo
    {
        public static (int, int) CalculateElo(int teamOneRating, int scoreOne, int teamTwoRating, int scoreTwo, int league, int category)
        {
            // Variables
            double K = 24;
            var scoreDifference = Math.Abs(scoreOne - scoreTwo);

            if (scoreDifference == 2)
            {
                K += K * 0.5;
            }else if (scoreDifference == 3)
            {
                K += K * 0.75;
            }else if (scoreDifference > 3){
                if (category < 3)
                {
                    K += K * 0.75 + (scoreDifference-3)/8D;
                }
            }
            var denominator = 400;
            float W = 1;              // w is the margin of victory weighting
            float Sa, Sb;
            if (scoreOne > scoreTwo)
            {
                Sa = 1.0f;
                Sb = 0.0f;
            }
            else if (scoreTwo > scoreOne)
            {
                Sa = 0.0f;
                Sb = 1.0f;
            }else
            {
                Sa = 0.5f;
                Sb = 0.5f;
            }
            double ratingDifferenceA = teamTwoRating - teamOneRating;
            double ratingDifferenceB = teamOneRating - teamTwoRating;
            double expoA = ratingDifferenceA/denominator;
            double expoB = ratingDifferenceB/denominator;

            // Expected probability
            var ea = 1 / (1 + Math.Pow(10D, expoA));
            var eb = 1 / (1 + Math.Pow(10D, expoB));

            // New Elo calculations
            var teamOneEloChange = Convert.ToInt32(Math.Round(K * W * (Sa - ea)));
            var teamTwoEloChange = Convert.ToInt32(Math.Round(K * W * (Sb - eb)));
            var teamOneNewElo = teamOneRating + teamOneEloChange;
            var teamTwoNewElo = teamTwoRating + teamTwoEloChange;

            // var EloChange = Math.Abs(Math.Round(K * W * (Sb - Eb)));

            // Absolute Floor
            if (teamOneNewElo < 100)
            {
                teamOneNewElo = teamOneRating;
            }
            if (teamTwoNewElo < 100)
            {
                teamTwoNewElo = teamTwoRating;
            }

            return (teamOneEloChange, teamTwoEloChange);
        }
    }
}