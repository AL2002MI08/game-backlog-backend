using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameBacklog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverImageAndLabelIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Objectives_GameId_Label",
                table: "Objectives");

            migrationBuilder.AddColumn<string>(
                name: "CoverImage",
                table: "Games",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_GameId",
                table: "Objectives",
                column: "GameId");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Objectives_GameId_LowerLabel\" ON \"Objectives\" (\"GameId\", lower(\"Label\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IX_Objectives_GameId_LowerLabel\";");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_GameId",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "CoverImage",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_GameId_Label",
                table: "Objectives",
                columns: new[] { "GameId", "Label" },
                unique: true);
        }
    }
}
