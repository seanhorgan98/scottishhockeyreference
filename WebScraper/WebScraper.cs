using WebScraper.Interfaces;
using WebScraper.Scrapers;

namespace WebScraper
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    public class WebScraper : IWebScraper
    {
        private readonly ILogger<WebScraper> _log;
        private readonly IConfiguration _config;
        private readonly ILeagueScraper _leagueScraper;
        private readonly ITeamScraper _teamScraper;
        private readonly IPointsScraper _pointsScraper;

        public WebScraper
        (
            ILogger<WebScraper> log, 
            IConfiguration config, 
            ILeagueScraper leagueScraper, 
            ITeamScraper teamScraper,
            IPointsScraper pointsScraper
        )
        {
            _log = log;
            _config = config;
            _leagueScraper = leagueScraper;
            _teamScraper = teamScraper;
            _pointsScraper = pointsScraper;
        }

        public async Task Run()
        {
            _log.LogInformation("Entered Web Scraper class");
            
            // Scrape Leagues
            var leagueList = await _leagueScraper.Scrape();
            _log.LogInformation("Leagues: { leagueList }", leagueList);
            
            // Scrape Teams
            var teamList = await _teamScraper.Scrape();
            _log.LogInformation("Teams: { teamList }", teamList);
            
            // Scrape Results
            
            
            // Scrape Points
            var pointsList = await _pointsScraper.Scrape();
            // foreach team in points list, call updatepoints
        }
    }
}
