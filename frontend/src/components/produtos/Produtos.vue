<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Categoria, Produto } from '../../api'
import ModalNovaCategoria from './ModalNovaCategoria.vue'
import ModalNovoProduto from './ModalNovoProduto.vue'
import {
  generatedBarcode,
  money,
  printMultipleProductLabels,
  printProductLabels,
} from './etiquetasUtils'

const props = defineProps<{
  produtos: Produto[]
  categorias: Categoria[]
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'refresh'): void
}>()

const isNovoProdutoOpen = ref(false)
const isNovaCategoriaOpen = ref(false)
const searchTerm = ref('')
const selectedProductIds = ref<number[]>([])

const filteredProdutos = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  if (!term) return props.produtos

  return props.produtos.filter((produto) => {
    const nome = (produto.nome ?? '').toLowerCase()
    const marca = (produto.marca ?? '').toLowerCase()
    const categoria = (produto.categoriaNome ?? '').toLowerCase()
    const barcode = produto.variacoes?.[0]
      ? (produto.variacoes[0].codigoBarras || generatedBarcode(produto, produto.variacoes[0])).toLowerCase()
      : ''

    return (
      nome.includes(term) ||
      marca.includes(term) ||
      categoria.includes(term) ||
      barcode.includes(term)
    )
  })
})

const isAllSelected = computed(() => {
  if (filteredProdutos.value.length === 0) return false
  return filteredProdutos.value.every((p) => selectedProductIds.value.includes(p.id))
})

const isSomeSelected = computed(() => {
  return (
    selectedProductIds.value.length > 0 &&
    !isAllSelected.value
  )
})

const selectedProdutos = computed(() => {
  return props.produtos.filter((p) => selectedProductIds.value.includes(p.id))
})

function toggleSelectAll() {
  if (isAllSelected.value) {
    const filteredIds = new Set(filteredProdutos.value.map((p) => p.id))
    selectedProductIds.value = selectedProductIds.value.filter((id) => !filteredIds.has(id))
  } else {
    const newSelected = new Set(selectedProductIds.value)
    filteredProdutos.value.forEach((p) => newSelected.add(p.id))
    selectedProductIds.value = Array.from(newSelected)
  }
}

function toggleProductSelection(id: number) {
  const index = selectedProductIds.value.indexOf(id)
  if (index > -1) {
    selectedProductIds.value.splice(index, 1)
  } else {
    selectedProductIds.value.push(id)
  }
}

function clearSelection() {
  selectedProductIds.value = []
}

function handleGerarEtiquetasSelecionadas() {
  if (selectedProdutos.value.length === 0) return
  printMultipleProductLabels(selectedProdutos.value)
}

function handlePrintIndividual(produto: Produto) {
  printProductLabels(produto)
}

function handleCreated() {
  emit('refresh')
}
</script>

