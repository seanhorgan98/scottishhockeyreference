using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class Team
    {
#nullable enable
        public int Id { get; set; }

        [Display(Name = "Team name")]
        public string? Teamname { get; set; }
        public int? League_ID { get; set; }
        public string? Sponsor { get; set; }
        public int Movement { get; set; }
        public int? Rating { get; set; }

        [Display(Name = "Drawn")]
        public int SeasonDrawn { get; set; }

        [Display(Name = "GD")]
        public int SeasonGoalDifference { get; set; }

        [Display(Name = "GA")]
        public int SeasonGoalsAgainst { get; set; }

        [Display(Name = "GF")]
        public int SeasonGoalsFor { get; set; }

        [Display(Name = "Lost")]
        public int SeasonLost { get; set; }

        [Display(Name = "Played")]
        public int SeasonPlayed { get; set; }

        [Display(Name = "Points")]
        public int SeasonPoints { get; set; }

        [Display(Name = "Won")]
        public int SeasonWon { get; set; }
        public int TotalDrawn { get; set; }
        public int TotalGoalDifference { get; set; }
        public int TotalGoalsAgainst { get; set; }
        public int TotalGoalsFor { get; set; }
        public int TotalLost { get; set; }
        public int TotalPlayed { get; set; }
        public int TotalPoints { get; set; }
        public int TotalWon { get; set; }
        public int? Hockey_Category_ID { get; set; }
        public int? League_Rank { get; set; }
    }
#nullable disable
}
