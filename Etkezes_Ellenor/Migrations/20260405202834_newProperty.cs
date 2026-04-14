using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etkezes_Ellenor.Migrations
{
    /// <inheritdoc />
    public partial class newProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Elfogyasztva",
                table: "Etkezesek",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Elfogyasztva",
                table: "Etkezesek");
        }
    }
}
