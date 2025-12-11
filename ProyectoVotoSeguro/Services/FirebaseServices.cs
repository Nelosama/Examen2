using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;

namespace ProyectoVotoSeguro.Services;

public class FirebaseServices
{
    private readonly FirestoreDb _firestoreDb;

    public FirebaseServices(IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"];

        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentNullException("Firebase:ProjectId", "Firebase Project ID is not configured in appsettings.json");
        }
        
        // Path to the firebase credentials file in the output directory
        string credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-credentials.json");

        if (File.Exists(credentialPath))
        {
             var builder = new FirestoreDbBuilder
             {
                 ProjectId = projectId,
                 JsonCredentials = File.ReadAllText(credentialPath)
             };
             _firestoreDb = builder.Build();
        }
        else
        {
             // Fallback to environment variables if file not found (though requirements say file provided)
             _firestoreDb = FirestoreDb.Create(projectId);
        }
    }
        
    public FirestoreDb GetFirestoreDb()
    {
        return _firestoreDb;
    }

    public CollectionReference GetCollection(string collectionName)
    {
        return _firestoreDb.Collection(collectionName);   
    }
}