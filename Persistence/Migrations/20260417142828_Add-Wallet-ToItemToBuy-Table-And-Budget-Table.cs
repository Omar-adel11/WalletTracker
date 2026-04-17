using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletToItemToBuyTableAndBudgetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budget_AspNetUsers_UserId",
                table: "Budget");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "ItemToBuy",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "ItemToBuy",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "Budget",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemToBuy_CategoryId",
                table: "ItemToBuy",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemToBuy_WalletId",
                table: "ItemToBuy",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Budget_WalletId",
                table: "Budget",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_AspNetUsers_UserId",
                table: "Budget",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_Wallet_WalletId",
                table: "Budget",
                column: "WalletId",
                principalTable: "Wallet",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemToBuy_Category_CategoryId",
                table: "ItemToBuy",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemToBuy_Wallet_WalletId",
                table: "ItemToBuy",
                column: "WalletId",
                principalTable: "Wallet",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budget_AspNetUsers_UserId",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_Budget_Wallet_WalletId",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemToBuy_Category_CategoryId",
                table: "ItemToBuy");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemToBuy_Wallet_WalletId",
                table: "ItemToBuy");

            migrationBuilder.DropIndex(
                name: "IX_ItemToBuy_CategoryId",
                table: "ItemToBuy");

            migrationBuilder.DropIndex(
                name: "IX_ItemToBuy_WalletId",
                table: "ItemToBuy");

            migrationBuilder.DropIndex(
                name: "IX_Budget_WalletId",
                table: "Budget");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ItemToBuy");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "ItemToBuy");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Budget");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_AspNetUsers_UserId",
                table: "Budget",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
