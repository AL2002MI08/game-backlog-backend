using GameBacklog.Api.Dtos;
using GameBacklog.Api.Exceptions;
using GameBacklog.Api.Services;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Api.UnitTests.Fixtures;
using GameBacklog.Domain.Entities;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.UnitTests.Services;

public class AuthServiceTests : IDisposable
{
    private const string Password = "Passw0rd!";

    private readonly AppDbContext _context = TestDatabase.CreateContext();
    private readonly AuthService _sut;
    private readonly string _email = $"{Guid.NewGuid()}@example.com";

    public AuthServiceTests()
    {
        _sut = new AuthService(_context, new FakeTokenService());
    }

    public void Dispose()
    {
        _context.Users.Where(u => u.Email == _email).ExecuteDelete();
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_NewUser_StoresHashedPassword()
    {
        var result = await _sut.RegisterAsync(new RegisterRequest { Email = _email, Password = Password, ConfirmPassword = Password });

        Assert.True(result.Succeeded);
        var user = await _context.Users.SingleAsync(u => u.Email == _email);
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        await _sut.RegisterAsync(new RegisterRequest { Email = _email, Password = Password, ConfirmPassword = Password });

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.LoginAsync(new LoginRequest { Email = _email, Password = "WrongPassw0rd!" }));
    }

    private class FakeTokenService : ITokenService
    {
        public (string Token, DateTime ExpiresAt) CreateToken(User user) => ("fake-token", DateTime.UtcNow.AddMinutes(30));
    }
}
