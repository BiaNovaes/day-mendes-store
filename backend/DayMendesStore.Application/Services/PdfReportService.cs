using System.Collections.Concurrent;
using DayMendesStore.Application.DTOs;
using DayMendesStore.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DayMendesStore.Application.Services;

public class PdfReportService : IPdfReportService
{
    private readonly IRelatorioService _relatorioService;
    private static readonly ConcurrentDictionary<string, byte[]?> ImageCache = new();

    public PdfReportService(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GerarRelatorioCompletoPdfAsync(RelatorioFiltroDto filtro, CancellationToken cancellationToken = default)
    {
        var dados = await _relatorioService.GetRelatorioCompletoAsync(filtro, cancellationToken);

        var corPrimaria = ParseColor(dados.Loja.CorPrimaria, "#C2185B");
        var corSecundaria = ParseColor(dados.Loja.CorSecundaria, "#7B1FA2");
        var logoBytes = LoadStoreLogo(dados.Loja.Foto);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(22);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => ComposeHeader(c, dados, corPrimaria, corSecundaria, logoBytes));
                page.Content().Element(c => ComposeContent(c, dados, corPrimaria, corSecundaria));
                page.Footer().Element(c => ComposeFooter(c, corSecundaria));
            });
        });

        return doc.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, RelatorioCompletoDto dados, string corPrimaria, string corSecundaria, byte[]? logoBytes)
    {
        container.BorderBottom(2).BorderColor(corPrimaria).PaddingBottom(8).Row(row =>
        {
            row.RelativeItem().Row(leftRow =>
            {
                if (logoBytes != null && logoBytes.Length > 0)
                {
                    leftRow.ConstantItem(90).Height(38).AlignMiddle().Image(logoBytes).FitArea();
                    leftRow.ConstantItem(8);
                }

                leftRow.RelativeItem().AlignMiddle().Column(col =>
                {
                    col.Item().Text(string.IsNullOrWhiteSpace(dados.Loja.Nome) ? "Day Mendes Store" : dados.Loja.Nome)
                        .FontSize(15).Bold().FontColor(corPrimaria);

                    col.Item().Text("Relatório de Vendas e Peças da Loja")
                        .FontSize(9).SemiBold().FontColor(corSecundaria);

                    if (!string.IsNullOrWhiteSpace(dados.Loja.Cnpj))
                    {
                        col.Item().Text($"CNPJ: {dados.Loja.Cnpj}").FontSize(7.5f).FontColor(Colors.Grey.Medium);
                    }
                });
            });

            row.ConstantItem(210).AlignRight().AlignMiddle().Column(col =>
            {
                col.Item().Text($"Gerado em: {dados.DataGeracao.ToLocalTime():dd/MM/yyyy às HH:mm}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                var periodoStr = (dados.DataInicio.HasValue && dados.DataFim.HasValue)
                    ? $"{dados.DataInicio.Value:dd/MM/yyyy} até {dados.DataFim.Value:dd/MM/yyyy}"
                    : "Todo o histórico registrado";
                col.Item().Text($"Período analisado: {periodoStr}").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container, RelatorioCompletoDto dados, string corPrimaria, string corSecundaria)
    {
        container.PaddingVertical(8).Column(col =>
        {
            col.Item().Text("Como foram as vendas?").FontSize(11.5f).Bold().FontColor(corPrimaria);
            col.Item().PaddingTop(4).Element(c => ComposeResumoVendas(c, dados.ResumoGeral, corPrimaria, corSecundaria));

            col.Item().PaddingTop(12);

            col.Item().Text("💳 Como os clientes pagaram?").FontSize(11.5f).Bold().FontColor(corPrimaria);
            col.Item().PaddingTop(4).Element(c => ComposeFormasPagamento(c, dados.ResumoGeral.VendasPorFormaPagamento, corPrimaria));

            col.Item().PaddingTop(12);

            col.Item().Text("👚 Tamanhos que mais venderam").FontSize(11.5f).Bold().FontColor(corPrimaria);
            col.Item().PaddingTop(4).Element(c => ComposeTamanhosMaisVendidos(c, dados.AnaliseTamanhos, corSecundaria));

            col.Item().PaddingTop(14);

            col.Item().Text("👗 Roupas que mais venderam").FontSize(11.5f).Bold().FontColor(corPrimaria);
            col.Item().Text("As peças que tiveram maior saída no período analisado.").FontSize(7.5f).Italic().FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(4).Element(c => ComposeProdutosMaisVendidos(c, dados.ProdutosMaisVendidos, corPrimaria));

            col.Item().PaddingTop(14);

            col.Item().Text("⚠️ Poucas peças disponíveis").FontSize(11.5f).Bold().FontColor(corPrimaria);
            col.Item().Text("Peças que estão com estoque zerado ou quase acabando na loja.").FontSize(7.5f).Italic().FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(4).Element(c => ComposePoucasPecasDisponiveis(c, dados.ProdutosEstoqueBaixo));

            col.Item().PaddingTop(14);

            col.Item().Text("💤 Roupas que estão demorando para vender").FontSize(11.5f).Bold().FontColor(corPrimaria);
            col.Item().Text("Peças com mais de 30 dias sem nenhuma venda registrada.").FontSize(7.5f).Italic().FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(4).Element(c => ComposeRoupasDemorandoVender(c, dados.ProdutosParados));

            col.Item().PaddingTop(14);

            col.Item().Text("🛍️ O que comprar novamente").FontSize(11.5f).Bold().FontColor(corPrimaria);
            col.Item().Text("Sugestões de reposição baseadas no ritmo de vendas recente e nas peças que estão no fim.").FontSize(7.5f).Italic().FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(4).Element(c => ComposeOQueComprarNovamente(c, dados.SugestoesReposicao, corPrimaria));

            if (dados.SugestoesReposicao.Any())
            {
                col.Item().PageBreak();
                col.Item().Text("📋 Lista Rápida de Compras — Consulta no Brás / Fornecedores").FontSize(12).Bold().FontColor(corPrimaria);
                col.Item().Text("Guia prático com caixas de marcação [  ] para facilitar suas compras com fornecedores.")
                    .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(6).Element(c => ComposeListaRapidaBras(c, dados.SugestoesReposicao, corPrimaria));
            }
        });
    }

    private void ComposeResumoVendas(IContainer container, ResumoGeralDashboardDto resumo, string corPrimaria, string corSecundaria)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeCard(c, "Total vendido", $"R$ {resumo.TotalFaturado:N2}", corPrimaria));
                row.ConstantItem(4);
                row.RelativeItem().Element(c => ComposeCard(c, "Lucro", $"R$ {resumo.LucroBrutoEstimado:N2}", "#2E7D32"));
                row.ConstantItem(4);
                row.RelativeItem().Element(c => ComposeCard(c, "Peças vendidas", $"{resumo.TotalProdutosVendidos}", corSecundaria));
                row.ConstantItem(4);
                row.RelativeItem().Element(c => ComposeCard(c, "Valor médio por compra", $"R$ {resumo.TicketMedio:N2}", "#1565C0"));
                row.ConstantItem(4);
                row.RelativeItem().Element(c => ComposeCard(c, "Clientes atendidos", $"{resumo.TotalVendas}", "#00838F"));
            });

            col.Item().PaddingTop(6).Background("#F8F9FA").Border(0.5f).BorderColor("#E5E7EB").CornerRadius(4).Padding(6).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    if (resumo.TotalProdutosVendidos > 0)
                    {
                        var lucroStr = resumo.LucroBrutoEstimado >= 0
                            ? $"aproximadamente R$ {resumo.LucroBrutoEstimado:N2} de lucro"
                            : $"um saldo de R$ {resumo.LucroBrutoEstimado:N2}";

                        text.Span("💬 Resumo: ").Bold().FontColor(corPrimaria);
                        text.Span($"Você vendeu {resumo.TotalProdutosVendidos} peças neste período e teve {lucroStr}.");
                        if (resumo.TotalVendas > 0)
                        {
                            text.Span($" Ao todo, foram {resumo.TotalVendas} atendimentos com valor médio de R$ {resumo.TicketMedio:N2} por compra.");
                        }
                    }
                    else
                    {
                        text.Span("Nenhuma venda foi registrada no período analisado.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                });
            });
        });
    }

    private void ComposeCard(IContainer container, string titulo, string valor, string corBorda)
    {
        container.Border(1).BorderColor(corBorda).Background(Colors.Grey.Lighten5).Padding(5).CornerRadius(4).Column(col =>
        {
            col.Item().Text(titulo).FontSize(6.5f).SemiBold().FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(1).Text(valor).FontSize(10.5f).Bold().FontColor(corBorda);
        });
    }

    private void ComposeFormasPagamento(IContainer container, List<FormaPagamentoItemDto> formas, string corPrimaria)
    {
        if (!formas.Any())
        {
            container.Background(Colors.Grey.Lighten4).CornerRadius(4).Padding(6)
                .Text("Nenhuma venda registrada no período analisado.").FontSize(8).Italic();
            return;
        }

        container.Row(row =>
        {
            for (int i = 0; i < formas.Count; i++)
            {
                if (i > 0) row.ConstantItem(5);
                var f = formas[i];
                var nomeAmigavel = FormatFormaPagamento(f.FormaPagamento);

                row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).CornerRadius(4).Padding(5).Column(c =>
                {
                    c.Item().Text(nomeAmigavel).FontSize(8).Bold().FontColor(corPrimaria);
                    c.Item().PaddingTop(2).Text($"{f.Quantidade} vendas").FontSize(8.5f).SemiBold();
                    c.Item().Text($"R$ {f.ValorTotal:N2}").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(1).Text($"{f.Percentual:0.#}% do total vendido").FontSize(7).Italic().FontColor(Colors.Grey.Darken1);
                });
            }
        });
    }

    private void ComposeTamanhosMaisVendidos(IContainer container, List<TamanhoDesempenhoDto> tamanhos, string corSecundaria)
    {
        if (!tamanhos.Any())
        {
            container.Text("Nenhum dado de tamanhos registrado no período analisado.").FontSize(8).Italic();
            return;
        }

        var maxQtd = Math.Max(1, tamanhos.Max(t => t.QuantidadeVendida));

        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn(5);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(corSecundaria).Padding(4).AlignCenter().Text("Tamanho").FontSize(7.5f).Bold().FontColor(Colors.White);
                    header.Cell().Background(corSecundaria).Padding(4).Text("Peças vendidas").FontSize(7.5f).Bold().FontColor(Colors.White);
                    header.Cell().Background(corSecundaria).Padding(4).AlignRight().Text("Peças disponíveis").FontSize(7.5f).Bold().FontColor(Colors.White);
                });

                foreach (var t in tamanhos)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().AlignMiddle()
                        .Background(Colors.Grey.Lighten4).CornerRadius(3).PaddingVertical(2).Text(t.Tamanho).FontSize(8).Bold();

                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignMiddle().Row(row =>
                    {
                        var pct = maxQtd > 0 ? (float)t.QuantidadeVendida / maxQtd : 0f;
                        var barWidth = Math.Max(3, (int)(pct * 140));

                        row.ConstantItem(barWidth).Height(7).Background(corSecundaria).CornerRadius(2);
                        row.ConstantItem(6);
                        row.RelativeItem().Text($"{t.QuantidadeVendida} peças vendidas (R$ {t.ValorTotal:N2})").FontSize(7.5f).Bold();
                    });

                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().AlignMiddle()
                        .Text($"{t.QuantidadeEmEstoque} disponíveis").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                }
            });

            var maisVendido = tamanhos.OrderByDescending(t => t.QuantidadeVendida).FirstOrDefault();
            col.Item().PaddingTop(4).Row(row =>
            {
                if (maisVendido != null && maisVendido.QuantidadeVendida > 0)
                {
                    row.RelativeItem().Text($"👚 O tamanho {maisVendido.Tamanho} foi o mais vendido neste período, com {maisVendido.QuantidadeVendida} peças vendidas.")
                        .FontSize(7.5f).SemiBold().FontColor(corSecundaria);
                }
                else
                {
                    row.RelativeItem().Text("Nenhum tamanho teve peças vendidas no período analisado.")
                        .FontSize(7.5f).Italic().FontColor(Colors.Grey.Darken1);
                }
            });
        });
    }

    private void ComposeProdutosMaisVendidos(IContainer container, List<ProdutoDesempenhoDto> produtos, string corPrimaria)
    {
        if (!produtos.Any())
        {
            container.Background(Colors.Grey.Lighten4).CornerRadius(4).Padding(6)
                .Text("Nenhuma venda registrada no período analisado.").FontSize(8).Italic();
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(42);
                columns.RelativeColumn(3.2f);
                columns.RelativeColumn(1.8f);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.8f);
                columns.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell().Background(corPrimaria).Padding(4).AlignCenter().Text("Foto").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).Text("Roupa").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).AlignRight().Text("Peças vendidas").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).AlignRight().Text("Total vendido").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).AlignRight().Text("Lucro").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).AlignRight().Text("Disponível").FontSize(7.5f).Bold().FontColor(Colors.White);
            });

            foreach (var p in produtos)
            {
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().AlignMiddle()
                    .Element(c => RenderProductThumbnail(c, p.Foto, 34));

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignMiddle().Column(c =>
                {
                    c.Item().Text(p.Nome).FontSize(8).Bold();
                    var cat = string.IsNullOrWhiteSpace(p.CategoriaNome) ? "" : p.CategoriaNome;
                    var varStr = $"{p.Tamanho ?? "-"} / {p.Cor ?? "-"}";
                    c.Item().Text(string.IsNullOrWhiteSpace(cat) ? varStr : $"{cat} • {varStr}").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                });

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().AlignMiddle()
                    .Text($"{p.QuantidadeVendida} un").FontSize(8).Bold();

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().AlignMiddle()
                    .Text($"R$ {p.ValorTotalVendido:N2}").FontSize(7.5f);

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().AlignMiddle()
                    .Text($"R$ {p.LucroEstimado:N2}").FontSize(7.5f).Bold().FontColor(Colors.Green.Darken3);

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().AlignMiddle()
                    .Text($"{p.EstoqueAtual} un").FontSize(7.5f);
            }
        });
    }

    private void ComposePoucasPecasDisponiveis(IContainer container, List<ProdutoEstoqueBaixoDto> produtos)
    {
        if (!produtos.Any())
        {
            container.Background(Colors.Green.Lighten5).Border(0.5f).BorderColor(Colors.Green.Lighten2).CornerRadius(4).Padding(6)
                .Text("Todas as peças estão com boa quantidade disponível na loja.").FontSize(8).Italic().FontColor(Colors.Green.Darken3);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(42);
                columns.RelativeColumn(3.2f);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2.5f);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Orange.Darken3).Padding(4).AlignCenter().Text("Foto").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(Colors.Orange.Darken3).Padding(4).Text("Roupa").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(Colors.Orange.Darken3).Padding(4).Text("Peças restantes").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(Colors.Orange.Darken3).Padding(4).Text("Tamanhos disponíveis").FontSize(7.5f).Bold().FontColor(Colors.White);
            });

            foreach (var p in produtos)
            {
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().AlignMiddle()
                    .Element(c => RenderProductThumbnail(c, p.Foto, 34));

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignMiddle().Column(c =>
                {
                    c.Item().Text(p.Nome).FontSize(8).Bold();
                    if (!string.IsNullOrWhiteSpace(p.CategoriaNome))
                    {
                        c.Item().Text(p.CategoriaNome).FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                    }
                });

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignMiddle().Column(c =>
                {
                    if (p.EstoqueAtual == 0)
                    {
                        c.Item().Text("Esgotada (0 peças na loja)").FontSize(8).Bold().FontColor(Colors.Red.Darken3);
                    }
                    else if (p.EstoqueAtual == 1)
                    {
                        c.Item().Text("Resta apenas 1 peça").FontSize(8).Bold().FontColor(Colors.Orange.Darken3);
                    }
                    else
                    {
                        c.Item().Text($"Restam {p.EstoqueAtual} peças").FontSize(8).Bold().FontColor(Colors.Orange.Darken3);
                    }
                    c.Item().Text($"Mínimo recomendado: {p.EstoqueMinimo} peças").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                });

                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignMiddle().Column(c =>
                {
                    var tam = string.IsNullOrWhiteSpace(p.Tamanho) ? "Único" : p.Tamanho;
                    c.Item().Text($"Tamanhos: {tam}").FontSize(7.5f).Bold();
                    if (!string.IsNullOrWhiteSpace(p.Cor))
                    {
                        c.Item().Text($"Cores: {p.Cor}").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                    }
                });
            }
        });
    }

    private void ComposeRoupasDemorandoVender(IContainer container, List<ProdutoSemVendaDto> produtos)
    {
        if (!produtos.Any())
        {
            container.Background(Colors.Green.Lighten5).Border(0.5f).BorderColor(Colors.Green.Lighten2).CornerRadius(4).Padding(6)
                .Text("Parabéns! Todas as peças estão girando e vendendo bem na loja.").FontSize(8).Italic().FontColor(Colors.Green.Darken3);
            return;
        }

        container.Column(col =>
        {
            foreach (var p in produtos)
            {
                col.Item().PaddingBottom(4).ShowEntire().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).CornerRadius(4).Padding(5).Row(row =>
                {
                    row.ConstantItem(42).AlignCenter().AlignMiddle().Element(c => RenderProductThumbnail(c, p.Foto, 36));
                    row.ConstantItem(6);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text(p.Nome).FontSize(8.5f).Bold();
                            r.ConstantItem(120).AlignRight().Text($"{p.EstoqueAtual} peças na loja").FontSize(8).Bold().FontColor(Colors.Grey.Darken3);
                        });

                        var catStr = string.IsNullOrWhiteSpace(p.CategoriaNome) ? "" : $"{p.CategoriaNome} • ";
                        var varStr = $"{p.Tamanho ?? "-"} / {p.Cor ?? "-"}";
                        c.Item().Text($"{catStr}Tamanho e Cor: {varStr}").FontSize(7).FontColor(Colors.Grey.Darken1);

                        var diasStr = p.DiasSemVenda > 0
                            ? $"Essa roupa está há {p.DiasSemVenda} dias sem vender e ainda existem {p.EstoqueAtual} peças disponíveis."
                            : $"Essa roupa nunca foi vendida e ainda existem {p.EstoqueAtual} peças disponíveis na loja.";
                        c.Item().PaddingTop(2).Text(diasStr).FontSize(7.5f).FontColor(Colors.Grey.Darken3);

                        c.Item().PaddingTop(1).Text("💡 Dica: Talvez seja interessante fazer uma promoção ou avaliar se vale a pena manter essa peça no estoque.")
                            .FontSize(7).Italic().FontColor(Colors.Amber.Darken4);
                    });
                });
            }
        });
    }

    private void ComposeOQueComprarNovamente(IContainer container, List<SugestaoReposicaoItemDto> sugestoes, string corPrimaria)
    {
        if (!sugestoes.Any())
        {
            container.Background(Colors.Green.Lighten5).Border(0.5f).BorderColor(Colors.Green.Lighten2).CornerRadius(4).Padding(6)
                .Text("Estoque equilibrado. Nenhuma reposição prioritária identificada no momento.").FontSize(8).Italic().FontColor(Colors.Green.Darken3);
            return;
        }

        container.Column(col =>
        {
            foreach (var s in sugestoes)
            {
                col.Item().PaddingBottom(4).ShowEntire().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).CornerRadius(4).Padding(5).Row(row =>
                {
                    row.ConstantItem(42).AlignCenter().AlignMiddle().Element(c => RenderProductThumbnail(c, s.Foto, 36));
                    row.ConstantItem(6);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text(s.Nome).FontSize(8.5f).Bold();
                            r.ConstantItem(150).AlignRight().Background(Colors.Green.Lighten5).Border(0.5f).BorderColor(Colors.Green.Medium).PaddingVertical(1).PaddingHorizontal(5).CornerRadius(3)
                                .Text($"➡️ Sugestão: comprar {s.QuantidadeSugerida} peças").FontSize(7.5f).Bold().FontColor(Colors.Green.Darken3);
                        });

                        var catStr = string.IsNullOrWhiteSpace(s.CategoriaNome) ? "" : $"{s.CategoriaNome} • ";
                        var varStr = $"{s.Tamanho ?? "-"} / {s.Cor ?? "-"}";
                        c.Item().Text($"{catStr}Tamanho e Cor: {varStr}").FontSize(7).FontColor(Colors.Grey.Darken1);

                        c.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text(t =>
                            {
                                t.DefaultTextStyle(x => x.FontSize(7.5f));
                                t.Span("Vendeu: ").Bold();
                                t.Span($"{s.VendasUltimos30Dias} peças (últimos 30 dias)   |   ");
                                t.Span("Disponível agora: ").Bold();
                                t.Span($"{s.EstoqueAtual} peças (mínimo: {s.EstoqueMinimo})");
                            });
                        });

                        c.Item().PaddingTop(1).Text(s.JustificativaSugestao).FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
                    });
                });
            }
        });
    }

    private void ComposeListaRapidaBras(IContainer container, List<SugestaoReposicaoItemDto> sugestoes, string corPrimaria)
    {
        if (!sugestoes.Any())
        {
            container.Text("Nenhuma peça para comprar no momento.").FontSize(8).Italic();
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(24);
                columns.RelativeColumn(3.5f);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Background(corPrimaria).Padding(4).AlignCenter().Text("[ ]").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).Text("Roupa").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).Text("Categoria").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).Text("Tamanho").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).Text("Cor").FontSize(7.5f).Bold().FontColor(Colors.White);
                header.Cell().Background(corPrimaria).Padding(4).AlignRight().Text("Comprar (peças)").FontSize(7.5f).Bold().FontColor(Colors.White);
            });

            foreach (var s in sugestoes)
            {
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text("[  ]").FontSize(7.5f);
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(s.Nome).FontSize(7.5f).Bold();
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(s.CategoriaNome).FontSize(7.5f);
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(s.Tamanho ?? "Único").FontSize(7.5f);
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(s.Cor ?? "Variadas").FontSize(7.5f);
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{s.QuantidadeSugerida} peças").FontSize(7.5f).Bold().FontColor(Colors.Green.Darken3);
            }
        });
    }

    private void ComposeFooter(IContainer container, string corSecundaria)
    {
        container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text("Day Mendes Store • Relatório simples e prático para sua loja").FontSize(7).Italic().FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1));
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }

    private static void RenderProductThumbnail(IContainer container, string? fotoPath, float size = 34)
    {
        var bytes = ResolveImageBytes(fotoPath);
        if (bytes != null && bytes.Length > 0)
        {
            try
            {
                container
                    .Width(size)
                    .Height(size)
                    .Border(0.5f)
                    .BorderColor(Colors.Grey.Lighten2)
                    .CornerRadius(3)
                    .Image(bytes)
                    .FitArea();
                return;
            }
            catch
            {
                // Fallback gracefully to placeholder if QuestPDF/Skia cannot decode
            }
        }

        container
            .Width(size)
            .Height(size)
            .Background("#F3F4F6")
            .Border(0.5f)
            .BorderColor("#E5E7EB")
            .CornerRadius(3)
            .AlignCenter()
            .AlignMiddle()
            .Column(col =>
            {
                col.Item().AlignCenter().Text("👗").FontSize(size >= 34 ? 10 : 8);
                col.Item().AlignCenter().Text("Sem foto").FontSize(5).FontColor(Colors.Grey.Medium);
            });
    }

    private static byte[]? ResolveImageBytes(string? imageRef)
    {
        if (string.IsNullOrWhiteSpace(imageRef))
            return null;

        var trimmed = imageRef.Trim();
        if (ImageCache.TryGetValue(trimmed, out var cached))
            return cached;

        var result = LoadImageBytesInternal(trimmed);
        ImageCache[trimmed] = result;
        return result;
    }

    private static byte[]? LoadImageBytesInternal(string pathOrUrl)
    {
        try
        {
            // 1. Data URI (Base64)
            if (pathOrUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                var base64Index = pathOrUrl.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
                if (base64Index >= 0)
                {
                    var base64 = pathOrUrl.Substring(base64Index + 7);
                    return Convert.FromBase64String(base64);
                }
            }

            // 2. HTTP / HTTPS URL
            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1.5) };
                    return client.GetByteArrayAsync(uri, cts.Token).GetAwaiter().GetResult();
                }
                catch
                {
                    return null;
                }
            }

            // 3. Absolute local file path
            if (File.Exists(pathOrUrl))
            {
                return File.ReadAllBytes(pathOrUrl);
            }

            // 4. Relative paths (e.g. /uploads/..., uploads/..., etc.)
            var cleanPath = pathOrUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);

            var candidateDirectories = new List<string>
            {
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                Path.Combine(Directory.GetCurrentDirectory(), "DayMendesStore.API", "wwwroot"),
                Path.Combine(Directory.GetCurrentDirectory(), "backend", "DayMendesStore.API", "wwwroot"),
                AppContext.BaseDirectory,
                Path.Combine(AppContext.BaseDirectory, "wwwroot"),
            };

            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            for (int i = 0; i < 4 && dir != null; i++)
            {
                candidateDirectories.Add(Path.Combine(dir.FullName, "wwwroot"));
                candidateDirectories.Add(Path.Combine(dir.FullName, "DayMendesStore.API", "wwwroot"));
                candidateDirectories.Add(Path.Combine(dir.FullName, "backend", "DayMendesStore.API", "wwwroot"));
                dir = dir.Parent;
            }

            var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 5 && baseDir != null; i++)
            {
                candidateDirectories.Add(Path.Combine(baseDir.FullName, "wwwroot"));
                candidateDirectories.Add(Path.Combine(baseDir.FullName, "DayMendesStore.API", "wwwroot"));
                candidateDirectories.Add(Path.Combine(baseDir.FullName, "backend", "DayMendesStore.API", "wwwroot"));
                baseDir = baseDir.Parent;
            }

            foreach (var candidateDir in candidateDirectories.Distinct())
            {
                try
                {
                    var combined = Path.Combine(candidateDir, cleanPath);
                    if (File.Exists(combined))
                    {
                        return File.ReadAllBytes(combined);
                    }
                }
                catch { }
            }
        }
        catch
        {
            // Safe catch-all: return null on any error so report generation never crashes
        }

        return null;
    }

    private static byte[]? LoadStoreLogo(string? storeFoto)
    {
        if (!string.IsNullOrWhiteSpace(storeFoto))
        {
            var customLogo = ResolveImageBytes(storeFoto);
            if (customLogo != null) return customLogo;
        }

        var candidateFiles = new[]
        {
            "logo-horizontal.png",
            "logo.png"
        };

        var candidateDirs = new List<string>
        {
            Directory.GetCurrentDirectory(),
            Path.Combine(Directory.GetCurrentDirectory(), "logo"),
            AppContext.BaseDirectory,
            Path.Combine(AppContext.BaseDirectory, "logo")
        };

        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (int i = 0; i < 4 && dir != null; i++)
        {
            candidateDirs.Add(Path.Combine(dir.FullName, "logo"));
            candidateDirs.Add(Path.Combine(dir.FullName, "DayMendesStore.API", "wwwroot"));
            dir = dir.Parent;
        }

        var baseDir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 5 && baseDir != null; i++)
        {
            candidateDirs.Add(Path.Combine(baseDir.FullName, "logo"));
            baseDir = baseDir.Parent;
        }

        foreach (var file in candidateFiles)
        {
            foreach (var cDir in candidateDirs.Distinct())
            {
                try
                {
                    var full = Path.Combine(cDir, file);
                    if (File.Exists(full))
                    {
                        return File.ReadAllBytes(full);
                    }
                }
                catch { }
            }
        }

        return null;
    }

    private static string FormatFormaPagamento(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Outro";
        var clean = raw.Trim().ToLowerInvariant();
        if (clean.Contains("pix")) return "Pix";
        if (clean.Contains("credito") || clean.Contains("crédito")) return "Cartão de Crédito";
        if (clean.Contains("debito") || clean.Contains("débito")) return "Cartão de Débito";
        if (clean.Contains("cartao") || clean.Contains("cartão")) return "Cartão";
        if (clean.Contains("dinheiro")) return "Dinheiro";
        if (clean.Contains("transfer")) return "Transferência";
        if (clean.Contains("boleto")) return "Boleto";
        return raw;
    }

    private static string ParseColor(string? hex, string fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallback;

        var clean = hex.Trim();
        if (clean.StartsWith("#") && (clean.Length == 7 || clean.Length == 4))
            return clean;

        return fallback;
    }
}
