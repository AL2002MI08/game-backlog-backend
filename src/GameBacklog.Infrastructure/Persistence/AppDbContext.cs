using Microsoft.EntityFrameworkCore;
using GameBacklog.Domain.Entities;

namespace GameBacklog.Infrastructure.Persistence {
    public class AppDbContext: DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<Objective> Objectives => Set<Objective>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
