using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebScraper.Interfaces;
using WebScraper.Models;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WebScraper.Scrapers
{
    public class PointsScraper : IPointsScraper
    {
        private readonly ILogger<PointsScraper> _log;
        private readonly IConfiguration _config;
        private readonly IDbInteraction _db;

        public PointsScraper(ILogger<PointsScraper> log, IConfiguration config, IDbInteraction db)
        {
            _log = log;
            _config = config;
            _db = db;
        }
        
        public async Task<List<Team>> ScrapeAsync()
        {
            var returnList = new List<Team>();
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(_config.GetValue<string>("LeaguesURLPath"));

            // Get list of all teams
            var teamList = _db.GetAllTeams();

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
                if (league.PreviousElementSibling.Text().Contains("Super") || league.PreviousElementSibling.Text().Contains("Conference"))
                {
                    _log.LogInformation("League Skipped: Non-standard league: {League}", league.PreviousElementSibling.Text());
                    continue;
                }
                
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach (var div in teamRow)
                {
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
                        switch (j)
                        {
                            case 0:
                                rank = Int32.Parse(item.TextContent);
                                break;
                            case 1:
                                played = Int32.Parse(item.TextContent);
                                break;
                            case 2:
                                won = Int32.Parse(item.TextContent);
                                break;
                            case 3:
                                drawn = Int32.Parse(item.TextContent);
                                break;
                            case 4:
                                lost = Int32.Parse(item.TextContent);
                                break;
                            case 5:
                                gfor = Int32.Parse(item.TextContent);
                                break;
                            case 6:
                                gagainst = Int32.Parse(item.TextContent);
                                break;
                            case 7:
                                gd = Int32.Parse(item.TextContent);
                                break;
                            case 8:
                                points = Int32.Parse(item.TextContent);
                                break;
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
                        
                        // SavePointsSQL(teamToUpdate);
                        returnList.Add(teamToUpdate);
                    }
                }
            }
            
            return returnList;
        }
    }
}