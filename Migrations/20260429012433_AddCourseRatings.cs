using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace leapcert_back.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_curso_avaliacao",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    codigo_usuario = table.Column<int>(type: "int", nullable: false),
                    nota = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    comentario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_avaliacao", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_avaliacao_Usuario_codigo_usuario",
                        column: x => x.codigo_usuario,
                        principalTable: "Usuario",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_curso_avaliacao_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_avaliacao_codigo_curso_codigo_usuario",
                table: "tb_curso_avaliacao",
                columns: new[] { "codigo_curso", "codigo_usuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_avaliacao_codigo_usuario",
                table: "tb_curso_avaliacao",
                column: "codigo_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_curso_avaliacao");
        }
    }
}
