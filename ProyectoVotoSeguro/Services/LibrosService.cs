using Google.Cloud.Firestore;
using ProyectoVotoSeguro.Models;

namespace ProyectoVotoSeguro.Services
{
    public class LibrosService
    {
        private readonly FirebaseServices _firebaseService;
        private readonly string _collectionName = "libros";

        public LibrosService(FirebaseServices firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public async Task<List<Libro>> GetAllLibros()
        {
            var collection = _firebaseService.GetCollection(_collectionName);
            var snapshot = await collection.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Libro>()).ToList();
        }

        public async Task<Libro?> GetLibroById(string id)
        {
            var docRef = _firebaseService.GetCollection(_collectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists) return null;
            return snapshot.ConvertTo<Libro>();
        }

        public async Task<Libro> AddLibro(Libro libro)
        {
            // Ensure unique ISBN? Firestore doesn't enforce uniqueness on fields other than ID.
            // We should check manually.
            var collection = _firebaseService.GetCollection(_collectionName);
            var query = collection.WhereEqualTo("isbn", libro.Isbn).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            if (snapshot.Count > 0) throw new Exception("Ya existe un libro con este ISBN.");

            libro.FechaIngreso = Timestamp.FromDateTime(DateTime.UtcNow);
            var docRef = await collection.AddAsync(libro);
            libro.Id = docRef.Id;
            return libro;
        }

        public async Task UpdateLibro(string id, Libro libro)
        {
            var docRef = _firebaseService.GetCollection(_collectionName).Document(id);
            await docRef.SetAsync(libro, SetOptions.MergeAll);
        }

        public async Task DeleteLibro(string id)
        {
            var docRef = _firebaseService.GetCollection(_collectionName).Document(id);
            await docRef.DeleteAsync();
        }
    }
}
