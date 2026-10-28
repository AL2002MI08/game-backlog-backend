using GameBacklog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameBacklog.Infrastructure.Persistence.Configurations
{
    public class GameStatusHistoryConfiguration : IEntityTypeConfiguration<GameStatusHistory>
    {
        public void Configure(EntityTypeBuilder<GameStatusHistory> builder)
        {
            builder.HasKey(h => h.Id);
            builder.HasOne<Game>()
                .WithMany()
                .HasForeignKey(h => h.GameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
