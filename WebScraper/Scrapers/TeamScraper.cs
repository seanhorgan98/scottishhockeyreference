using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebScraper.Interfaces;
using WebScraper.Models;
using static AngleSharp.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WebScraper.Scrapers
{
    public class TeamScraper : ITeamScraper
    {
        private readonly ILogger<TeamScraper> _log;
        private readonly IConfiguration _config;
        private readonly IDbInteraction _db;

        public TeamScraper(ILogger<TeamScraper> log, IConfiguration config, IDbInteraction db)
        {
            _log = log;
            _config = config;
            _db = db;
        }

        public async Task<List<Team>> ScrapeAsync()
        {
            _log.LogInformation("Entering Team Scraper");
            var config = Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(_config.GetValue<string>("LeaguesURLPath"));

            var returnList = new List<Team>();

            // Get Leagues from Database
            var dbLeagueList = _db.GetAllLeagues();

            // Scrape all leagues
            var allLeagues = document.QuerySelectorAll("h2.text-uppercase");

            var leagueList = allLeagues.Select(item => item.TextContent).ToList();

            var index = 0;
            _ = document.QuerySelectorAll("div.tableWrap");

            // For each league table
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach (var league in leagueTeams)
            {
                if (leagueList[index].Contains("Conference") || leagueList[index].Contains("Super"))
                {
                    _log.LogInformation("Skipped non-standard league: {SkippedLeague}", leagueList[index]);
                    index++;
                    continue;
                }
                var rank = 1;
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach (var div in teamRow)
                {
                    var currentLeague = leagueList[index];
                    var currentTeam = "";
                    var currentSponsor = "";
                    // Take only the team name and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");

                    var i = 0;
                    foreach (var item in teamDetails)
                    {
                        switch (i % 3)
                        {
                            case 0:
                                i++;
                                continue;
                            case 1:
                                currentTeam = item.TextContent;
                                break;
                            case 2:
                                currentSponsor = item.TextContent;
                                break;
                        }

                        i++;
                    }
                    // Get Category
                    var teamToPost = new Team
                    {
                        TeamName = currentTeam,
                        Sponsor = currentSponsor,
                        LeagueRank = rank
                    };
                    GetLeagueIdAndCategoryByName(dbLeagueList, currentLeague, teamToPost);
                    
                    returnList.Add(teamToPost);
                    rank++;
                }
                index++;
            }

            return returnList;
        }
        
        private static void GetLeagueIdAndCategoryByName(IEnumerable<League> leagueList, string currentLeague, Team team)
        {
            foreach (var league in leagueList)
            {
                if (!league.Name.Equals(currentLeague)) continue;
                team.LeagueID = league.Id;
                team.HockeyCategoryID = league.HockeyCategoryID;
            }
        }
    }
}