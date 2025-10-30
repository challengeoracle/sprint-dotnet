using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medix.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelasPacienteColaborador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colaboradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cargo = table.Column<int>(type: "int", nullable: false),
                    Especialidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistroProfissional = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UnidadeMedicaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colaboradores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colaboradores_UnidadesMedicas_UnidadeMedicaId",
                        column: x => x.UnidadeMedicaId,
                        principalTable: "UnidadesMedicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CPF = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnidadeMedicaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pacientes_UnidadesMedicas_UnidadeMedicaId",
                        column: x => x.UnidadeMedicaId,
                        principalTable: "UnidadesMedicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_UnidadeMedicaId",
                table: "Colaboradores",
                column: "UnidadeMedicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_UnidadeMedicaId",
                table: "Pacientes",
                column: "UnidadeMedicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colaboradores");

            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
