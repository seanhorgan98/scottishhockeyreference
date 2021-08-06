using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper
{
    public class DbInteraction : IDbInteraction
    {
        private readonly ILogger<DbInteraction> _log;
        private readonly IConfiguration _config;

        public DbInteraction(ILogger<DbInteraction> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public void UpdatePoints(Team teamToUpdate)
        {
            _log.LogTrace("Updating points for team: {Name}, ID: {Id}", teamToUpdate.TeamName, teamToUpdate.ID);
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Teams
SET League_Rank = @LEAGUE_RANK,
    SeasonPlayed = @PLAYED,
    SeasonWon = @WON,
    SeasonDrawn = @DRAWN,
    SeasonLost = @LOST,
    SeasonGoalsFor = @GFOR,
    SeasonGoalsAgainst = @GAGAINST,
    SeasonGoalDifference = @GDIFFERENCE,
    SeasonPoints = @POINTS
WHERE ID = @ID;";
            cmd.Parameters.AddWithValue("@LEAGUE_RANK", teamToUpdate.LeagueRank);
            cmd.Parameters.AddWithValue("@PLAYED", teamToUpdate.SeasonPlayed);
            cmd.Parameters.AddWithValue("@WON", teamToUpdate.SeasonWon);
            cmd.Parameters.AddWithValue("@DRAWN", teamToUpdate.SeasonDrawn);
            cmd.Parameters.AddWithValue("@LOST", teamToUpdate.SeasonLost);
            cmd.Parameters.AddWithValue("@GFOR", teamToUpdate.SeasonGoalsFor);
            cmd.Parameters.AddWithValue("@GAGAINST", teamToUpdate.SeasonGoalsAgainst);
            cmd.Parameters.AddWithValue("@GDIFFERENCE", teamToUpdate.SeasonGoalDifference);
            cmd.Parameters.AddWithValue("@POINTS", teamToUpdate.SeasonPoints);
            cmd.Parameters.AddWithValue("@ID", teamToUpdate.ID);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void UpdateEloRating(int teamID, int eloChange)
        {
            _log.LogTrace("Updating Elo for team: {Id}, Value: {Elo}", teamID, eloChange);
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Teams
SET Movement=@ELOCHANGE,
    Rating = Rating + @ELOCHANGE
WHERE Id = @TEAMID";
            cmd.Parameters.AddWithValue("@ELOCHANGE", eloChange);
            cmd.Parameters.AddWithValue("@TEAMID", teamID);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void InsertFixture(DateTime date, int league, int teamOne, int teamTwo, int teamOneScore,
            int teamTwoScore, string location, int eloOne, int category, int eloTwo)
        {
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Fixtures
    (Date,
    Team_1_ID,
    Team_2_ID,
    Location,
    Season_ID,
    League_ID,
    Team_1_Score,
    Team_2_Score,
    Team_1_Elo_Change,
    Hockey_Category_ID,
    Team_2_Elo_Change)
VALUES
	(@MRDATE,
    @TEAMONE,
    @TEAMTWO,
    @LOCATION,
    @SEASON,
    @LEAGUE,
    @SCOREONE,
    @SCORETWO,
    @ELOONE,
    @CATEGORY,
    @ELOTWO);";
            cmd.Parameters.AddWithValue("@MRDATE", date);
            cmd.Parameters.AddWithValue("@TEAMONE", teamOne);
            cmd.Parameters.AddWithValue("@TEAMTWO", teamTwo);
            cmd.Parameters.AddWithValue("@LOCATION", location);
            cmd.Parameters.AddWithValue("@SEASON", _config.GetValue<string>("SeasonID"));
            cmd.Parameters.AddWithValue("@LEAGUE", league);
            cmd.Parameters.AddWithValue("@SCOREONE", teamOneScore);
            cmd.Parameters.AddWithValue("@SCORETWO", teamTwoScore);
            cmd.Parameters.AddWithValue("@ELOONE", eloOne);
            cmd.Parameters.AddWithValue("@CATEGORY", category);
            cmd.Parameters.AddWithValue("@ELOTWO", eloTwo);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public DateTime GetMostRecentDate()
        {
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var mrDate = new DateTime();
            const string sqlSelect = "SELECT * FROM Most_Recent_Date;";
            var cmd = new MySqlCommand(sqlSelect, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                mrDate = rdr.GetDateTime(0);
            }

            return mrDate;
        }

        public void UpdateMostRecentDate(DateTime mrDate)
        {
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Scraper SET MostRecentDay = @MRDATE WHERE ID = 1";
            cmd.Parameters.AddWithValue("@MRDATE", mrDate);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        
        public int GetTeamRating(int teamID)
        {
            var rating = 0;
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Rating FROM Teams WHERE ID = @TEAMID;";
            cmd.Parameters.AddWithValue("@TEAMID", teamID);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) {
                rating = rdr.GetInt32(0);
            }

            return rating;
        }
        
        public void InsertTeam(Team teamToPost)
        {
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Teams (TeamName, League_ID, Sponsor, League_Rank, Hockey_Category_ID)
VALUES (@TEAMNAME, @LEAGUE_ID, @SPONSOR, @LEAGUE_RANK, @CATEGORY)";
            cmd.Parameters.AddWithValue("@TEAMNAME", teamToPost.TeamName);
            cmd.Parameters.AddWithValue("@LEAGUE_ID", teamToPost.LeagueID);
            cmd.Parameters.AddWithValue("@SPONSOR", teamToPost.Sponsor);
            cmd.Parameters.AddWithValue("@LEAGUE_RANK", teamToPost.LeagueRank);
            cmd.Parameters.AddWithValue("@CATEGORY", teamToPost.HockeyCategoryID);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        
        public void InsertLeague(string name, int category)
        {
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Leagues (Name, Hockey_Category_ID) VALUES (@NAME, @CATEGORY)";
            cmd.Parameters.AddWithValue("@NAME", name);
            cmd.Parameters.AddWithValue("@CATEGORY", category);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public List<League> GetAllLeagues()
        {
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            var leagueList = new List<League>();
            var sqlSelect = "SELECT * FROM Leagues;";
            var cmd = new MySqlCommand(sqlSelect, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) {
                /* iterate once per row */
                var league = new League
                {
                    Id = rdr.GetInt32(0), Name = rdr.GetString(1), HockeyCategoryID = rdr.GetInt32(2)
                };
                leagueList.Add(league);
            }

            return leagueList;
        }

        public List<Team> GetAllTeams()
        {
            var returnList = new List<Team>();
            var conn = new MySqlConnection(_config.GetValue<string>("ConnectionString"));
            conn.Open();
            const string sqlSelect = @"SELECT ID,
    League_ID,
    Hockey_Category_ID,
    Sponsor,
    League_Rank,
    SeasonPlayed,
    SeasonWon,
    SeasonDrawn,
    SeasonLost,
    SeasonGoalsFor,
    SeasonGoalsAgainst,
    SeasonGoalDifference,
    SeasonPoints,
    Teamname FROM Teams; ";
            var cmd = new MySqlCommand(sqlSelect, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                /* iterate once per row */
                var team = new Team
                {
                    TeamName = (rdr.IsDBNull(13)) ? "" : rdr.GetString(13),
                    ID = rdr.GetInt32(0),
                    LeagueID = rdr.GetInt32(1),
                    HockeyCategoryID = rdr.GetInt32(2),
                    Sponsor = (rdr.IsDBNull(3)) ? "" : rdr.GetString(3),
                    LeagueRank = rdr.GetInt32(4),
                    SeasonPlayed = rdr.GetInt32(5),
                    SeasonWon = rdr.GetInt32(6),
                    SeasonDrawn = rdr.GetInt32(7),
                    SeasonLost = rdr.GetInt32(8),
                    SeasonGoalsFor = rdr.GetInt32(9),
                    SeasonGoalsAgainst = rdr.GetInt32(10),
                    SeasonGoalDifference = rdr.GetInt32(11),
                    SeasonPoints = rdr.GetInt32(12)
                };
                returnList.Add(team);
            }

            return returnList;
        }
        
    }
}