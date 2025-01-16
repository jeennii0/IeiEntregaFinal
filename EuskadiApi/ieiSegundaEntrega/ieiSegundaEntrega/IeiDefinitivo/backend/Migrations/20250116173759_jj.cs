using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iei.Migrations
{
    /// <inheritdoc />
    public partial class jj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Localidad_Nombre_ProvinciaId",
                table: "Localidad");

            migrationBuilder.CreateIndex(
                name: "IX_Localidad_Nombre",
                table: "Localidad",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Localidad_Nombre",
                table: "Localidad");

            migrationBuilder.CreateIndex(
                name: "IX_Localidad_Nombre_ProvinciaId",
                table: "Localidad",
                columns: new[] { "Nombre", "ProvinciaId" },
                unique: true);
        }
    }
}
