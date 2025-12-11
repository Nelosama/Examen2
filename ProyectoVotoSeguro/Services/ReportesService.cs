using Google.Cloud.Firestore;
using ProyectoVotoSeguro.DTOs;
using ProyectoVotoSeguro.Models;

namespace ProyectoVotoSeguro.Services
{
    public class ReportesService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly LibrosService _librosService;
        
        public ReportesService(FirebaseServices firebaseService, LibrosService librosService)
        {
            _firebaseService = firebaseService;
            _librosService = librosService;
        }

        public async Task<List<UsuarioMorosoDto>> GetUsuariosMorosos()
        {
            var usuariosQuery = _firebaseService.GetCollection("usuarios").WhereGreaterThan("multas", 0);
            var usuariosSnap = await usuariosQuery.GetSnapshotAsync();
            
            var result = new List<UsuarioMorosoDto>();
            var prestamosColl = _firebaseService.GetCollection("prestamos");

            // Optimisation: Fetch all active loans once and filter in memory if dataset is small
            // Or query per user. Assuming small dataset for exam.
            var allPrestamosSnap = await prestamosColl
                .WhereEqualTo("estado", "activo")
                .GetSnapshotAsync();
            
            var allPrestamos = allPrestamosSnap.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .ToList();

            foreach (var doc in usuariosSnap.Documents)
            {
                var usuario = doc.ConvertTo<Usuario>();
                
                // Count overdue loans
                // Define overdue: FechaDevolucionEsperada < Now
                int vencidos = allPrestamos
                    .Count(p => p.UsuarioId == usuario.Id && 
                                p.FechaDevolucionEsperada.ToDateTime() < DateTime.UtcNow);

                result.Add(new UsuarioMorosoDto
                {
                    UsuarioId = usuario.Id,
                    NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                    Correo = usuario.Correo,
                    MultasPendientes = usuario.Multas,
                    PrestamosVencidosCount = vencidos
                });
            }

            return result;
        }

        public async Task<List<LibroPopularDto>> GetLibrosPopulares()
        {
            var fechaLimite = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-30));
            
            var prestamosQuery = _firebaseService.GetCollection("prestamos")
                .WhereGreaterThanOrEqualTo("fechaPrestamo", fechaLimite);
            
            var prestamosSnap = await prestamosQuery.GetSnapshotAsync();
            var prestamos = prestamosSnap.Documents.Select(d => d.ConvertTo<Prestamo>());

            var grouped = prestamos.GroupBy(p => p.LibroId)
                .Select(g => new { LibroId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var result = new List<LibroPopularDto>();
            
            // Get pending reservations count for these books
            // We can query all pending reservations once or per book.
            var reservasSnap = await _firebaseService.GetCollection("reservas")
                .WhereEqualTo("estado", "pendiente")
                .GetSnapshotAsync();
            
            var allReservas = reservasSnap.Documents.Select(d => d.ConvertTo<Reserva>()).ToList();

            foreach (var item in grouped)
            {
                var libro = await _librosService.GetLibroById(item.LibroId);
                if (libro != null)
                {
                    int reservasCount = allReservas.Count(r => r.LibroId == item.LibroId);
                    
                    result.Add(new LibroPopularDto
                    {
                        LibroId = libro.Id,
                        Titulo = libro.Titulo,
                        Autor = libro.Autor,
                        CantidadPrestamos = item.Count,
                        ReservasPendientes = reservasCount
                    });
                }
            }

            return result;
        }

        public async Task<List<HistorialPrestamoDto>> GetMiHistorial(string usuarioId)
        {
            var prestamosQuery = _firebaseService.GetCollection("prestamos")
                .WhereEqualTo("usuarioId", usuarioId);
                // .OrderBy("fechaPrestamo") // Requires composite index usually
            
            var prestamosSnap = await prestamosQuery.GetSnapshotAsync();
            var prestamos = prestamosSnap.Documents
                .Select(d => d.ConvertTo<Prestamo>())
                .OrderBy(p => p.FechaPrestamo) // InMemory sorting to avoid index requirement errors during testing
                .ToList();

            var result = new List<HistorialPrestamoDto>();
            
            foreach (var p in prestamos)
            {
                var libro = await _librosService.GetLibroById(p.LibroId);
                
                result.Add(new HistorialPrestamoDto
                {
                    PrestamoId = p.Id,
                    LibroTitulo = libro?.Titulo ?? "Desconocido",
                    LibroAutor = libro?.Autor ?? "Desconocido",
                    FechaPrestamo = p.FechaPrestamo.ToDateTime(),
                    FechaDevolucionEsperada = p.FechaDevolucionEsperada.ToDateTime(),
                    FechaDevolucionReal = p.FechaDevolucionReal?.ToDateTime(),
                    MultaGenerada = p.MultaGenerada,
                    Estado = p.Estado
                });
            }

            return result;
        }

        public async Task<EstadisticasDto> GetEstadisticas()
        {
            // Note: Count on collections is expensive/slow in Firestore directly without aggregation queries.
            // Using GetSnapshotAsync().Count is OK for small assignments.
            
            var usuariosSnap = await _firebaseService.GetCollection("usuarios").WhereEqualTo("activo", true).GetSnapshotAsync();
            var librosSnap = await _firebaseService.GetCollection("libros").GetSnapshotAsync();
            var prestamosSnap = await _firebaseService.GetCollection("prestamos").GetSnapshotAsync();
            var reservasSnap = await _firebaseService.GetCollection("reservas").WhereEqualTo("estado", "pendiente").GetSnapshotAsync();

            var prestamos = prestamosSnap.Documents.Select(d => d.ConvertTo<Prestamo>()).ToList();
            
            var activos = prestamos.Count(p => p.Estado == "activo");
            var vencidos = prestamos.Count(p => p.Estado == "activo" && p.FechaDevolucionEsperada.ToDateTime() < DateTime.UtcNow);
            
            // For total multas, we can sum from users collection as it tracks its own multas
            var allUsersSnap = await _firebaseService.GetCollection("usuarios").GetSnapshotAsync();
            double totalMultas = allUsersSnap.Documents
                .Select(d => d.GetValue<double>("multas"))
                .Sum();

            return new EstadisticasDto
            {
                TotalUsuariosActivos = usuariosSnap.Count,
                TotalLibros = librosSnap.Count,
                TotalPrestamosActivos = activos,
                TotalPrestamosVencidos = vencidos,
                TotalMultasPendientes = totalMultas,
                TotalReservasPendientes = reservasSnap.Count
            };
        }
    }
}
