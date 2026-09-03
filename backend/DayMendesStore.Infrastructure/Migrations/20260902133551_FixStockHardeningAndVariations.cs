using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayMendesStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStockHardeningAndVariations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VariacaoProdutoId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_VariacoesProduto_QuantidadeEstoque",
                table: "VariacoesProduto",
                sql: "`QuantidadeEstoque` >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VariacoesProduto_QuantidadeEstoque",
                table: "VariacoesProduto");

            migrationBuilder.AlterColumn<int>(
                name: "VariacaoProdutoId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
