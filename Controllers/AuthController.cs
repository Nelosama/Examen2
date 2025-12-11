using Microsoft.AspNetCore.Mvc;
using ProyectoVotoSeguro.DTOs;
using ProyectoVotoSeguro.Services;

namespace ProyectoVotoSeguro.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
             _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
             try 
             {
                 var result = await _authService.Register(registerDto);
                 return Ok(result);
             }
             catch (Exception ex)
             {
                 return BadRequest(ex.Message);
             }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
             try
             {
                 var result = await _authService.Login(loginDto);
                 return Ok(result);
             }
             catch (Exception ex)
             {
                 return Unauthorized(ex.Message);
             }
        }
    }
}
