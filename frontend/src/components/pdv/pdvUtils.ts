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
