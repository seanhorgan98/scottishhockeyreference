using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using WebScraper.Interfaces;
using WebScraper.Models;
using static WebScraper.Scrapers.Elo;

namespace WebScraper.Scrapers
{
    using c = Microsoft.Extensions.Configuration;
    
    /*
     * Scraper for pulling in fixtures from a league page file.
     */
    public class FileResultScraper : IResultScraper
    {
        private const int LeagueNumber = 5;
        private const string HtmlFile = "";
        private readonly ILogger<FileResultScraper> _log;
        private readonly c.IConfiguration _config;
        private readonly IDbInteraction _db;

        public FileResultScraper(ILogger<FileResultScraper> log, c.IConfiguration config, IDbInteraction db)
        {
            _log = log;
            _config = config;
            _db = db;
        }

        public async Task<List<Fixture>> ScrapeAsync()
        {
            // Get List of Leagues
            var leagueList = _db.GetAllLeagues();
            

            // Get List of Teams
            var teamList = _db.GetAllTeams();

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var htmlText = await System.IO.File.ReadAllTextAsync(HtmlFile);
            //var document = await context.OpenAsync(resultsURL);
            var document = await context.OpenAsync(req => req.Content(htmlText));
            var tableWrap = document.All.Where(m => m.LocalName == "tr");
            DateTime currentDate = DateTime.MinValue;
            var mostRecentDate = _db.GetMostRecentDate();
            //var topDate = DateTime.Parse(tableWrap.First().Text());
            var fixtureList = new List<Fixture>();

            foreach (var row in tableWrap)
            {
                //System.Console.WriteLine(row.Text() + ", out of " + tableWrap.ToArray().Length);
                // If row is a date and not umpires
                if (row.Children.Length == 1 && (!row.Text().Contains("Umpires") && !row.Text().Contains("Officials")))
                {
                    // Console.WriteLine(row.Text());
                    currentDate = DateTime.Parse(row.Text());
                }
                // If row has a bye or missing a column
                else if (row.Children.Length < 5 || row.Children.Length > 7) // Make 6 when on latest fixtures, 5 for league page
                {
                    Console.WriteLine("LENGTH");
                }
                // If row is postponed
                // else if (row.Children.SingleOrDefault(r => r.ClassName == "text-center scores homeScore").Text() == "P")
                // {
                //     // Console.WriteLine(row.Text());
                // }
                else
                {
                    if (DateTime.Compare(currentDate,mostRecentDate) < 0)
                    {
                        // System.Console.WriteLine($"Current Date: {currentDate}, Database Date: {mostRecentDate}");
                        // break;
                    }
                    if (row.Children[1].Text().Contains("P") || row.Children[2].Text().Contains("P")) continue;
                    var newFixture = new Fixture
                    {
                        Date = currentDate,
                        League = LeagueNumber,  //GetLeagueIDByName(leagueList, tempLeague);
                        TeamOne = GetTeamIdByName(teamList, row.Children[0].Text()),
                        TeamOneScore = Convert.ToInt32(row.Children[1].Text()),
                        TeamTwoScore = Convert.ToInt32(row.Children[2].Text()),
                        TeamTwo = GetTeamIdByName(teamList, row.Children[3].Text()),
                        Location = row.Children[4].Text()
                    };
                    // var tempLeague = row.Children[0].Text();

                    // ADD WHEN RUNNING IN PRODUCTION
                    // if (tempLeague.Contains("Super") || tempLeague.Contains("Conference"))
                    // {
                    //     continue;
                    // }
                    // IF LEAGUE IS ACTUALLY A CUP IGNORE??
                    
                    newFixture.Category = GetCategoryByLeague(leagueList, newFixture.League);
                    if (newFixture.TeamOne == 0 || newFixture.TeamTwo == 0) continue;
                    fixtureList.Add(newFixture);
                }
            }

            fixtureList.Reverse();

            foreach (var fixture in fixtureList)
            {
                var teamOneRating = _db.GetTeamRating(fixture.TeamOne);
                var teamTwoRating = _db.GetTeamRating(fixture.TeamTwo);
                var eloChanges = CalculateElo(teamOneRating, fixture.TeamOneScore, teamTwoRating, fixture.TeamTwoScore, 0, fixture.Category);
                var eloOneChange = eloChanges.Item1;
                var eloTwoChange = eloChanges.Item2;
                // PostFixtureToDatabase(fixture.Date, fixture.League, fixture.TeamOne, fixture.TeamTwo, fixture.TeamOneScore, fixture.TeamTwoScore, fixture.Location, eloOneChange, fixture.Category, eloTwoChange);
                _db.UpdateEloRating(fixture.TeamOne, eloOneChange);
                _db.UpdateEloRating(fixture.TeamTwo, eloTwoChange);
                // Console.WriteLine($"{fixture.Date.ToShortDateString()}: {fixture.League}, {fixture.TeamOne} {fixture.TeamOneScore} - {fixture.TeamTwoScore} {fixture.TeamTwo}, {fixture.Location}");
            }
            //SetMostRecentDay(topDate);

            return fixtureList;
        }
        
        private static int GetTeamIdByName(IEnumerable<Team> teamList, string currentTeam)
        {
            return (from team in teamList where team.TeamName.Equals(currentTeam) select team.ID).FirstOrDefault();
        }
        
        private static int GetCategoryByLeague(IEnumerable<League> leagueList, int currentLeagueId)
        {
            var temp = leagueList.SingleOrDefault(x => x.Id == currentLeagueId);
            return temp?.HockeyCategoryID ?? 1;
        }
        
        private static int GetLeagueIDByName(IEnumerable<League> leagueList, string currentLeague)
        {
            return (from league in leagueList where league.Name.Equals(currentLeague) select league.Id).FirstOrDefault();
        }
    }
}