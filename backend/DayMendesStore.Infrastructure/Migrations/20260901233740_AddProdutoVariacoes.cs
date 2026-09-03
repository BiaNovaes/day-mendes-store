using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DayMendesStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoVariacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Produtos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VariacaoProdutoId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariacaoProdutoId",
                table: "ItensVenda",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VariacoesProduto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Tamanho = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuantidadeEstoque = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariacoesProduto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariacoesProduto_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Migrate existing product sizes, colors and stock to VariacoesProduto
            migrationBuilder.Sql(@"
                INSERT INTO VariacoesProduto (ProdutoId, Tamanho, Cor, QuantidadeEstoque, Status, CreatedAt, UpdatedAt)
                SELECT Id, COALESCE(Tamanho, ''), COALESCE(Cor, ''), QuantidadeEstoque, Status, CreatedAt, UpdatedAt
                FROM Produtos
                WHERE Id NOT IN (SELECT DISTINCT ProdutoId FROM VariacoesProduto);
            ");

            // Link existing sale items to the created product variation
            migrationBuilder.Sql(@"
                UPDATE ItensVenda iv
                JOIN VariacoesProduto vp ON vp.ProdutoId = iv.ProdutoId
                SET iv.VariacaoProdutoId = vp.Id
                WHERE iv.VariacaoProdutoId = 0 OR iv.VariacaoProdutoId IS NULL;
            ");

            // Link existing stock movements to the created product variation
            migrationBuilder.Sql(@"
                UPDATE MovimentacoesEstoque me
                JOIN VariacoesProduto vp ON vp.ProdutoId = me.ProdutoId
                SET me.VariacaoProdutoId = vp.Id
                WHERE me.VariacaoProdutoId IS NULL;
            ");

            migrationBuilder.DropColumn(
                name: "Cor",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "QuantidadeEstoque",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Tamanho",
                table: "Produtos");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_VariacaoProdutoId",
                table: "MovimentacoesEstoque",
                column: "VariacaoProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_VariacaoProdutoId",
                table: "ItensVenda",
                column: "VariacaoProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_VariacoesProduto_ProdutoId_Tamanho_Cor",
                table: "VariacoesProduto",
                columns: new[] { "ProdutoId", "Tamanho", "Cor" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensVenda_VariacoesProduto_VariacaoProdutoId",
                table: "ItensVenda",
                column: "VariacaoProdutoId",
                principalTable: "VariacoesProduto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_VariacoesProduto_VariacaoProdutoId",
                table: "MovimentacoesEstoque",
                column: "VariacaoProdutoId",
                principalTable: "VariacoesProduto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensVenda_VariacoesProduto_VariacaoProdutoId",
                table: "ItensVenda");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_VariacoesProduto_VariacaoProdutoId",
                table: "MovimentacoesEstoque");

            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "Produtos",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeEstoque",
                table: "Produtos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tamanho",
                table: "Produtos",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Rollback stock sum to Produtos
            migrationBuilder.Sql(@"
                UPDATE Produtos p
                SET QuantidadeEstoque = COALESCE((SELECT SUM(vp.QuantidadeEstoque) FROM VariacoesProduto vp WHERE vp.ProdutoId = p.Id AND vp.Status = 1), 0);
            ");

            migrationBuilder.DropTable(
                name: "VariacoesProduto");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_VariacaoProdutoId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "IX_ItensVenda_VariacaoProdutoId",
                table: "ItensVenda");

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "VariacaoProdutoId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "VariacaoProdutoId",
                table: "ItensVenda");
        }
    }
}
