using System.ComponentModel.DataAnnotations;

namespace ProyectoVotoSeguro.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        public int Edad { get; set; }

        [Required]
        public string NumeroIdentidad { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        public string Rol { get; set; } = "usuario"; // Default to usuario
    }
}