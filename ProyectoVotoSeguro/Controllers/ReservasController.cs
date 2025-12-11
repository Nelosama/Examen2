using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoVotoSeguro.Services;

namespace ProyectoVotoSeguro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private readonly ReservasService _reservasService;

        public ReservasController(ReservasService reservasService)
        {
            _reservasService = reservasService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearReserva([FromBody] CrearReservaDto dto)
        {
            try
            {
                string usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

                var reserva = await _reservasService.CrearReserva(usuarioId, dto.LibroId);
                return Ok(reserva);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/notificar")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> Notificar(string id)
        {
             try
             {
                 await _reservasService.NotificarReserva(id);
                 return Ok("Notificado");
             }
             catch(Exception ex)
             {
                 return BadRequest(ex.Message);
             }
        }

        [HttpGet("mis-reservas")]
        public async Task<IActionResult> GetMisReservas()
        {
             var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             if (userId == null) return Unauthorized();
             var reservas = await _reservasService.GetReservasByUsuario(userId);
             return Ok(reservas);
        }
    }

    public class CrearReservaDto
    {
        public string LibroId { get; set; }
    }
}
