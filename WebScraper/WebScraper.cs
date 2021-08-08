using WebScraper.Interfaces;
using WebScraper.Scrapers;

namespace WebScraper
{
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    public class WebScraper : IWebScraper
    {
        private readonly ILogger<WebScraper> _log;
        private readonly ILeagueScraper _leagueScraper;
        private readonly ITeamScraper _teamScraper;
        private readonly IPointsScraper _pointsScraper;
        private readonly IResultScraper _resultScraper;

        public WebScraper
        (
            ILogger<WebScraper> log,
            ILeagueScraper leagueScraper, 
            ITeamScraper teamScraper,
            IPointsScraper pointsScraper,
            IResultScraper resultScraper,
            Elo elo
        )
        {
            _log = log;
            _leagueScraper = leagueScraper;
            _teamScraper = teamScraper;
            _pointsScraper = pointsScraper;
            _resultScraper = resultScraper;
        }

        public async Task Run()
        {
            _log.LogInformation("Entered Web Scraper class");
            
            /*// Scrape Leagues
            var leagueList = await _leagueScraper.ScrapeAsync();
            foreach (var league in leagueList)
            {
                _log.LogInformation("Scraped League: {League}", league.Name);
                // SaveLeagueSql(item.TextContent, GetLeagueHockeyCategoryByName(item.TextContent));
            }*/
            
            
            /*// Scrape Teams
            var teamList = await _teamScraper.ScrapeAsync();
            foreach (var team in teamList)
            {
                _log.LogInformation("Scraped Team: {Team}", team.TeamName);
            }*/
            
            
            /*// Scrape Results
            var fixtureList = await _resultScraper.ScrapeAsync();
            foreach (var fixture in fixtureList)
            {
                _log.LogInformation("Scraped Fixture: {TeamOne}: {OneScore} - {TwoScore} :{TeamTwo}", fixture.TeamOne, fixture.TeamOneScore, fixture.TeamTwoScore, fixture.TeamTwo);
            }*/

            // Scrape Points
            var pointsList = await _pointsScraper.ScrapeAsync();
            foreach (var team in pointsList)
            {
                    _log.LogInformation("Team: {Teamname}, Points: {SeasonPoints}", team.TeamName, team.SeasonPoints);
            }
            // foreach team in points list, call updatepoints
        }
    }
}
