using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iei.Migrations
{
    /// <inheritdoc />
    public partial class unico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Provincia_Nombre",
                table: "Provincia",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Localidad_Nombre_ProvinciaId",
                table: "Localidad",
                columns: new[] { "Nombre", "ProvinciaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Provincia_Nombre",
                table: "Provincia");

            migrationBuilder.DropIndex(
                name: "IX_Localidad_Nombre_ProvinciaId",
                table: "Localidad");
        }
    }
}
