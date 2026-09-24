<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import {
  api,
  type Categoria,
  type RelatorioCompleto,
  type RelatorioFiltro,
} from '../api'

const props = defineProps<{
  categorias: Categoria[]
}>()

const loading = ref(false)
const generatingPdf = ref(false)
const error = ref('')
const successMessage = ref('')
const isFilterModalOpen = ref(false)
const relatorio = ref<RelatorioCompleto | null>(null)
const activeTab = ref<'geral' | 'pecas' | 'compras' | 'tamanhos'>('geral')
const subTabPecas = ref<'maisVendidas' | 'estoqueBaixo' | 'paradas' | 'todas'>('maisVendidas')
const subTabTamanhos = ref<'ambos' | 'tamanhos' | 'clientes'>('ambos')

const temFiltrosAtivos = computed(() => {
  return Boolean(filtros.dataInicio || filtros.dataFim || filtros.categoriaId || filtros.tamanho)
})

const totalPecasAlerta = computed(() => {
  const baixo = relatorio.value?.produtosEstoqueBaixo.length || 0
  const parados = relatorio.value?.produtosParados.length || 0
  return baixo + parados
})

const filtros = reactive<RelatorioFiltro>({
  dataInicio: '',
  dataFim: '',
  categoriaId: '',
  tamanho: '',
})

const tempFiltros = reactive<RelatorioFiltro>({
  dataInicio: '',
  dataFim: '',
  categoriaId: '',
  tamanho: '',
})

const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
function money(val?: number) {
  return currency.format(val ?? 0)
}

function formatDate(dateStr?: string) {
  if (!dateStr) return '-'
  const datePart = (dateStr.split('T')[0]) || ''
  const parts = datePart.split('-')
  if (parts.length === 3 && parts[0] && parts[1] && parts[2]) {
    return `${parts[2]}/${parts[1]}/${parts[0]}`
  }
  return new Date(dateStr).toLocaleDateString('pt-BR')
}

const periodoTexto = computed(() => {
  if (filtros.dataInicio && filtros.dataFim) {
    return `${formatDate(filtros.dataInicio)} → ${formatDate(filtros.dataFim)}`
  }
  if (filtros.dataInicio) {
    return `A partir de ${formatDate(filtros.dataInicio)}`
  }
  if (filtros.dataFim) {
    return `Até ${formatDate(filtros.dataFim)}`
  }
  return 'Todo o histórico registrado'
})

const categoriaTexto = computed(() => {
  if (!filtros.categoriaId) return 'Todas'
  const cat = props.categorias.find((c) => c.id === Number(filtros.categoriaId))
  return cat ? cat.nome : 'Todas'
})

const tamanhoTexto = computed(() => {
  return filtros.tamanho || 'Todos'
})

async function carregarRelatorio() {
  loading.value = true
  error.value = ''
  try {
    const payload: RelatorioFiltro = {}
    if (filtros.dataInicio) payload.dataInicio = filtros.dataInicio
    if (filtros.dataFim) payload.dataFim = filtros.dataFim
    if (filtros.categoriaId) payload.categoriaId = Number(filtros.categoriaId)
    if (filtros.tamanho) payload.tamanho = filtros.tamanho

    relatorio.value = await api.relatorioCompleto(payload)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Não foi possível carregar os dados do relatório.'
  } finally {
    loading.value = false
  }
}

function abrirModalFiltros() {
  tempFiltros.dataInicio = filtros.dataInicio
  tempFiltros.dataFim = filtros.dataFim
  tempFiltros.categoriaId = filtros.categoriaId
  tempFiltros.tamanho = filtros.tamanho
  isFilterModalOpen.value = true
}

function aplicarFiltrosModal() {
  filtros.dataInicio = tempFiltros.dataInicio
  filtros.dataFim = tempFiltros.dataFim
  filtros.categoriaId = tempFiltros.categoriaId
  filtros.tamanho = tempFiltros.tamanho
  isFilterModalOpen.value = false
  carregarRelatorio()
}

function limparEAplicarFiltros() {
  tempFiltros.dataInicio = ''
  tempFiltros.dataFim = ''
  tempFiltros.categoriaId = ''
  tempFiltros.tamanho = ''
  filtros.dataInicio = ''
  filtros.dataFim = ''
  filtros.categoriaId = ''
  filtros.tamanho = ''
  isFilterModalOpen.value = false
  carregarRelatorio()
}

async function baixarPdf() {
  generatingPdf.value = true
  error.value = ''
  successMessage.value = ''
  try {
    const payload: RelatorioFiltro = {}
    if (filtros.dataInicio) payload.dataInicio = filtros.dataInicio
    if (filtros.dataFim) payload.dataFim = filtros.dataFim
    if (filtros.categoriaId) payload.categoriaId = Number(filtros.categoriaId)
    if (filtros.tamanho) payload.tamanho = filtros.tamanho

    const blob = await api.relatorioPdf(payload)
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    const timestamp = new Date().toISOString().slice(0, 19).replace(/[-:T]/g, '')
    link.download = `relatorio_day_mendes_${timestamp}.pdf`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

    successMessage.value = 'Relatório exportado em PDF com sucesso!'
    setTimeout(() => {
      successMessage.value = ''
    }, 4000)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erro ao gerar o arquivo PDF no backend.'
  } finally {
    generatingPdf.value = false
  }
}

onMounted(() => {
  carregarRelatorio()
})
</script>

