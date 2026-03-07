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
                name: "VendingMachine");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "VendingMachine");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transactions",
                newSchema: "VendingMachine");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "VendingMachine");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "VendingMachine",
                table: "Transactions",
                newName: "IdUser");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                schema: "VendingMachine",
                table: "Transactions",
                newName: "IX_Transactions_IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IdProduct",
                schema: "VendingMachine",
                table: "Transactions",
                column: "IdProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Products_IdProduct",
                schema: "VendingMachine",
                table: "Transactions",
                column: "IdProduct",
                principalSchema: "VendingMachine",
                principalTable: "Products",
                principalColumn: "IdProduct");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_IdUser",
                schema: "VendingMachine",
                table: "Transactions",
                column: "IdUser",
                principalSchema: "VendingMachine",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Products_IdProduct",
                schema: "VendingMachine",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_IdUser",
                schema: "VendingMachine",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_IdProduct",
                schema: "VendingMachine",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "VendingMachine",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Transactions",
                schema: "VendingMachine",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "VendingMachine",
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
