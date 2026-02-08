using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendingMachineApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameTransactionKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Products_ProductIdProduct",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ProductIdProduct",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ProductIdProduct",
                table: "Transactions");

            migrationBuilder.EnsureSchema(
                name: "vendingmachine");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "vendingmachine");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transactions",
                newSchema: "vendingmachine");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "vendingmachine");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "vendingmachine",
                table: "Transactions",
                newName: "IdUser");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                schema: "vendingmachine",
                table: "Transactions",
                newName: "IX_Transactions_IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IdProduct",
                schema: "vendingmachine",
                table: "Transactions",
                column: "IdProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Products_IdProduct",
                schema: "vendingmachine",
                table: "Transactions",
                column: "IdProduct",
                principalSchema: "vendingmachine",
                principalTable: "Products",
                principalColumn: "IdProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_IdUser",
                schema: "vendingmachine",
                table: "Transactions",
                column: "IdUser",
                principalSchema: "vendingmachine",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Products_IdProduct",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_IdUser",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_IdProduct",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "vendingmachine",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Transactions",
                schema: "vendingmachine",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "vendingmachine",
                newName: "Products");

            migrationBuilder.RenameColumn(
                name: "IdUser",
                table: "Transactions",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_IdUser",
                table: "Transactions",
                newName: "IX_Transactions_UserId");

            migrationBuilder.AddColumn<int>(
                name: "ProductIdProduct",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ProductIdProduct",
                table: "Transactions",
                column: "ProductIdProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Products_ProductIdProduct",
                table: "Transactions",
                column: "ProductIdProduct",
                principalTable: "Products",
                principalColumn: "IdProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
