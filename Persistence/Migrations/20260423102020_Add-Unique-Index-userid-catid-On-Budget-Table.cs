using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexuseridcatidOnBudgetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budget_UserId",
                table: "Budget");

            migrationBuilder.CreateIndex(
                name: "IX_Budget_UserId_CategoryId",
                table: "Budget",
                columns: new[] { "UserId", "CategoryId" },
                unique: true,
                filter: "[CategoryId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budget_UserId_CategoryId",
                table: "Budget");

            migrationBuilder.CreateIndex(
                name: "IX_Budget_UserId",
                table: "Budget",
                column: "UserId");
        }
    }
}
