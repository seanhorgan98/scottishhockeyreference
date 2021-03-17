using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FixtureController : ControllerBase
    {
        private readonly FixtureContext _context;

        public FixtureController(FixtureContext context)
        {
            _context = context;
        }

        // GET: api/Fixture
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fixture>>> GetFixtures()
        {
            return await _context.Fixtures.ToListAsync();
        }

        // GET: api/Fixture/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fixture>> GetFixture(long id)
        {
            var fixture = await _context.Fixtures.FindAsync(id);

            if (fixture == null)
            {
                return NotFound();
            }

            return fixture;
        }

        // PUT: api/Fixture/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFixture(long id, Fixture fixture)
        {
            if (id != fixture.Id)
            {
                return BadRequest();
            }

            _context.Entry(fixture).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FixtureExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Fixture
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Fixture>> PostFixture(Fixture fixture)
        {
            _context.Fixtures.Add(fixture);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFixture), new { id = fixture.Id }, fixture);
        }

        // DELETE: api/Fixture/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFixture(long id)
        {
            var fixture = await _context.Fixtures.FindAsync(id);
            if (fixture == null)
            {
                return NotFound();
            }

            _context.Fixtures.Remove(fixture);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FixtureExists(long id)
        {
            return _context.Fixtures.Any(e => e.Id == id);
        }
    }
}
