using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayMendesStore.API.Controllers;

public class EstoqueController : ApiBaseController
{
    private readonly IEstoqueService _estoqueService;

    public EstoqueController(IEstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
    }

    /// <summary>
    /// Consulta o saldo atual de estoque de todos os produtos ativos e suas variações.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EstoqueProdutoDto>>> GetSaldoEstoque(CancellationToken cancellationToken)
    {
        var result = await _estoqueService.GetSaldoEstoqueAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulta o saldo atual de estoque de um produto específico e suas variações.
    /// </summary>
    [HttpGet("produto/{produtoId:int}")]
    public async Task<ActionResult<EstoqueProdutoDto>> GetSaldoPorProduto(int produtoId, CancellationToken cancellationToken)
    {
        var result = await _estoqueService.GetSaldoPorProdutoAsync(produtoId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza diretamente a quantidade atual disponível em estoque de uma variação física do produto.
    /// </summary>
    [HttpPut("variacao/{variacaoId:int}")]
    public async Task<ActionResult<EstoqueVariacaoDto>> AtualizarQuantidade(int variacaoId, [FromBody] AtualizarEstoqueDto dto, CancellationToken cancellationToken)
    {
        var result = await _estoqueService.AtualizarQuantidadeAsync(variacaoId, dto.QuantidadeEstoque, cancellationToken);
        return Ok(result);
    }
}
