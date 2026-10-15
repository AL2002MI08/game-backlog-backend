using GameBacklog.Domain.Entities;
using GameBacklog.Infrastructure.IntegrationTests.Fixtures;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static GameBacklog.Infrastructure.IntegrationTests.Fixtures.TestDatabase;

namespace GameBacklog.Infrastructure.IntegrationTests.Persistence;

public class ObjectiveConfigurationTests : IDisposable
{
    private readonly AppDbContext _context = TestDatabase.CreateContext();
    private readonly Guid _userId;
    private readonly Game _game;

    public ObjectiveConfigurationTests()
    {
        _userId = AddUser(_context);
        _game = NewGame(_userId);
        _context.Games.Add(_game);
        _context.SaveChanges();
    }

    public void Dispose() => DeleteUser(_context, _userId);

    [Fact]
    public async Task Save_DuplicateLabel_ThrowsUniqueViolation()
    {
        _context.Objectives.Add(NewObjective(_game.Id));
        await _context.SaveChangesAsync();

        _context.Objectives.Add(NewObjective(_game.Id));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, await SaveAndGetErrorCodeAsync(_context));
    }

    [Fact]
    public async Task Save_SameLabelOtherGame_Succeeds()
    {
        var otherGame = NewGame(_userId, title: "Hades");
        _context.Games.Add(otherGame);
        _context.Objectives.Add(NewObjective(_game.Id));
        _context.Objectives.Add(NewObjective(otherGame.Id));
        await _context.SaveChangesAsync();

        Assert.Equal(2, await _context.Objectives.CountAsync(o => o.GameId == _game.Id || o.GameId == otherGame.Id));
    }

    [Fact]
    public async Task Save_UnknownGame_ThrowsForeignKeyViolation()
    {
        _context.Objectives.Add(NewObjective(Guid.NewGuid()));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, await SaveAndGetErrorCodeAsync(_context));
    }

    [Fact]
    public async Task Save_LabelOver200Chars_ThrowsTooLong()
    {
        _context.Objectives.Add(NewObjective(_game.Id, label: new string('x', 201)));

        Assert.Equal(PostgresErrorCodes.StringDataRightTruncation, await SaveAndGetErrorCodeAsync(_context));
    }
}
