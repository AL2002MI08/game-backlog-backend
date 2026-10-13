using System.Net;
using System.Net.Http.Json;
using GameBacklog.Api.Dtos;
using GameBacklog.Api.IntegrationTests.Fixtures;
using GameBacklog.Api.IntegrationTests.Helpers;
using GameBacklog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using static GameBacklog.Api.IntegrationTests.Helpers.TestHelpers;

namespace GameBacklog.Api.IntegrationTests.Controllers {
    [Collection(ApiCollection.Name)]
    public class GameControllerTests(GameBacklogApplicationFactory factory)
    {
        // ---------- Create ----------

        [Fact]
        public async Task Create_WithObjectives_SavesGameAndObjectives()
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync("/api/games",
                new { title = "Hades", platform = "EPIC", objectives = new[] { new { label = "Escape" }, new { label = "Beat Hades" } } });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var game = await response.ReadAsync<GameResponse>();
            Assert.Equal(2, await factory.UseDbAsync(db => db.Objectives.CountAsync(o => o.GameId == game.Id)));
        }

        [Fact]
        public async Task Create_NewGame_IsUnplayed()
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var game = await client.CreateGameAsync();

            Assert.Equal(GameStatus.UNPLAYED, game.Status);
        }

        [Fact]
        public async Task Create_SameTitleAndPlatform_Returns409()
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);
            await client.CreateGameAsync("Hades", "EPIC");

            var response = await client.PostAsJsonAsync("/api/games", new { title = "Hades", platform = "EPIC" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(1, await factory.CountGamesAsync(email));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Escape")]
        public async Task Create_InvalidObjective_SavesNothing(string secondLabel)
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostAsJsonAsync("/api/games",
                new { title = "Hades", platform = "EPIC", objectives = new[] { new { label = "Escape" }, new { label = secondLabel } } });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await factory.CountGamesAsync(email));
        }

        [Fact]
        public async Task Create_RatingWhileUnplayed_Returns400()
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostAsJsonAsync("/api/games", new { title = "Hades", platform = "EPIC", status = "UNPLAYED", rating = 8 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await factory.CountGamesAsync(email));
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
            Assert.Equal(0, await factory.CountGamesAsync(email));
        }

        [Theory]
        [InlineData("""{ "title": "Hades", "platform": "NINTENDO" }""")]
        [InlineData("""{ "title": "Hades", "platform": "EPIC", "status": "WON" }""")]
        public async Task Create_InvalidEnumValue_Returns400(string json)
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostRawJsonAsync("/api/games", json);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await factory.CountGamesAsync(email));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("11")]
        [InlineData("7.5")]
        public async Task Create_InvalidRating_Returns400(string rating)
        {
            var email = NewEmail();
            var client = await factory.CreateAuthenticatedClientAsync(email);

            var response = await client.PostRawJsonAsync("/api/games", $$"""{ "title": "Hades", "platform": "EPIC", "status": "PLAYING", "rating": {{rating}} }""");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, await factory.CountGamesAsync(email));
        }

        // ---------- Ownership ----------

        [Fact]
        public async Task Get_AnotherUsersGame_Returns404()
        {
            var owner = await factory.CreateAuthenticatedClientAsync();
            var otherUser = await factory.CreateAuthenticatedClientAsync();
            var game = await owner.CreateGameAsync();

            var response = await otherUser.GetAsync($"/api/games/{game.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_AnotherUsersGame_Returns404()
        {
            var owner = await factory.CreateAuthenticatedClientAsync();
            var otherUser = await factory.CreateAuthenticatedClientAsync();
            var game = await owner.CreateGameAsync();

            var response = await otherUser.PutAsJsonAsync($"/api/games/{game.Id}", new { title = "Hades", platform = "EPIC", notes = "Not mine" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var saved = await factory.UseDbAsync(db => db.Games.SingleAsync(g => g.Id == game.Id));
            Assert.Null(saved.Notes);
        }

        [Fact]
        public async Task GetLibrary_AnotherUsersGames_AreHidden()
        {
            var owner = await factory.CreateAuthenticatedClientAsync();
            var otherUser = await factory.CreateAuthenticatedClientAsync();
            await owner.CreateGameAsync();

            var library = await otherUser.GetAsync<PagedResponse<GameListResponse>>("/api/games");

            Assert.Empty(library.Items);
        }

        // ---------- Status ----------

        [Fact]
        public async Task ChangeStatus_UnplayedToPlaying_SetsStartDate()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.ChangeStatusAsync(game.Id, "PLAYING");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull((await response.ReadAsync<UpdateGameStatusResponse>()).Game.StartedAt);
        }

        [Fact]
        public async Task ChangeStatus_EveryChange_RecordsHistory()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            await client.ChangeStatusAsync(game.Id, "PLAYING");
            await client.ChangeStatusAsync(game.Id, "FINISHED");

            var history = await factory.UseDbAsync(db => db.GameStatusHistories
                .Where(h => h.GameId == game.Id)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync());
            Assert.Equal(2, history.Count);
            Assert.Equal((GameStatus.UNPLAYED, GameStatus.PLAYING), (history[0].PreviousStatus, history[0].NewStatus));
            Assert.Equal((GameStatus.PLAYING, GameStatus.FINISHED), (history[1].PreviousStatus, history[1].NewStatus));
        }

        [Fact]
        public async Task ChangeStatus_FinishWithUntickedObjectives_ReportsOutstanding()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            await client.AddObjectiveAsync(game.Id, "Escape");

            var response = await client.ChangeStatusAsync(game.Id, "FINISHED");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True((await response.ReadAsync<UpdateGameStatusResponse>()).HasOutstandingObjectives);
        }

        [Fact]
        public async Task ChangeStatus_ToCurrentStatus_Returns409()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.ChangeStatusAsync(game.Id, "UNPLAYED");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_BackToUnplayed_ClearsRating()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            await client.ChangeStatusAsync(game.Id, "PLAYING");
            await client.PutAsJsonAsync($"/api/games/{game.Id}", new { title = "Hades", platform = "EPIC", rating = 8 });

            var response = await client.ChangeStatusAsync(game.Id, "UNPLAYED");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null((await response.ReadAsync<UpdateGameStatusResponse>()).Game.Rating);
        }

        // ---------- Progress ----------

        [Fact]
        public async Task GetProgress_OneOfThreeComplete_Returns33Percent()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            var first = await client.AddObjectiveAsync(game.Id, "Escape");
            await client.AddObjectiveAsync(game.Id, "Beat Hades");
            await client.AddObjectiveAsync(game.Id, "Max a weapon");
            await client.CompleteObjectiveAsync(game.Id, first.Id);

            var progress = await client.GetAsync<CompletionProgressResponse>($"/api/games/{game.Id}/progress");

            Assert.Equal(3, progress.Total);
            Assert.Equal(1, progress.Completed);
            Assert.Equal(33, progress.Percentage);
        }

        [Fact]
        public async Task GetProgress_NoObjectives_ReturnsNullPercentage()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var progress = await client.GetAsync<CompletionProgressResponse>($"/api/games/{game.Id}/progress");

            Assert.Equal(0, progress.Total);
            Assert.Null(progress.Percentage);
        }

        [Fact]
        public async Task GetLibrary_WithObjectives_MatchesDetailProgress()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            var first = await client.AddObjectiveAsync(game.Id, "Escape");
            await client.AddObjectiveAsync(game.Id, "Beat Hades");
            await client.CompleteObjectiveAsync(game.Id, first.Id);

            var library = await client.GetAsync<PagedResponse<GameListResponse>>("/api/games");
            var detail = await client.GetAsync<GameDetailResponse>($"/api/games/{game.Id}");

            Assert.Equal(detail.Progress, Assert.Single(library.Items).Progress);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task Delete_GameWithObjectives_DeletesObjectives()
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
    }
}
