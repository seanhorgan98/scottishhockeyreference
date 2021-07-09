using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using WebScraper.Interfaces;
using WebScraper.Models;
using static AngleSharp.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace WebScraper.Scrapers
{
    public class TeamScraper : ITeamScraper
    {
        private readonly ILogger<ScrapeLeagues> _log;
        private readonly IConfiguration _config;

        public TeamScraper(ILogger<ScrapeLeagues> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public async Task<List<Team>> Scrape()
        {
            var config = Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(LeagueUrl);

            // Get Leagues from Database
            var dbLeagueList = new List<League>();
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            const string sqlSelect = "SELECT * FROM Leagues";
            var cmd = new MySqlCommand(sqlSelect, conn);
            await using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    /* iterate once per row */
                    var league = new League { Id = rdr.GetInt32(0), Name = ((rdr.IsDBNull(1)) ? "" : rdr.GetString(1)), HockeyCategoryID = rdr.GetInt32(2)};
                    dbLeagueList.Add(league);
                }
            }


            //// var leagueResponse = await client.GetAsync("http://localhost:33988/api/Leagues");
            //var leagueResponse = await client.GetAsync("http://localhost:5000/api/Leagues");
            //leagueResponse.EnsureSuccessStatusCode();
            //var leagueResponseBody = await leagueResponse.Content.ReadAsStringAsync();
            //var leagueList = JsonConvert.DeserializeObject<List<League>>(leagueResponseBody);

            // Scrape all leagues
            var leagueList = new List<string>();
            var allLeagues = document.QuerySelectorAll("h2.text-uppercase");

            foreach (var item in allLeagues)
            {
                leagueList.Add(item.TextContent);
            }

            var index = 0;
            _ = document.QuerySelectorAll("div.tableWrap");

            // For each league table
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach (var league in leagueTeams)
            {
                if (leagueList[index].Contains("Conference") || leagueList[index].Contains("Super"))
                {
                    Console.WriteLine("Skipped non-standard league: " + leagueList[index]);
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
                    // Take only the teamname and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");

                    var i = 0;
                    foreach (var item in teamDetails)
                    {
                        if (i % 3 == 0)
                        {
                            i++;
                            continue;
                        }
                        else if (i % 3 == 1)
                        {
                            currentTeam = item.TextContent;

                        }
                        else if (i % 3 == 2)
                        {
                            currentSponsor = item.TextContent;

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
                    // Console.WriteLine(JsonConvert.SerializeObject(teamToPost));
                    GetLeagueIdAndCategoryByName(dbLeagueList, currentLeague, teamToPost);

                    SaveTeamSql(teamToPost);
                    rank++;
                }
                index++;
            }
            
            
        }
    }
}