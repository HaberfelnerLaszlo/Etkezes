using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Etkezes_API.Migrations
{
    /// <inheritdoc />
    public partial class v0_5_change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LoginUsers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Elfogyasztva",
                table: "Etkezesek",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SyncDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SyncDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Table = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncDatas", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Etkezesek_UserId",
                table: "Etkezesek",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Etkezesek_Users_UserId",
                table: "Etkezesek",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etkezesek_Users_UserId",
                table: "Etkezesek");

            migrationBuilder.DropTable(
                name: "SyncDatas");

            migrationBuilder.DropIndex(
                name: "IX_Etkezesek_UserId",
                table: "Etkezesek");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LoginUsers");

            migrationBuilder.DropColumn(
                name: "Elfogyasztva",
                table: "Etkezesek");
        }
    }
}
