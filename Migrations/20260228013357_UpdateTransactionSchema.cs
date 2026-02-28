using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VendingMachineApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "vendingmachine");

            migrationBuilder.CreateTable(
                name: "UserTransactions",
                schema: "vendingmachine",
                columns: table => new
                {
                    IdTransaction = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    TrxCode = table.Column<string>(type: "text", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BalanceAfterTransaction = table.Column<decimal>(type: "numeric", nullable: true),
                    TransactionType = table.Column<string>(type: "text", nullable: true),
                    UserCreated = table.Column<string>(type: "text", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTransactions", x => x.IdTransaction);
                    table.ForeignKey(
                        name: "FK_UserTransactions_UserLogins_IdUser",
                        column: x => x.IdUser,
                        principalSchema: "vendingmachine",
                        principalTable: "UserLogins",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionDetails",
                schema: "vendingmachine",
                columns: table => new
                {
                    IdDetail = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdTransaction = table.Column<int>(type: "integer", nullable: false),
                    IdProduct = table.Column<int>(type: "integer", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDetails", x => x.IdDetail);
                    table.ForeignKey(
                        name: "FK_TransactionDetails_Products_IdProduct",
                        column: x => x.IdProduct,
                        principalSchema: "vendingmachine",
                        principalTable: "Products",
                        principalColumn: "IdProduct");
                    table.ForeignKey(
                        name: "FK_TransactionDetails_UserTransactions_IdTransaction",
                        column: x => x.IdTransaction,
                        principalSchema: "vendingmachine",
                        principalTable: "UserTransactions",
                        principalColumn: "IdTransaction",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDetails_IdProduct",
                schema: "vendingmachine",
                table: "TransactionDetails",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDetails_IdTransaction",
                schema: "vendingmachine",
                table: "TransactionDetails",
                column: "IdTransaction");

            migrationBuilder.CreateIndex(
                name: "IX_UserTransactions_IdUser",
                schema: "vendingmachine",
                table: "UserTransactions",
                column: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionDetails",
                schema: "vendingmachine");

            migrationBuilder.DropTable(
                name: "UserTransactions",
                schema: "vendingmachine");

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "vendingmachine",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdProduct = table.Column<int>(type: "integer", nullable: true),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceAfterTransaction = table.Column<decimal>(type: "numeric", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: true),
                    TrxCode = table.Column<string>(type: "text", nullable: true),
                    UserCreated = table.Column<string>(type: "text", nullable: false),
                    UserModified = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_Products_IdProduct",
                        column: x => x.IdProduct,
                        principalSchema: "vendingmachine",
                        principalTable: "Products",
                        principalColumn: "IdProduct");
                    table.ForeignKey(
                        name: "FK_Transactions_UserLogins_IdUser",
                        column: x => x.IdUser,
                        principalSchema: "vendingmachine",
                        principalTable: "UserLogins",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IdProduct",
                schema: "vendingmachine",
                table: "Transactions",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IdUser",
                schema: "vendingmachine",
                table: "Transactions",
                column: "IdUser");
        }
    }
}
