using System.Collections.Generic;
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

    public class LeagueScraper : ILeagueScraper
    {
        private readonly ILogger<LeagueScraper> _log;
        private readonly c.IConfiguration _config;

        public LeagueScraper(ILogger<LeagueScraper> log, c.IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public async Task<List<League>> Scrape()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(_config.GetValue<string>("LeaguesURLPath"));
            var allLeagues = document.QuerySelectorAll("h2.text-uppercase");

            var leagueList = new List<League>();

            foreach (var item in allLeagues)
            {
                if (item.TextContent.Contains("Conference") || item.TextContent.Contains("Super"))
                {
                    _log.LogInformation("Skipped non-standard league: {league}", item.TextContent);
                    continue;
                }
                // SaveLeagueSql(item.TextContent, GetLeagueHockeyCategoryByName(item.TextContent));
                if (!leagueList.Exists(x => x.Name == item.Text()))
                {
                    leagueList.Add(new League {Name = item.Text()});
                }
            }

            return leagueList;
        }
    }
}
