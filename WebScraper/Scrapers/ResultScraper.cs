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

namespace WebScraper.Scrapers
{
    using c = Microsoft.Extensions.Configuration;
    
    public class ResultScraper : IResultScraper
    {
        private readonly ILogger<ResultScraper> _log;
        private readonly c.IConfiguration _config;
        private readonly IDbInteraction _db;

        public ResultScraper(ILogger<ResultScraper> log, c.IConfiguration config, IDbInteraction db)
        {
            _log = log;
            _config = config;
            _db = db;
        }
        
        public async Task<List<Fixture>> ScrapeAsync()
        {
            var fixtureList = new List<Fixture>();
            
            // Get List of Leagues
            var leagueList = _db.GetAllLeagues();

            // Get List of Teams
            var teamList = _db.GetAllTeams();

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            // var htmlText = await System.IO.File.ReadAllTextAsync(HtmlFile);
            var document = await context.OpenAsync(_config.GetValue<string>("ResultsURLPath"));
            //var document = await context.OpenAsync(req => req.Content(htmlText));
            var tableWrap = document.All.Where(m => m.LocalName == "tr");
            var currentDate = DateTime.MinValue;
            var mostRecentDate = _db.GetMostRecentDate();
            //var topDate = DateTime.Parse(tableWrap.First().Text());

            foreach (var row in tableWrap)
            {
                // If row is a date and not umpires
                if (row.Children.Length == 1 && (!row.Text().Contains("Umpires") && !row.Text().Contains("Officials")))
                {
                    currentDate = DateTime.Parse(row.Text());
                }
                
                // If row has a bye or missing a column
                else if (row.Children.Length < 6) // Make 6 when on latest fixtures, 5 for league page
                {
                    _log.LogInformation("Skipping: Fixture has a bye or missing a column: {Fixture}", row.Text());
                }
                // If row is postponed
                else if (row.Children.SingleOrDefault(r => r.ClassName == "text-center scores homeScore").Text() == "P")
                {
                    _log.LogInformation("Skipping: Fixture has been postponed: {Fixture}", row.Text());
                }
                else
                {
                    if (DateTime.Compare(currentDate,mostRecentDate) < 0)
                    {
                        _log.LogInformation("Not reading fixture as should already be in database due to date. Current: {CurDate}, Most Recent DB Date: {DbDate}", currentDate, mostRecentDate);
                        break;
                    }

                    var fixtureLeague = row.Children[0].Text();
                    var newFixture = new Fixture
                    {
                        Date = currentDate,
                        League = GetLeagueIDByName(leagueList, fixtureLeague),
                        TeamOne = GetTeamIdByName(teamList, row.Children[1].Text()),
                        TeamOneScore = Convert.ToInt32(row.Children[2].Text()),
                        TeamTwoScore = Convert.ToInt32(row.Children[3].Text()),
                        TeamTwo = GetTeamIdByName(teamList, row.Children[4].Text()),
                        Location = row.Children[5].Text()
                    };

                    // ADD WHEN RUNNING IN PRODUCTION
                    if (fixtureLeague.Contains("Super") || fixtureLeague.Contains("Conference"))
                    {
                        continue;
                    }
                    // IF LEAGUE IS ACTUALLY A CUP IGNORE??
                    
                    newFixture.Category = GetCategoryByLeague(leagueList, newFixture.League);
                    if (newFixture.TeamOne == 0 || newFixture.TeamTwo == 0)
                    {
                        _log.LogInformation("Team is not registered in the DB");
                        continue;
                    }
                    fixtureList.Add(newFixture);
                }
            }

            fixtureList.Reverse();

            /*foreach (var fixture in fixtureList)
            {
                // var teamOneRating = _db.GetTeamRating(fixture.TeamOne);
                // var teamTwoRating = _db.GetTeamRating(fixture.TeamTwo);
                // var eloChanges = Elo.CalculateElo(teamOneRating, fixture.TeamOneScore, teamTwoRating, fixture.TeamTwoScore, 0, fixture.Category);
                // var eloOneChange = eloChanges.Item1;
                // var eloTwoChange = eloChanges.Item2;
                // PostFixtureToDatabase(fixture.Date, fixture.League, fixture.TeamOne, fixture.TeamTwo, fixture.TeamOneScore, fixture.TeamTwoScore, fixture.Location, eloOneChange, fixture.Category, eloTwoChange);
                // _db.UpdateEloRating(fixture.TeamOne, eloOneChange);
                // _db.UpdateEloRating(fixture.TeamTwo, eloTwoChange);
                // Console.WriteLine($"{fixture.Date.ToShortDateString()}: {fixture.League}, {fixture.TeamOne} {fixture.TeamOneScore} - {fixture.TeamTwoScore} {fixture.TeamTwo}, {fixture.Location}");
            }*/
            //SetMostRecentDay(topDate);

            return fixtureList;
        }
        
        private static int GetCategoryByLeague(IEnumerable<League> leagueList, int currentLeagueId)
        {
            var temp = leagueList.SingleOrDefault(x => x.Id == currentLeagueId);
            return temp?.HockeyCategoryID ?? 1;
        }
        
        private static int GetTeamIdByName(IEnumerable<Team> teamList, string currentTeam)
        {
            return (from team in teamList where team.TeamName.Equals(currentTeam) select team.ID).FirstOrDefault();
        }
        
        private static int GetLeagueIDByName(IEnumerable<League> leagueList, string currentLeague)
        {
            return (from league in leagueList where league.Name.Equals(currentLeague) select league.Id).FirstOrDefault();
        }
    }
}