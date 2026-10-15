using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameBacklog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnplayedRatingConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Games_UnplayedHasNoRating",
                table: "Games",
                sql: "\"Status\" <> 0 OR \"Rating\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Games_UnplayedHasNoRating",
                table: "Games");
        }
    }
}
