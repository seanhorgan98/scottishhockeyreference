namespace Server.Models
{
    public class Team
    {
        public long Id { get; set; }
        public string Teamname { get; set; }
        public int League_ID { get; set; }
        public string Sponsor { get; set; }
        public int Movement { get; set; }
        public int Rating { get; set; }
        public int SeasonDrawn { get; set; }
        public int SeasonGoalDifference { get; set; }
        public int SeasonGoalsAgainst { get; set; }
        public int SeasonGoalsFor { get; set; }
        public int SeasonLost { get; set; }
        public int SeasonPlayed { get; set; }
        public int SeasonPoints { get; set; }
        public int SeasonWon { get; set; }
        public int TotalDrawn { get; set; }
        public int TotalGoalDifference { get; set; }
        public int TotalGoalsAgainst { get; set; }
        public int TotalGoalsFor { get; set; }
        public int TotalLost { get; set; }
        public int TotalPlayed { get; set; }
        public int TotalPoints { get; set; }
        public int TotalWon { get; set; }
        public int Hockey_Category_ID { get; set; }
        public int League_Rank { get; set; }


    }
}
