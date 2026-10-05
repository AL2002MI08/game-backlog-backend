using GameBacklog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBacklog.Infrastructure.Persistence.Configurations
{

    public class ObjectiveConfiguration : IEntityTypeConfiguration<Objective>
    {
        public void Configure(EntityTypeBuilder<Objective> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Label).HasMaxLength(200).IsRequired();

            builder.HasOne<Game>()
                .WithMany()
                .HasForeignKey(o => o.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(o => new { o.GameId, o.Label }).IsUnique();
        }
    }
}
