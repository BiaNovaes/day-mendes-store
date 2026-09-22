<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { Venda } from '../../api'
import VendaRow from './VendaRow.vue'
import VendaDetalhes from './VendaDetalhes.vue'

const props = defineProps<{
  vendas: Venda[]
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'refresh'): void
}>()

const searchTerm = ref('')
const dataInicio = ref('')
const dataFim = ref('')
const statusFiltro = ref<'todos' | 1 | 2 | 3>('todos')
const formaPagamento = ref('todas')
const selectedVenda = ref<Venda | null>(null)
const isModalOpen = ref(false)

const counts = computed(() => {
  const total = props.vendas.length
  const finalizadas = props.vendas.filter((v) => v.statusVenda === 2).length
  const canceladas = props.vendas.filter((v) => v.statusVenda === 3).length
  const rascunhos = props.vendas.filter((v) => v.statusVenda === 1).length

  return {
    total,
    finalizadas,
    canceladas,
    rascunhos,
  }
})

function toLocalDateString(dateStr: string): string {
  const d = new Date(dateStr)
  if (isNaN(d.getTime())) return ''
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

const hasActiveFilters = computed(() => {
  return Boolean(
    searchTerm.value.trim() ||
    dataInicio.value ||
    dataFim.value ||
    statusFiltro.value !== 'todos' ||
    formaPagamento.value !== 'todas'
  )
})

function limparFiltros() {
  searchTerm.value = ''
  dataInicio.value = ''
  dataFim.value = ''
  statusFiltro.value = 'todos'
  formaPagamento.value = 'todas'
}

const filteredVendas = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()

  return props.vendas.filter((venda) => {
    if (dataInicio.value) {
      const vendaDate = toLocalDateString(venda.dataVenda)
      if (vendaDate && vendaDate < dataInicio.value) {
        return false
      }
    }

    if (dataFim.value) {
      const vendaDate = toLocalDateString(venda.dataVenda)
      if (vendaDate && vendaDate > dataFim.value) {
        return false
      }
    }

    if (statusFiltro.value !== 'todos' && venda.statusVenda !== statusFiltro.value) {
      return false
    }

    if (formaPagamento.value !== 'todas') {
      const norm = (venda.formaPagamento ?? '').toLowerCase()
      const target = formaPagamento.value.toLowerCase()
      if (!norm.includes(target) && !target.includes(norm)) {
        return false
      }
    }

    if (!term) return true

    const idMatch = `#${venda.id}`.includes(term) || String(venda.id).includes(term)
    const clienteMatch = (venda.clienteNome ?? '').toLowerCase().includes(term)
    const telefoneMatch = (venda.clienteTelefone ?? '').toLowerCase().includes(term)
    const pagamentoMatch = (venda.formaPagamento ?? '').toLowerCase().includes(term)

    return idMatch || clienteMatch || telefoneMatch || pagamentoMatch
  })
})

function abrirDetalhes(venda: Venda) {
  selectedVenda.value = venda
  isModalOpen.value = true
}

function fecharDetalhes() {
  isModalOpen.value = false
  selectedVenda.value = null
}

function handleVendaUpdated() {
  emit('refresh')
}

watch(
  () => props.vendas,
  (newVendas) => {
    if (selectedVenda.value) {
      const found = newVendas.find((v) => v.id === selectedVenda.value?.id)
      if (found) {
        selectedVenda.value = found
      }
    }
  },
  { deep: true }
)
</script>

