using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayMendesStore.Infrastructure.Migrations
{
    public partial class AddMotivoCancelamentoToVenda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelamento",
                table: "Vendas",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoCancelamento",
                table: "Vendas");
        }
    }
}
