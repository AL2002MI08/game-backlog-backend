using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameBacklog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseInsensitiveGameTitleIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_UserId_Title_Platform",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Games_UserId",
                table: "Games",
                column: "UserId");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Games_UserId_LowerTitle_Platform\" ON \"Games\" (\"UserId\", lower(\"Title\"), \"Platform\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IX_Games_UserId_LowerTitle_Platform\";");

            migrationBuilder.DropIndex(
                name: "IX_Games_UserId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Games_UserId_Title_Platform",
                table: "Games",
                columns: new[] { "UserId", "Title", "Platform" },
                unique: true);
        }
    }
}
