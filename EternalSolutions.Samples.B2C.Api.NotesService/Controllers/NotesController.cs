using EternalSolutions.Samples.B2C.Api.NotesService.Notes;
using EternalSolutions.Samples.B2C.Common.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EternalSolutions.Samples.B2C.Api.NotesService.Controllers
{
    [Route("api/[controller]")]
    public class NotesController : Controller
    {
        private readonly NotesContext _dbContext;

        public NotesController(NotesContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/values
        [HttpGet]
        [Authorize(Policy = "ReadNotes")]
        public async Task<IActionResult> All()
        {
            var notes = await _dbContext.Notes.ToListAsync();
            return Ok(notes);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [Authorize(Policy = "ReadNotes")]
        public async Task<IActionResult> Get(int id)
        {
            var note = await _dbContext.Notes.FindAsync(id);
            if (note != null)
                return Ok(note);
            else
                return NotFound();
        }

        // POST api/values
        [HttpPost]
        [Authorize(Policy = "WriteNotes")]
        public async Task<IActionResult> Post([FromBody]Note note)
        {
            await _dbContext.Notes.AddAsync(note);
            await _dbContext.SaveChangesAsync();

            return Ok(note.Id);
        }

        // DELETE api/values/5
        [Authorize(Policy = "DeleteNotes")]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _dbContext.Notes.FindAsync(id);
            if (note == null)
                return NotFound();

            _dbContext.Notes.Remove(note);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
