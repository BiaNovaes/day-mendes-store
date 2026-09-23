<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { api, clearAuthToken, hasAuthToken, type Categoria, type Cliente, type Dashboard, type Loja, type Produto, type ProdutoEstoqueBaixo, type VariacaoProduto, type Venda } from './api'
import Sidebar, { type View } from './components/Sidebar.vue'
import MobileBottomNav from './components/MobileBottomNav.vue'
import Relatorio from './components/Relatorio.vue'
import Vendas from './components/vendas/Vendas.vue'
import Login from './pages/Login.vue'

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
const categoriaForm = reactive({ nome: '', descricao: '' })
const clienteForm = reactive({ nome: '', apelido: '', email: '', telefone: '', endereco: '' })
const produtoForm = reactive({ nome: '', marca: '', descricao: '', categoriaId: '', valorCompra: '', valorVenda: '', estoqueMinimo: '1', tamanho: '', cor: '', quantidadeEstoque: '0' })
const vendaForm = reactive({ clienteId: '', formaPagamento: 'Pix' })
const selectedVariation = ref('')
const selectedQuantity = ref(1)
const scanCode = ref('')
const cart = ref<Array<{ produto: Produto; variacao: VariacaoProduto; quantidade: number }>>([])
const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const menu: Array<{ id: View; label: string }> = [
  { id: 'dashboard', label: 'Painel' },
  { id: 'produtos', label: 'Produtos' },
  { id: 'clientes', label: 'Clientes' },
  { id: 'pdv', label: 'PDV' },
  { id: 'vendas', label: 'Vendas' },
  { id: 'relatorios', label: 'Relatórios' },
]
const cartTotal = computed(() => cart.value.reduce((total, item) => total + item.produto.valorVenda * item.quantidade, 0))
const activeProducts = computed(() => produtos.value.filter((produto) => produto.status === 1))
const availableVariations = computed(() => activeProducts.value.flatMap((produto) => produto.variacoes.filter((variacao) => variacao.status === 1 && variacao.quantidadeEstoque > 0).map((variacao) => ({ produto, variacao }))))
const currentTitle = computed(() => menu.find((item) => item.id === view.value)?.label ?? 'Painel')
const totalEstoque = computed(() => produtos.value.reduce((total, produto) => total + produto.estoqueTotal, 0))
function money(value?: number) { return currency.format(value ?? 0) }
function statusText(status: number) { return status === 1 ? 'Ativo' : 'Inativo' }
function vendaStatus(status: number) { return ({ 1: 'Rascunho', 2: 'Finalizada', 3: 'Cancelada' } as Record<number, string>)[status] ?? 'Pendente' }
function saleDate(date: string) { return new Date(date).toLocaleDateString('pt-BR') }
function generatedBarcode(produto: Produto, variacao: VariacaoProduto) { return `DMS${String(produto.id).padStart(5, '0')}${String(variacao.id).padStart(5, '0')}` }
function escapeHtml(value: string) { return value.replace(/[&<>"]/g, (char) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[char] ?? char) }
function code39Svg(value: string) {
  const patterns: Record<string, string> = { '0': 'nnnwwnwnn', '1': 'wnnwnnnnw', '2': 'nnwwnnnnw', '3': 'wnwwnnnnn', '4': 'nnnwwnnnw', '5': 'wnnwwnnnn', '6': 'nnwwwnnnn', '7': 'nnnwnnwnw', '8': 'wnnwnnwnn', '9': 'nnwwnnwnn', A: 'wnnnnwnnw', B: 'nnwnnwnnw', C: 'wnwnnwnnn', D: 'nnnnwwnnw', E: 'wnnnwwnnn', F: 'nnwnwwnnn', G: 'nnnnnwwnw', H: 'wnnnnwwnn', I: 'nnwnnwwnn', J: 'nnnnwwwnn', K: 'wnnnnnnww', L: 'nnwnnnnww', M: 'wnwnnnnwn', N: 'nnnnwnnww', O: 'wnnnwnnwn', P: 'nnwnwnnwn', Q: 'nnnnnnwww', R: 'wnnnnnwwn', S: 'nnwnnnwwn', T: 'nnnnwnwwn', U: 'wwnnnnnnw', V: 'nwwnnnnnw', W: 'wwwnnnnnn', X: 'nwnnwnnnw', Y: 'wwnnwnnnn', Z: 'nwwnwnnnn', '-': 'nwnnnnwnw', '.': 'wwnnnnwnn', ' ': 'nwwnnnwnn', '$': 'nwnwnwnnn', '/': 'nwnwnnnwn', '+': 'nwnnnwnwn', '%': 'nnnwnwnwn', '*': 'nwnnwnwnn' }
  const text = `*${value.toUpperCase().replace(/[^0-9A-Z ./$+%-]/g, '')}*`
  let x = 0
  const bars: string[] = []
  for (const char of text) {
    const pattern = patterns[char]
    if (!pattern) continue
    pattern.split('').forEach((widthKey, index) => { const width = widthKey === 'w' ? 3 : 1; if (index % 2 === 0) bars.push(`<rect x="${x}" y="0" width="${width}" height="70"/>`); x += width })
    x += 1
  }
  return `<svg class="barcode" viewBox="0 0 ${x} 70" preserveAspectRatio="none">${bars.join('')}</svg>`
}
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
async function saveCategoria() { await runAndReload(async () => { await api.salvarCategoria(categoriaForm); categoriaForm.nome = ''; categoriaForm.descricao = '' }) }
async function saveCliente() { await runAndReload(async () => { await api.salvarCliente(clienteForm); Object.assign(clienteForm, { nome: '', apelido: '', email: '', telefone: '', endereco: '' }) }) }
async function saveProduto() { await runAndReload(async () => { await api.salvarProduto({ categoriaId: Number(produtoForm.categoriaId), nome: produtoForm.nome, marca: produtoForm.marca || null, descricao: produtoForm.descricao || null, valorCompra: Number(produtoForm.valorCompra), valorVenda: Number(produtoForm.valorVenda), estoqueMinimo: Number(produtoForm.estoqueMinimo), variacoes: [{ tamanho: produtoForm.tamanho, cor: produtoForm.cor, quantidadeEstoque: Number(produtoForm.quantidadeEstoque) }] }); Object.assign(produtoForm, { nome: '', marca: '', descricao: '', categoriaId: '', valorCompra: '', valorVenda: '', estoqueMinimo: '1', tamanho: '', cor: '', quantidadeEstoque: '0' }) }) }
function addItem(produto: Produto, variacao: VariacaoProduto, quantity = 1) { const existing = cart.value.find((item) => item.variacao.id === variacao.id); if (existing) existing.quantidade += quantity; else cart.value.push({ produto, variacao, quantidade: quantity }) }
function addScannedItem() { const code = scanCode.value.trim().toUpperCase(); const item = availableVariations.value.find(({ produto, variacao }) => generatedBarcode(produto, variacao) === code || variacao.codigoBarras === code); if (!item) { error.value = `Codigo ${scanCode.value} nao encontrado.`; scanCode.value = ''; return } addItem(item.produto, item.variacao, 1); scanCode.value = ''; error.value = '' }
function addToCart() { const item = availableVariations.value.find(({ variacao }) => variacao.id === Number(selectedVariation.value)); if (!item) return; addItem(item.produto, item.variacao, Math.max(1, selectedQuantity.value)); selectedVariation.value = ''; selectedQuantity.value = 1 }
function removeFromCart(variationId: number) { cart.value = cart.value.filter((item) => item.variacao.id !== variationId) }
function printLabel(produto: Produto, variacao: VariacaoProduto) { const code = generatedBarcode(produto, variacao); const label = window.open('', 'etiqueta', 'width=420,height=320'); if (!label) return; label.document.write(`<!doctype html><html><head><title>Etiqueta ${code}</title><style>body{font-family:Arial,sans-serif;margin:0;padding:18px}.label{border:1px solid #111;width:300px;height:180px;display:grid;place-items:center;text-align:center;padding:10px}.name{font-weight:700;font-size:16px}.meta{font-size:12px}.price{font-size:18px;font-weight:700}.barcode{display:block;width:250px;height:70px;margin:8px auto 4px}.code{font-size:12px;letter-spacing:1px}</style></head><body><div class="label"><div><div class="name">${escapeHtml(produto.nome)}</div><div class="meta">${escapeHtml(variacao.tamanho)} / ${escapeHtml(variacao.cor)}</div><div class="price">${money(produto.valorVenda)}</div>${code39Svg(code)}<div class="code">${code}</div></div></div><scr` + `ipt>window.print()</scr` + `ipt></body></html>`); label.document.close() }
function printProductLabels(produto: Produto) { produto.variacoes.filter((variacao) => variacao.status === 1).forEach((variacao) => printLabel(produto, variacao)) }
async function finishSale() { await runAndReload(async () => { await api.criarVenda({ clienteId: vendaForm.clienteId ? Number(vendaForm.clienteId) : null, formaPagamento: vendaForm.formaPagamento, finalizarImediatamente: true, itens: cart.value.map((item) => ({ produtoId: item.produto.id, variacaoProdutoId: item.variacao.id, quantidade: item.quantidade })) }); cart.value = []; vendaForm.clienteId = '' }) }
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
      <header v-if="view !== 'relatorios' && view !== 'vendas'" class="topbar">
        <div>
          <p class="eyebrow">{{ loading ? 'Sincronizando' : 'Day Mendes Store' }}</p>
          <h1>{{ currentTitle }}</h1>
        </div>
        <button type="button" @click="loadAll" :disabled="loading">Atualizar</button>
      </header>
      <p v-if="error && view !== 'relatorios' && view !== 'vendas'" class="error">{{ error }}</p>
      <section v-if="view === 'dashboard'" class="stack"><div class="metrics"><article><span>Faturamento</span><strong>{{ money(dashboard?.totalFaturado) }}</strong></article><article><span>Vendas</span><strong>{{ dashboard?.totalVendas ?? 0 }}</strong></article><article><span>Ticket medio</span><strong>{{ money(dashboard?.ticketMedio) }}</strong></article><article><span>Estoque</span><strong>{{ totalEstoque }}</strong></article></div><div class="panel"><h2>Estoque baixo</h2><div class="table"><div v-for="item in estoqueBaixo" :key="`${item.produtoId}-${item.tamanho}-${item.cor}`" class="row three"><span>{{ item.nome }}<small>{{ item.categoriaNome }}</small></span><span>{{ item.tamanho }} / {{ item.cor }}</span><strong>{{ item.estoqueAtual }}</strong></div></div><p v-if="!estoqueBaixo.length" class="empty">Nenhum produto abaixo do minimo.</p></div></section>
      <section v-if="view === 'produtos'" class="split"><form class="panel form-grid" @submit.prevent="saveProduto"><h2>Novo produto</h2><label>Nome <input v-model="produtoForm.nome" required></label><label>Categoria <select v-model="produtoForm.categoriaId" required><option value="">Selecione</option><option v-for="categoria in categorias" :key="categoria.id" :value="categoria.id">{{ categoria.nome }}</option></select></label><label>Marca <input v-model="produtoForm.marca"></label><label>Descricao <textarea v-model="produtoForm.descricao"></textarea></label><div class="inline-fields"><label>Compra <input v-model="produtoForm.valorCompra" type="number" min="0" step="0.01" required></label><label>Venda <input v-model="produtoForm.valorVenda" type="number" min="0" step="0.01" required></label><label>Minimo <input v-model="produtoForm.estoqueMinimo" type="number" min="0" required></label></div><div class="inline-fields"><label>Tamanho <input v-model="produtoForm.tamanho" required></label><label>Cor <input v-model="produtoForm.cor" required></label><label>Qtd. <input v-model="produtoForm.quantidadeEstoque" type="number" min="0" required></label></div><button :disabled="loading">Cadastrar produto</button></form><div class="stack"><form class="panel form-grid" @submit.prevent="saveCategoria"><h2>Nova categoria</h2><label>Nome <input v-model="categoriaForm.nome" required></label><label>Descricao <input v-model="categoriaForm.descricao"></label><button :disabled="loading">Cadastrar categoria</button></form><div class="panel table-panel"><h2>Catalogo</h2><div class="table"><div v-for="produto in produtos" :key="produto.id" class="row"><span>{{ produto.nome }}<small>{{ produto.variacoes[0] ? generatedBarcode(produto, produto.variacoes[0]) : 'Sem codigo' }}</small></span><strong>{{ money(produto.valorVenda) }}</strong><span>{{ produto.estoqueTotal }}</span><button class="ghost small-button" @click="printProductLabels(produto)">Etiquetas</button></div></div></div></div></section>
      <section v-if="view === 'clientes'" class="split"><form class="panel form-grid" @submit.prevent="saveCliente"><h2>Novo cliente</h2><label>Nome <input v-model="clienteForm.nome" required></label><label>Apelido <input v-model="clienteForm.apelido"></label><label>E-mail <input v-model="clienteForm.email" type="email"></label><label>Telefone <input v-model="clienteForm.telefone"></label><label>Endereco <textarea v-model="clienteForm.endereco"></textarea></label><button :disabled="loading">Cadastrar cliente</button></form><div class="panel table-panel"><h2>Clientes</h2><div class="table"><div v-for="cliente in clientes" :key="cliente.id" class="row three"><span>{{ cliente.nome }}<small>{{ cliente.apelido }}</small></span><span>{{ cliente.telefone || cliente.email || '-' }}</span><strong>{{ money(cliente.valorTotalComprado) }}</strong></div></div></div></section>
      <section v-if="view === 'pdv'" class="split"><div class="stack"><form class="panel form-grid" @submit.prevent="addScannedItem"><h2>Bipar produto</h2><label>Codigo de barras <input v-model="scanCode" autofocus autocomplete="off" placeholder="Bipe a etiqueta"></label><button>Adicionar bipado</button></form><form class="panel form-grid" @submit.prevent="addToCart"><h2>Busca manual</h2><label>Cliente <select v-model="vendaForm.clienteId"><option value="">Venda sem cliente</option><option v-for="cliente in clientes" :key="cliente.id" :value="cliente.id">{{ cliente.nome }}</option></select></label><label>Produto <select v-model="selectedVariation" required><option value="">Selecione</option><option v-for="item in availableVariations" :key="item.variacao.id" :value="item.variacao.id">{{ item.produto.nome }} - {{ item.variacao.tamanho }} / {{ item.variacao.cor }}</option></select></label><label>Quantidade <input v-model.number="selectedQuantity" type="number" min="1" required></label><button>Adicionar manualmente</button></form></div><div class="panel table-panel"><div class="panel-header"><h2>Carrinho</h2><strong>{{ money(cartTotal) }}</strong></div><div class="table"><div v-for="item in cart" :key="item.variacao.id" class="row cart-row"><span>{{ item.produto.nome }}<small>{{ generatedBarcode(item.produto, item.variacao) }}</small></span><span>{{ item.quantidade }}</span><strong>{{ money(item.produto.valorVenda * item.quantidade) }}</strong><button class="icon-button" @click="removeFromCart(item.variacao.id)">x</button></div></div><p v-if="!cart.length" class="empty">Carrinho vazio.</p><div class="checkout"><label>Pagamento <select v-model="vendaForm.formaPagamento"><option>Pix</option><option>Debito</option><option>Credito</option><option>Dinheiro</option></select></label><button :disabled="!cart.length || loading" @click="finishSale">Finalizar venda</button></div></div></section>
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
</style>
