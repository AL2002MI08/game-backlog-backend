using GameBacklog.Api.Dtos;
using GameBacklog.Api.Exceptions;
using GameBacklog.Api.Services;
using GameBacklog.Api.UnitTests.Fixtures;
using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.UnitTests.Services;

public class ObjectiveServiceTests : IDisposable
{
    private readonly AppDbContext _context = TestDatabase.CreateContext();
    private readonly ObjectiveService _sut;
    private readonly Guid _userId;
    private readonly Guid _gameId;

    public ObjectiveServiceTests()
    {
        _sut = new ObjectiveService(_context);
        _userId = CreateUser();
        _gameId = CreateGame();
    }

    public void Dispose()
    {
        _context.ChangeTracker.Clear();
        _context.Users.Where(u => u.Id == _userId).ExecuteDelete();
        _context.Dispose();
    }

    [Fact]
    public async Task Create_WithoutPosition_AddsItLast()
    {
        await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Margit"));

        var second = await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Radahn"));

        Assert.Equal(2, second.Position);
    }

    [Fact]
    public async Task Create_SameLabelTwice_ThrowsConflict()
    {
        await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Margit"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Margit")));
    }

    [Fact]
    public async Task Get_AnotherUsersGame_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetObjectivesAsync(Guid.NewGuid(), _gameId));
    }

    [Fact]
    public async Task Complete_AlreadyCompleted_ThrowsBadRequest()
    {
        var objective = await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Margit"));
        await _sut.CompleteObjectiveAsync(_userId, _gameId, objective.Id);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.CompleteObjectiveAsync(_userId, _gameId, objective.Id));
    }

    [Fact]
    public async Task Update_MoveToFirst_ShiftsOthersDown()
    {
        var first = await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Margit"));
        var second = await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Radahn"));

        await _sut.PartialUpdateObjectiveAsync(_userId, _gameId, second.Id, new UpdateObjectiveRequest { Position = 1 });

        var objectives = await _sut.GetObjectivesAsync(_userId, _gameId);
        Assert.Equal([second.Id, first.Id], objectives.Select(o => o.Id));
    }

    [Fact]
    public async Task Reorder_MissingObjective_ThrowsBadRequest()
    {
        var first = await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Margit"));
        await _sut.CreateObjectiveAsync(_userId, _gameId, NewObjective("Beat Radahn"));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ReorderObjectivesAsync(_userId, _gameId, new ReorderObjectivesRequest { ObjectiveIds = [first.Id] }));
    }

    private Guid CreateUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@example.com",
            PasswordHash = "not-used",
            CreatedAt = DateTime.UtcNow,
        };
        _context.Users.Add(user);
        _context.SaveChanges();
        return user.Id;
    }

    private Guid CreateGame()
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Title = "Elden Ring",
            Platform = Platform.STEAM,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Games.Add(game);
        _context.SaveChanges();
        return game.Id;
    }

    private static CreateObjectiveRequest NewObjective(string label) => new() { Label = label };
}
