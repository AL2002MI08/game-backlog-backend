using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GameBacklog.Api.Extensions {
    public static class JwtAuthExtension {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
        {
            var jwtKey   = config["JWT_KEY"] ?? throw new InvalidOperationException("JWT_KEY is missing.");
            var issuer   = config["JWT_ISSUER"] ?? throw new InvalidOperationException("JWT_ISSUER is missing.");
            var audience = config["JWT_AUDIENCE"] ?? throw new InvalidOperationException("JWT_AUDIENCE is missing.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    };
                });
            services.AddAuthorization();

            return services;
        }
    }
}
