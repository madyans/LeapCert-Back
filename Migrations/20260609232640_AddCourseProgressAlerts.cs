using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace leapcert_back.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseProgressAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_curso_alerta_progresso_usuario",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_usuario = table.Column<int>(type: "int", nullable: false),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    ultimo_percentual = table.Column<int>(type: "int", nullable: false),
                    ultima_evolucao_em = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ultima_exibicao_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_alerta_progresso_usuario", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_alerta_progresso_usuario_Usuario_codigo_usuario",
                        column: x => x.codigo_usuario,
                        principalTable: "Usuario",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_curso_alerta_progresso_usuario_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_alerta_progresso_usuario_codigo_curso",
                table: "tb_curso_alerta_progresso_usuario",
                column: "codigo_curso");

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_alerta_progresso_usuario_codigo_usuario_codigo_curso",
                table: "tb_curso_alerta_progresso_usuario",
                columns: new[] { "codigo_usuario", "codigo_curso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_alerta_progresso_usuario_codigo_usuario_ultima_evolucao_em",
                table: "tb_curso_alerta_progresso_usuario",
                columns: new[] { "codigo_usuario", "ultima_evolucao_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_curso_alerta_progresso_usuario");
        }
    }
}
