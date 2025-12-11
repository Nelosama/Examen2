using Google.Cloud.Firestore;
using ProyectoVotoSeguro.Models;

namespace ProyectoVotoSeguro.Services
{
    public class PrestamosService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly string _collectionName = "prestamos";
        private readonly LibrosService _librosService;

        public PrestamosService(FirebaseServices firebaseService, LibrosService librosService)
        {
            _firebaseService = firebaseService;
            _librosService = librosService;
        }

        public async Task<Prestamo> CrearPrestamo(string usuarioId, string libroId, int diasPrestamo = 7)
        {
            var libro = await _librosService.GetLibroById(libroId);
            if (libro == null) throw new Exception("Libro no encontrado");
            if (libro.CopiasDisponibles <= 0) throw new Exception("No hay copias disponibles");

            // Decrement copies
            libro.CopiasDisponibles--;
            if (libro.CopiasDisponibles == 0) libro.Estado = "agotado";
            await _librosService.UpdateLibro(libroId, libro);

            var prestamo = new Prestamo
            {
                UsuarioId = usuarioId,
                LibroId = libroId,
                FechaPrestamo = Timestamp.FromDateTime(DateTime.UtcNow),
                FechaDevolucionEsperada = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(diasPrestamo)),
                Estado = "activo",
                Renovaciones = 0,
                DiasRetraso = 0,
                MultaGenerada = 0
            };

            var collection = _firebaseService.GetCollection(_collectionName);
            var docRef = await collection.AddAsync(prestamo);
            prestamo.Id = docRef.Id;
            return prestamo;
        }

        public async Task<Prestamo> DevolverPrestamo(string prestamoId)
        {
            var docRef = _firebaseService.GetCollection(_collectionName).Document(prestamoId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) throw new Exception("Préstamo no encontrado");
            
            var prestamo = snapshot.ConvertTo<Prestamo>();
            if (prestamo.Estado == "devuelto") throw new Exception("El préstamo ya fue devuelto");

            prestamo.FechaDevolucionReal = Timestamp.FromDateTime(DateTime.UtcNow);
            
            // Calculate fines
            var fechaDevolucionUTC = prestamo.FechaDevolucionReal.Value.ToDateTime();
            var fechaEsperadaUTC = prestamo.FechaDevolucionEsperada.ToDateTime();

            if (fechaDevolucionUTC > fechaEsperadaUTC)
            {
                var diff = fechaDevolucionUTC - fechaEsperadaUTC;
                prestamo.DiasRetraso = (int)Math.Ceiling(diff.TotalDays); 
                if (prestamo.DiasRetraso > 0)
                {
                    prestamo.MultaGenerada = prestamo.DiasRetraso * 50; // 50 Lempiras per day
                }
            }
            prestamo.Estado = "devuelto"; 

            // Update user fines if any
            if (prestamo.MultaGenerada > 0)
            {
                var userRef = _firebaseService.GetCollection("usuarios").Document(prestamo.UsuarioId);
                await _firebaseService.GetFirestoreDb().RunTransactionAsync(async transaction => 
                {
                     var userSnap = await transaction.GetSnapshotAsync(userRef);
                     if (userSnap.Exists)
                     {
                         // Careful with type conversion from Firestore
                         var currentMultas = userSnap.GetValue<double>("multas");
                         transaction.Update(userRef, new Dictionary<string, object> { { "multas", currentMultas + prestamo.MultaGenerada } });
                     }
                });
            }

            await docRef.SetAsync(prestamo, SetOptions.MergeAll);

            // Increment book copies
            var libro = await _librosService.GetLibroById(prestamo.LibroId);
            if (libro != null)
            {
                libro.CopiasDisponibles++;
                if (libro.CopiasDisponibles > 0 && libro.Estado == "agotado") libro.Estado = "disponible";
                await _librosService.UpdateLibro(prestamo.LibroId, libro);
            }

            return prestamo;
        }
        
        public async Task<List<Prestamo>> GetPrestamosByUsuario(string usuarioId)
        {
             var query = _firebaseService.GetCollection(_collectionName).WhereEqualTo("usuarioId", usuarioId);
             var snapshot = await query.GetSnapshotAsync();
             return snapshot.Documents.Select(d => d.ConvertTo<Prestamo>()).ToList();
        }

        public async Task<List<Prestamo>> GetAllPrestamos()
        {
             var collection = _firebaseService.GetCollection(_collectionName);
             var snapshot = await collection.GetSnapshotAsync();
             return snapshot.Documents.Select(d => d.ConvertTo<Prestamo>()).ToList();
        }
    }
}
