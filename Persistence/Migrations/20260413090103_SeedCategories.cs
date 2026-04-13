using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "id", "CreatedAt", "Name", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2026, 4, 13, 9, 1, 2, 207, DateTimeKind.Unspecified).AddTicks(2682), new TimeSpan(0, 0, 0, 0, 0)), "Food & Drinks", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { 2, new DateTimeOffset(new DateTime(2026, 4, 13, 9, 1, 2, 207, DateTimeKind.Unspecified).AddTicks(2686), new TimeSpan(0, 0, 0, 0, 0)), "Transportation", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { 3, new DateTimeOffset(new DateTime(2026, 4, 13, 9, 1, 2, 207, DateTimeKind.Unspecified).AddTicks(2687), new TimeSpan(0, 0, 0, 0, 0)), "Shopping", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { 4, new DateTimeOffset(new DateTime(2026, 4, 13, 9, 1, 2, 207, DateTimeKind.Unspecified).AddTicks(2689), new TimeSpan(0, 0, 0, 0, 0)), "Housing & Utilities", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { 5, new DateTimeOffset(new DateTime(2026, 4, 13, 9, 1, 2, 207, DateTimeKind.Unspecified).AddTicks(2690), new TimeSpan(0, 0, 0, 0, 0)), "Entertainment", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Category",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Category",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Category",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Category",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Category",
                keyColumn: "id",
                keyValue: 5);
        }
    }
}
