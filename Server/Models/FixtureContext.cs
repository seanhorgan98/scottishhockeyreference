using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    public class FixtureContext : DbContext
    {
        public FixtureContext(DbContextOptions<FixtureContext> options)
            : base(options)
        {
        }

        public DbSet<Fixture> Fixtures { get; set; }
    }
}
