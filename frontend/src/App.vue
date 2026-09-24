<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { api, clearAuthToken, hasAuthToken, type Categoria, type Cliente, type Dashboard, type Loja, type Produto, type ProdutoEstoqueBaixo, type VariacaoProduto, type Venda } from './api'
import Sidebar, { type View } from './components/Sidebar.vue'
import MobileBottomNav from './components/MobileBottomNav.vue'
import Relatorio from './components/Relatorio.vue'
import Vendas from './components/vendas/Vendas.vue'
import Pdv from './components/pdv/Pdv.vue'
import Login from './pages/Login.vue'
import Produtos from './components/produtos/Produtos.vue'

const loading = ref(false)
const error = ref('')
const view = ref<View>('dashboard')
const isAuthenticated = ref(hasAuthToken())
const lojaNome = ref('Day Mendes Store')
const dashboard = ref<Dashboard | null>(null)
const estoqueBaixo = ref<ProdutoEstoqueBaixo[]>([])
const categorias = ref<Categoria[]>([])
const produtos = ref<Produto[]>([])
const clientes = ref<Cliente[]>([])
const vendas = ref<Venda[]>([])
const clienteForm = reactive({ nome: '', apelido: '', email: '', telefone: '', endereco: '' })
const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const menu: Array<{ id: View; label: string }> = [
  { id: 'dashboard', label: 'Painel' },
  { id: 'produtos', label: 'Produtos' },
  { id: 'clientes', label: 'Clientes' },
  { id: 'pdv', label: 'PDV' },
  { id: 'vendas', label: 'Vendas' },
  { id: 'relatorios', label: 'Relatórios' },
]
const activeProducts = computed(() => produtos.value.filter((produto) => produto.status === 1))
const availableVariations = computed(() => activeProducts.value.flatMap((produto) => produto.variacoes.filter((variacao) => variacao.status === 1 && variacao.quantidadeEstoque > 0).map((variacao) => ({ produto, variacao }))))
const clienteSearch = ref('')
const filteredClientes = computed(() => {
  const term = clienteSearch.value.trim().toLowerCase()
  if (!term) return clientes.value
  return clientes.value.filter((cliente) => {
    const nome = (cliente.nome ?? '').toLowerCase()
    const apelido = (cliente.apelido ?? '').toLowerCase()
    const email = (cliente.email ?? '').toLowerCase()
    const telefone = (cliente.telefone ?? '').toLowerCase()
    return nome.includes(term) || apelido.includes(term) || email.includes(term) || telefone.includes(term)
  })
})
const currentTitle = computed(() => menu.find((item) => item.id === view.value)?.label ?? 'Painel')
const totalEstoque = computed(() => produtos.value.reduce((total, produto) => total + produto.estoqueTotal, 0))
function money(value?: number) { return currency.format(value ?? 0) }
function statusText(status: number) { return status === 1 ? 'Ativo' : 'Inativo' }
function vendaStatus(status: number) { return ({ 1: 'Rascunho', 2: 'Finalizada', 3: 'Cancelada' } as Record<number, string>)[status] ?? 'Pendente' }
function saleDate(date: string) { return new Date(date).toLocaleDateString('pt-BR') }
async function loadAll() {
  loading.value = true; error.value = ''
  try { const [perfil, dashboardData, estoqueData, categoriaData, produtoData, clienteData, vendaData] = await Promise.all([api.perfil(), api.dashboard(), api.estoqueBaixo(), api.categorias(), api.produtos(), api.clientes(), api.vendas()]); lojaNome.value = perfil.nome; dashboard.value = dashboardData; estoqueBaixo.value = estoqueData; categorias.value = categoriaData.items; produtos.value = produtoData.items; clientes.value = clienteData.items; vendas.value = vendaData.items } catch (err) { error.value = err instanceof Error ? err.message : 'Nao foi possivel carregar os dados.'; if (error.value.includes('401')) logout() } finally { loading.value = false }
}
async function handleLoginSuccess(loja: Loja) {
  lojaNome.value = loja.nome
  isAuthenticated.value = true
  await loadAll()
}
function logout() { clearAuthToken(); isAuthenticated.value = false }
async function runAndReload(action: () => Promise<void>) { loading.value = true; error.value = ''; try { await action(); await loadAll() } catch (err) { error.value = err instanceof Error ? err.message : 'Operacao nao concluida.' } finally { loading.value = false } }
async function saveCliente() { await runAndReload(async () => { await api.salvarCliente(clienteForm); Object.assign(clienteForm, { nome: '', apelido: '', email: '', telefone: '', endereco: '' }) }) }
onMounted(() => { if (isAuthenticated.value) loadAll() })
</script>

