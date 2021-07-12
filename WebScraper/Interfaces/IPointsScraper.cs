using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraper.Models;

namespace WebScraper.Interfaces
{
    public interface IPointsScraper
    {
        Task<List<Team>> Scrape();
    }
}