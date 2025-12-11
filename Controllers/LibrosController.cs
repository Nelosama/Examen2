using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoVotoSeguro.Models;
using ProyectoVotoSeguro.Services;

namespace ProyectoVotoSeguro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LibrosController : ControllerBase
    {
        private readonly LibrosService _librosService;

        public LibrosController(LibrosService librosService)
        {
            _librosService = librosService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var libros = await _librosService.GetAllLibros();
            return Ok(libros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var libro = await _librosService.GetLibroById(id);
            if (libro == null) return NotFound();
            return Ok(libro);
        }

        [HttpPost]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> Create([FromBody] Libro libro)
        {
            try
            {
                 var created = await _librosService.AddLibro(libro);
                 return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                 return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> Update(string id, [FromBody] Libro libro)
        {
            if (id != libro.Id && !string.IsNullOrEmpty(libro.Id)) 
                 return BadRequest("ID mismatch"); // Optional check

            await _librosService.UpdateLibro(id, libro);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> Delete(string id)
        {
            await _librosService.DeleteLibro(id);
            return NoContent();
        }
    }
}
