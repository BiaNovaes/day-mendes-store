using DayMendesStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayMendesStore.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260925180000_AddCodigoBarrasToVariacaoProduto")]
public partial class AddCodigoBarrasToVariacaoProduto : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CodigoBarras",
            table: "VariacoesProduto",
            type: "varchar(80)",
            maxLength: 80,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_VariacoesProduto_CodigoBarras",
            table: "VariacoesProduto",
            column: "CodigoBarras",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_VariacoesProduto_CodigoBarras",
            table: "VariacoesProduto");

        migrationBuilder.DropColumn(
            name: "CodigoBarras",
            table: "VariacoesProduto");
    }
}
