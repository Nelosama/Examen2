using Google.Cloud.Firestore;

namespace ProyectoVotoSeguro.Models
{
    [FirestoreData]
    public class Reserva
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("usuarioId")]
        public string UsuarioId { get; set; }

        [FirestoreProperty("libroId")]
        public string LibroId { get; set; }

        [FirestoreProperty("fechaReserva")]
        public Timestamp FechaReserva { get; set; }

        [FirestoreProperty("estado")]
        public string Estado { get; set; } // "pendiente", "notificada", "completada", "cancelada"

        [FirestoreProperty("fechaNotificacion")]
        public Timestamp? FechaNotificacion { get; set; }

        [FirestoreProperty("fechaExpiracion")]
        public Timestamp? FechaExpiracion { get; set; }

        [FirestoreProperty("prioridad")]
        public int Prioridad { get; set; }
    }
}
