using GameBacklog.Api.Dtos;
using GameBacklog.Api.Exceptions;
using GameBacklog.Api.Services.Interfaces;
using GameBacklog.Domain.Entities;
using GameBacklog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameBacklog.Api.Services {
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, ITokenService tokenService){
            _context = context;
            _tokenService = tokenService;
        }
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLower();
            var userExists = await _context.Users.AnyAsync(user => user.Email == email);
            if (userExists)
            {
                throw new ConflictException("Email is already registered.");
            }
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new RegisterResponse(true, "Account created successfully");
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request) {
            var email = request.Email.Trim().ToLower();
            var user = await _context.Users.SingleOrDefaultAsync(user => user.Email == email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }
            return CreateResponse(user);
        }

        private LoginResponse CreateResponse(User user)
        {
            var (token, expiresAt) = _tokenService.CreateToken(user);
            return new LoginResponse(token, user.Id, user.Email, expiresAt);
        }
    }
}
