using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using WebScraper.Interfaces;
using WebScraper.Models;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WebScraper.Scrapers
{
    public class PointsScraper : IPointsScraper
    {
        private readonly ILogger<PointsScraper> _log;
        private readonly IConfiguration _config;

        public PointsScraper(ILogger<PointsScraper> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }
        
        public async Task<List<Team>> Scrape()
        {
            var returnList = new List<Team>();
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(_config.GetValue<string>("LeaguesURLPath"));

            // Get list of all teams
            var teamList = new List<Team>();
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
            await using (var rdr = cmd.ExecuteReader())
            {
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
                    teamList.Add(team);
                    // System.Console.WriteLine(team.TeamName);
                }
            }

            var played = 0;
            var won = 0;
            var drawn = 0;
            var lost = 0;
            var gfor = 0;
            var gagainst = 0;
            var gd = 0;
            var points = 0;
            var rank = 0;

            // For each league table
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach (var league in leagueTeams)
            {
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach (var div in teamRow)
                {
                    // Console.WriteLine("THERE");
                    _log.LogInformation("There");
                    var currentTeam = "";
                    // Take only the teamname and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");
                    var i = 0;
                    foreach (var item in teamDetails)
                    {
                        if (i == 1)
                        {
                            currentTeam = item.TextContent;

                        }
                        i++;
                    }
                    var scoreDetails = div.QuerySelectorAll("th.no-border.text-center.scores");
                    var j = 0;
                    foreach (var item in scoreDetails)
                    {
                        if (j == 0)
                        {
                            rank = Int32.Parse(item.TextContent);
                        }
                        else if (j == 1)
                        {
                            // Played
                            played = Int32.Parse(item.TextContent);
                        }
                        else if (j == 2)
                        {
                            // Won
                            won = Int32.Parse(item.TextContent);
                        }
                        else if (j == 3)
                        {
                            // Drawn
                            drawn = Int32.Parse(item.TextContent);
                        }
                        else if (j == 4)
                        {
                            // Lost
                            lost = Int32.Parse(item.TextContent);
                        }
                        else if (j == 5)
                        {
                            // Goals For
                            gfor = Int32.Parse(item.TextContent);
                        }
                        else if (j == 6)
                        {
                            // Goals Against
                            gagainst = Int32.Parse(item.TextContent);
                        }
                        else if (j == 7)
                        {
                            // Goal Difference
                            gd = Int32.Parse(item.TextContent);
                        }
                        else if (j == 8)
                        {
                            // Points
                            points = Int32.Parse(item.TextContent);
                        }
                        j++;
                    }
                    // Update team with new points
                    var teamToUpdate = new Team
                    {
                        TeamName = currentTeam
                    };
                    if (teamList.Any(x => x.TeamName == teamToUpdate.TeamName))
                    {
                        //TODO: Simplify this code.
                        teamToUpdate.ID = teamList.FirstOrDefault(x => x.TeamName == teamToUpdate.TeamName).ID;
                        teamToUpdate.LeagueID = teamList.FirstOrDefault(x => x.ID == teamToUpdate.ID).LeagueID;
                        teamToUpdate.HockeyCategoryID = teamList.FirstOrDefault(x => x.ID == teamToUpdate.ID).HockeyCategoryID;
                        teamToUpdate.Sponsor =   teamList.FirstOrDefault(x => x.ID == teamToUpdate.ID).Sponsor;
                        teamToUpdate.LeagueRank = rank;
                        teamToUpdate.SeasonPlayed = played;
                        teamToUpdate.SeasonWon = won;
                        teamToUpdate.SeasonDrawn = drawn;
                        teamToUpdate.SeasonLost = lost;
                        teamToUpdate.SeasonGoalsFor = gfor;
                        teamToUpdate.SeasonGoalsAgainst = gagainst;
                        teamToUpdate.SeasonGoalDifference = gd;
                        teamToUpdate.SeasonPoints = points;
                        // System.Console.WriteLine(JsonConvert.SerializeObject(teamToUpdate));
                        // SavePointsSQL(teamToUpdate);
                        returnList.Add(teamToUpdate);
                    }
                }
            }
            
            return returnList;
        }
    }
}