<template>
  <div class="vendas-page-container">
    <header class="vendas-topbar">
      <div>
        <p class="eyebrow">{{ loading ? 'Sincronizando' : 'Day Mendes Store' }}</p>
        <h1>Últimas vendas</h1>
      </div>

      <div class="topbar-actions">
        <button
          type="button"
          class="btn-atualizar"
          :disabled="loading"
          @click="emit('refresh')"
        >
          <svg
            viewBox="0 0 24 24"
            width="15"
            height="15"
            fill="none"
            stroke="currentColor"
            stroke-width="2.2"
            class="icon-refresh"
            :class="{ spinning: loading }"
          >
            <path d="M23 4v6h-6" />
            <path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10" />
          </svg>
          <span>{{ loading ? 'Atualizando...' : 'Atualizar' }}</span>
        </button>
      </div>
    </header>

    <section class="vendas-panel">
      <div class="filtros-container">
        <div class="filtros-header">
          <div class="filtros-title">
            <svg viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2">
              <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
            </svg>
            <span>Filtros</span>
          </div>
          <button
            v-if="hasActiveFilters"
            type="button"
            class="btn-limpar-filtros"
            @click="limparFiltros"
          >
            Limpar filtros
          </button>
        </div>

        <div class="filtros-grid">
          <div class="campo-filtro campo-busca">
            <label for="filtro-busca">Buscar</label>
            <div class="input-com-icone">
              <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" class="icon-input">
                <circle cx="11" cy="11" r="8" />
                <line x1="21" y1="21" x2="16.65" y2="16.65" />
              </svg>
              <input
                id="filtro-busca"
                v-model="searchTerm"
                type="text"
                placeholder="Nº (#153), cliente..."
              />
            </div>
          </div>

          <div class="campo-filtro">
            <label for="filtro-data-inicio">Data inicial</label>
            <input
              id="filtro-data-inicio"
              v-model="dataInicio"
              type="date"
            />
          </div>

          <div class="campo-filtro">
            <label for="filtro-data-fim">Data final</label>
            <input
              id="filtro-data-fim"
              v-model="dataFim"
              type="date"
            />
          </div>

          <div class="campo-filtro">
            <label for="filtro-status">Status</label>
            <select id="filtro-status" v-model="statusFiltro">
              <option value="todos">Todos ({{ counts.total }})</option>
              <option :value="2">Finalizadas ({{ counts.finalizadas }})</option>
              <option :value="3">Canceladas ({{ counts.canceladas }})</option>
              <option :value="1">Rascunhos ({{ counts.rascunhos }})</option>
            </select>
          </div>

          <div class="campo-filtro">
            <label for="filtro-pagamento">Pagamento</label>
            <select id="filtro-pagamento" v-model="formaPagamento">
              <option value="todas">Todas as formas</option>
              <option value="Pix">Pix</option>
              <option value="Crédito">Cartão de Crédito</option>
              <option value="Débito">Cartão de Débito</option>
              <option value="Dinheiro">Dinheiro</option>
            </select>
          </div>
        </div>

        <div v-if="hasActiveFilters" class="filtros-feedback">
          <span>
            Exibindo <strong>{{ filteredVendas.length }}</strong> de <strong>{{ vendas.length }}</strong> vendas
          </span>
        </div>
      </div>

      <div class="table-container">
        <table class="vendas-table">
          <thead>
            <tr>
              <th class="col-th-id">Venda</th>
              <th class="col-th-date">Data / Hora</th>
              <th class="col-th-client">Cliente</th>
              <th class="col-th-payment">Pagamento</th>
              <th class="col-th-status">Status</th>
              <th class="col-th-total text-right">Total</th>
              <th class="col-th-action" aria-label="Ações"></th>
            </tr>
          </thead>
          <tbody>
            <VendaRow
              v-for="venda in filteredVendas"
              :key="venda.id"
              :venda="venda"
              @click="abrirDetalhes"
            />
          </tbody>
        </table>

        <div v-if="filteredVendas.length === 0" class="empty-state">
          <div class="empty-icon-circle">
            <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="1.8">
              <line x1="12" y1="1" x2="12" y2="23" />
              <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
            </svg>
          </div>
          <h3 class="empty-title">
            {{ hasActiveFilters ? 'Nenhuma venda encontrada' : 'Nenhuma venda registrada ainda' }}
          </h3>
          <p class="empty-text">
            {{
              hasActiveFilters
                ? 'Tente ajustar ou limpar os filtros para visualizar outras vendas.'
                : 'As vendas registradas no PDV aparecerão organizadas aqui.'
            }}
          </p>
          <button
            v-if="hasActiveFilters"
            type="button"
            class="btn-reset-search"
            @click="limparFiltros"
          >
            Limpar filtros
          </button>
        </div>
      </div>
    </section>

    <VendaDetalhes
      :venda="selectedVenda"
      :open="isModalOpen"
      @close="fecharDetalhes"
      @updated="handleVendaUpdated"
    />
  </div>
