using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VendingMachineApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthorizeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_IdUser",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "vendingmachine");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "vendingmachine",
                table: "Transactions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                schema: "vendingmachine",
                table: "Transactions",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                schema: "vendingmachine",
                table: "Transactions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "vendingmachine",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserCreated",
                schema: "vendingmachine",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserModified",
                schema: "vendingmachine",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                schema: "vendingmachine",
                table: "Products",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                schema: "vendingmachine",
                table: "Products",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "vendingmachine",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserCreated",
                schema: "vendingmachine",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserModified",
                schema: "vendingmachine",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "vendingmachine",
                columns: table => new
                {
                    IdUser = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => x.IdUser);
                });

            migrationBuilder.CreateTable(
                name: "BalanceHistories",
                schema: "vendingmachine",
                columns: table => new
                {
                    IdBalanceHistory = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    DebitBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    CreditBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceHistories", x => x.IdBalanceHistory);
                    table.ForeignKey(
                        name: "FK_BalanceHistories_UserLogins_IdUser",
                        column: x => x.IdUser,
                        principalSchema: "vendingmachine",
                        principalTable: "UserLogins",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBalances",
                schema: "vendingmachine",
                columns: table => new
                {
                    IdUserBalance = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUser = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserCreated = table.Column<string>(type: "text", nullable: true),
                    UserModified = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBalances", x => x.IdUserBalance);
                    table.ForeignKey(
                        name: "FK_UserBalances_UserLogins_IdUser",
                        column: x => x.IdUser,
                        principalSchema: "vendingmachine",
                        principalTable: "UserLogins",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalanceHistories_IdUser",
                schema: "vendingmachine",
                table: "BalanceHistories",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalances_IdUser",
                schema: "vendingmachine",
                table: "UserBalances",
                column: "IdUser",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UserLogins_IdUser",
                schema: "vendingmachine",
                table: "Transactions",
                column: "IdUser",
                principalSchema: "vendingmachine",
                principalTable: "UserLogins",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UserLogins_IdUser",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "BalanceHistories",
                schema: "vendingmachine");

            migrationBuilder.DropTable(
                name: "UserBalances",
                schema: "vendingmachine");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "vendingmachine");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DateModified",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserModified",
                schema: "vendingmachine",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                schema: "vendingmachine",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DateModified",
                schema: "vendingmachine",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "vendingmachine",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                schema: "vendingmachine",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UserModified",
                schema: "vendingmachine",
                table: "Products");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "vendingmachine",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "vendingmachine",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Balance = table.Column<decimal>(type: "numeric", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

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
    }
}
