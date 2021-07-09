namespace WebScraper.Models
{
    public class Team
    {
        public int ID { get; set; }
        public string TeamName { get; set; }
        public int LeagueID { get; set; }
        public string Sponsor { get; set; }
        public int HockeyCategoryID { get; set; }
        public int LeagueRank { get; set; }
        public int SeasonDrawn { get; set; }
        public int SeasonGoalDifference { get; set; }
        public int SeasonGoalsAgainst { get; set; }
        public int SeasonGoalsFor { get; set; }
        public int SeasonLost { get; set; }
        public int SeasonPlayed { get; set; }
        public int SeasonPoints { get; set; }
        public int SeasonWon { get; set; }

        public Team()
        {
        }
    }
}