<template>
  <div class="relatorios-container">
    <header class="report-topbar">
      <div>
        <p class="eyebrow">{{ loading ? 'Sincronizando' : 'Day Mendes Store' }}</p>
        <h1 class="header-title">Relatórios</h1>
      </div>
      <div class="report-topbar-actions">
        <button
          type="button"
          class="btn-report-ghost"
          @click="carregarRelatorio"
          :disabled="loading"
          title="Atualizar dados do relatório"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2" />
          </svg>
          <span>Atualizar</span>
        </button>

        <button
          type="button"
          class="btn-report-filter"
          :class="{ 'has-filter': temFiltrosAtivos }"
          @click="abrirModalFiltros"
          :disabled="loading"
          title="Filtrar dados do relatório"
        >
          <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2">
            <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
          </svg>
          <span>Filtros</span>
          <span v-if="temFiltrosAtivos" class="filter-dot-indicator"></span>
        </button>

        <button
          type="button"
          class="btn-report-export"
          :disabled="generatingPdf || loading"
          @click="baixarPdf"
          title="Exportar relatório em PDF"
        >
          <svg v-if="!generatingPdf" viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
            <polyline points="7 10 12 15 17 10" />
            <line x1="12" y1="15" x2="12" y2="3" />
          </svg>
          <span v-else class="btn-spinner"></span>
          <span>{{ generatingPdf ? 'Gerando...' : 'Exportar PDF' }}</span>
        </button>
      </div>
    </header>

    <div v-if="successMessage" class="feedback-alert success">
      <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
        <polyline points="20 6 9 17 4 12" />
      </svg>
      <span>{{ successMessage }}</span>
    </div>

    <div v-if="error" class="feedback-alert error">
      <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10" />
        <line x1="12" y1="8" x2="12" y2="12" />
        <line x1="12" y1="16" x2="12.01" y2="16" />
      </svg>
      <span>{{ error }}</span>
    </div>

    <div class="report-filter-bar">
      <div class="filter-chips">
        <span class="filter-chip">
          <span class="chip-label">Período:</span>
          <strong>{{ periodoTexto }}</strong>
        </span>
        <span class="chip-sep">•</span>
        <span class="filter-chip">
          <span class="chip-label">Categoria:</span>
          <strong>{{ categoriaTexto }}</strong>
        </span>
        <span class="chip-sep">•</span>
        <span class="filter-chip">
          <span class="chip-label">Tamanho:</span>
          <strong>{{ tamanhoTexto }}</strong>
        </span>
      </div>
      <div v-if="temFiltrosAtivos" class="filter-clear-wrap">
        <button
          type="button"
          class="btn-clear-inline"
          @click="limparEAplicarFiltros"
          title="Remover filtros aplicados"
        >
          Limpar filtros
        </button>
      </div>
    </div>

    <Teleport to="body">
      <div v-if="isFilterModalOpen" class="modal-overlay" @click.self="isFilterModalOpen = false">
        <div class="modal-box" role="dialog" aria-modal="true" aria-labelledby="modal-title">
          <div class="modal-head">
            <div>
              <h3 id="modal-title" class="modal-heading">Filtrar relatório</h3>
              <p class="modal-caption">Selecione o período e critérios para atualizar o relatório na tela e no PDF</p>
            </div>
            <button
              type="button"
              class="modal-close"
              aria-label="Fechar"
              @click="isFilterModalOpen = false"
            >
              ✕
            </button>
          </div>

          <div class="modal-body-content">
            <div class="form-grid-modal">
              <div class="field-item">
                <label for="modal-data-inicio">Data inicial</label>
                <input id="modal-data-inicio" v-model="tempFiltros.dataInicio" type="date" />
              </div>

              <div class="field-item">
                <label for="modal-data-fim">Data final</label>
                <input id="modal-data-fim" v-model="tempFiltros.dataFim" type="date" />
              </div>

              <div class="field-item">
                <label for="modal-categoria">Categoria</label>
                <select id="modal-categoria" v-model="tempFiltros.categoriaId">
                  <option value="">Todas as categorias</option>
                  <option v-for="cat in categorias" :key="cat.id" :value="cat.id">{{ cat.nome }}</option>
                </select>
              </div>

              <div class="field-item">
                <label for="modal-tamanho">Tamanho</label>
                <select id="modal-tamanho" v-model="tempFiltros.tamanho">
                  <option value="">Todos os tamanhos</option>
                  <option value="P">P</option>
                  <option value="M">M</option>
                  <option value="G">G</option>
                  <option value="GG">GG</option>
                  <option value="Único">Único</option>
                </select>
              </div>
            </div>
          </div>

          <div class="modal-footer-actions">
            <button
              type="button"
              class="btn-modal-clear"
              @click="limparEAplicarFiltros"
            >
              Limpar filtros
            </button>

            <button
              type="button"
              class="btn-modal-apply"
              @click="aplicarFiltrosModal"
            >
              Aplicar filtros
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <nav class="report-nav-tabs" role="tablist">
      <button
        type="button"
        class="nav-tab-btn"
        :class="{ active: activeTab === 'geral' }"
        @click="activeTab = 'geral'"
      >
        <span>Visão geral</span>
      </button>

      <button
        type="button"
        class="nav-tab-btn"
        :class="{ active: activeTab === 'pecas' }"
        @click="activeTab = 'pecas'"
      >
        <span>Peças</span>
        <span v-if="totalPecasAlerta > 0" class="badge-tab-warning" title="Peças com estoque baixo ou paradas">
          {{ totalPecasAlerta }}
        </span>
      </button>

      <button
        type="button"
        class="nav-tab-btn"
        :class="{ active: activeTab === 'compras' }"
        @click="activeTab = 'compras'"
      >
        <span>Sugestões de compra</span>
        <span v-if="relatorio?.sugestoesReposicao.length" class="badge-tab-counter">
          {{ relatorio.sugestoesReposicao.length }}
        </span>
      </button>

      <button
        type="button"
        class="nav-tab-btn"
        :class="{ active: activeTab === 'tamanhos' }"
        @click="activeTab = 'tamanhos'"
      >
        <span>Tamanhos e clientes</span>
      </button>
    </nav>

    <div v-if="loading" class="loading-state-card">
      <span class="btn-spinner large"></span>
      <p>Carregando dados do relatório...</p>
    </div>

    <div v-else-if="activeTab === 'geral'" class="tab-pane">
      <div class="metrics-container">
        <article class="stat-card primary">
          <span class="stat-caption">Total faturado</span>
          <strong class="stat-number">{{ money(relatorio?.resumoGeral.totalFaturado) }}</strong>
          <span class="stat-subtext">Faturamento bruto no período</span>
        </article>

        <article class="stat-card success">
          <span class="stat-caption">Lucro estimado</span>
          <strong class="stat-number text-success">{{ money(relatorio?.resumoGeral.lucroBrutoEstimado) }}</strong>
          <span class="stat-subtext">
            {{ relatorio?.resumoGeral.margemLucroMedia ? `${relatorio.resumoGeral.margemLucroMedia.toFixed(1)}% de margem` : 'Margem bruta' }}
          </span>
        </article>

        <article class="stat-card accent">
          <span class="stat-caption">Peças vendidas</span>
          <strong class="stat-number">{{ relatorio?.resumoGeral.totalProdutosVendidos ?? 0 }}</strong>
          <span class="stat-subtext">Itens comercializados</span>
        </article>

        <article class="stat-card info">
          <span class="stat-caption">Valor médio por compra</span>
          <strong class="stat-number">{{ money(relatorio?.resumoGeral.ticketMedio) }}</strong>
          <span class="stat-subtext">{{ relatorio?.resumoGeral.totalVendas ?? 0 }} compras atendidas</span>
        </article>
      </div>

      <div class="two-columns-grid">
        <div class="content-box">
          <h3 class="box-title">Formas de pagamento</h3>
          <div v-if="relatorio?.resumoGeral.vendasPorFormaPagamento.length" class="rows-list">
            <div
              v-for="fp in relatorio.resumoGeral.vendasPorFormaPagamento"
              :key="fp.formaPagamento"
              class="data-row"
            >
              <div class="row-left">
                <strong>{{ fp.formaPagamento }}</strong>
                <span>{{ fp.quantidade }} compras ({{ fp.percentual.toFixed(1) }}%)</span>
              </div>
              <div class="row-right">
                <strong>{{ money(fp.valorTotal) }}</strong>
              </div>
            </div>
          </div>
          <p v-else class="empty-notice">Nenhuma venda registrada no período selecionado.</p>
        </div>

        <div class="content-box">
          <h3 class="box-title">Categorias com maior faturamento</h3>
          <div v-if="relatorio?.categoriasMaisVendidas.length" class="rows-list">
            <div
              v-for="cat in relatorio.categoriasMaisVendidas"
              :key="cat.categoriaId"
              class="data-row"
            >
              <div class="row-left">
                <strong>{{ cat.nome }}</strong>
                <span>{{ cat.quantidadeProdutosVendidos }} peças vendidas</span>
              </div>
              <div class="row-right">
                <strong>{{ money(cat.valorTotalFaturado) }}</strong>
              </div>
            </div>
          </div>
          <p v-else class="empty-notice">Nenhum dado de categoria disponível no período.</p>
        </div>
      </div>
    </div>

    <div v-else-if="activeTab === 'pecas'" class="tab-pane">
      <div class="subnav-segmented" role="tablist">
        <button
          type="button"
          class="subnav-pill"
          :class="{ active: subTabPecas === 'maisVendidas' }"
          @click="subTabPecas = 'maisVendidas'"
        >
          <span>Mais vendidas</span>
          <span v-if="relatorio?.produtosMaisVendidos.length" class="subnav-pill-badge">
            {{ relatorio.produtosMaisVendidos.length }}
          </span>
        </button>

        <button
          type="button"
          class="subnav-pill"
          :class="{ active: subTabPecas === 'estoqueBaixo' }"
          @click="subTabPecas = 'estoqueBaixo'"
        >
          <span>Estoque baixo</span>
          <span v-if="relatorio?.produtosEstoqueBaixo.length" class="subnav-pill-badge warning">
            {{ relatorio.produtosEstoqueBaixo.length }}
          </span>
        </button>

        <button
          type="button"
          class="subnav-pill"
          :class="{ active: subTabPecas === 'paradas' }"
          @click="subTabPecas = 'paradas'"
        >
          <span>Peças paradas (> 30d)</span>
          <span v-if="relatorio?.produtosParados.length" class="subnav-pill-badge">
            {{ relatorio.produtosParados.length }}
          </span>
        </button>

        <button
          type="button"
          class="subnav-pill"
          :class="{ active: subTabPecas === 'todas' }"
          @click="subTabPecas = 'todas'"
        >
          <span>Ver todas</span>
        </button>
      </div>

      <div v-if="subTabPecas === 'maisVendidas' || subTabPecas === 'todas'" class="content-box">
        <div class="box-header-wrap">
          <h3 class="box-title">Peças mais vendidas</h3>
          <p class="box-desc">Produtos com maior volume e rentabilidade no período</p>
        </div>
        <div v-if="relatorio?.produtosMaisVendidos.length">
          <div class="table-container desktop-table-view">
            <table class="styled-table">
              <thead>
                <tr>
                  <th>Peça</th>
                  <th>Tam/Cor</th>
                  <th class="text-right">Qtd</th>
                  <th class="text-right">Total</th>
                  <th class="text-right">Lucro</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in relatorio.produtosMaisVendidos" :key="`${p.produtoId}-${p.tamanho}-${p.cor}`">
                  <td><strong>{{ p.nome }}</strong></td>
                  <td class="text-muted">{{ p.tamanho || '-' }} / {{ p.cor || '-' }}</td>
                  <td class="text-right font-bold">{{ p.quantidadeVendida }}</td>
                  <td class="text-right">{{ money(p.valorTotalVendido) }}</td>
                  <td class="text-right text-success font-bold">{{ money(p.lucroEstimado) }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="mobile-cards-list">
            <article
              v-for="p in relatorio.produtosMaisVendidos"
              :key="`${p.produtoId}-${p.tamanho}-${p.cor}`"
              class="mobile-product-card"
            >
              <div class="mobile-card-top">
                <strong class="mobile-card-title">{{ p.nome }}</strong>
                <span class="mobile-card-tag">{{ p.tamanho || '-' }} / {{ p.cor || '-' }}</span>
              </div>
              <div class="mobile-card-metrics">
                <div class="mobile-metric-item">
                  <span class="m-label">Vendas</span>
                  <strong class="m-value">{{ p.quantidadeVendida }} un</strong>
                </div>
                <div class="mobile-metric-item">
                  <span class="m-label">Faturamento</span>
                  <strong class="m-value">{{ money(p.valorTotalVendido) }}</strong>
                </div>
                <div class="mobile-metric-item">
                  <span class="m-label">Lucro est.</span>
                  <strong class="m-value text-success">{{ money(p.lucroEstimado) }}</strong>
                </div>
              </div>
            </article>
          </div>
        </div>
        <p v-else class="empty-notice">Nenhuma peça vendida no período.</p>
      </div>

      <div v-if="subTabPecas === 'estoqueBaixo' || subTabPecas === 'todas'" class="content-box" :class="{ 'mt-16': subTabPecas === 'todas' }">
        <div class="box-header-wrap">
          <h3 class="box-title">Peças com pouco estoque</h3>
          <p class="box-desc">Itens próximos ou abaixo do limite mínimo configurado</p>
        </div>
        <div v-if="relatorio?.produtosEstoqueBaixo.length">
          <div class="table-container desktop-table-view">
            <table class="styled-table">
              <thead>
                <tr>
                  <th>Peça</th>
                  <th>Tam/Cor</th>
                  <th class="text-right">Atual</th>
                  <th class="text-right">Mínimo</th>
                  <th>Situação</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in relatorio.produtosEstoqueBaixo" :key="`${p.produtoId}-${p.tamanho}-${p.cor}`">
                  <td><strong>{{ p.nome }}</strong></td>
                  <td class="text-muted">{{ p.tamanho || '-' }} / {{ p.cor || '-' }}</td>
                  <td class="text-right font-bold">{{ p.estoqueAtual }}</td>
                  <td class="text-right text-muted">{{ p.estoqueMinimo }}</td>
                  <td>
                    <span class="pill-badge" :class="p.estoqueAtual === 0 ? 'danger' : 'warning'">
                      {{ p.statusEstoque }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="mobile-cards-list">
            <article
              v-for="p in relatorio.produtosEstoqueBaixo"
              :key="`${p.produtoId}-${p.tamanho}-${p.cor}`"
              class="mobile-product-card"
            >
              <div class="mobile-card-top">
                <strong class="mobile-card-title">{{ p.nome }}</strong>
                <span class="pill-badge" :class="p.estoqueAtual === 0 ? 'danger' : 'warning'">
                  {{ p.statusEstoque }}
                </span>
              </div>
              <span class="mobile-card-subtitle">{{ p.tamanho || '-' }} / {{ p.cor || '-' }}</span>
              <div class="mobile-card-metrics">
                <div class="mobile-metric-item">
                  <span class="m-label">Estoque atual</span>
                  <strong class="m-value" :class="p.estoqueAtual === 0 ? 'text-danger' : ''">{{ p.estoqueAtual }} un</strong>
                </div>
                <div class="mobile-metric-item">
                  <span class="m-label">Estoque mínimo</span>
                  <span class="m-value">{{ p.estoqueMinimo }} un</span>
                </div>
              </div>
            </article>
          </div>
        </div>
        <p v-else class="empty-notice">Todas as peças estão com níveis adequados de estoque.</p>
      </div>

      <div v-if="subTabPecas === 'paradas' || subTabPecas === 'todas'" class="content-box" :class="{ 'mt-16': subTabPecas === 'todas' }">
        <div class="box-header-wrap">
          <h3 class="box-title">Peças paradas (> 30 dias sem venda)</h3>
          <p class="box-desc">Produtos sem giro nos últimos 30 dias para estratégias de remarcação e queima</p>
        </div>
        <div v-if="relatorio?.produtosParados.length">
          <div class="table-container desktop-table-view">
            <table class="styled-table">
              <thead>
                <tr>
                  <th>Peça</th>
                  <th>Categoria</th>
                  <th>Tam/Cor</th>
                  <th class="text-right">Estoque</th>
                  <th class="text-right">Preço</th>
                  <th class="text-right">Tempo sem venda</th>
                  <th>Classificação</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="p in relatorio.produtosParados" :key="`${p.produtoId}-${p.tamanho}-${p.cor}`">
                  <td><strong>{{ p.nome }}</strong></td>
                  <td>{{ p.categoriaNome }}</td>
                  <td class="text-muted">{{ p.tamanho || '-' }} / {{ p.cor || '-' }}</td>
                  <td class="text-right font-bold">{{ p.estoqueAtual }}</td>
                  <td class="text-right">{{ money(p.valorVenda) }}</td>
                  <td class="text-right">{{ p.diasSemVenda }} dias</td>
                  <td>
                    <span class="pill-badge neutral">{{ p.classificacaoRotatividade }}</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="mobile-cards-list">
            <article
              v-for="p in relatorio.produtosParados"
              :key="`${p.produtoId}-${p.tamanho}-${p.cor}`"
              class="mobile-product-card"
            >
              <div class="mobile-card-top">
                <strong class="mobile-card-title">{{ p.nome }}</strong>
                <span class="pill-badge neutral">{{ p.classificacaoRotatividade }}</span>
              </div>
              <span class="mobile-card-subtitle">{{ p.categoriaNome }} • {{ p.tamanho || '-' }} / {{ p.cor || '-' }}</span>
              <div class="mobile-card-metrics">
                <div class="mobile-metric-item">
                  <span class="m-label">Estoque</span>
                  <strong class="m-value">{{ p.estoqueAtual }} un</strong>
                </div>
                <div class="mobile-metric-item">
                  <span class="m-label">Preço</span>
                  <strong class="m-value">{{ money(p.valorVenda) }}</strong>
                </div>
                <div class="mobile-metric-item">
                  <span class="m-label">Parado há</span>
                  <strong class="m-value color-primary">{{ p.diasSemVenda }} dias</strong>
                </div>
              </div>
            </article>
          </div>
        </div>
        <p v-else class="empty-notice">Nenhum produto parado identificado.</p>
      </div>
    </div>

    <div v-else-if="activeTab === 'compras'" class="tab-pane">
      <div class="content-box">
        <div class="box-header-wrap">
          <div>
            <h3 class="box-title">Sugestões de compra para fornecedores</h3>
            <p class="box-desc">Recomendações automáticas baseadas nas peças mais procuradas e que estão no fim do estoque.</p>
          </div>
        </div>

        <div v-if="relatorio?.sugestoesReposicao.length">
          <div class="table-container desktop-table-view">
            <table class="styled-table">
              <thead>
                <tr>
                  <th>Peça</th>
                  <th>Categoria</th>
                  <th>Tamanho / Cor</th>
                  <th class="text-right">Estoque atual</th>
                  <th class="text-right">Vendas (30d)</th>
                  <th class="text-right color-primary">Comprar (Qtd)</th>
                  <th>Justificativa</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in relatorio.sugestoesReposicao" :key="`${item.produtoId}-${item.tamanho}-${item.cor}`">
                  <td><strong>{{ item.nome }}</strong></td>
                  <td>{{ item.categoriaNome }}</td>
                  <td class="text-muted">{{ item.tamanho || '-' }} / {{ item.cor || '-' }}</td>
                  <td class="text-right">{{ item.estoqueAtual }} un</td>
                  <td class="text-right">{{ item.vendasUltimos30Dias }} un</td>
                  <td class="text-right font-bold color-primary">{{ item.quantidadeSugerida }} un</td>
                  <td class="text-muted">{{ item.justificativaSugestao }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="mobile-cards-list">
            <article
              v-for="item in relatorio.sugestoesReposicao"
              :key="`${item.produtoId}-${item.tamanho}-${item.cor}`"
              class="mobile-product-card purchase-suggestion-card"
            >
              <div class="mobile-card-top">
                <div>
                  <strong class="mobile-card-title">{{ item.nome }}</strong>
                  <div class="mobile-card-subtitle">{{ item.categoriaNome }} • {{ item.tamanho || '-' }} / {{ item.cor || '-' }}</div>
                </div>
                <div class="suggestion-highlight-badge">
                  <span class="sugg-label">Comprar</span>
                  <strong class="sugg-qty">{{ item.quantidadeSugerida }} un</strong>
                </div>
              </div>
              <div class="mobile-card-metrics">
                <div class="mobile-metric-item">
                  <span class="m-label">Estoque atual</span>
                  <span class="m-value">{{ item.estoqueAtual }} un</span>
                </div>
                <div class="mobile-metric-item">
                  <span class="m-label">Vendas (30d)</span>
                  <span class="m-value">{{ item.vendasUltimos30Dias }} un</span>
                </div>
              </div>
              <p v-if="item.justificativaSugestao" class="mobile-card-note">
                <svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="10" />
                  <line x1="12" y1="16" x2="12" y2="12" />
                  <line x1="12" y1="8" x2="12.01" y2="8" />
                </svg>
                <span>{{ item.justificativaSugestao }}</span>
              </p>
            </article>
          </div>
        </div>
        <p v-else class="empty-notice">Nenhuma sugestão de reposição necessária no momento.</p>
      </div>
    </div>

    <div v-else-if="activeTab === 'tamanhos'" class="tab-pane">
      <div class="subnav-segmented" role="tablist">
        <button
          type="button"
          class="subnav-pill"
          :class="{ active: subTabTamanhos === 'ambos' }"
          @click="subTabTamanhos = 'ambos'"
        >
          <span>Visão completa</span>
        </button>
        <button
          type="button"
          class="subnav-pill"
          :class="{ active: subTabTamanhos === 'tamanhos' }"
          @click="subTabTamanhos = 'tamanhos'"
        >
          <span>Tamanhos</span>
        </button>
        <button
          type="button"
          class="subnav-pill"
          :class="{ active: subTabTamanhos === 'clientes' }"
          @click="subTabTamanhos = 'clientes'"
        >
          <span>Melhores clientes</span>
        </button>
      </div>

      <div :class="subTabTamanhos === 'ambos' ? 'two-columns-grid' : 'single-column-grid'">
        <div v-if="subTabTamanhos === 'ambos' || subTabTamanhos === 'tamanhos'" class="content-box">
          <div class="box-header-wrap">
            <h3 class="box-title">Análise por tamanho</h3>
            <p class="box-desc">Giro de estoque e faturamento por grade</p>
          </div>
          <div v-if="relatorio?.analiseTamanhos.length">
            <div class="table-container desktop-table-view">
              <table class="styled-table">
                <thead>
                  <tr>
                    <th>Tamanho</th>
                    <th class="text-right">Peças vendidas</th>
                    <th class="text-right">Total faturado</th>
                    <th class="text-right">Disponíveis</th>
                    <th>Velocidade</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="t in relatorio.analiseTamanhos" :key="t.tamanho">
                    <td><strong>{{ t.tamanho }}</strong></td>
                    <td class="text-right font-bold">{{ t.quantidadeVendida }}</td>
                    <td class="text-right">{{ money(t.valorTotal) }}</td>
                    <td class="text-right text-muted">{{ t.quantidadeEmEstoque }}</td>
                    <td>
                      <span class="pill-badge" :class="t.velocidadeSaida === 'Alta' ? 'success' : 'neutral'">
                        {{ t.velocidadeSaida }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div class="mobile-cards-list">
              <article
                v-for="t in relatorio.analiseTamanhos"
                :key="t.tamanho"
                class="mobile-product-card"
              >
                <div class="mobile-card-top">
                  <div class="size-badge-box">
                    <span class="size-letter">{{ t.tamanho }}</span>
                  </div>
                  <span class="pill-badge" :class="t.velocidadeSaida === 'Alta' ? 'success' : 'neutral'">
                    {{ t.velocidadeSaida }}
                  </span>
                </div>
                <div class="mobile-card-metrics">
                  <div class="mobile-metric-item">
                    <span class="m-label">Vendidas</span>
                    <strong class="m-value">{{ t.quantidadeVendida }} un</strong>
                  </div>
                  <div class="mobile-metric-item">
                    <span class="m-label">Faturado</span>
                    <strong class="m-value">{{ money(t.valorTotal) }}</strong>
                  </div>
                  <div class="mobile-metric-item">
                    <span class="m-label">Disponíveis</span>
                    <span class="m-value">{{ t.quantidadeEmEstoque }} un</span>
                  </div>
                </div>
              </article>
            </div>
          </div>
          <p v-else class="empty-notice">Sem dados de tamanhos para o período selecionado.</p>
        </div>

        <div v-if="subTabTamanhos === 'ambos' || subTabTamanhos === 'clientes'" class="content-box">
          <div class="box-header-wrap">
            <h3 class="box-title">Melhores clientes</h3>
            <p class="box-desc">Clientes com maior volume financeiro e compras no período</p>
          </div>
          <div v-if="relatorio?.melhoresClientes.length">
            <div class="table-container desktop-table-view">
              <table class="styled-table">
                <thead>
                  <tr>
                    <th>Cliente</th>
                    <th class="text-right">Compras</th>
                    <th class="text-right">Total comprado</th>
                    <th>Última compra</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="c in relatorio.melhoresClientes" :key="c.clienteId || c.nome">
                    <td>
                      <strong>{{ c.nome }}</strong>
                      <div v-if="c.telefone" class="small-text">{{ c.telefone }}</div>
                    </td>
                    <td class="text-right font-bold">{{ c.totalCompras }}</td>
                    <td class="text-right text-success font-bold">{{ money(c.valorTotalComprado) }}</td>
                    <td class="text-muted">{{ formatDate(c.ultimaCompra) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div class="mobile-cards-list">
              <article
                v-for="c in relatorio.melhoresClientes"
                :key="c.clienteId || c.nome"
                class="mobile-product-card"
              >
                <div class="mobile-card-top">
                  <div>
                    <strong class="mobile-card-title">{{ c.nome }}</strong>
                    <div v-if="c.telefone" class="small-text">{{ c.telefone }}</div>
                  </div>
                  <span class="client-orders-badge">{{ c.totalCompras }} compras</span>
                </div>
                <div class="mobile-card-metrics">
                  <div class="mobile-metric-item">
                    <span class="m-label">Total gasto</span>
                    <strong class="m-value text-success">{{ money(c.valorTotalComprado) }}</strong>
                  </div>
                  <div class="mobile-metric-item">
                    <span class="m-label">Última compra</span>
                    <span class="m-value">{{ formatDate(c.ultimaCompra) }}</span>
                  </div>
                </div>
              </article>
            </div>
          </div>
          <p v-else class="empty-notice">Sem dados de clientes para o período selecionado.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.relatorios-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.report-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.report-topbar .header-title {
  margin: 0;
  font-size: 1.4rem;
  font-weight: 800;
  color: #25201f;
  letter-spacing: -0.3px;
}

.report-topbar-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.btn-report-ghost {
  min-height: 38px;
  padding: 0 14px;
  background: transparent;
  color: #625955;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  font-size: 0.84rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.15s ease;
}

.btn-report-ghost:hover:not(:disabled) {
  background: #eee7e3;
  color: #25201f;
  border-color: #bfaea6;
}

.btn-report-filter {
  min-height: 38px;
  padding: 0 14px;
  background: #ffffff;
  color: #25201f;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  font-size: 0.84rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  position: relative;
  transition: all 0.15s ease;
}

.btn-report-filter:hover:not(:disabled) {
  background: #f7f3f1;
  border-color: #bfaea6;
}

.btn-report-filter.has-filter {
  border-color: #ecc5ce;
  background: #fdf8f9;
  color: #7b2943;
}

.filter-dot-indicator {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #b33f62;
  display: inline-block;
}

.btn-report-export {
  min-height: 38px;
  padding: 0 18px;
  background: #b33f62;
  color: #ffffff;
  border: 0;
  border-radius: 6px;
  font-size: 0.84rem;
  font-weight: 800;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 7px;
  transition: all 0.15s ease;
  box-shadow: 0 2px 8px rgba(179, 63, 98, 0.25);
}

.btn-report-export:hover:not(:disabled) {
  background: #9d3556;
}

.btn-report-ghost:disabled,
.btn-report-filter:disabled,
.btn-report-export:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.feedback-alert {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 0.86rem;
  font-weight: 600;
}

.feedback-alert.success {
  background: #ecfdf5;
  border: 1px solid #a7f3d0;
  color: #065f46;
}

.feedback-alert.error {
  background: #fff1f2;
  border: 1px solid #fecdd3;
  color: #9f1239;
}

.report-filter-bar {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 8px;
  padding: 8px 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.filter-chips {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  font-size: 0.8rem;
  color: #625955;
}

.filter-chip {
  display: inline-flex;
  align-items: center;
  gap: 5px;
}

.chip-label {
  color: #8b807b;
  font-weight: 500;
}

.chip-sep {
  color: #d8cfca;
}

.filter-clear-wrap {
  margin-left: auto;
}

.btn-clear-inline {
  background: transparent;
  border: 0;
  color: #b91c1c;
  font-size: 0.76rem;
  font-weight: 700;
  cursor: pointer;
  padding: 3px 8px;
  border-radius: 4px;
  transition: background 0.15s ease;
  min-height: auto;
}

.btn-clear-inline:hover {
  background: #fff0f2;
}

.btn-spinner {
  width: 15px;
  height: 15px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  display: inline-block;
}

.btn-spinner.large {
  width: 32px;
  height: 32px;
  border-width: 3px;
  border-color: rgba(179, 63, 98, 0.2);
  border-top-color: #b33f62;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(26, 22, 21, 0.55);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
  animation: fadeIn 0.18s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.modal-box {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  width: 100%;
  max-width: 520px;
  box-shadow: 0 24px 60px rgba(0, 0, 0, 0.2);
  overflow: hidden;
  animation: scaleUp 0.18s ease;
}

@keyframes scaleUp {
  from {
    transform: scale(0.96);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

.modal-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  padding: 22px 24px 18px;
  border-bottom: 1px solid #eee7e3;
  background: #faf8f6;
}

.modal-heading {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 800;
  color: #25201f;
}

.modal-caption {
  margin: 4px 0 0;
  font-size: 0.8rem;
  color: #736965;
}

.modal-close {
  background: transparent;
  border: none;
  font-size: 1.1rem;
  color: #8b807b;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  line-height: 1;
}

.modal-close:hover {
  background: #eee7e3;
  color: #25201f;
}

.modal-body-content {
  padding: 24px;
}

.form-grid-modal {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.field-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field-item label {
  font-size: 0.78rem;
  font-weight: 700;
  color: #6b5f5a;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.field-item input,
.field-item select {
  min-height: 40px;
  padding: 8px 12px;
  border: 1px solid #d8cfca;
  border-radius: 8px;
  background: #faf8f6;
  color: #25201f;
  font-size: 0.88rem;
}

.field-item input:focus,
.field-item select:focus {
  outline: none;
  border-color: #b33f62;
  background: #ffffff;
  box-shadow: 0 0 0 3px rgba(179, 63, 98, 0.12);
}

.modal-footer-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 18px 24px;
  border-top: 1px solid #eee7e3;
  background: #faf8f6;
}

.btn-modal-clear {
  min-height: 40px;
  padding: 0 16px;
  background: transparent;
  color: #6b5f5a;
  border: 1px solid #d8cfca;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.16s ease;
}

.btn-modal-clear:hover {
  background: #eee7e3;
  color: #25201f;
}

.btn-modal-apply {
  min-height: 40px;
  padding: 0 20px;
  background: #b33f62;
  color: #ffffff;
  border: 1px solid transparent;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.16s ease;
}

.btn-modal-apply:hover {
  background: #9d3556;
}

.report-nav-tabs {
  display: flex;
  gap: 8px;
  border-bottom: 1px solid #e5ddd8;
  padding-bottom: 2px;
  overflow-x: auto;
}

.nav-tab-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 18px;
  background: transparent;
  border: none;
  border-bottom: 3px solid transparent;
  color: #736965;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.16s ease;
  white-space: nowrap;
}

.nav-tab-btn:hover {
  color: #25201f;
}

.nav-tab-btn.active {
  color: #b33f62;
  border-bottom-color: #b33f62;
}

.badge-tab-counter {
  font-size: 0.68rem;
  font-weight: 800;
  background: #b33f62;
  color: #ffffff;
  padding: 1px 7px;
  border-radius: 10px;
}

.badge-tab-warning {
  font-size: 0.68rem;
  font-weight: 800;
  background: #fef3c7;
  color: #92400e;
  border: 1px solid #fde68a;
  padding: 1px 6px;
  border-radius: 10px;
}

.subnav-segmented {
  display: inline-flex;
  align-items: center;
  background: #f4f1ee;
  border: 1px solid #e5ddd8;
  border-radius: 8px;
  padding: 2px;
  gap: 2px;
  width: fit-content;
  max-width: 100%;
  overflow-x: auto;
}

.subnav-pill {
  min-height: 32px;
  padding: 0 12px;
  border: 0;
  background: transparent;
  color: #6b5f5a;
  font-size: 0.78rem;
  font-weight: 700;
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.15s ease;
}

.subnav-pill:hover:not(.active) {
  color: #25201f;
  background: rgba(0, 0, 0, 0.04);
}

.subnav-pill.active {
  background: #ffffff;
  color: #9d3556;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.subnav-pill-badge {
  font-size: 0.68rem;
  font-weight: 800;
  background: #eee7e3;
  color: #625955;
  padding: 1px 6px;
  border-radius: 8px;
}

.subnav-pill.active .subnav-pill-badge {
  background: #fdf2f5;
  color: #9d3556;
}

.subnav-pill-badge.warning {
  background: #fee2e2;
  color: #991b1b;
}

.single-column-grid {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.tab-pane {
  display: flex;
  flex-direction: column;
  gap: 16px;
  animation: fadeIn 0.2s ease;
}

.metrics-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 14px;
}

.stat-card {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 10px;
  padding: 16px 18px;
  display: flex;
  flex-direction: column;
  gap: 3px;
  box-shadow: 0 2px 8px rgba(48, 35, 30, 0.03);
  transition: border-color 0.15s ease;
}

.stat-card:hover {
  border-color: #d8cfca;
}

.stat-caption {
  font-size: 0.78rem;
  font-weight: 700;
  color: #736965;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.stat-number {
  font-size: 1.55rem;
  font-weight: 800;
  color: #25201f;
}

.stat-subtext {
  font-size: 0.74rem;
  color: #9e938f;
}

.two-columns-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.content-box {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  padding: 22px;
  box-shadow: 0 4px 16px rgba(48, 35, 30, 0.04);
}

.box-title {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 800;
  color: #25201f;
}

.box-desc {
  margin: 4px 0 0;
  font-size: 0.8rem;
  color: #736965;
}

.box-header-wrap {
  margin-bottom: 14px;
}

.rows-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 16px;
}

.data-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: #faf8f6;
  border-radius: 8px;
  border: 1px solid #eee7e3;
}

.row-left {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.row-left span {
  font-size: 0.76rem;
  color: #736965;
}

.table-container {
  width: 100%;
  overflow-x: auto;
  margin-top: 14px;
}

.styled-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.84rem;
}

.styled-table th {
  background: #faf8f6;
  color: #6b5f5a;
  font-weight: 700;
  padding: 11px 14px;
  border-bottom: 1px solid #e5ddd8;
  text-align: left;
}

.styled-table td {
  padding: 11px 14px;
  border-bottom: 1px solid #eee7e3;
  color: #25201f;
}

.styled-table tbody tr:hover {
  background: #fdfbf9;
}

.text-right {
  text-align: right;
}

.text-center {
  text-align: center;
}

.font-bold {
  font-weight: 700;
}

.text-success {
  color: #059669;
}

.text-muted {
  color: #736965;
  font-size: 0.78rem;
}

.small-text {
  font-size: 0.74rem;
  color: #8b807b;
}

.color-primary {
  color: #b33f62;
}

.check-box-ui {
  display: inline-block;
  width: 14px;
  height: 14px;
  border: 1.5px solid #a39793;
  border-radius: 3px;
}

.pill-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 12px;
  display: inline-block;
}

.pill-badge.danger {
  background: #fee2e2;
  color: #991b1b;
}

.pill-badge.warning {
  background: #fef3c7;
  color: #92400e;
}

.pill-badge.success {
  background: #d1fae5;
  color: #065f46;
}

.pill-badge.neutral {
  background: #f3f4f6;
  color: #4b5563;
}

.empty-notice {
  margin: 16px 0 0;
  padding: 24px;
  text-align: center;
  color: #8b807b;
  font-size: 0.88rem;
  font-style: italic;
  background: #faf8f6;
  border: 1px dashed #d8cfca;
  border-radius: 8px;
}

.loading-state-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 60px 20px;
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  color: #736965;
  font-size: 0.95rem;
}

.mt-16 {
  margin-top: 20px;
}

.desktop-table-view {
  display: block;
}

.mobile-cards-list {
  display: none;
}

.tab-label-desktop {
  display: inline;
}

.tab-label-mobile {
  display: none;
}

.mobile-product-card {
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 10px;
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  transition: background-color 0.15s ease;
}

.mobile-card-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 8px;
}

.mobile-card-title {
  font-size: 0.9rem;
  font-weight: 700;
  color: #25201f;
  line-height: 1.25;
}

.mobile-card-subtitle {
  font-size: 0.76rem;
  color: #736965;
}

.mobile-card-tag {
  font-size: 0.74rem;
  font-weight: 600;
  color: #6b5f5a;
  background: #f0e9e4;
  padding: 2px 7px;
  border-radius: 6px;
  white-space: nowrap;
}

.mobile-card-metrics {
  display: flex;
  align-items: center;
  gap: 12px;
  padding-top: 6px;
  border-top: 1px solid #eee7e3;
  flex-wrap: wrap;
}

.mobile-metric-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.mobile-metric-item .m-label {
  font-size: 0.68rem;
  text-transform: uppercase;
  color: #8b807b;
  font-weight: 700;
  letter-spacing: 0.3px;
}

.mobile-metric-item .m-value {
  font-size: 0.86rem;
  color: #25201f;
}

.suggestion-highlight-badge {
  background: #fbeff1;
  border: 1px solid rgba(218, 92, 129, 0.25);
  border-radius: 8px;
  padding: 4px 10px;
  text-align: right;
  flex-shrink: 0;
}

.sugg-label {
  display: block;
  font-size: 0.62rem;
  text-transform: uppercase;
  color: #da5c81;
  font-weight: 800;
  letter-spacing: 0.3px;
}

.sugg-qty {
  font-size: 0.95rem;
  color: #b33f62;
  font-weight: 800;
}

.mobile-card-note {
  margin: 0;
  padding: 6px 10px;
  background: #ffffff;
  border-radius: 6px;
  border: 1px dashed #d8cfca;
  font-size: 0.74rem;
  color: #6b5f5a;
  display: flex;
  align-items: flex-start;
  gap: 6px;
  line-height: 1.3;
}

.mobile-card-note svg {
  color: #b33f62;
  flex-shrink: 0;
  margin-top: 1px;
}

.size-badge-box {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: #ffffff;
  border: 1.5px solid #b33f62;
  color: #b33f62;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 0.86rem;
}

.client-orders-badge {
  font-size: 0.72rem;
  font-weight: 700;
  background: #f0e9e4;
  color: #594e4a;
  padding: 3px 8px;
  border-radius: 12px;
  white-space: nowrap;
}

.text-danger {
  color: #dc2626;
}

@media (max-width: 980px) {
  .two-columns-grid {
    grid-template-columns: 1fr;
  }
  .report-topbar {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }
  .report-topbar-actions {
    width: 100%;
    justify-content: flex-start;
  }
  .form-grid-modal {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .desktop-table-view {
    display: none !important;
  }

  .mobile-cards-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
    margin-top: 12px;
  }

  .report-topbar .header-title {
    font-size: 1.25rem;
  }

  .report-topbar-actions {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
    width: 100%;
  }

  .btn-report-ghost {
    grid-column: span 2;
    justify-content: center;
  }

  .btn-report-filter,
  .btn-report-export {
    justify-content: center;
    width: 100%;
    font-size: 0.82rem;
    padding: 0 10px;
  }

  .report-filter-bar {
    flex-direction: column;
    align-items: flex-start;
    gap: 6px;
    padding: 10px 12px;
  }

  .filter-chips {
    font-size: 0.76rem;
    gap: 6px;
  }

  .filter-clear-wrap {
    margin-left: 0;
    width: 100%;
    padding-top: 4px;
    border-top: 1px dashed #eee7e3;
  }

  .report-nav-tabs {
    display: flex;
    gap: 6px;
    padding: 4px 0 8px;
    overflow-x: auto;
    scrollbar-width: none;
    -webkit-overflow-scrolling: touch;
    border-bottom: 1px solid #eee7e3;
  }

  .report-nav-tabs::-webkit-scrollbar {
    display: none;
  }

  .nav-tab-btn {
    flex-shrink: 0;
    padding: 7px 12px;
    border-radius: 20px;
    border: 1px solid #e5ddd8;
    background: #ffffff;
    font-size: 0.78rem;
    color: #6b5f5a;
    min-height: 36px;
  }

  .nav-tab-btn.active {
    background: #b33f62;
    color: #ffffff;
    border-color: #b33f62;
    box-shadow: 0 2px 8px rgba(179, 63, 98, 0.25);
  }

  .nav-tab-btn.active .badge-tab-counter {
    background: #ffffff;
    color: #b33f62;
  }

  .nav-tab-btn.active .badge-tab-warning {
    background: #ffffff;
    color: #92400e;
  }

  .subnav-segmented {
    width: 100%;
    display: flex;
    overflow-x: auto;
    scrollbar-width: none;
    padding: 3px;
  }

  .subnav-segmented::-webkit-scrollbar {
    display: none;
  }

  .subnav-pill {
    flex: 1;
    justify-content: center;
    padding: 0 8px;
    font-size: 0.75rem;
    min-height: 30px;
  }

  .metrics-container {
    grid-template-columns: 1fr 1fr;
    gap: 10px;
  }

  .stat-card {
    padding: 12px 14px;
    gap: 2px;
    border-radius: 10px;
  }

  .stat-caption {
    font-size: 0.66rem;
  }

  .stat-number {
    font-size: 1.15rem;
  }

  .stat-subtext {
    font-size: 0.68rem;
  }

  .content-box {
    padding: 16px 14px;
    border-radius: 10px;
  }

  .box-title {
    font-size: 0.95rem;
  }

  .box-desc {
    font-size: 0.76rem;
  }

  .data-row {
    padding: 10px 12px;
    gap: 8px;
  }

  .row-left strong {
    font-size: 0.84rem;
  }

  .row-right strong {
    font-size: 0.88rem;
  }

  .modal-overlay {
    padding: 12px;
    align-items: flex-end;
  }

  .modal-box {
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    border-radius: 16px 16px 10px 10px;
  }

  .modal-body-content {
    overflow-y: auto;
    padding: 16px 14px;
  }

  .modal-head {
    padding: 16px 16px 12px;
  }

  .modal-heading {
    font-size: 1.02rem;
  }

  .modal-footer-actions {
    padding: 12px 14px;
    gap: 8px;
  }

  .btn-modal-clear,
  .btn-modal-apply {
    flex: 1;
    min-height: 40px;
    font-size: 0.82rem;
  }
}

@media (max-width: 480px) {
  .metrics-container {
    grid-template-columns: 1fr;
    gap: 8px;
  }

  .subnav-pill {
    padding: 0 6px;
    font-size: 0.72rem;
  }
}
</style>
