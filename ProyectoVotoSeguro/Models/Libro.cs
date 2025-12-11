using Google.Cloud.Firestore;

namespace ProyectoVotoSeguro.Models
{
    [FirestoreData]
    public class Libro
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("titulo")]
        public string Titulo { get; set; }

        [FirestoreProperty("autor")]
        public string Autor { get; set; }

        [FirestoreProperty("isbn")]
        public string Isbn { get; set; }

        [FirestoreProperty("categoria")]
        public string Categoria { get; set; }

        [FirestoreProperty("editorial")]
        public string Editorial { get; set; }

        [FirestoreProperty("añoPublicacion")]
        public int AñoPublicacion { get; set; }

        [FirestoreProperty("copiasDisponibles")]
        public int CopiasDisponibles { get; set; }

        [FirestoreProperty("copiasTotal")]
        public int CopiasTotal { get; set; }

        [FirestoreProperty("ubicacion")]
        public string Ubicacion { get; set; }

        [FirestoreProperty("estado")]
        public string Estado { get; set; } // "disponible", "agotado", "en mantenimiento"

        [FirestoreProperty("descripcion")]
        public string Descripcion { get; set; }

        [FirestoreProperty("fechaIngreso")]
        public Timestamp FechaIngreso { get; set; }
    }
}
