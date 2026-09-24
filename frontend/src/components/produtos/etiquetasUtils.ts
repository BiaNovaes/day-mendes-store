import type { Produto, VariacaoProduto } from '../../api'

const currency = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

export function money(value?: number): string {
  return currency.format(value ?? 0)
}

export function generatedBarcode(produto: Produto, variacao: VariacaoProduto): string {
  return `DMS${String(produto.id).padStart(5, '0')}${String(variacao.id).padStart(5, '0')}`
}

export function escapeHtml(value: string): string {
  return value.replace(/[&<>"]/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[char] ?? char)
}

export function code39Svg(value: string): string {
  const patterns: Record<string, string> = {
    '0': 'nnnwwnwnn', '1': 'wnnwnnnnw', '2': 'nnwwnnnnw', '3': 'wnwwnnnnn',
    '4': 'nnnwwnnnw', '5': 'wnnwwnnnn', '6': 'nnwwwnnnn', '7': 'nnnwnnwnw',
    '8': 'wnnwnnwnn', '9': 'nnwwnnwnn', A: 'wnnnnwnnw', B: 'nnwnnwnnw',
    C: 'wnwnnwnnn', D: 'nnnnwwnnw', E: 'wnnnwwnnn', F: 'nnwnwwnnn',
    G: 'nnnnnwwnw', H: 'wnnnnwwnn', I: 'nnwnnwwnn', J: 'nnnnwwwnn',
    K: 'wnnnnnnww', L: 'nnwnnnnww', M: 'wnwnnnnwn', N: 'nnnnwnnww',
    O: 'wnnnwnnwn', P: 'nnwnwnnwn', Q: 'nnnnnnwww', R: 'wnnnnnwwn',
    S: 'nnwnnnwwn', T: 'nnnnwnwwn', U: 'wwnnnnnnw', V: 'nwwnnnnnw',
    W: 'wwwnnnnnn', X: 'nwnnwnnnw', Y: 'wwnnwnnnn', Z: 'nwwnwnnnn',
    '-': 'nwnnnnwnw', '.': 'wwnnnnwnn', ' ': 'nwwnnnwnn', '$': 'nwnwnwnnn',
    '/': 'nwnwnnnwn', '+': 'nwnnnwnwn', '%': 'nnnwnwnwn', '*': 'nwnnwnwnn',
  }
  const text = `*${value.toUpperCase().replace(/[^0-9A-Z ./$+%-]/g, '')}*`
  let x = 0
  const bars: string[] = []
  for (const char of text) {
    const pattern = patterns[char]
    if (!pattern) continue
    pattern.split('').forEach((widthKey, index) => {
      const width = widthKey === 'w' ? 3 : 1
      if (index % 2 === 0) bars.push(`<rect x="${x}" y="0" width="${width}" height="70"/>`)
      x += width
    })
    x += 1
  }
  return `<svg class="barcode" viewBox="0 0 ${x} 70" preserveAspectRatio="none">${bars.join('')}</svg>`
}

export function printMultipleProductLabels(produtos: Produto[]): void {
  const items: Array<{ produto: Produto; variacao: VariacaoProduto }> = []
  for (const produto of produtos) {
    const activeVars = (produto.variacoes || []).filter((v) => v.status === 1)
    if (activeVars.length > 0) {
      for (const v of activeVars) {
        items.push({ produto, variacao: v })
      }
    } else if (produto.variacoes && produto.variacoes.length > 0 && produto.variacoes[0]) {
      items.push({ produto, variacao: produto.variacoes[0] })
    }
  }

  if (items.length === 0) return

  const printWindow = window.open('', '_blank', 'width=840,height=720')
  if (!printWindow) {
    alert('Por favor, permita pop-ups no navegador para gerar as etiquetas.')
    return
  }

  const cardsHtml = items.map(({ produto, variacao }) => {
    const code = variacao.codigoBarras || generatedBarcode(produto, variacao)
    const tamanho = variacao.tamanho ? escapeHtml(variacao.tamanho) : 'U'
    const cor = variacao.cor ? escapeHtml(variacao.cor) : 'Padrão'
    return `
      <div class="label-card">
        <div class="label-inner">
          <div class="store-tag">DAY MENDES STORE</div>
          <div class="name">${escapeHtml(produto.nome)}</div>
          <div class="meta">${tamanho} / ${cor}</div>
          <div class="price">${money(produto.valorVenda)}</div>
          ${code39Svg(code)}
          <div class="code">${escapeHtml(code)}</div>
        </div>
      </div>
    `
  }).join('')

  const title = `Etiquetas - Day Mendes Store (${items.length} ${items.length === 1 ? 'etiqueta' : 'etiquetas'})`

  printWindow.document.write(`<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <title>${escapeHtml(title)}</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
      background: #f4f1ee;
      color: #25201f;
      padding: 24px;
      display: flex;
      flex-direction: column;
      align-items: center;
    }
    .print-header {
      width: 100%;
      max-width: 720px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      padding: 14px 20px;
      background: #ffffff;
      border: 1px solid #e5ddd8;
      border-radius: 8px;
      box-shadow: 0 4px 14px rgba(48, 35, 30, 0.06);
    }
    .print-header-info h1 {
      font-size: 1.15rem;
      font-weight: 800;
      color: #25201f;
    }
    .print-header-info p {
      font-size: 0.82rem;
      color: #756a65;
      margin-top: 2px;
    }
    .print-header-actions {
      display: flex;
      gap: 10px;
    }
    .btn {
      min-height: 38px;
      padding: 0 16px;
      border-radius: 6px;
      font-size: 0.85rem;
      font-weight: 700;
      cursor: pointer;
      border: none;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
    }
    .btn-print {
      background: #b33f62;
      color: #ffffff;
    }
    .btn-print:hover {
      background: #9d3556;
    }
    .btn-close {
      background: #eee7e3;
      color: #625955;
    }
    .btn-close:hover {
      background: #e2dbd7;
    }
    .labels-container {
      display: flex;
      flex-wrap: wrap;
      gap: 16px;
      justify-content: center;
      max-width: 720px;
    }
    .label-card {
      background: #ffffff;
      border: 1px dashed #756a65;
      border-radius: 4px;
      width: 300px;
      height: 180px;
      display: flex;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 8px 12px;
      page-break-inside: avoid;
      break-inside: avoid;
    }
    .label-inner {
      width: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }
    .store-tag {
      font-size: 8px;
      letter-spacing: 1.5px;
      font-weight: 800;
      color: #9d3556;
      text-transform: uppercase;
      margin-bottom: 2px;
    }
    .name {
      font-weight: 800;
      font-size: 14px;
      line-height: 1.2;
      color: #111111;
      max-width: 270px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .meta {
      font-size: 11px;
      color: #625955;
      margin-top: 1px;
    }
    .price {
      font-size: 16px;
      font-weight: 800;
      color: #111111;
      margin-top: 2px;
    }
    .barcode {
      display: block;
      width: 240px;
      height: 52px;
      margin: 4px auto 2px;
    }
    .code {
      font-size: 11px;
      letter-spacing: 1px;
      font-family: monospace;
      color: #222222;
      font-weight: 600;
    }
    @page {
      size: auto;
      margin: 6mm;
    }
    @media print {
      body {
        background: #ffffff;
        padding: 0;
        margin: 0;
      }
      .no-print {
        display: none !important;
      }
      .labels-container {
        display: flex;
        flex-wrap: wrap;
        gap: 0;
        max-width: none;
        width: 100%;
        justify-content: flex-start;
      }
      .label-card {
        border: 1px dashed #aaaaaa;
        margin: 4px;
        page-break-inside: avoid;
        break-inside: avoid;
      }
    }
  </style>
</head>
<body>
  <div class="print-header no-print">
    <div class="print-header-info">
      <h1>Etiquetas para Impressão</h1>
      <p>${items.length} ${items.length === 1 ? 'etiqueta gerada' : 'etiquetas geradas'} para ${produtos.length} ${produtos.length === 1 ? 'produto selecionado' : 'produtos selecionados'}</p>
    </div>
    <div class="print-header-actions">
      <button class="btn btn-close" onclick="window.close()">Fechar</button>
      <button class="btn btn-print" onclick="window.print()">Imprimir / Salvar PDF</button>
    </div>
  </div>
  <div class="labels-container">
    ${cardsHtml}
  </div>
  <script>
    window.onload = function() {
      setTimeout(function() {
        window.print();
      }, 300);
    };
  </script>
</body>
</html>`)
  printWindow.document.close()
}

export function printLabel(produto: Produto, variacao: VariacaoProduto): void {
  printMultipleProductLabels([{ ...produto, variacoes: [variacao] }])
}

export function printProductLabels(produto: Produto): void {
  printMultipleProductLabels([produto])
}