<template>
  <div class="produtos-page-container">
    <header class="produtos-topbar">
      <div>
        <p class="eyebrow">{{ loading ? 'Sincronizando' : 'Day Mendes Store' }}</p>
        <h1>Produtos</h1>
      </div>
      <div class="topbar-actions">
        <button
          type="button"
          class="btn btn-ghost"
          @click="emit('refresh')"
          :disabled="loading"
          title="Atualizar dados"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2" />
          </svg>
          <span>Atualizar</span>
        </button>
        <button
          type="button"
          class="btn btn-secondary"
          @click="isNovaCategoriaOpen = true"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
          </svg>
          <span>Nova categoria</span>
        </button>
        <button
          type="button"
          class="btn btn-primary"
          @click="isNovoProdutoOpen = true"
        >
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
          </svg>
          <span>Novo produto</span>
        </button>
      </div>
    </header>

    <div v-if="selectedProductIds.length > 0" class="selection-banner">
      <div class="selection-info">
        <span class="selection-badge">{{ selectedProductIds.length }}</span>
        <span class="selection-text">
          {{ selectedProductIds.length === 1 ? 'produto selecionado' : 'produtos selecionados' }}
        </span>
      </div>
      <div class="selection-actions">
        <button
          type="button"
          class="btn btn-outline-ghost btn-sm"
          @click="clearSelection"
        >
          Limpar seleção
        </button>
        <button
          type="button"
          class="btn btn-primary btn-sm btn-gerar-etiquetas"
          @click="handleGerarEtiquetasSelecionadas"
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M6 2h12a2 2 0 0 1 2 2v16l-8-4-8 4V4a2 2 0 0 1 2-2z" />
          </svg>
          <span>Gerar etiquetas</span>
        </button>
      </div>
    </div>

    <section class="panel catalogo-panel">
      <div class="catalogo-header">
        <div class="catalogo-title-area">
          <h2>Catálogo</h2>
          <span class="muted">
            {{ filteredProdutos.length }} {{ filteredProdutos.length === 1 ? 'produto' : 'produtos' }}
          </span>
        </div>

        <div class="search-wrapper">
          <span class="search-icon" aria-hidden="true">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="11" cy="11" r="8" />
              <line x1="21" y1="21" x2="16.65" y2="16.65" />
            </svg>
          </span>
          <input
            v-model="searchTerm"
            type="text"
            class="search-input"
            placeholder="Buscar produto por nome, código ou marca..."
            autocomplete="off"
          />
          <button
            v-if="searchTerm"
            type="button"
            class="btn-clear-search"
            title="Limpar busca"
            aria-label="Limpar busca"
            @click="searchTerm = ''"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>
      </div>

      <div v-if="filteredProdutos.length > 0" class="table-container">
        <table class="catalogo-table">
          <thead>
            <tr>
              <th class="th-checkbox">
                <input
                  type="checkbox"
                  class="custom-checkbox"
                  :checked="isAllSelected"
                  :indeterminate="isSomeSelected"
                  @change="toggleSelectAll"
                  title="Selecionar todos os produtos listados"
                  aria-label="Selecionar todos os produtos listados"
                />
              </th>
              <th class="th-produto">Produto</th>
              <th class="th-categoria">Categoria</th>
              <th class="th-preco">Preço</th>
              <th class="th-estoque">Estoque</th>
              <th class="th-acoes">Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="produto in filteredProdutos"
              :key="produto.id"
              :class="{ 'row-selected': selectedProductIds.includes(produto.id) }"
            >
              <td class="td-checkbox">
                <input
                  type="checkbox"
                  class="custom-checkbox"
                  :checked="selectedProductIds.includes(produto.id)"
                  @change="toggleProductSelection(produto.id)"
                  :aria-label="`Selecionar ${produto.nome}`"
                />
              </td>
              <td class="td-produto">
                <div class="produto-info">
                  <span class="produto-nome">{{ produto.nome }}</span>
                  <div class="produto-meta">
                    <span class="produto-code">
                      {{ produto.variacoes[0] ? (produto.variacoes[0].codigoBarras || generatedBarcode(produto, produto.variacoes[0])) : 'Sem código' }}
                    </span>
                    <span v-if="produto.marca" class="produto-marca">
                      {{ produto.marca }}
                    </span>
                  </div>
                </div>
              </td>
              <td class="td-categoria">
                <span class="categoria-tag">
                  {{ produto.categoriaNome || 'Geral' }}
                </span>
              </td>
              <td class="td-preco">
                <strong>{{ money(produto.valorVenda) }}</strong>
              </td>
              <td class="td-estoque">
                <span
                  class="estoque-badge"
                  :class="{ 'estoque-baixo': produto.estoqueTotal <= produto.estoqueMinimo }"
                >
                  {{ produto.estoqueTotal }} un
                </span>
              </td>
              <td class="td-acoes">
                <button
                  type="button"
                  class="btn-etiquetas"
                  title="Gerar etiqueta para este produto"
                  @click="handlePrintIndividual(produto)"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M6 2h12a2 2 0 0 1 2 2v16l-8-4-8 4V4a2 2 0 0 1 2-2z" />
                  </svg>
                  <span>Etiquetas</span>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-else class="empty-state">
        <p class="empty-text">
          {{ searchTerm ? `Nenhum produto encontrado para "${searchTerm}".` : 'Nenhum produto cadastrado no momento.' }}
        </p>
        <button
          v-if="searchTerm"
          type="button"
          class="btn btn-secondary btn-sm"
          @click="searchTerm = ''"
        >
          Limpar busca
        </button>
        <button
          v-else
          type="button"
          class="btn btn-primary btn-sm"
          @click="isNovoProdutoOpen = true"
        >
          Cadastrar primeiro produto
        </button>
      </div>
    </section>

    <ModalNovoProduto
      :open="isNovoProdutoOpen"
      :categorias="categorias"
      @close="isNovoProdutoOpen = false"
      @success="handleCreated"
    />

    <ModalNovaCategoria
      :open="isNovaCategoriaOpen"
      @close="isNovaCategoriaOpen = false"
      @success="handleCreated"
    />
  </div>
</template>

<style scoped>
.produtos-page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.produtos-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.eyebrow {
  color: #9d3556;
  font-size: 0.76rem;
  font-weight: 900;
  text-transform: uppercase;
  margin: 0;
}

h1 {
  margin: 0;
  font-size: 1.6rem;
  font-weight: 800;
  color: #25201f;
}

.topbar-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.btn {
  min-height: 40px;
  padding: 0 16px;
  border-radius: 8px;
  font-size: 0.86rem;
  font-weight: 800;
  cursor: pointer;
  border: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: background-color 0.15s ease, opacity 0.15s ease;
}

.btn-sm {
  min-height: 34px;
  padding: 0 12px;
  font-size: 0.8rem;
}

.btn-ghost {
  background: transparent;
  border: 1px solid #d8cfca;
  color: #625955;
}

.btn-ghost:hover:not(:disabled) {
  background: #eee7e3;
  color: #25201f;
}

.btn-secondary {
  background: #eee7e3;
  color: #625955;
}

.btn-secondary:hover:not(:disabled) {
  background: #e2dbd7;
  color: #25201f;
}

.btn-primary {
  background: #b33f62;
  color: #ffffff;
}

.btn-primary:hover:not(:disabled) {
  background: #9d3556;
}

