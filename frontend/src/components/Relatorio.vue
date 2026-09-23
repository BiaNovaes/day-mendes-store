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
    <header class="report-top-header">
      <span class="header-tag">Gestão inteligente</span>
      <h1 class="header-title">Relatório de exportação</h1>
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

    <section class="report-summary-card">
      <div class="summary-left">
        <div class="summary-badge-icon">
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
            <polyline points="14 2 14 8 20 8" />
            <line x1="16" y1="13" x2="8" y2="13" />
            <line x1="16" y1="17" x2="8" y2="17" />
          </svg>
        </div>

        <div class="summary-text-block">
          <h2 class="summary-name">Relatório de vendas</h2>
          <div class="summary-details">
            <span class="detail-item">
              <strong class="detail-label">Período:</strong> {{ periodoTexto }}
            </span>
            <span class="detail-sep">•</span>
            <span class="detail-item">
              <strong class="detail-label">Categorias:</strong> {{ categoriaTexto }}
            </span>
            <span class="detail-sep">•</span>
            <span class="detail-item">
              <strong class="detail-label">Tamanhos:</strong> {{ tamanhoTexto }}
            </span>
          </div>
        </div>
      </div>

      <div class="summary-actions">
        <button
          type="button"
          class="btn-filter-modal"
          :disabled="loading"
          @click="abrirModalFiltros"
        >
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
            <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
          </svg>
          <span>Filtrar relatório</span>
        </button>

        <button
          type="button"
          class="btn-export-pdf"
          :disabled="generatingPdf || loading"
          @click="baixarPdf"
        >
          <svg v-if="!generatingPdf" viewBox="0 0 24 24" width="17" height="17" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
            <polyline points="7 10 12 15 17 10" />
            <line x1="12" y1="15" x2="12" y2="3" />
          </svg>
          <span v-else class="btn-spinner"></span>
          <span>{{ generatingPdf ? 'Gerando PDF...' : 'Exportar PDF' }}</span>
        </button>
      </div>
    </section>

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

    <nav class="report-nav-tabs">
      <button
        type="button"
        class="nav-tab-btn"
        :class="{ active: activeTab === 'geral' }"
        @click="activeTab = 'geral'"
      >
        <span class="tab-label-desktop">Visão geral das vendas</span>
        <span class="tab-label-mobile">Visão geral</span>
      </button>

      <button
        type="button"
        class="nav-tab-btn"
        :class="{ active: activeTab === 'pecas' }"
        @click="activeTab = 'pecas'"
      >
        <span class="tab-label-desktop">Desempenho das peças</span>
        <span class="tab-label-mobile">Peças</span>
      </button>

      <button
        type="button"
        class="nav-tab-btn"
        :class="{ active: activeTab === 'compras' }"
        @click="activeTab = 'compras'"
      >
        <span class="tab-label-desktop">Sugestões de compra (Brás)</span>
        <span class="tab-label-mobile">Compras</span>
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
        <span class="tab-label-desktop">Tamanhos e clientes</span>
        <span class="tab-label-mobile">Tamanhos</span>
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
      <div class="two-columns-grid">
        <div class="content-box">
          <h3 class="box-title">Peças mais vendidas</h3>
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

        <div class="content-box">
          <h3 class="box-title">Peças com pouco estoque</h3>
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
      </div>

      <div class="content-box mt-16">
        <h3 class="box-title">💤 Peças paradas (> 30 dias sem venda)</h3>
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
                  <th style="width: 38px;"></th>
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
                  <td class="text-center">
                    <span class="check-box-ui"></span>
                  </td>
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
      <div class="two-columns-grid">
        <div class="content-box">
          <h3 class="box-title">Análise por tamanho</h3>
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

        <div class="content-box">
          <h3 class="box-title">Melhores clientes</h3>
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
  gap: 20px;
}

.report-top-header {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.header-tag {
  color: #b33f62;
  font-size: 0.76rem;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.8px;
}

.header-title {
  margin: 0;
  font-size: 1.65rem;
  font-weight: 800;
  color: #25201f;
  letter-spacing: -0.4px;
}

.feedback-alert {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 0.88rem;
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

.report-summary-card {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  padding: 18px 22px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  box-shadow: 0 4px 18px rgba(48, 35, 30, 0.05);
  flex-wrap: wrap;
}

.summary-left {
  display: flex;
  align-items: center;
  gap: 16px;
  min-width: 0;
}

.summary-badge-icon {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: #fbf5f7;
  color: #b33f62;
  border: 1px solid rgba(179, 63, 98, 0.18);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.summary-text-block {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.summary-name {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 800;
  color: #25201f;
  letter-spacing: -0.2px;
}

.summary-details {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  font-size: 0.82rem;
  color: #6b5f5a;
}

.detail-label {
  font-weight: 700;
  color: #3b3331;
}

.detail-sep {
  color: #d1c7c2;
}

.summary-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
}

.btn-filter-modal {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  min-height: 42px;
  padding: 0 16px;
  background: #ffffff;
  color: #3b3331;
  border: 1px solid #d8cfca;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.16s ease;
}

.btn-filter-modal:hover:not(:disabled) {
  background: #f7f3f1;
  border-color: #baa8a1;
  color: #25201f;
}

.btn-export-pdf {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  min-height: 42px;
  padding: 0 20px;
  background: #b33f62;
  color: #ffffff;
  border: 1px solid transparent;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.16s ease;
  box-shadow: 0 4px 14px rgba(179, 63, 98, 0.28);
}

.btn-export-pdf:hover:not(:disabled) {
  background: #9d3556;
  box-shadow: 0 6px 18px rgba(179, 63, 98, 0.36);
}

.btn-filter-modal:disabled,
.btn-export-pdf:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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

.tab-pane {
  display: flex;
  flex-direction: column;
  gap: 20px;
  animation: fadeIn 0.2s ease;
}

.metrics-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.stat-card {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  padding: 20px 22px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  box-shadow: 0 4px 16px rgba(48, 35, 30, 0.04);
}

.stat-card.primary {
  border-left: 4px solid #b33f62;
}

.stat-card.success {
  border-left: 4px solid #10b981;
}

.stat-card.accent {
  border-left: 4px solid #7b1fa2;
}

.stat-card.info {
  border-left: 4px solid #0284c7;
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
  .report-summary-card {
    flex-direction: column;
    align-items: stretch;
  }
  .summary-actions {
    justify-content: stretch;
  }
  .summary-actions button {
    flex: 1;
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

  .tab-label-desktop {
    display: none;
  }

  .tab-label-mobile {
    display: inline;
  }

  .header-tag {
    font-size: 0.7rem;
    letter-spacing: 0.6px;
  }

  .header-title {
    font-size: 1.3rem;
    letter-spacing: -0.3px;
  }

  .report-summary-card {
    padding: 14px 16px;
    gap: 14px;
    border-radius: 10px;
  }

  .summary-badge-icon {
    width: 38px;
    height: 38px;
  }

  .summary-name {
    font-size: 0.95rem;
  }

  .summary-details {
    font-size: 0.76rem;
    gap: 6px;
  }

  .summary-actions {
    width: 100%;
    display: flex;
    gap: 8px;
  }

  .btn-filter-modal,
  .btn-export-pdf {
    flex: 1;
    min-height: 40px;
    padding: 0 10px;
    font-size: 0.8rem;
    justify-content: center;
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

  .summary-actions {
    flex-direction: column;
  }

  .btn-filter-modal,
  .btn-export-pdf {
    width: 100%;
  }
}
</style>
