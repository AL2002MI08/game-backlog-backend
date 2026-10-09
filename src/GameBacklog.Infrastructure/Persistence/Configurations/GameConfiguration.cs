using GameBacklog.Domain.Entities;
using GameBacklog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBacklog.Infrastructure.Persistence.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Title).HasMaxLength(100).IsRequired();
            builder.Property(g => g.Platform);
            builder.Property(g => g.Status).HasDefaultValue(GameStatus.UNPLAYED);
            builder.Property(g => g.Notes).HasMaxLength(2000);
            builder.HasOne<User>()
                .WithMany(u => u.Games)
                .HasForeignKey(g => g.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(g => new { g.UserId, g.Title, g.Platform }).IsUnique();

            builder.ToTable(t => t.HasCheckConstraint("CK_Games_Rating", "\"Rating\" BETWEEN 1 AND 10"));
        }
    }
}