.btn:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.selection-banner {
  background: #fdf2f4;
  border: 1.5px solid #f1bdc8;
  border-radius: 8px;
  padding: 10px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  animation: slideDown 0.15s ease;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.selection-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.selection-badge {
  background: #b33f62;
  color: #ffffff;
  font-size: 0.76rem;
  font-weight: 800;
  padding: 2px 7px;
  border-radius: 10px;
}

.selection-text {
  font-size: 0.88rem;
  font-weight: 700;
  color: #832742;
}

.selection-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.btn-outline-ghost {
  background: transparent;
  border: 1px solid #e2aab7;
  color: #832742;
}

.btn-outline-ghost:hover {
  background: rgba(179, 63, 98, 0.08);
}

.btn-gerar-etiquetas {
  box-shadow: 0 2px 8px rgba(179, 63, 98, 0.25);
}

.catalogo-panel {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 8px;
  box-shadow: 0 8px 26px rgba(48, 35, 30, 0.07);
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.catalogo-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.catalogo-title-area {
  display: flex;
  align-items: baseline;
  gap: 10px;
}

.catalogo-title-area h2 {
  margin: 0;
  font-size: 1.2rem;
  font-weight: 800;
  color: #25201f;
}

.muted {
  color: #8b807b;
  font-size: 0.84rem;
}

.search-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  min-width: 280px;
  max-width: 400px;
  flex: 1;
}

.search-icon {
  position: absolute;
  left: 12px;
  color: #8b807b;
  pointer-events: none;
  display: flex;
}

.search-input {
  width: 100%;
  min-height: 38px;
  padding: 8px 34px 8px 36px;
  border: 1.5px solid #d8cfca;
  border-radius: 6px;
  background: #ffffff;
  color: #25201f;
  font-size: 0.86rem;
}

.search-input:focus {
  outline: none;
  border-color: #b33f62;
  box-shadow: 0 0 0 3px rgba(179, 63, 98, 0.1);
}

.btn-clear-search {
  position: absolute;
  right: 8px;
  background: transparent;
  border: 0;
  color: #8b807b;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
}

.btn-clear-search:hover {
  color: #25201f;
  background: rgba(0, 0, 0, 0.05);
}

.table-container {
  overflow-x: auto;
  border: 1px solid #eee7e3;
  border-radius: 6px;
}

.catalogo-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.88rem;
  text-align: left;
}

.catalogo-table th {
  background: #faf8f6;
  color: #736965;
  font-size: 0.74rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  padding: 12px 14px;
  border-bottom: 1px solid #eee7e3;
  white-space: nowrap;
}

.catalogo-table td {
  padding: 12px 14px;
  border-bottom: 1px solid #eee7e3;
  vertical-align: middle;
}

.catalogo-table tr:last-child td {
  border-bottom: none;
}

.catalogo-table tbody tr:hover {
  background: #fdfcfa;
}

.row-selected {
  background: #fdf7f8 !important;
}

.th-checkbox,
.td-checkbox {
  width: 44px;
  text-align: center;
  padding-right: 4px;
}

.custom-checkbox {
  width: 17px;
  height: 17px;
  accent-color: #b33f62;
  cursor: pointer;
}

.th-produto {
  min-width: 200px;
}

.produto-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.produto-nome {
  font-weight: 700;
  color: #25201f;
}

.produto-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.76rem;
}

.produto-code {
  color: #8b807b;
  font-family: monospace;
  font-weight: 600;
}

.produto-marca {
  color: #625955;
  background: #f4f1ee;
  padding: 1px 6px;
  border-radius: 4px;
}

.th-categoria {
  width: 150px;
}

.categoria-tag {
  display: inline-block;
  background: #f4f1ee;
  color: #625955;
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 0.78rem;
  font-weight: 600;
}

.th-preco {
  width: 120px;
}

.th-estoque {
  width: 110px;
}

.estoque-badge {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 700;
  color: #25201f;
  background: #f4f1ee;
}

.estoque-baixo {
  background: #fff0f2;
  color: #991b1b;
  border: 1px solid #fecdd3;
}

.th-acoes,
.td-acoes {
  width: 110px;
  text-align: right;
}

.btn-etiquetas {
  min-height: 32px;
  padding: 0 10px;
  border-radius: 6px;
  background: transparent;
  border: 1px solid #d8cfca;
  color: #625955;
  font-size: 0.78rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.15s ease;
}

.btn-etiquetas:hover {
  background: #eee7e3;
  color: #25201f;
  border-color: #bfaea6;
}

.empty-state {
  background: #faf8f6;
  border: 1px dashed #d8cfca;
  border-radius: 8px;
  padding: 32px 20px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.empty-text {
  color: #756a65;
  margin: 0;
  font-size: 0.9rem;
}

@media (max-width: 768px) {
  .produtos-topbar {
    flex-direction: column;
    align-items: stretch;
  }

  .topbar-actions {
    justify-content: flex-start;
  }

  .catalogo-header {
    flex-direction: column;
    align-items: stretch;
  }

  .search-wrapper {
    max-width: none;
    min-width: 0;
  }

  .selection-banner {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .selection-actions {
    width: 100%;
    justify-content: space-between;
  }
}
</style>
