using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoVotoSeguro.Services;
using System.Security.Claims;

namespace ProyectoVotoSeguro.Controllers
{
    [ApiController]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly ReportesService _reportesService;

        public ReportesController(ReportesService reportesService)
        {
            _reportesService = reportesService;
        }

        [HttpGet("api/reportes/usuarios-morosos")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetUsuariosMorosos()
        {
            var result = await _reportesService.GetUsuariosMorosos();
            return Ok(result);
        }

        [HttpGet("api/reportes/libros-populares")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GetLibrosPopulares()
        {
            var result = await _reportesService.GetLibrosPopulares();
            return Ok(result);
        }

        [HttpGet("api/reportes/mi-historial")]
        public async Task<IActionResult> GetMiHistorial()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _reportesService.GetMiHistorial(userId);
            return Ok(result);
        }

        [HttpGet("api/estadisticas")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetEstadisticas()
        {
            var result = await _reportesService.GetEstadisticas();
            return Ok(result);
        }
    }
}
