using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;
using GameBacklog.Infrastructure.IntegrationTests.Fixtures;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static GameBacklog.Infrastructure.IntegrationTests.Fixtures.TestDatabase;

namespace GameBacklog.Infrastructure.IntegrationTests.Persistence;

public class GameConfigurationTests : IDisposable
{
    private readonly AppDbContext _context = TestDatabase.CreateContext();
    private readonly Guid _userId;

    public GameConfigurationTests()
    {
        _userId = AddUser(_context);
    }

    public void Dispose() => DeleteUser(_context, _userId);

    [Fact]
    public async Task Save_DuplicateGame_ThrowsUniqueViolation()
    {
        _context.Games.Add(NewGame(_userId));
        await _context.SaveChangesAsync();

        _context.Games.Add(NewGame(_userId));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, await SaveAndGetErrorCodeAsync(_context));
    }

    [Fact]
    public async Task Save_SameTitleOtherPlatform_Succeeds()
    {
        _context.Games.Add(NewGame(_userId, platform: Platform.STEAM));
        _context.Games.Add(NewGame(_userId, platform: Platform.XBOX));
        await _context.SaveChangesAsync();

        Assert.Equal(2, await _context.Games.CountAsync(g => g.UserId == _userId));
    }

    [Fact]
    public async Task Save_RatingAbove10_ThrowsCheckViolation()
    {
        var game = NewGame(_userId);
        game.Status = GameStatus.PLAYING;
        game.Rating = 11;
        _context.Games.Add(game);

        Assert.Equal(PostgresErrorCodes.CheckViolation, await SaveAndGetErrorCodeAsync(_context));
    }

    [Fact]
    public async Task Save_RatingOnUnplayed_ThrowsCheckViolation()
    {
        var game = NewGame(_userId);
        game.Rating = 5;
        _context.Games.Add(game);

        Assert.Equal(PostgresErrorCodes.CheckViolation, await SaveAndGetErrorCodeAsync(_context));
    }

    [Fact]
    public async Task Save_TitleOver100Chars_ThrowsTooLong()
    {
        _context.Games.Add(NewGame(_userId, title: new string('x', 101)));

        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, await SaveAndGetErrorCodeAsync(_context));
    }

    [Fact]
    public async Task Delete_Game_RemovesObjectivesAndHistory()
    {
        var game = NewGame(_userId);
        _context.Games.Add(game);
        _context.Objectives.Add(NewObjective(game.Id));
        _context.GameStatusHistories.Add(new GameStatusHistory
        {
            Id = Guid.NewGuid(),
            GameId = game.Id,
            PreviousStatus = GameStatus.UNPLAYED,
            NewStatus = GameStatus.PLAYING,
            ChangedBy = _userId,
            ChangedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        await _context.Games.Where(g => g.Id == game.Id).ExecuteDeleteAsync();

        Assert.False(await _context.Objectives.AnyAsync(o => o.GameId == game.Id));
        Assert.False(await _context.GameStatusHistories.AnyAsync(h => h.GameId == game.Id));
    }
}
