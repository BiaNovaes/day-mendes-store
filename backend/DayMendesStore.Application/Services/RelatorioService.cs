using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using DayMendesStore.Domain.Entities;
using DayMendesStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DayMendesStore.Application.Services;

public class RelatorioService : IRelatorioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILojaService _lojaService;

    public RelatorioService(IUnitOfWork unitOfWork, ILojaService lojaService)
    {
        _unitOfWork = unitOfWork;
        _lojaService = lojaService;
    }

    public async Task<ResumoGeralDashboardDto> GetResumoGeralAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var vendasQuery = GetBaseVendasQuery(filtro);
        var itensQuery = GetBaseItensQuery(filtro);

        var vendas = await vendasQuery.ToListAsync(cancellationToken);
        var itens = await itensQuery.ToListAsync(cancellationToken);

        var totalFaturado = vendas.Sum(v => v.ValorTotal);
        var totalVendas = vendas.Count;
        var totalProdutosVendidos = itens.Sum(i => i.Quantidade);
        var ticketMedio = totalVendas > 0 ? Math.Round(totalFaturado / totalVendas, 2) : 0;

        var custoTotal = itens.Sum(i => (i.Produto?.ValorCompra ?? 0) * i.Quantidade);
        var lucroBruto = totalFaturado - custoTotal;
        var margemLucroMedia = totalFaturado > 0 ? Math.Round((lucroBruto / totalFaturado) * 100, 2) : 0;

        var vendasPorPeriodo = vendas
            .GroupBy(v => v.DataVenda.ToString("dd/MM/yyyy"))
            .Select(g => new VendaPeriodoItemDto
            {
                Periodo = g.Key,
                QuantidadeVendas = g.Count(),
                TotalFaturado = g.Sum(v => v.ValorTotal)
            })
            .OrderBy(p => p.Periodo)
            .ToList();

        var vendasPorFormaPagamento = vendas
            .GroupBy(v => string.IsNullOrWhiteSpace(v.FormaPagamento) ? "Não Informado" : v.FormaPagamento)
            .Select(g => new FormaPagamentoItemDto
            {
                FormaPagamento = g.Key,
                Quantidade = g.Count(),
                ValorTotal = g.Sum(v => v.ValorTotal),
                Percentual = totalFaturado > 0 ? Math.Round((g.Sum(v => v.ValorTotal) / totalFaturado) * 100, 2) : 0
            })
            .OrderByDescending(fp => fp.ValorTotal)
            .ToList();

        return new ResumoGeralDashboardDto
        {
            TotalFaturado = totalFaturado,
            TotalVendas = totalVendas,
            TotalProdutosVendidos = totalProdutosVendidos,
            TicketMedio = ticketMedio,
            LucroBrutoEstimado = lucroBruto,
            MargemLucroMedia = margemLucroMedia,
            VendasPorPeriodo = vendasPorPeriodo,
            VendasPorFormaPagamento = vendasPorFormaPagamento
        };
    }

    public async Task<List<ProdutoDesempenhoDto>> GetProdutosMaisVendidosAsync(RelatorioFiltroDto filtro, int top = 10, CancellationToken cancellationToken = default)
    {
        var itensQuery = GetBaseItensQuery(filtro);
        var itens = await itensQuery.ToListAsync(cancellationToken);

        return itens
            .GroupBy(i => new
            {
                i.ProdutoId,
                Nome = i.Produto != null ? i.Produto.Nome : "Produto",
                Foto = i.Produto != null ? i.Produto.Foto : null,
                CategoriaNome = i.Produto != null && i.Produto.Categoria != null ? i.Produto.Categoria.Nome : "",
                Tamanho = i.VariacaoProduto != null ? i.VariacaoProduto.Tamanho : null,
                Cor = i.VariacaoProduto != null ? i.VariacaoProduto.Cor : null,
                EstoqueAtual = i.VariacaoProduto != null ? i.VariacaoProduto.QuantidadeEstoque : 0
            })
            .Select(g => new ProdutoDesempenhoDto
            {
                ProdutoId = g.Key.ProdutoId,
                Nome = g.Key.Nome,
                Foto = g.Key.Foto,
                CategoriaNome = g.Key.CategoriaNome,
                Tamanho = g.Key.Tamanho,
                Cor = g.Key.Cor,
                QuantidadeVendida = g.Sum(i => i.Quantidade),
                ValorTotalVendido = g.Sum(i => i.Subtotal),
                LucroEstimado = g.Sum(i => i.Subtotal - ((i.Produto?.ValorCompra ?? 0) * i.Quantidade)),
                EstoqueAtual = g.Key.EstoqueAtual
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .ThenByDescending(p => p.ValorTotalVendido)
            .Take(top)
            .ToList();
    }

    public async Task<List<ProdutoDesempenhoDto>> GetProdutosMenosVendidosAsync(RelatorioFiltroDto filtro, int top = 10, CancellationToken cancellationToken = default)
    {
        var itensQuery = GetBaseItensQuery(filtro);
        var itens = await itensQuery.ToListAsync(cancellationToken);

        return itens
            .GroupBy(i => new
            {
                i.ProdutoId,
                Nome = i.Produto != null ? i.Produto.Nome : "Produto",
                Foto = i.Produto != null ? i.Produto.Foto : null,
                CategoriaNome = i.Produto != null && i.Produto.Categoria != null ? i.Produto.Categoria.Nome : "",
                Tamanho = i.VariacaoProduto != null ? i.VariacaoProduto.Tamanho : null,
                Cor = i.VariacaoProduto != null ? i.VariacaoProduto.Cor : null,
                EstoqueAtual = i.VariacaoProduto != null ? i.VariacaoProduto.QuantidadeEstoque : 0
            })
            .Select(g => new ProdutoDesempenhoDto
            {
                ProdutoId = g.Key.ProdutoId,
                Nome = g.Key.Nome,
                Foto = g.Key.Foto,
                CategoriaNome = g.Key.CategoriaNome,
                Tamanho = g.Key.Tamanho,
                Cor = g.Key.Cor,
                QuantidadeVendida = g.Sum(i => i.Quantidade),
                ValorTotalVendido = g.Sum(i => i.Subtotal),
                LucroEstimado = g.Sum(i => i.Subtotal - ((i.Produto?.ValorCompra ?? 0) * i.Quantidade)),
                EstoqueAtual = g.Key.EstoqueAtual
            })
            .OrderBy(p => p.QuantidadeVendida)
            .ThenBy(p => p.ValorTotalVendido)
            .Take(top)
            .ToList();
    }

    public async Task<List<ProdutoSemVendaDto>> GetProdutosParadosAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var produtosQuery = _unitOfWork.Produtos.Query()
            .Include(p => p.Categoria)
            .Include(p => p.Variacoes)
            .Where(p => p.Status == Status.Ativo);

        if (filtro.CategoriaId.HasValue)
        {
            produtosQuery = produtosQuery.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tamanho))
        {
            var tam = filtro.Tamanho.Trim().ToLower();
            produtosQuery = produtosQuery.Where(p => p.Variacoes.Any(v => v.Status != Status.Deletado && v.Tamanho.ToLower() == tam));
        }

        var produtos = await produtosQuery.ToListAsync(cancellationToken);
        var produtoIds = produtos.Select(p => p.Id).ToList();

        var ultimasVendas = await _unitOfWork.ItensVenda.Query()
            .Where(i => produtoIds.Contains(i.ProdutoId) && i.Venda != null && i.Venda.StatusVenda == StatusVenda.Finalizada)
            .GroupBy(i => i.ProdutoId)
            .Select(g => new
            {
                ProdutoId = g.Key,
                UltimaVenda = g.Max(i => i.Venda!.DataVenda)
            })
            .ToDictionaryAsync(g => g.ProdutoId, cancellationToken);

        var agora = DateTime.UtcNow;
        var resultado = new List<ProdutoSemVendaDto>();

        foreach (var p in produtos)
        {
            DateTime dataReferencia;
            string classificacao;

            if (ultimasVendas.TryGetValue(p.Id, out var vendaInfo))
            {
                dataReferencia = vendaInfo.UltimaVenda;
                var dias = (int)(agora - dataReferencia).TotalDays;

                if (dias >= RotatividadeConstants.DiasSemVendaParado)
                {
                    classificacao = "Parada há mais de 60 dias";
                }
                else if (dias >= RotatividadeConstants.DiasSemVendaBaixaRotatividade)
                {
                    classificacao = "Demorando para vender";
                }
                else
                {
                    continue;
                }
            }
            else
            {
                dataReferencia = p.CreatedAt;
                var dias = (int)(agora - dataReferencia).TotalDays;
                classificacao = dias >= RotatividadeConstants.DiasSemVendaParado ? "Parada (Sem vendas)" : "Sem vendas registradas";
            }

            var diasSemVenda = (int)(agora - dataReferencia).TotalDays;
            var estoqueTotal = p.Variacoes.Where(v => v.Status != Status.Deletado).Sum(v => v.QuantidadeEstoque);

            resultado.Add(new ProdutoSemVendaDto
            {
                ProdutoId = p.Id,
                Nome = p.Nome,
                Foto = p.Foto,
                CategoriaNome = p.Categoria?.Nome ?? string.Empty,
                Tamanho = string.Join(", ", p.Variacoes.Where(v => v.Status != Status.Deletado).Select(v => v.Tamanho).Distinct()),
                Cor = string.Join(", ", p.Variacoes.Where(v => v.Status != Status.Deletado).Select(v => v.Cor).Distinct()),
                EstoqueAtual = estoqueTotal,
                ValorVenda = p.ValorVenda,
                DiasSemVenda = diasSemVenda,
                ClassificacaoRotatividade = classificacao
            });
        }

        return resultado.OrderByDescending(p => p.DiasSemVenda).ToList();
    }

    public async Task<List<ProdutoEstoqueBaixoDto>> GetProdutosEstoqueBaixoAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Produtos.Query()
            .Include(p => p.Categoria)
            .Include(p => p.Variacoes)
            .Where(p => p.Status == Status.Ativo);

        if (filtro.CategoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tamanho))
        {
            var tam = filtro.Tamanho.Trim().ToLower();
            query = query.Where(p => p.Variacoes.Any(v => v.Status != Status.Deletado && v.Tamanho.ToLower() == tam));
        }

        var produtos = await query.ToListAsync(cancellationToken);

        return produtos
            .Select(p => new
            {
                Produto = p,
                EstoqueAtual = p.Variacoes.Where(v => v.Status != Status.Deletado).Sum(v => v.QuantidadeEstoque)
            })
            .Where(x => x.EstoqueAtual <= x.Produto.EstoqueMinimo)
            .OrderBy(x => x.EstoqueAtual)
            .Select(x => new ProdutoEstoqueBaixoDto
            {
                ProdutoId = x.Produto.Id,
                Nome = x.Produto.Nome,
                Foto = x.Produto.Foto,
                CategoriaNome = x.Produto.Categoria != null ? x.Produto.Categoria.Nome : "",
                Tamanho = string.Join(", ", x.Produto.Variacoes.Where(v => v.Status != Status.Deletado).Select(v => v.Tamanho).Distinct()),
                Cor = string.Join(", ", x.Produto.Variacoes.Where(v => v.Status != Status.Deletado).Select(v => v.Cor).Distinct()),
                EstoqueAtual = x.EstoqueAtual,
                EstoqueMinimo = x.Produto.EstoqueMinimo,
                StatusEstoque = x.EstoqueAtual == 0 ? "Zerado" : "Abaixo do Mínimo"
            })
            .ToList();
    }

    public async Task<List<CategoriaDesempenhoDto>> GetCategoriasMaisVendidasAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var itensQuery = GetBaseItensQuery(filtro);
        var itens = await itensQuery.ToListAsync(cancellationToken);

        var totalFaturadoGeral = itens.Sum(i => i.Subtotal);

        return itens
            .GroupBy(i => new
            {
                CategoriaId = i.Produto != null ? i.Produto.CategoriaId : 0,
                CategoriaNome = i.Produto != null && i.Produto.Categoria != null ? i.Produto.Categoria.Nome : "Sem Categoria"
            })
            .Select(g => new CategoriaDesempenhoDto
            {
                CategoriaId = g.Key.CategoriaId,
                Nome = g.Key.CategoriaNome,
                QuantidadeProdutosVendidos = g.Sum(i => i.Quantidade),
                ValorTotalFaturado = g.Sum(i => i.Subtotal),
                PercentualFaturamento = totalFaturadoGeral > 0 ? Math.Round((g.Sum(i => i.Subtotal) / totalFaturadoGeral) * 100, 2) : 0
            })
            .OrderByDescending(c => c.ValorTotalFaturado)
            .ToList();
    }

    public async Task<List<TamanhoDesempenhoDto>> GetAnaliseTamanhosAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var itens = await GetBaseItensQuery(filtro).ToListAsync(cancellationToken);
        var variacoes = await _unitOfWork.VariacoesProduto.Query()
            .Include(v => v.Produto)
            .Where(v => v.Status != Status.Deletado && v.Produto != null && v.Produto.Status == Status.Ativo)
            .ToListAsync(cancellationToken);

        var totalVendidoGeral = itens.Sum(i => i.Quantidade);

        var tamanhos = variacoes
            .Select(v => string.IsNullOrWhiteSpace(v.Tamanho) ? "Único" : v.Tamanho.Trim().ToUpperInvariant())
            .Union(itens.Select(i => string.IsNullOrWhiteSpace(i.VariacaoProduto?.Tamanho) ? "Único" : i.VariacaoProduto.Tamanho.Trim().ToUpperInvariant()))
            .Distinct()
            .ToList();

        var resultado = new List<TamanhoDesempenhoDto>();

        foreach (var tam in tamanhos)
        {
            var itensTam = itens.Where(i => (string.IsNullOrWhiteSpace(i.VariacaoProduto?.Tamanho) ? "Único" : i.VariacaoProduto.Tamanho.Trim().ToUpperInvariant()) == tam).ToList();
            var variacoesTam = variacoes.Where(v => (string.IsNullOrWhiteSpace(v.Tamanho) ? "Único" : v.Tamanho.Trim().ToUpperInvariant()) == tam).ToList();

            var qtdVendida = itensTam.Sum(i => i.Quantidade);
            var valorTotal = itensTam.Sum(i => i.Subtotal);
            var estoqueTotal = variacoesTam.Sum(v => v.QuantidadeEstoque);
            var zerados = variacoesTam.Count(v => v.QuantidadeEstoque == 0);

            string velocidade;
            if (totalVendidoGeral > 0)
            {
                var perc = (decimal)qtdVendida / totalVendidoGeral;
                if (perc >= 0.35m) velocidade = "Alta";
                else if (perc >= 0.15m) velocidade = "Média";
                else velocidade = "Baixa";
            }
            else
            {
                velocidade = "Sem Vendas";
            }

            resultado.Add(new TamanhoDesempenhoDto
            {
                Tamanho = tam,
                QuantidadeVendida = qtdVendida,
                ValorTotal = valorTotal,
                QuantidadeEmEstoque = estoqueTotal,
                QuantidadeProdutosZerados = zerados,
                VelocidadeSaida = velocidade
            });
        }

        return resultado.OrderByDescending(t => t.QuantidadeVendida).ThenByDescending(t => t.QuantidadeEmEstoque).ToList();
    }

    public async Task<List<ClienteDesempenhoDto>> GetMelhoresClientesAsync(RelatorioFiltroDto filtro, int top = 10, CancellationToken cancellationToken = default)
    {
        var vendasQuery = GetBaseVendasQuery(filtro);

        return await vendasQuery
            .Where(v => v.ClienteId != null)
            .GroupBy(v => new
            {
                v.ClienteId,
                Nome = v.Cliente != null ? v.Cliente.Nome : "Cliente",
                Telefone = v.Cliente != null ? v.Cliente.Telefone : null,
                Email = v.Cliente != null ? v.Cliente.Email : null
            })
            .Select(g => new ClienteDesempenhoDto
            {
                ClienteId = g.Key.ClienteId,
                Nome = g.Key.Nome,
                Telefone = g.Key.Telefone,
                Email = g.Key.Email,
                TotalCompras = g.Count(),
                ValorTotalComprado = g.Sum(v => v.ValorTotal),
                TicketMedio = Math.Round(g.Sum(v => v.ValorTotal) / g.Count(), 2),
                UltimaCompra = g.Max(v => v.DataVenda)
            })
            .OrderByDescending(c => c.ValorTotalComprado)
            .Take(top)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SugestaoReposicaoItemDto>> GetSugestoesReposicaoAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var produtosQuery = _unitOfWork.Produtos.Query()
            .Include(p => p.Categoria)
            .Include(p => p.Variacoes)
            .Where(p => p.Status == Status.Ativo);

        if (filtro.CategoriaId.HasValue)
        {
            produtosQuery = produtosQuery.Where(p => p.CategoriaId == filtro.CategoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tamanho))
        {
            var tam = filtro.Tamanho.Trim().ToLower();
            produtosQuery = produtosQuery.Where(p => p.Variacoes.Any(v => v.Status != Status.Deletado && v.Tamanho.ToLower() == tam));
        }

        var produtos = await produtosQuery.ToListAsync(cancellationToken);
        var produtoIds = produtos.Select(p => p.Id).ToList();

        var dataLimite30Dias = DateTime.UtcNow.AddDays(-30);
        var vendas30DiasPorProduto = await _unitOfWork.ItensVenda.Query()
            .Where(i => produtoIds.Contains(i.ProdutoId) 
                     && i.Venda != null 
                     && i.Venda.StatusVenda == StatusVenda.Finalizada 
                     && i.Venda.DataVenda >= dataLimite30Dias)
            .GroupBy(i => i.ProdutoId)
            .Select(g => new
            {
                ProdutoId = g.Key,
                TotalVendido = g.Sum(i => i.Quantidade)
            })
            .ToDictionaryAsync(g => g.ProdutoId, g => g.TotalVendido, cancellationToken);

        var resultado = new List<SugestaoReposicaoItemDto>();

        foreach (var p in produtos)
        {
            var estoqueAtual = p.Variacoes.Where(v => v.Status != Status.Deletado).Sum(v => v.QuantidadeEstoque);
            if (estoqueAtual > p.EstoqueMinimo)
            {
                continue;
            }

            vendas30DiasPorProduto.TryGetValue(p.Id, out var vendas30);

            int baseSugerida = Math.Max((p.EstoqueMinimo * 2) - estoqueAtual, Math.Max(p.EstoqueMinimo, 1));
            int sugestaoFinal = Math.Max(baseSugerida, vendas30 > 0 ? (vendas30 - estoqueAtual + p.EstoqueMinimo) : baseSugerida);

            string justificativa;
            if (estoqueAtual == 0 && vendas30 > 0)
            {
                justificativa = $"Peça esgotada na loja! Vendeu {vendas30} peças nos últimos 30 dias. Vale a pena comprar logo.";
            }
            else if (estoqueAtual == 0)
            {
                justificativa = $"Nenhuma peça disponível na loja. Recomendamos comprar {sugestaoFinal} peças para voltar a ter na loja.";
            }
            else
            {
                justificativa = $"Restam apenas {estoqueAtual} peças na loja (mínimo recomendado: {p.EstoqueMinimo}). Vendeu {vendas30} peças recentemente.";
            }

            resultado.Add(new SugestaoReposicaoItemDto
            {
                ProdutoId = p.Id,
                Nome = p.Nome,
                Foto = p.Foto,
                CategoriaNome = p.Categoria?.Nome ?? string.Empty,
                Tamanho = string.Join(", ", p.Variacoes.Where(v => v.Status != Status.Deletado).Select(v => v.Tamanho).Distinct()),
                Cor = string.Join(", ", p.Variacoes.Where(v => v.Status != Status.Deletado).Select(v => v.Cor).Distinct()),
                EstoqueAtual = estoqueAtual,
                EstoqueMinimo = p.EstoqueMinimo,
                VendasUltimos30Dias = vendas30,
                QuantidadeSugerida = sugestaoFinal,
                JustificativaSugestao = justificativa
            });
        }

        return resultado.OrderBy(s => s.EstoqueAtual).ThenByDescending(s => s.VendasUltimos30Dias).ToList();
    }

    public async Task<RelatorioCompletoDto> GetRelatorioCompletoAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var loja = await _lojaService.GetLojaAsync(cancellationToken);
        var resumo = await GetResumoGeralAsync(filtro, cancellationToken);
        var maisVendidos = await GetProdutosMaisVendidosAsync(filtro, 10, cancellationToken);
        var menosVendidos = await GetProdutosMenosVendidosAsync(filtro, 10, cancellationToken);
        var parados = await GetProdutosParadosAsync(filtro, cancellationToken);
        var estoqueBaixo = await GetProdutosEstoqueBaixoAsync(filtro, cancellationToken);
        var categorias = await GetCategoriasMaisVendidasAsync(filtro, cancellationToken);
        var tamanhos = await GetAnaliseTamanhosAsync(filtro, cancellationToken);
        var clientes = await GetMelhoresClientesAsync(filtro, 10, cancellationToken);
        var sugestoes = await GetSugestoesReposicaoAsync(filtro, cancellationToken);

        return new RelatorioCompletoDto
        {
            Loja = loja,
            DataInicio = filtro.DataInicio,
            DataFim = filtro.DataFim,
            DataGeracao = DateTime.UtcNow,
            ResumoGeral = resumo,
            ProdutosMaisVendidos = maisVendidos,
            ProdutosMenosVendidos = menosVendidos,
            ProdutosParados = parados,
            ProdutosEstoqueBaixo = estoqueBaixo,
            CategoriasMaisVendidas = categorias,
            AnaliseTamanhos = tamanhos,
            MelhoresClientes = clientes,
            SugestoesReposicao = sugestoes
        };
    }

    private IQueryable<Venda> GetBaseVendasQuery(RelatorioFiltroDto filtro)
    {
        var query = _unitOfWork.Vendas.Query()
            .Include(v => v.Cliente)
            .Where(v => v.StatusVenda == StatusVenda.Finalizada);

        if (filtro.DataInicio.HasValue)
        {
            var dataInicioUtc = DateTime.SpecifyKind(filtro.DataInicio.Value, DateTimeKind.Utc);
            query = query.Where(v => v.DataVenda >= dataInicioUtc);
        }

        if (filtro.DataFim.HasValue)
        {
            var dataFimUtc = DateTime.SpecifyKind(filtro.DataFim.Value, DateTimeKind.Utc);
            query = query.Where(v => v.DataVenda <= dataFimUtc);
        }

        return query;
    }

    private IQueryable<ItemVenda> GetBaseItensQuery(RelatorioFiltroDto filtro)
    {
        var query = _unitOfWork.ItensVenda.Query()
            .Include(i => i.Produto)
                .ThenInclude(p => p!.Categoria)
            .Include(i => i.VariacaoProduto)
            .Include(i => i.Venda)
            .Where(i => i.Venda != null && i.Venda.StatusVenda == StatusVenda.Finalizada);

        if (filtro.DataInicio.HasValue)
        {
            var dataInicioUtc = DateTime.SpecifyKind(filtro.DataInicio.Value, DateTimeKind.Utc);
            query = query.Where(i => i.Venda!.DataVenda >= dataInicioUtc);
        }

        if (filtro.DataFim.HasValue)
        {
            var dataFimUtc = DateTime.SpecifyKind(filtro.DataFim.Value, DateTimeKind.Utc);
            query = query.Where(i => i.Venda!.DataVenda <= dataFimUtc);
        }

        if (filtro.CategoriaId.HasValue)
        {
            query = query.Where(i => i.Produto != null && i.Produto.CategoriaId == filtro.CategoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tamanho))
        {
            var tam = filtro.Tamanho.Trim().ToLower();
            query = query.Where(i => i.VariacaoProduto != null && i.VariacaoProduto.Tamanho.ToLower() == tam);
        }

        return query;
    }
}
