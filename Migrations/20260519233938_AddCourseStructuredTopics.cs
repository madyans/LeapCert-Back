using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace leapcert_back.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseStructuredTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_curso_anotacao_usuario",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    codigo_usuario = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_anotacao_usuario", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_anotacao_usuario_Usuario_codigo_usuario",
                        column: x => x.codigo_usuario,
                        principalTable: "Usuario",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_curso_anotacao_usuario_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_curso_avaliacao_item",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantidade_questoes = table.Column<int>(type: "int", nullable: true),
                    duracao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prazo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_avaliacao_item", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_avaliacao_item_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_curso_certificado",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    progresso_padrao = table.Column<int>(type: "int", nullable: false),
                    disponivel_padrao = table.Column<bool>(type: "bit", nullable: false),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_certificado", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_certificado_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_curso_forum_topico",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    autor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    resumo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_forum_topico", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_forum_topico_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_curso_professor_contato",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    nome_professor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    subtitulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mensagem_orientacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_professor_contato", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_professor_contato_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_curso_trilha",
                columns: table => new
                {
                    codigo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_curso = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    concluido_padrao = table.Column<bool>(type: "bit", nullable: false),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_curso_trilha", x => x.codigo);
                    table.ForeignKey(
                        name: "FK_tb_curso_trilha_tb_curso_codigo_curso",
                        column: x => x.codigo_curso,
                        principalTable: "tb_curso",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_anotacao_usuario_codigo_curso_codigo_usuario_updated_at",
                table: "tb_curso_anotacao_usuario",
                columns: new[] { "codigo_curso", "codigo_usuario", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_anotacao_usuario_codigo_usuario",
                table: "tb_curso_anotacao_usuario",
                column: "codigo_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_avaliacao_item_codigo_curso_ordem",
                table: "tb_curso_avaliacao_item",
                columns: new[] { "codigo_curso", "ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_certificado_codigo_curso_ordem",
                table: "tb_curso_certificado",
                columns: new[] { "codigo_curso", "ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_forum_topico_codigo_curso_ordem",
                table: "tb_curso_forum_topico",
                columns: new[] { "codigo_curso", "ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_professor_contato_codigo_curso",
                table: "tb_curso_professor_contato",
                column: "codigo_curso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_curso_trilha_codigo_curso_ordem",
                table: "tb_curso_trilha",
                columns: new[] { "codigo_curso", "ordem" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_curso_anotacao_usuario");

            migrationBuilder.DropTable(
                name: "tb_curso_avaliacao_item");

            migrationBuilder.DropTable(
                name: "tb_curso_certificado");

            migrationBuilder.DropTable(
                name: "tb_curso_forum_topico");

            migrationBuilder.DropTable(
                name: "tb_curso_professor_contato");

            migrationBuilder.DropTable(
                name: "tb_curso_trilha");
        }
    }
}
