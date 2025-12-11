using ProyectoVotoSeguro.DTOs;
using ProyectoVotoSeguro.Models;

namespace ProyectoVotoSeguro.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto registerDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);
        Task<Usuario?> GetUserById(string userId);
        Task<Usuario?> GetUserByEmail(string correo);
        string GenerateJwtToken(Usuario usuario);
    }
}
