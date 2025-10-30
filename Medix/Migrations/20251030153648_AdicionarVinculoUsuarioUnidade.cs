using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medix.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarVinculoUsuarioUnidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdministradorUserId",
                table: "UnidadesMedicas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedicas_AdministradorUserId",
                table: "UnidadesMedicas",
                column: "AdministradorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnidadesMedicas_AspNetUsers_AdministradorUserId",
                table: "UnidadesMedicas",
                column: "AdministradorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnidadesMedicas_AspNetUsers_AdministradorUserId",
                table: "UnidadesMedicas");

            migrationBuilder.DropIndex(
                name: "IX_UnidadesMedicas_AdministradorUserId",
                table: "UnidadesMedicas");

            migrationBuilder.DropColumn(
                name: "AdministradorUserId",
                table: "UnidadesMedicas");
        }
    }
}
