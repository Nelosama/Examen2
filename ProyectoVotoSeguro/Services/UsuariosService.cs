using Google.Cloud.Firestore;
using ProyectoVotoSeguro.Models;

namespace ProyectoVotoSeguro.Services
{
    public class UsuariosService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly PrestamosService _prestamosService;

        public UsuariosService(FirebaseServices firebaseService, PrestamosService prestamosService)
        {
            _firebaseService = firebaseService;
            _prestamosService = prestamosService;
        }

        public async Task CambiarRol(string usuarioId, string nuevoRol, string adminId)
        {
            if (usuarioId == adminId)
            {
                throw new Exception("No puedes cambiar tu propio rol.");
            }

            var validRoles = new[] { "usuario", "bibliotecario", "admin" };
            if (!validRoles.Contains(nuevoRol.ToLower()))
            {
                throw new Exception("Rol inválido.");
            }

            var docRef = _firebaseService.GetCollection("usuarios").Document(usuarioId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) throw new Exception("Usuario no encontrado.");

            await docRef.UpdateAsync("rol", nuevoRol.ToLower());
        }

        public async Task GestionarMulta(string usuarioId, double monto)
        {
            var docRef = _firebaseService.GetCollection("usuarios").Document(usuarioId);
            
            await _firebaseService.GetFirestoreDb().RunTransactionAsync(async transaction =>
            {
                var snapshot = await transaction.GetSnapshotAsync(docRef);
                if (!snapshot.Exists) throw new Exception("Usuario no encontrado.");

                var currentMultas = snapshot.GetValue<double>("multas");
                var newMultas = currentMultas + monto;

                if (newMultas < 0) throw new Exception("El monto total de multas no puede ser negativo.");

                transaction.Update(docRef, new Dictionary<string, object> { { "multas", newMultas } });
            });
        }

        public async Task ToggleEstado(string usuarioId)
        {
            // Check active loans
            var prestamos = await _prestamosService.GetPrestamosByUsuario(usuarioId);
            if (prestamos.Any(p => p.Estado == "activo"))
            {
                throw new Exception("No se puede desactivar un usuario con préstamos activos.");
            }

            var docRef = _firebaseService.GetCollection("usuarios").Document(usuarioId);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) throw new Exception("Usuario no encontrado.");

            var currentEstado = snapshot.GetValue<bool>("activo");
            await docRef.UpdateAsync("activo", !currentEstado);
        }
    }
}
