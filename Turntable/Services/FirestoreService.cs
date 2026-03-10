using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Turntable.Models;

namespace Turntable.Services;

public class FirestoreService
{
    private const string ProjectId = "turntable-32899";
    private readonly HttpClient _httpClient;

    public FirestoreService()
    {
        _httpClient = new HttpClient();
    }

    public async Task CreateUserDocument(UserModel user, string idToken)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/users?documentId={user.Uid}";

        var firestorePayload = new
        {
            fields = new
            {
                email = new { stringValue = user.Email },
                role = new { stringValue = user.Role }
            }
        };

        var json = JsonConvert.SerializeObject(firestorePayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }
    }

    public async Task<string> GetUserRole(string uid, string idToken)
    {
        var url = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/users/{uid}";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var doc = JObject.Parse(content);
            return doc["fields"]["role"]["stringValue"].ToString();
        }

        return "Collector";
    }
}