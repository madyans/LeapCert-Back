using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace leapcert_back.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningPathAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "arquivo_nome",
                table: "tb_curso_trilha",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "arquivo_path",
                table: "tb_curso_trilha",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "arquivo_tipo",
                table: "tb_curso_trilha",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "arquivo_nome",
                table: "tb_curso_trilha");

            migrationBuilder.DropColumn(
                name: "arquivo_path",
                table: "tb_curso_trilha");

            migrationBuilder.DropColumn(
                name: "arquivo_tipo",
                table: "tb_curso_trilha");
        }
    }
}
