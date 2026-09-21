const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5114'

export type Status = 0 | 1
export type StatusVenda = 1 | 2 | 3

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
  totalItens: number
  itens: Array<{
    id: number
    produtoId: number
    produtoNome: string
    variacaoProdutoId: number
    tamanho?: string
    cor?: string
    quantidade: number
    valorUnitario: number
    subtotal: number
  }>
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

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  if (!(options.body instanceof FormData)) headers.set('Content-Type', 'application/json')
  if (authToken) headers.set('Authorization', `Bearer ${authToken}`)

  const response = await fetch(`${API_BASE_URL}${path}`, { ...options, headers })
  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || `Erro ${response.status} ao chamar a API`)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

function query(params: Record<string, string | number | boolean | undefined | null>) {
  const search = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') search.set(key, String(value))
  })
  const text = search.toString()
  return text ? `?${text}` : ''
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
  dashboard: () => request<Dashboard>('/api/relatorio/dashboard'),
  estoqueBaixo: () => request<ProdutoEstoqueBaixo[]>('/api/relatorio/estoque-baixo'),
  categorias: () => request<PagedResult<Categoria>>('/api/categoria?pageSize=100'),
  categoriasAtivas: () => request<Categoria[]>('/api/categoria/ativas'),
  salvarCategoria: (payload: { nome: string; descricao?: string }) =>
    request<Categoria>('/api/categoria', { method: 'POST', body: JSON.stringify(payload) }),
  produtos: (termoBusca = '') =>
    request<PagedResult<Produto>>(`/api/produto${query({ pageSize: 100, termoBusca })}`),
  produtosVenda: () => request<Produto[]>('/api/produto/ativos-para-venda'),
  salvarProduto: (payload: unknown) =>
    request<Produto>('/api/produto', { method: 'POST', body: JSON.stringify(payload) }),
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
  vendas: () => request<PagedResult<Venda>>('/api/venda?pageSize=20'),
  criarVenda: (payload: unknown) =>
    request<Venda>('/api/venda', { method: 'POST', body: JSON.stringify(payload) }),
}
