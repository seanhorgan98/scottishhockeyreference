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

        public WebScraper(ILogger<WebScraper> log, IConfiguration config, ILeagueScraper leagueScraper)
        {
            _log = log;
            _config = config;
            _leagueScraper = leagueScraper;
        }

        public async Task Run()
        {
            _log.LogInformation("Entered Web Scraper class.");
            
            // Scrape Leagues
            var leagueList = await _leagueScraper.Scrape();
            _log.LogInformation("Leagues: {leagueList}", leagueList);
            
            // Scrape Teams
            var teamScraper = new TeamScraper(_log, _config);
        }
    }
}
