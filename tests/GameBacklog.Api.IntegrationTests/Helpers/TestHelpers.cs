using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameBacklog.Api.Dtos;
using GameBacklog.Api.IntegrationTests.Fixtures;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace GameBacklog.Api.IntegrationTests.Helpers {
    public static class TestHelpers
    {
        public const string Password = "Passw0rd!";

        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
        };

        public static string NewEmail() => $"{Guid.NewGuid()}@example.com";

        public static async Task<HttpClient> CreateAuthenticatedClientAsync(this GameBacklogApplicationFactory factory, string? email = null)
        {
            email ??= NewEmail();
            var client = factory.CreateClient();

            await client.PostAsJsonAsync("/api/auth/register", new { email, password = Password, confirmPassword = Password });
            var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
            var body = await login.ReadAsync<LoginResponse>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
            return client;
        }

        public static async Task<T> UseDbAsync<T>(this GameBacklogApplicationFactory factory, Func<AppDbContext, Task<T>> query)
        {
            using var scope = factory.Services.CreateScope();
            return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        public static async Task<GameResponse> CreateGameAsync(this HttpClient client, string title = "Hades", string platform = "EPIC")
        {
            var response = await client.PostAsJsonAsync("/api/games", new { title, platform });
            response.EnsureSuccessStatusCode();
            return await response.ReadAsync<GameResponse>();
        }

        public static async Task<ObjectiveResponse> AddObjectiveAsync(this HttpClient client, Guid gameId, string label)
        {
            var response = await client.PostAsJsonAsync($"/api/games/{gameId}/objectives", new { label });
            response.EnsureSuccessStatusCode();
            return await response.ReadAsync<ObjectiveResponse>();
        }

        public static Task<HttpResponseMessage> PostRawJsonAsync(this HttpClient client, string url, string json) =>
            client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

        public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
            (await response.Content.ReadFromJsonAsync<T>(Json))!;

        public static Task<HttpResponseMessage> RegisterAsync(
            this HttpClient client, 
            string email, 
            string password = Password) =>
            client.PostAsJsonAsync("/api/auth/register", new { email, password, confirmPassword = password });

        public static Task<HttpResponseMessage> LoginAsync(
            this HttpClient client, 
            string email, 
            string password = Password) =>
            client.PostAsJsonAsync("/api/auth/login", new { email, password });
    }
}
