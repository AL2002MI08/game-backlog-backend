using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameBacklog.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using GameBacklog.Api.IntegrationTests.Fixtures;
using GameBacklog.Api.IntegrationTests.Helpers;
using static GameBacklog.Api.IntegrationTests.Helpers.TestHelpers;

namespace GameBacklog.Api.IntegrationTests.Controllers {
    [Collection(ApiCollection.Name)]
    public class AuthControllerTests(GameBacklogApplicationFactory factory)
    {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly string _email = NewEmail();

        [Fact]
        public async Task Register_ValidRequest_Returns201AndSavesHashedPassword()
        {
            var response = await _client.RegisterAsync(_email);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var user = await factory.UseDbAsync(db => db.Users.SingleAsync(u => u.Email == _email));
            Assert.NotEqual(Password, user.PasswordHash);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            await _client.RegisterAsync(_email);

            var response = await _client.RegisterAsync(_email);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(1, await factory.UseDbAsync(db => db.Users.CountAsync(u => u.Email == _email)));
        }

        [Theory]
        [InlineData("not-an-email", "Passw0rd!")]
        [InlineData("valid@example.com", "short")]
        [InlineData("valid@example.com", "nouppercase1!")]
        public async Task Register_InvalidEmailOrPassword_Returns400(string email, string password)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/register", new { email, password, confirmPassword = password });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            await _client.RegisterAsync(_email);

            var response = await _client.LoginAsync(_email, Password);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.ReadAsync<LoginResponse>();
            Assert.False(string.IsNullOrEmpty(body.Token));
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            await _client.RegisterAsync(_email);

            var response = await _client.LoginAsync(_email, "WrongPassw0rd!");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_UnknownEmail_Returns401()
        {
            var response = await _client.LoginAsync(NewEmail(), Password);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_NoToken_Returns401()
        {
            var response = await _client.GetAsync("/api/games");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