</template>

<style scoped>
.vendas-page-container {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.vendas-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
}

.eyebrow {
  color: #9d3556;
  font-size: 0.76rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  margin: 0;
}

.vendas-topbar h1 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 800;
  color: #25201f;
  letter-spacing: -0.3px;
}

.btn-atualizar {
  min-height: 40px;
  padding: 0 16px;
  background: #b33f62;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 0.86rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: background-color 0.15s ease;
}

.btn-atualizar:hover:not(:disabled) {
  background: #9d3556;
}

.btn-atualizar:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.icon-refresh.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.vendas-panel {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(48, 35, 30, 0.04);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.filtros-container {
  padding: 16px 20px;
  background: #faf8f6;
  border-bottom: 1px solid #eee7e3;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.filtros-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.filtros-title {
  display: flex;
  align-items: center;
  gap: 7px;
  font-size: 0.82rem;
  font-weight: 700;
  color: #625955;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.filtros-title svg {
  color: #b33f62;
}

.btn-limpar-filtros {
  background: transparent;
  border: none;
  color: #b33f62;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
  padding: 2px 6px;
  border-radius: 4px;
}

.btn-limpar-filtros:hover {
  text-decoration: underline;
}

.filtros-grid {
  display: grid;
  grid-template-columns: 1.4fr 1fr 1fr 1fr 1.1fr;
  gap: 12px;
  align-items: end;
}

.campo-filtro {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.campo-filtro label {
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #736965;
  letter-spacing: 0.3px;
}

.campo-filtro input,
.campo-filtro select {
  width: 100%;
  min-height: 38px;
  padding: 6px 10px;
  border: 1px solid #d8cfca;
  border-radius: 8px;
  background: #ffffff;
  color: #25201f;
  font-size: 0.85rem;
  transition: border-color 0.15s ease;
}

.campo-filtro input:focus,
.campo-filtro select:focus {
  outline: none;
  border-color: #b33f62;
}

.input-com-icone {
  position: relative;
  display: flex;
  align-items: center;
}

.icon-input {
  position: absolute;
  left: 10px;
  color: #8b807b;
  pointer-events: none;
}

.input-com-icone input {
  padding-left: 32px;
}

.filtros-feedback {
  font-size: 0.78rem;
  color: #736965;
  padding-top: 2px;
}

.filtros-feedback strong {
  color: #25201f;
}

.table-container {
  width: 100%;
  overflow-x: auto;
}

.vendas-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.86rem;
  text-align: left;
}

.vendas-table th {
  background: #ffffff;
  color: #6b5f5a;
  font-size: 0.74rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  padding: 12px 16px;
  border-bottom: 1px solid #eee7e3;
  white-space: nowrap;
}

.text-right {
  text-align: right;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 48px 20px;
  text-align: center;
}

.empty-icon-circle {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #faf8f6;
  border: 1px solid #e5ddd8;
  color: #8b807b;
  display: flex;
  align-items: center;
  justify-content: center;
}

.empty-title {
  margin: 0;
  font-size: 0.98rem;
  font-weight: 700;
  color: #25201f;
}

.empty-text {
  margin: 0;
  font-size: 0.84rem;
  color: #736965;
  max-width: 360px;
}

.btn-reset-search {
  margin-top: 6px;
  min-height: 34px;
  padding: 0 14px;
  background: transparent;
  color: #b33f62;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
}

@media (max-width: 900px) {
  .filtros-grid {
    grid-template-columns: 1fr 1fr;
  }
  .campo-busca {
    grid-column: span 2;
  }
}

@media (max-width: 768px) {
  .vendas-table thead {
    display: none;
  }
  .vendas-table,
  .vendas-table tbody {
    display: block;
    width: 100%;
  }
  .table-container {
    padding: 12px;
    background: #faf8f6;
  }
}

@media (max-width: 560px) {
  .vendas-topbar {
    flex-direction: column;
    align-items: stretch;
  }
  .btn-atualizar {
    width: 100%;
    justify-content: center;
  }
  .filtros-grid {
    grid-template-columns: 1fr;
  }
  .campo-busca {
    grid-column: span 1;
  }
}
</style>
