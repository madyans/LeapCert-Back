using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace leapcert_back.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseConnectionsAndLearningProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_curso_conexao_usuario",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_usuario = table.Column<int>(type: "int", nullable: false),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    codigo_criador_curso = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_conexao_usuario", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_conexao_usuario_Usuario_codigo_criador_curso",
                        column: x => x.codigo_criador_curso,
                        principalTable: "Usuario",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_curso_conexao_usuario_Usuario_codigo_usuario",
                        column: x => x.codigo_usuario,
                        principalTable: "Usuario",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_curso_conexao_usuario_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_curso_trilha_progresso_usuario",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_usuario = table.Column<int>(type: "int", nullable: false),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    codigo_trilha_item = table.Column<int>(type: "int", nullable: false),
                    concluido = table.Column<bool>(type: "bit", nullable: false),
                    concluido_em = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_trilha_progresso_usuario", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_trilha_progresso_usuario_Usuario_codigo_usuario",
                        column: x => x.codigo_usuario,
                        principalTable: "Usuario",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_curso_trilha_progresso_usuario_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_curso_trilha_progresso_usuario_tb_curso_trilha_codigo_trilha_item",
                        column: x => x.codigo_trilha_item,
                        principalTable: "tb_curso_trilha",
                        principalColumn: "codigo");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_conexao_usuario_codigo_criador_curso",
                table: "tb_curso_conexao_usuario",
                column: "codigo_criador_curso");

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_conexao_usuario_codigo_curso_status",
                table: "tb_curso_conexao_usuario",
                columns: new[] { "codigo_curso", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_conexao_usuario_codigo_usuario_codigo_curso",
                table: "tb_curso_conexao_usuario",
                columns: new[] { "codigo_usuario", "codigo_curso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_trilha_progresso_usuario_codigo_curso",
                table: "tb_curso_trilha_progresso_usuario",
                column: "codigo_curso");

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_trilha_progresso_usuario_codigo_trilha_item",
                table: "tb_curso_trilha_progresso_usuario",
                column: "codigo_trilha_item");

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_trilha_progresso_usuario_codigo_usuario_codigo_curso",
                table: "tb_curso_trilha_progresso_usuario",
                columns: new[] { "codigo_usuario", "codigo_curso" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_trilha_progresso_usuario_codigo_usuario_codigo_trilha_item",
                table: "tb_curso_trilha_progresso_usuario",
                columns: new[] { "codigo_usuario", "codigo_trilha_item" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_curso_conexao_usuario");

            migrationBuilder.DropTable(
                name: "tb_curso_trilha_progresso_usuario");
        }
    }
}
