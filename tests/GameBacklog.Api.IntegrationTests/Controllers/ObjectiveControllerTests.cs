using System.Net;
using System.Net.Http.Json;
using GameBacklog.Api.Dtos;
using GameBacklog.Api.IntegrationTests.Fixtures;
using GameBacklog.Api.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.IntegrationTests.Controllers {
    [Collection(ApiCollection.Name)]
    public class ObjectiveControllerTests(GameBacklogApplicationFactory factory)
    {
        // ---------- Create ----------

        [Fact]
        public async Task Add_ValidObjective_Returns201()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.PostAsJsonAsync($"/api/games/{game.Id}/objectives", new { label = "Escape" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var objective = await response.ReadAsync<ObjectiveResponse>();
            Assert.Equal("Escape", objective.Label);
            Assert.Equal(1, objective.Position);
        }

        [Fact]
        public async Task Add_ToNonExistingGame_Returns404()
        {
            var client = await factory.CreateAuthenticatedClientAsync();

            var response = await client.PostAsJsonAsync($"/api/games/{Guid.NewGuid()}/objectives", new { label = "Escape" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Add_ToAnotherUsersGame_Returns404()
        {
            var owner = await factory.CreateAuthenticatedClientAsync();
            var otherUser = await factory.CreateAuthenticatedClientAsync();
            var game = await owner.CreateGameAsync();

            var response = await otherUser.PostAsJsonAsync($"/api/games/{game.Id}/objectives", new { label = "Escape" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.False(await factory.UseDbAsync(db => db.Objectives.AnyAsync(o => o.GameId == game.Id)));
        }

        [Fact]
        public async Task Add_DuplicateLabel_Returns409()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            await client.AddObjectiveAsync(game.Id, "Escape");

            var response = await client.PostAsJsonAsync($"/api/games/{game.Id}/objectives", new { label = "Escape" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Add_WithoutLabel_Returns400()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.PostRawJsonAsync($"/api/games/{game.Id}/objectives", "{}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- Read ----------

        [Fact]
        public async Task Get_ExistingObjective_Returns200()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            var objective = await client.AddObjectiveAsync(game.Id, "Escape");

            var response = await client.GetAsync($"/api/games/{game.Id}/objectives/{objective.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_UnknownObjective_Returns404()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();

            var response = await client.GetAsync($"/api/games/{game.Id}/objectives/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_ObjectiveFromAnotherGame_Returns404()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var hades = await client.CreateGameAsync("Hades", "EPIC");
            var celeste = await client.CreateGameAsync("Celeste", "STEAM");
            var objective = await client.AddObjectiveAsync(hades.Id, "Escape");

            var response = await client.GetAsync($"/api/games/{celeste.Id}/objectives/{objective.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------- Complete / Reopen ----------

        [Fact]
        public async Task Tick_Objective_MarksCompleteWithTime()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            var objective = await client.AddObjectiveAsync(game.Id, "Escape");

            var response = await client.CompleteObjectiveAsync(game.Id, objective.Id);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var ticked = await response.ReadAsync<ObjectiveResponse>();
            Assert.True(ticked.Completed);
            Assert.NotNull(ticked.CompletedAt);
        }

        [Fact]
        public async Task Untick_Objective_ClearsCompleteAndTime()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            var objective = await client.AddObjectiveAsync(game.Id, "Escape");
            await client.CompleteObjectiveAsync(game.Id, objective.Id);

            var response = await client.PatchAsync($"/api/games/{game.Id}/objectives/{objective.Id}/reopen", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var unticked = await response.ReadAsync<ObjectiveResponse>();
            Assert.False(unticked.Completed);
            Assert.Null(unticked.CompletedAt);
        }

        // ---------- Reorder ----------

        [Fact]
        public async Task Reorder_AllObjectives_UpdatesEveryPosition()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            var first = await client.AddObjectiveAsync(game.Id, "Escape");
            var second = await client.AddObjectiveAsync(game.Id, "Beat Hades");
            var third = await client.AddObjectiveAsync(game.Id, "Max a weapon");

            var response = await client.PutAsJsonAsync($"/api/games/{game.Id}/objectives/order",
                new { objectiveIds = new[] { third.Id, first.Id, second.Id } });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var objectives = await client.GetAsync<List<ObjectiveResponse>>($"/api/games/{game.Id}/objectives");
            Assert.Equal([third.Id, first.Id, second.Id], objectives.Select(o => o.Id));
            Assert.Equal([1, 2, 3], objectives.Select(o => o.Position));
        }

        [Fact]
        public async Task Reorder_Fails_KeepsOriginalOrder()
        {
            var client = await factory.CreateAuthenticatedClientAsync();
            var game = await client.CreateGameAsync();
            var first = await client.AddObjectiveAsync(game.Id, "Escape");
            var second = await client.AddObjectiveAsync(game.Id, "Beat Hades");

            var response = await client.PutAsJsonAsync($"/api/games/{game.Id}/objectives/order",
                new { objectiveIds = new[] { second.Id } });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var objectives = await client.GetAsync<List<ObjectiveResponse>>($"/api/games/{game.Id}/objectives");
            Assert.Equal([first.Id, second.Id], objectives.Select(o => o.Id));
        }
    }
}
