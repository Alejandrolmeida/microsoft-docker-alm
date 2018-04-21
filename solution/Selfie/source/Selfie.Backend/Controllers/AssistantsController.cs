using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selfie.Backend.Models;

namespace Selfie.Backend.Controllers
{
    [Produces("application/json")]
    [Route("api/Assistants")]
    public class AssistantsController : Controller
    {
        #region MEMBERS
        private readonly BootCampContext _context;
        #endregion

        #region CONSTRUCTORS
        public AssistantsController(BootCampContext context)
        {
            _context = context;
        }
        #endregion

        #region CONTROLLERS
        // GET: api/Assistants
        [HttpGet]
        public IEnumerable<Assistant> GetAssistants()
        {
            return _context.Assistants;
        }

        // GET: api/Assistants/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssistant([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assistant = await _context.Assistants.SingleOrDefaultAsync(m => m.Id == id);

            if (assistant == null)
            {
                return NotFound();
            }

            return Ok(assistant);
        }

        // PUT: api/Assistants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssistant([FromRoute] int id, [FromBody] Assistant assistant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != assistant.Id)
            {
                return BadRequest();
            }

            _context.Entry(assistant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssistantExists(id))
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

        // POST: api/Assistants
        [HttpPost]
        public async Task<IActionResult> PostAssistant([FromBody] Assistant assistant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Assistants.Add(assistant);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAssistant", new { id = assistant.Id }, assistant);
        }

        // DELETE: api/Assistants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssistant([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var assistant = await _context.Assistants.SingleOrDefaultAsync(m => m.Id == id);
            if (assistant == null)
            {
                return NotFound();
            }

            _context.Assistants.Remove(assistant);
            await _context.SaveChangesAsync();

            return Ok(assistant);
        }
        #endregion

        #region PRIVATE METHODS
        private bool AssistantExists(int id)
        {
            return _context.Assistants.Any(e => e.Id == id);
        }
        #endregion
    }
}