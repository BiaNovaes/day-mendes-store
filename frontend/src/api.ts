const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5114'

export type Status = 0 | 1 | 2 | 3
export type StatusVenda = 1 | 2 | 3

export function calcularEstoqueTotal(produto?: { variacoes?: Array<{ quantidadeEstoque?: number; status?: number | Status }> | null } | null): number {
  if (!produto || !Array.isArray(produto.variacoes) || produto.variacoes.length === 0) {
    return 0
  }
  return produto.variacoes
    .filter((v) => v && (v.status === undefined || v.status === null || (v.status as number) === 1))
    .reduce((sum, v) => sum + (Number(v.quantidadeEstoque) || 0), 0)
}

export interface PagedResult<T> {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  items: T[]
}
export interface Loja {
  id: number
  nome: string
  foto?: string
  email: string
  cnpj?: string
  corPrimaria?: string
  corSecundaria?: string
  status: Status
}
export interface LoginResponse {
  token: string
  expiraEm: string
  loja: Loja
}
export interface Categoria {
  id: number
  nome: string
  descricao?: string
  status: Status
  totalProdutos: number
}
export interface VariacaoProduto {
  id: number
  produtoId: number
  tamanho: string
  cor: string
  codigoBarras?: string
  quantidadeEstoque: number
  status: Status
}
export interface Produto {
  id: number
  categoriaId: number
  categoriaNome: string
  foto?: string
  nome: string
  descricao?: string
  marca?: string
  valorCompra: number
  valorVenda: number
  margemLucro: number
  lucroUnitario: number
  quantidadeEstoque: number
  estoqueTotal: number
  estoqueMinimo: number
  estoqueBaixo: boolean
  status: Status
  variacoes: VariacaoProduto[]
}
export interface Cliente {
  id: number
  nome: string
  apelido?: string
  email?: string
  telefone?: string
  endereco?: string
  status: Status
  totalCompras: number
  valorTotalComprado: number
  ultimaCompraEm?: string
  createdAt?: string
  updatedAt?: string
}
export interface Venda {
  id: number
  clienteId?: number
  clienteNome?: string
  clienteTelefone?: string
  dataVenda: string
  formaPagamento: string
  valorTotal: number
  statusVenda: StatusVenda
  status: Status
  motivoCancelamento?: string
  totalItens: number
  itens: Array<{
    id: number
    produtoId: number
    produtoNome: string
    produtoFoto?: string
    variacaoProdutoId: number
    tamanho?: string
    cor?: string
    quantidade: number
    valorUnitario: number
    subtotal: number
  }>
  createdAt?: string
  updatedAt?: string
}

export interface VendaFiltro {
  page?: number
  pageSize?: number
  dataInicio?: string
  dataFim?: string
  clienteId?: number
  formaPagamento?: string
  statusVenda?: StatusVenda
}
export interface Dashboard {
  totalFaturado: number
  totalVendas: number
  totalProdutosVendidos: number
  ticketMedio: number
  lucroBrutoEstimado: number
  margemLucroMedia: number
  vendasPorPeriodo: Array<{ periodo: string; quantidadeVendas: number; totalFaturado: number }>
  vendasPorFormaPagamento: Array<{
    formaPagamento: string
    quantidade: number
    valorTotal: number
    percentual: number
  }>
}
export interface ProdutoEstoqueBaixo {
  produtoId: number
  nome: string
  categoriaNome: string
  tamanho?: string
  cor?: string
  estoqueAtual: number
  estoqueMinimo: number
  statusEstoque: string
}

export interface RelatorioFiltro {
  [key: string]: string | number | boolean | null | undefined
  dataInicio?: string
  dataFim?: string
  categoriaId?: number | string
  tamanho?: string
}

export interface ProdutoDesempenho {
  produtoId: number
  nome: string
  foto?: string
  categoriaNome: string
  tamanho?: string
  cor?: string
  quantidadeVendida: number
  valorTotalVendido: number
  lucroEstimado: number
  estoqueAtual: number
}

export interface ProdutoSemVenda {
  produtoId: number
  nome: string
  foto?: string
  categoriaNome: string
  tamanho?: string
  cor?: string
  estoqueAtual: number
  valorVenda: number
  diasSemVenda: number
  classificacaoRotatividade: string
}

export interface CategoriaDesempenho {
  categoriaId: number
  nome: string
  quantidadeProdutosVendidos: number
  valorTotalFaturado: number
  percentualFaturamento: number
}