<template>
  <Login v-if="!isAuthenticated" @success="handleLoginSuccess" />
  <main v-else class="app-shell">
    <Sidebar
      :current-view="view"
      :loja-nome="lojaNome"
      @navigate="(newView) => view = newView"
      @logout="logout"
    />
    <section class="content">
      <header v-if="view !== 'relatorios' && view !== 'vendas' && view !== 'produtos'" class="topbar">
        <div>
          <p class="eyebrow">{{ loading ? 'Sincronizando' : 'Day Mendes Store' }}</p>
          <h1>{{ currentTitle }}</h1>
        </div>
        <button type="button" @click="loadAll" :disabled="loading">Atualizar</button>
      </header>
      <p v-if="error && view !== 'relatorios' && view !== 'vendas' && view !== 'produtos'" class="error">{{ error }}</p>
      <section v-if="view === 'dashboard'" class="stack"><div class="metrics"><article><span>Faturamento</span><strong>{{ money(dashboard?.totalFaturado) }}</strong></article><article><span>Vendas</span><strong>{{ dashboard?.totalVendas ?? 0 }}</strong></article><article><span>Ticket medio</span><strong>{{ money(dashboard?.ticketMedio) }}</strong></article><article><span>Estoque</span><strong>{{ totalEstoque }}</strong></article></div><div class="panel"><h2>Estoque baixo</h2><div class="table"><div v-for="item in estoqueBaixo" :key="`${item.produtoId}-${item.tamanho}-${item.cor}`" class="row three"><span>{{ item.nome }}<small>{{ item.categoriaNome }}</small></span><span>{{ item.tamanho }} / {{ item.cor }}</span><strong>{{ item.estoqueAtual }}</strong></div></div><p v-if="!estoqueBaixo.length" class="empty">Nenhum produto abaixo do minimo.</p></div></section>
      <section v-if="view === 'produtos'" class="stack">
        <Produtos
          :produtos="produtos"
          :categorias="categorias"
          :loading="loading"
          @refresh="loadAll"
        />
      </section>
      <section v-if="view === 'clientes'" class="split">
        <form class="panel form-grid" @submit.prevent="saveCliente">
          <h2>Novo cliente</h2>
          <label>Nome <input v-model="clienteForm.nome" required></label>
          <label>Apelido <input v-model="clienteForm.apelido"></label>
          <label>E-mail <input v-model="clienteForm.email" type="email"></label>
          <label>Telefone <input v-model="clienteForm.telefone"></label>
          <label>Endereco <textarea v-model="clienteForm.endereco"></textarea></label>
          <button :disabled="loading">Cadastrar cliente</button>
        </form>
        <div class="panel table-panel">
          <div class="panel-header">
            <h2>Clientes</h2>
            <span class="muted">{{ filteredClientes.length }} {{ filteredClientes.length === 1 ? 'cliente' : 'clientes' }}</span>
          </div>
          <div class="client-search-wrapper">
            <span class="client-search-icon" aria-hidden="true">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="11" cy="11" r="8" />
                <line x1="21" y1="21" x2="16.65" y2="16.65" />
              </svg>
            </span>
            <input
              v-model="clienteSearch"
              type="text"
              class="client-search-input"
              placeholder="Buscar cliente por nome ou dados..."
              autocomplete="off"
            />
            <button
              v-if="clienteSearch"
              type="button"
              class="btn-clear-client-search"
              title="Limpar pesquisa"
              aria-label="Limpar pesquisa"
              @click="clienteSearch = ''"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
          </div>
          <div class="table">
            <div v-for="cliente in filteredClientes" :key="cliente.id" class="row three">
              <span>{{ cliente.nome }}<small>{{ cliente.apelido }}</small></span>
              <span>{{ cliente.telefone || cliente.email || '-' }}</span>
              <strong>{{ money(cliente.valorTotalComprado) }}</strong>
            </div>
          </div>
          <p v-if="!filteredClientes.length" class="empty">
            {{ clienteSearch ? `Nenhum cliente encontrado para "${clienteSearch}".` : 'Nenhum cliente cadastrado.' }}
          </p>
        </div>
      </section>
      <section v-if="view === 'pdv'" class="stack">
        <Pdv
          :clientes="clientes"
          :available-variations="availableVariations"
          :loading="loading"
          @venda-finalizada="loadAll"
          @refresh="loadAll"
        />
      </section>
      <section v-if="view === 'vendas'" class="stack">
        <Vendas
          :vendas="vendas"
          :loading="loading"
          @refresh="loadAll"
        />
      </section>
      <section v-if="view === 'relatorios'" class="stack">
        <Relatorio :categorias="categorias" />
      </section>
    </section>
    <MobileBottomNav
      :current-view="view"
      @navigate="(newView) => view = newView"
      @logout="logout"
    />
  </main>
