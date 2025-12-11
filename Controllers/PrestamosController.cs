using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoVotoSeguro.Services;

namespace ProyectoVotoSeguro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrestamosController : ControllerBase
    {
        private readonly PrestamosService _prestamosService;

        public PrestamosController(PrestamosService prestamosService)
        {
            _prestamosService = prestamosService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearPrestamo([FromBody] CrearPrestamoDto dto)
        {
            try
            {
                string usuarioId = dto.UsuarioId;
                if (string.IsNullOrEmpty(usuarioId))
                {
                    usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }
                else
                {
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var role = User.FindFirst(ClaimTypes.Role)?.Value;
                    if (usuarioId != currentUserId && role != "bibliotecario" && role != "admin")
                    {
                        return Forbid();
                    }
                }

                if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

                var prestamo = await _prestamosService.CrearPrestamo(usuarioId, dto.LibroId, dto.Dias);
                return Ok(prestamo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/devolver")]
        public async Task<IActionResult> Devolver(string id)
        {
            try
            {
                 var prestamo = await _prestamosService.DevolverPrestamo(id);
                 return Ok(prestamo);
            }
            catch (Exception ex)
            {
                 return BadRequest(ex.Message);
            }
        }

        [HttpGet("mis-prestamos")]
        public async Task<IActionResult> GetMisPrestamos()
        {
             var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             if (userId == null) return Unauthorized();
             var prestamos = await _prestamosService.GetPrestamosByUsuario(userId);
             return Ok(prestamos);
        }
        
        [HttpGet]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetAll()
        {
             var prestamos = await _prestamosService.GetAllPrestamos();
             return Ok(prestamos);
        }
    }

    public class CrearPrestamoDto
    {
        public string LibroId { get; set; }
        public string? UsuarioId { get; set; }
        public int Dias { get; set; } = 7;
    }
}