export interface TamanhoDesempenho {
  tamanho: string
  quantidadeVendida: number
  valorTotal: number
  quantidadeEmEstoque: number
  quantidadeProdutosZerados: number
  velocidadeSaida: string
}

export interface ClienteDesempenho {
  clienteId?: number
  nome: string
  telefone?: string
  email?: string
  totalCompras: number
  valorTotalComprado: number
  ticketMedio: number
  ultimaCompra?: string
}

export interface SugestaoReposicaoItem {
  produtoId: number
  nome: string
  foto?: string
  categoriaNome: string
  tamanho?: string
  cor?: string
  estoqueAtual: number
  estoqueMinimo: number
  vendasUltimos30Dias: number
  quantidadeSugerida: number
  justificativaSugestao: string
}

export interface RelatorioCompleto {
  loja: Loja
  dataInicio?: string
  dataFim?: string
  dataGeracao: string
  resumoGeral: Dashboard
  produtosMaisVendidos: ProdutoDesempenho[]
  produtosMenosVendidos: ProdutoDesempenho[]
  produtosParados: ProdutoSemVenda[]
  produtosEstoqueBaixo: ProdutoEstoqueBaixo[]
  categoriasMaisVendidas: CategoriaDesempenho[]
  analiseTamanhos: TamanhoDesempenho[]
  melhoresClientes: ClienteDesempenho[]
  sugestoesReposicao: SugestaoReposicaoItem[]
}

let authToken = localStorage.getItem('day-mendes-token') ?? ''

export function setAuthToken(token: string) {
  authToken = token
  localStorage.setItem('day-mendes-token', token)
}
export function clearAuthToken() {
  authToken = ''
  localStorage.removeItem('day-mendes-token')
}
export function hasAuthToken() {
  return Boolean(authToken)
}

async function parseErrorMessage(response: Response): Promise<string> {
  const text = await response.text()
  if (!text) return `Erro ${response.status} ao chamar a API`
  try {
    const data = JSON.parse(text)
    if (data && typeof data === 'object') {
      if (typeof data.message === 'string' && data.message.trim()) {
        return data.message.trim()
      }
      if (typeof data.detail === 'string' && data.detail.trim()) {
        return data.detail.trim()
      }
    }
  } catch {
  }
  return text
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  if (!(options.body instanceof FormData)) headers.set('Content-Type', 'application/json')
  if (authToken) headers.set('Authorization', `Bearer ${authToken}`)

  const response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers })
  if (!response.ok) {
    const message = await parseErrorMessage(response)
    throw new Error(message)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

async function requestBlob(path: string, options: RequestInit = {}): Promise<Blob> {
  const headers = new Headers(options.headers)
  if (authToken) headers.set('Authorization', `Bearer ${authToken}`)

  const response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers })
  if (!response.ok) {
    const message = await parseErrorMessage(response)
    throw new Error(message)
  }
  return response.blob()
}

function query(params: Record<string, string | number | boolean | undefined | null>) {
  const search = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') search.set(key, String(value))
  })
  const text = search.toString()
  return text ? `?${text}` : ''
}

export function getProductImageUrl(foto?: string | null): string {
  if (!foto || !foto.trim()) return ''
  if (foto.startsWith('http://') || foto.startsWith('https://') || foto.startsWith('data:')) {
    return foto
  }
  const baseUrl = (API_BASE_URL ?? 'http://localhost:5114').replace(/\/+$/, '')
  const cleanPath = foto.startsWith('/') ? foto : `/${foto}`
  return `${baseUrl}${cleanPath}`
}