</template>

<style scoped>
:global(*){box-sizing:border-box}:global(body){margin:0;background:#f4f1ee;color:#25201f;font-family:Inter,system-ui,sans-serif}button,input,select,textarea{font:inherit}button{min-height:42px;border:0;border-radius:8px;background:#b33f62;color:#fff;cursor:pointer;font-weight:800;padding:0 16px}button:disabled{opacity:.65}input,select,textarea{width:100%;border:1px solid #d8cfca;border-radius:8px;background:#fff;color:#25201f;min-height:42px;padding:10px 12px}textarea{min-height:84px;resize:vertical}label{display:grid;gap:7px;color:#625955;font-size:.88rem;font-weight:800}h1,h2,p{margin:0}.muted,small{color:#8b807b}.auth-page{display:grid;min-height:100vh;padding:24px;background:#f4f1ee}.auth-panel{display:grid;gap:20px;margin:auto;width:100%;max-width:480px;padding:32px;background:#fff;border:1px solid #e5ddd8;border-radius:8px;box-shadow:0 28px 80px rgba(48,35,30,.16)}.auth-panel img{max-width:220px}.app-shell{display:flex;min-height:100vh;background:#f4f1ee}.content{flex:1;display:flex;flex-direction:column;gap:20px;padding:28px 36px;min-width:0}.topbar,.panel-header{display:flex;justify-content:space-between;align-items:center;gap:16px}.eyebrow{color:#9d3556;font-size:.76rem;font-weight:900;text-transform:uppercase}.form-grid,.stack{display:grid;gap:14px}.split{display:grid;grid-template-columns:minmax(300px,390px) 1fr;gap:20px;align-items:start}.panel,.metrics article{background:#fff;border:1px solid #e5ddd8;border-radius:8px;box-shadow:0 8px 26px rgba(48,35,30,.07);padding:20px}.metrics{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:14px}.metrics article{display:grid;gap:8px}.metrics strong{font-size:1.4rem}.table{display:grid;margin-top:14px}.row{display:grid;grid-template-columns:1.4fr 1fr .7fr .7fr;align-items:center;gap:12px;min-height:54px;border-top:1px solid #eee7e3;padding:10px 0}.row.three{grid-template-columns:1.4fr 1fr .7fr}.cart-row{grid-template-columns:1.4fr .45fr .8fr 42px}.row span:first-child{display:grid;gap:3px}.inline-fields{display:grid;grid-template-columns:1fr 1fr .8fr;gap:10px}.checkout{display:grid;grid-template-columns:1fr auto;gap:14px;align-items:end;border-top:1px solid #eee7e3;margin-top:16px;padding-top:16px}.empty{background:#faf8f6;border:1px dashed #d8cfca;border-radius:8px;color:#756a65;margin-top:14px;padding:18px;text-align:center}.error{background:#fff0f2;border:1px solid #f1bdc8;border-radius:8px;color:#9b1c3f;padding:12px 14px}.small-button{min-height:34px}.icon-button{min-height:34px;width:34px;padding:0}.ghost{background:transparent;border:1px solid #d8cfca;color:#625955}@media(max-width:980px){.split,.metrics,.checkout{grid-template-columns:1fr}.topbar{align-items:stretch;flex-direction:column}.row,.row.three,.cart-row{grid-template-columns:1fr}}@media(max-width:768px){.app-shell{flex-direction:column}.content{padding:16px 14px calc(76px + env(safe-area-inset-bottom, 0px))}}@media(max-width:560px){.inline-fields{grid-template-columns:1fr}.panel,.metrics article,.auth-panel{padding:16px}.content{padding:14px 12px calc(76px + env(safe-area-inset-bottom, 0px))}}
.client-search-wrapper{position:relative;display:flex;align-items:center;margin-top:12px}.client-search-icon{position:absolute;left:12px;font-size:.85rem;color:#8b807b;pointer-events:none}.client-search-input{width:100%;min-height:40px;padding:8px 36px 8px 36px;border:1.5px solid #d8cfca;border-radius:6px;background:#fff;color:#25201f;font-size:.86rem}.client-search-input:focus{outline:none;border-color:#b33f62;box-shadow:0 0 0 3px rgba(179,63,98,.1)}.btn-clear-client-search{position:absolute;right:8px;background:transparent;border:0;color:#8b807b;font-size:.85rem;cursor:pointer;padding:4px 6px;border-radius:4px;min-height:auto}.btn-clear-client-search:hover{color:#25201f;background:rgba(0,0,0,.05)}
</style>
