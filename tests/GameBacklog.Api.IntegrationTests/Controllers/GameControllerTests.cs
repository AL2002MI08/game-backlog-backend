using System.Net;
using System.Net.Http.Json;
using GameBacklog.Api.Dtos;
using GameBacklog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using GameBacklog.Api.IntegrationTests.Fixtures;
using GameBacklog.Api.IntegrationTests.Helpers;
using static GameBacklog.Api.IntegrationTests.Helpers.TestHelpers;

namespace GameBacklog.Api.IntegrationTests.Controllers {
    [Collection(ApiCollection.Name)]
    public class GameControllerTests(GameBacklogApplicationFactory factory)
    {
        private const string NotImplemented = "Skipped tests.";

        // ---------- Create ----------

        [Fact]
        public async Task Create_ValidGame_Returns201AndSavesGame()
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostAsJsonAsync("/api/games", new { title = "Hades", platform = "EPIC" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(1, await CountGamesAsync(email));
        }

        [Fact]
        public async Task Create_NoStatus_DefaultsToUnplayed()
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var game = await client.CreateGameAsync();

            Assert.Equal(GameStatus.UNPLAYED, game.Status);
            var saved = await factory.UseDbAsync(db => db.Games.SingleAsync(g => g.Id == game.Id));
            Assert.Equal(GameStatus.UNPLAYED, saved.Status);
        }

        [Fact]
        public async Task Create_SameTitleAndPlatform_Returns409()
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);
            await client.CreateGameAsync("Hades", "EPIC");

            var response = await client.PostAsJsonAsync("/api/games", new { title = "Hades", platform = "EPIC" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(1, await CountGamesAsync(email));
        }

        [Theory(Skip = NotImplemented)]
        [InlineData("")]
        [InlineData("Escape")]
        public async Task Create_InvalidObjective_RollsBackWholeGame(string secondLabel)
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var title = $"Rollback {Guid.NewGuid()}";

            var response = await client.PostAsJsonAsync("/api/games",
                new { title, platform = "EPIC", objectives = new[] { new { label = "Escape" }, new { label = secondLabel } } });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await factory.UseDbAsync(db => db.Games.CountAsync(g => g.Title == title)));
        }

        [Fact(Skip = NotImplemented)]
        public async Task Create_RatingOnUnplayed_Returns400()
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync("/api/games", new { title = "Hades", platform = "EPIC", status = "UNPLAYED", rating = 8 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- Read ----------

        [Fact]
        public async Task GetById_OwnGame_Returns200()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.GetAsync($"/api/games/{game.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_OtherUsersGame_Returns404()
        {
            var owner = await factory.CreateAuthenticatedClientAsync();
            var otherUser = await factory.CreateAuthenticatedClientAsync();
            var game = await owner.CreateGameAsync();

            var response = await otherUser.GetAsync($"/api/games/{game.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- Update (PATCH) ----------

        [Fact]
        public async Task Update_OtherUsersGame_Returns404AndLeavesGameUnchanged()
        {
            var owner = await factory.CreateAuthenticatedClientAsync();
            var otherUser = await factory.CreateAuthenticatedClientAsync();
            var game = await owner.CreateGameAsync();

            var response = await otherUser.PatchAsJsonAsync($"/api/games/{game.Id}", new { rating = 1 });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var saved = await factory.UseDbAsync(db => db.Games.SingleAsync(g => g.Id == game.Id));
            Assert.Null(saved.Rating);
        }

        [Fact(Skip = NotImplemented)]
        public async Task Update_DuplicateTitleAndPlatform_Returns409()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            await client.CreateGameAsync("Hades", "EPIC");
            var game = await client.CreateGameAsync("Celeste", "EPIC");

            var response = await client.PatchAsJsonAsync($"/api/games/{game.Id}", new { title = "Hades" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact(Skip = NotImplemented)]
        public async Task Update_RatingOnUnplayed_Returns400()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.PatchAsJsonAsync($"/api/games/{game.Id}", new { rating = 8 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_OwnGame_Returns204AndRemovesGame()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.DeleteAsync($"/api/games/{game.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.False(await factory.UseDbAsync(db => db.Games.AnyAsync(g => g.Id == game.Id)));
        }

        [Fact]
        public async Task Delete_GameWithObjectives_Returns204AndRemovesObjectives()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            await client.AddObjectiveAsync(game.Id, "Escape");

            var response = await client.DeleteAsync($"/api/games/{game.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(0, await factory.UseDbAsync(db => db.Objectives.CountAsync(o => o.GameId == game.Id)));
        }

        [Fact]
        public async Task Delete_NonExistingGame_Returns404()
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var response = await client.DeleteAsync($"/api/games/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- Validation ----------

        [Theory]
        [InlineData("""{ "platform": "EPIC" }""")]
        [InlineData("""{ "title": "", "platform": "EPIC" }""")]
        [InlineData("""{ "title": "Hades" }""")]
        public async Task Create_MissingRequiredField_Returns400(string json)
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostRawJsonAsync("/api/games", json);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await CountGamesAsync(email));
        }

        [Fact]
        public async Task Create_InvalidPlatform_Returns400()
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostAsJsonAsync("/api/games", new { title = "Hades", platform = "NINTENDO" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await CountGamesAsync(email));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("11")]
        [InlineData("7.5")]
        public async Task Create_InvalidRating_Returns400(string rating)
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostRawJsonAsync("/api/games", $$"""{ "title": "Hades", "platform": "EPIC", "rating": {{rating}} }""");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await CountGamesAsync(email));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task Create_ValidRating_Returns201(int rating)
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync("/api/games", new { title = "Hades", platform = "EPIC", status = "FINISHED", rating });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private Task<int> CountGamesAsync(string email) =>
            factory.UseDbAsync(db => db.Games.CountAsync(g => db.Users.Any(u => u.Id == g.UserId && u.Email == email)));
    }
}
