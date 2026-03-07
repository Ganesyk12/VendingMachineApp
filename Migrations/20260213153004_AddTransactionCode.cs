using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendingMachineApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrxCode",
                schema: "VendingMachine",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrxCode",
                schema: "VendingMachine",
                table: "Transactions");
        }
    }
}
