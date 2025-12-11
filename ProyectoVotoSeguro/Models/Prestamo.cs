using Google.Cloud.Firestore;

namespace ProyectoVotoSeguro.Models
{
    [FirestoreData]
    public class Prestamo
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("usuarioId")]
        public string UsuarioId { get; set; }

        [FirestoreProperty("libroId")]
        public string LibroId { get; set; }

        [FirestoreProperty("fechaPrestamo")]
        public Timestamp FechaPrestamo { get; set; }

        [FirestoreProperty("fechaDevolucionEsperada")]
        public Timestamp FechaDevolucionEsperada { get; set; }

        [FirestoreProperty("fechaDevolucionReal")]
        public Timestamp? FechaDevolucionReal { get; set; }

        [FirestoreProperty("diasRetraso")]
        public int DiasRetraso { get; set; }

        [FirestoreProperty("multaGenerada")]
        public double MultaGenerada { get; set; }

        [FirestoreProperty("estado")]
        public string Estado { get; set; } // "activo", "devuelto", "vencido"

        [FirestoreProperty("renovaciones")]
        public int Renovaciones { get; set; }
    }
}
