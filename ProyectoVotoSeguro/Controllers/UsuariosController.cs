using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoVotoSeguro.DTOs;
using ProyectoVotoSeguro.Services;
using System.Security.Claims;

namespace ProyectoVotoSeguro.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosService _usuariosService;

        public UsuariosController(UsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        [HttpPut("{id}/cambiar-rol")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CambiarRol(string id, [FromBody] CambiarRolDto dto)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _usuariosService.CambiarRol(id, dto.NuevoRol, adminId);
                return Ok(new { message = "Rol actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/gestionar-multa")]
        [Authorize(Roles = "bibliotecario,admin")]
        public async Task<IActionResult> GestionarMulta(string id, [FromBody] GestionarMultaDto dto)
        {
            try
            {
                await _usuariosService.GestionarMulta(id, dto.Monto);
                return Ok(new { message = "Multa actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/toggle-estado")]
        [Authorize(Roles = "admin,bibliotecario")] // Assuming both can manage users, or restrict to admin strictly if preferred.
        // Requirement was vague, but "Gestión Administrativa" usually implies admin. I'll include bibliotecario as they often manage patrons.
        public async Task<IActionResult> ToggleEstado(string id)
        {
            try
            {
                await _usuariosService.ToggleEstado(id);
                return Ok(new { message = "Estado de usuario actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
