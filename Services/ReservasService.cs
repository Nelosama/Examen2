using Google.Cloud.Firestore;
using ProyectoVotoSeguro.Models;

namespace ProyectoVotoSeguro.Services
{
    public class ReservasService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly string _collectionName = "reservas";
        private readonly LibrosService _librosService;

        public ReservasService(FirebaseServices firebaseService, LibrosService librosService)
        {
            _firebaseService = firebaseService;
            _librosService = librosService;
        }

        public async Task<Reserva> CrearReserva(string usuarioId, string libroId)
        {
            var libro = await _librosService.GetLibroById(libroId);
            if (libro == null) throw new Exception("Libro no encontrado");
            if (libro.CopiasDisponibles > 0) throw new Exception("El libro está disponible, puede solicitarlo en préstamo directamente.");

            var collection = _firebaseService.GetCollection(_collectionName);
            var query = collection.WhereEqualTo("libroId", libroId).WhereEqualTo("estado", "pendiente");
            var snapshot = await query.GetSnapshotAsync();
            int prioridad = snapshot.Count + 1;

            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                LibroId = libroId,
                FechaReserva = Timestamp.FromDateTime(DateTime.UtcNow),
                Estado = "pendiente",
                Prioridad = prioridad
            };

            var docRef = await collection.AddAsync(reserva);
            reserva.Id = docRef.Id;
            return reserva;
        }

        public async Task<List<Reserva>> GetReservasByUsuario(string usuarioId)
        {
             var query = _firebaseService.GetCollection(_collectionName).WhereEqualTo("usuarioId", usuarioId);
             var snapshot = await query.GetSnapshotAsync();
             return snapshot.Documents.Select(d => d.ConvertTo<Reserva>()).ToList();
        }

        public async Task NotificarReserva(string reservaId)
        {
             var docRef = _firebaseService.GetCollection(_collectionName).Document(reservaId);
             var snapshot = await docRef.GetSnapshotAsync();
             if (!snapshot.Exists) throw new Exception("Reserva no encontrada");
             
             var reserva = snapshot.ConvertTo<Reserva>();
             reserva.Estado = "notificada";
             reserva.FechaNotificacion = Timestamp.FromDateTime(DateTime.UtcNow);
             reserva.FechaExpiracion = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(48));
             
             await docRef.SetAsync(reserva, SetOptions.MergeAll);
        }
    }
}
