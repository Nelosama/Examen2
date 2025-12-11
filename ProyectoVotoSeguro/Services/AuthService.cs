using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Cloud.Firestore;
using Microsoft.IdentityModel.Tokens;
using ProyectoVotoSeguro.DTOs;
using ProyectoVotoSeguro.Models;

namespace ProyectoVotoSeguro.Services
{
    public class AuthService : IAuthService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly IConfiguration _configuration;

        public AuthService(FirebaseServices firebaseService, IConfiguration configuration)
        {
            _firebaseService = firebaseService;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            var existingUser = await GetUserByEmail(registerDto.Correo);
            if (existingUser != null)
                throw new Exception("El correo ya está registrado.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Contrasena);

            var usuario = new Usuario
            {
                Nombre = registerDto.Nombre,
                Apellido = registerDto.Apellido,
                Correo = registerDto.Correo,
                Contrasena = passwordHash,
                Edad = registerDto.Edad,
                NumeroIdentidad = registerDto.NumeroIdentidad,
                Telefono = registerDto.Telefono,
                Rol = registerDto.Rol,
                Activo = true,
                FechaRegistro = Timestamp.FromDateTime(DateTime.UtcNow),
                Multas = 0
            };

            var collection = _firebaseService.GetCollection("usuarios");
            var docRef = await collection.AddAsync(usuario);
            usuario.Id = docRef.Id;

            var token = GenerateJwtToken(usuario);

            return new AuthResponseDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Correo = usuario.Correo,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Rol = usuario.Rol
            };
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            var usuario = await GetUserByEmail(loginDto.Correo);
            if (usuario == null)
                throw new Exception("Credenciales inválidas.");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Contrasena, usuario.Contrasena))
                throw new Exception("Credenciales inválidas.");

            if (!usuario.Activo)
                throw new Exception("Usuario inactivo.");

            var token = GenerateJwtToken(usuario);

            return new AuthResponseDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Correo = usuario.Correo,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Rol = usuario.Rol
            };
        }

        public async Task<Usuario?> GetUserByEmail(string correo)
        {
            var collection = _firebaseService.GetCollection("usuarios");
            var query = collection.WhereEqualTo("correo", correo).Limit(1);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count == 0) return null;

            var doc = snapshot.Documents[0];
            var usuario = doc.ConvertTo<Usuario>();
            // [FirestoreDocumentId] should handle ID mapping
            return usuario;
        }

        public async Task<Usuario?> GetUserById(string userId)
        {
            var docRef = _firebaseService.GetCollection("usuarios").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            return snapshot.ConvertTo<Usuario>();
        }

        public string GenerateJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("JWT Key missing in config");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id ?? ""),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60), // Default expiry
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}