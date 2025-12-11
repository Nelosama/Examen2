using Google.Cloud.Firestore;

namespace ProyectoVotoSeguro.DTOs
{
    public class UsuarioMorosoDto
    {
        public string UsuarioId { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public double MultasPendientes { get; set; }
        public int PrestamosVencidosCount { get; set; }
    }

    public class LibroPopularDto
    {
        public string LibroId { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public int CantidadPrestamos { get; set; }
        public int ReservasPendientes { get; set; }
    }

    public class HistorialPrestamoDto
    {
        public string PrestamoId { get; set; }
        public string LibroTitulo { get; set; }
        public string LibroAutor { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaDevolucionEsperada { get; set; }
        public DateTime? FechaDevolucionReal { get; set; }
        public double MultaGenerada { get; set; }
        public string Estado { get; set; }
    }

    public class PrestamoVencidoDto
    {
        public string PrestamoId { get; set; }
        public string UsuarioNombre { get; set; }
        public string LibroTitulo { get; set; }
        public DateTime FechaDevolucionEsperada { get; set; }
        public int DiasRetraso { get; set; }
    }

    public class EstadisticasDto
    {
        public int TotalUsuariosActivos { get; set; }
        public int TotalLibros { get; set; }
        public int TotalPrestamosActivos { get; set; }
        public int TotalPrestamosVencidos { get; set; }
        public double TotalMultasPendientes { get; set; }
        public int TotalReservasPendientes { get; set; }
    }

    public class GestionarMultaDto
    {
        public double Monto { get; set; } 
    }
    
    public class CambiarRolDto
    {
        public string NuevoRol { get; set; }
    }
}