export const api = {
  login: (email: string, senha: string) =>
    request<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, senha }),
    }),
  setupLoja: (payload: { nome: string; email: string; senha: string; cnpj?: string }) =>
    request<Loja>('/api/auth/setup-loja', { method: 'POST', body: JSON.stringify(payload) }),
  perfil: () => request<Loja>('/api/auth/perfil'),
  dashboard: (filtro?: RelatorioFiltro) =>
    request<Dashboard>(`/api/relatorio/dashboard${query(filtro ?? {})}`),
  estoqueBaixo: (filtro?: RelatorioFiltro) =>
    request<ProdutoEstoqueBaixo[]>(`/api/relatorio/estoque-baixo${query(filtro ?? {})}`),
  relatorioCompleto: (filtro?: RelatorioFiltro) =>
    request<RelatorioCompleto>(`/api/relatorio/completo${query(filtro ?? {})}`),
  relatorioPdf: (filtro?: RelatorioFiltro) =>
    requestBlob(`/api/relatorio/pdf${query(filtro ?? {})}`),
  categorias: () => request<PagedResult<Categoria>>('/api/categoria?pageSize=100'),
  categoriasAtivas: () => request<Categoria[]>('/api/categoria/ativas'),
  salvarCategoria: (payload: { nome: string; descricao?: string }) =>
    request<Categoria>('/api/categoria', { method: 'POST', body: JSON.stringify(payload) }),
  produtos: async (termoBusca = '') => {
    const res = await request<PagedResult<Produto>>(`/api/produto${query({ pageSize: 100, termoBusca, status: 1 })}`)
    if (res && Array.isArray(res.items)) {
      res.items = res.items.filter((p) => (p.status as number) === 1)
      res.items.forEach((p) => {
        if (p.variacoes) {
          p.variacoes = p.variacoes.filter((v) => (v.status as number) === 1)
        }
        const total = calcularEstoqueTotal(p)
        p.estoqueTotal = total
        p.quantidadeEstoque = total
        p.estoqueBaixo = total <= (p.estoqueMinimo ?? 0)
      })
    }
    return res
  },
  produtosVenda: async () => {
    const list = await request<Produto[]>('/api/produto/ativos-para-venda')
    if (Array.isArray(list)) {
      const activeList = list.filter((p) => (p.status as number) === 1)
      activeList.forEach((p) => {
        if (p.variacoes) {
          p.variacoes = p.variacoes.filter((v) => (v.status as number) === 1)
        }
        const total = calcularEstoqueTotal(p)
        p.estoqueTotal = total
        p.quantidadeEstoque = total
        p.estoqueBaixo = total <= (p.estoqueMinimo ?? 0)
      })
      return activeList
    }
    return list
  },
  salvarProduto: (payload: unknown) =>
    request<Produto>('/api/produto', { method: 'POST', body: JSON.stringify(payload) }),
  atualizarProduto: (id: number, payload: unknown) =>
    request<Produto>(`/api/produto/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  excluirProduto: (id: number) =>
    request<{ message: string }>(`/api/produto/${id}`, { method: 'DELETE' }),
  inativarProduto: (id: number) =>
    request<{ message: string }>(`/api/produto/${id}/inativar`, { method: 'PATCH' }),
  reativarProduto: (id: number) =>
    request<{ message: string }>(`/api/produto/${id}/reativar`, { method: 'PATCH' }),
  atualizarEstoqueVariacao: (variacaoId: number, quantidadeEstoque: number) =>
    request<{
      variacaoId: number
      produtoId: number
      tamanho: string
      cor: string
      quantidadeEstoque: number
    }>(`/api/estoque/variacao/${variacaoId}`, {
      method: 'PUT',
      body: JSON.stringify({ quantidadeEstoque }),
    }),
  uploadFoto: (file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return request<{ url: string }>('/api/upload/foto', {
      method: 'POST',
      body: formData,
    })
  },
  clientes: (termoBusca = '') =>
    request<PagedResult<Cliente>>(`/api/cliente${query({ pageSize: 100, termoBusca })}`),
  clientesAtivos: () => request<Cliente[]>('/api/cliente/ativos'),
  salvarCliente: (payload: {
    nome: string
    apelido?: string
    email?: string
    telefone?: string
    endereco?: string
  }) => request<Cliente>('/api/cliente', { method: 'POST', body: JSON.stringify(payload) }),
  atualizarCliente: (
    id: number,
    payload: {
      nome: string
      apelido?: string
      email?: string
      telefone?: string
      endereco?: string
    }
  ) =>
    request<Cliente>(`/api/cliente/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }),
  vendas: (filtro?: VendaFiltro) =>
    request<PagedResult<Venda>>(`/api/venda${query({ pageSize: 50, ...filtro })}`),
  obterVenda: (id: number) => request<Venda>(`/api/venda/${id}`),
  cancelarVenda: (id: number, motivo?: string) =>
    request<Venda>(`/api/venda/${id}/cancelar`, {
      method: 'POST',
      body: JSON.stringify(motivo ? { motivo } : {}),
    }),
  finalizarVenda: (id: number) =>
    request<Venda>(`/api/venda/${id}/finalizar`, { method: 'POST' }),
  criarVenda: (payload: unknown) =>
    request<Venda>('/api/venda', { method: 'POST', body: JSON.stringify(payload) }),
}
