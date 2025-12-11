using System.ComponentModel.DataAnnotations;

namespace ProyectoVotoSeguro.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }
}