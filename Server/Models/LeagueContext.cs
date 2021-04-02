using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    public class LeagueContext : DbContext
    {
        public LeagueContext(DbContextOptions<LeagueContext> options)
            : base(options)
        {
        }

        public DbSet<League> Leagues { get; set; }
    }
}
