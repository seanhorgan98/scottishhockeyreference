namespace WebScraper
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class WebScraper : IWebScraper
    {
        private readonly ILogger<WebScraper> _log;
        private readonly IConfiguration _config;

        public WebScraper(ILogger<WebScraper> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public void Run()
        {
            _log.LogInformation("Entered Web Scraper class.");
            _log.LogInformation($"Config value: {_config.GetValue<int>("TestValue")}");
        }
    }
}
