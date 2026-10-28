using GameBacklog.Domain.Entities;
using GameBacklog.Infrastructure.IntegrationTests.Fixtures;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static GameBacklog.Infrastructure.IntegrationTests.Fixtures.TestDatabase;

namespace GameBacklog.Infrastructure.IntegrationTests.Persistence;

public class UserConfigurationTests : IDisposable
{
    private readonly AppDbContext _context = TestDatabase.CreateContext();
    private readonly Guid _userId;

    public UserConfigurationTests()
    {
        _userId = AddUser(_context);
    }

    public void Dispose() => DeleteUser(_context, _userId);

    [Fact]
    public async Task Save_DuplicateEmail_ThrowsUniqueViolation()
    {
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = $"{_userId}@example.com",
            PasswordHash = "not-used",
            CreatedAt = DateTime.UtcNow,
        });

        Assert.Equal(PostgresErrorCodes.UniqueViolation, await SaveAndGetErrorCodeAsync(_context));
    }

    [Fact]
    public async Task Delete_User_RemovesGames()
    {
        _context.Games.Add(NewGame(_userId));
        await _context.SaveChangesAsync();

        await _context.Users.Where(u => u.Id == _userId).ExecuteDeleteAsync();

        Assert.False(await _context.Games.AnyAsync(g => g.UserId == _userId));
    }
}
