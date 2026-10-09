using GameBacklog.Api.Dtos;
using GameBacklog.Api.Exceptions;
using GameBacklog.Api.Services;
using GameBacklog.Api.UnitTests.Fixtures;
using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.UnitTests.Services;

public class GameServiceTests : IDisposable
{
    private readonly AppDbContext _context = TestDatabase.CreateContext();
    private readonly GameService _sut;
    private readonly Guid _userId;

    public GameServiceTests()
    {
        _sut = new GameService(_context);
        _userId = CreateUser();
    }

    public void Dispose()
    {
        _context.Users.Where(u => u.Id == _userId).ExecuteDelete();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateGameAsync_StatusPlaying_SetsStartedAt()
    {
        var game = await _sut.CreateGameAsync(_userId, NewGame(GameStatus.PLAYING));

        Assert.NotNull(game.StartedAt);
        Assert.Null(game.FinishedAt);
    }

    [Fact]
    public async Task CreateGameAsync_DuplicateTitleAndPlatform_ThrowsConflict()
    {
        await _sut.CreateGameAsync(_userId, NewGame());

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateGameAsync(_userId, NewGame()));
    }

    [Fact]
    public async Task GetGameAsync_OtherUsersGame_ThrowsNotFound()
    {
        var game = await _sut.CreateGameAsync(_userId, NewGame());

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetGameAsync(Guid.NewGuid(), game.Id));
    }

    [Fact]
    public async Task UpdateGameAsync_StatusFinished_SetsFinishedAt()
    {
        var game = await _sut.CreateGameAsync(_userId, NewGame(GameStatus.PLAYING));

        var updated = await _sut.UpdateGameAsync(_userId, game.Id, new UpdateGameRequest { Status = GameStatus.FINISHED });

        Assert.NotNull(updated.FinishedAt);
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

    private static CreateGameRequest NewGame(GameStatus status = GameStatus.UNPLAYED) =>
        new() { Title = "Elden Ring", Platform = Platform.STEAM, Status = status };
}
