<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { calcularEstoqueTotal, getProductImageUrl, type Categoria, type Produto } from '../../api'
import ModalNovaCategoria from './ModalNovaCategoria.vue'
import ModalNovoProduto from './ModalNovoProduto.vue'
import ModalEstoqueVariacoes from './ModalEstoqueVariacoes.vue'
import ModalExcluirProduto from './ModalExcluirProduto.vue'
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
const produtoEmEdicao = ref<Produto | null>(null)
const isNovaCategoriaOpen = ref(false)
const isEstoqueModalOpen = ref(false)
const produtoEstoqueModal = ref<Produto | null>(null)
const viewMode = ref<'cards' | 'table'>('cards')
const searchTerm = ref('')
const selectedProductIds = ref<number[]>([])

function handleOpenNovoProduto() {
  produtoEmEdicao.value = null
  isNovoProdutoOpen.value = true
}

function handleEditProduto(produto: Produto) {
  produtoEmEdicao.value = produto
  isNovoProdutoOpen.value = true
}

function handleCloseModal() {
  isNovoProdutoOpen.value = false
  produtoEmEdicao.value = null
}

function handleOpenEstoqueModal(produto: Produto) {
  produtoEstoqueModal.value = produto
  isEstoqueModalOpen.value = true
}

function handleCloseEstoqueModal() {
  isEstoqueModalOpen.value = false
  produtoEstoqueModal.value = null
}

function getEstoqueTotal(produto: Produto): number {
  return calcularEstoqueTotal(produto)
}

function handleVariacaoEstoqueUpdated(data: { produtoId: number; variacaoId: number; novaQuantidade: number }) {
  const prod = props.produtos.find((p) => p.id === data.produtoId)
  if (prod) {
    const v = prod.variacoes?.find((item) => item.id === data.variacaoId)
    if (v) {
      v.quantidadeEstoque = data.novaQuantidade
    }
    const total = calcularEstoqueTotal(prod)
    prod.estoqueTotal = total
    prod.quantidadeEstoque = total
    prod.estoqueBaixo = total <= prod.estoqueMinimo
  }
  emit('refresh')
}

watch(
  () => props.produtos,
  (newProds) => {
    if (produtoEstoqueModal.value) {
      const match = newProds.find((p) => p.id === produtoEstoqueModal.value?.id)
      if (match) {
        produtoEstoqueModal.value = match
      }
    }
  },
  { deep: true }
)

const filteredProdutos = computed(() => {
  const baseList = props.produtos.filter(
    (produto) => produto && (produto.status === undefined || (produto.status as number) === 1)
  )
  const term = searchTerm.value.trim().toLowerCase()
  if (!term) return baseList

  return baseList.filter((produto) => {
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
  return props.produtos.filter(
    (p) => (p.status === undefined || (p.status as number) === 1) && selectedProductIds.value.includes(p.id)
  )
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

const isExcluirModalOpen = ref(false)
const produtosParaExcluir = ref<Produto[]>([])

function handlePromptDeleteProduto(produto: Produto) {
  produtosParaExcluir.value = [produto]
  isExcluirModalOpen.value = true
}

function handlePromptDeleteSelected() {
  if (selectedProdutos.value.length === 0) return
  produtosParaExcluir.value = [...selectedProdutos.value]
  isExcluirModalOpen.value = true
}

function handleDeleteFromEditModal(produto: Produto) {
  isNovoProdutoOpen.value = false
  produtoEmEdicao.value = null
  produtosParaExcluir.value = [produto]
  isExcluirModalOpen.value = true
}

function handleCloseExcluirModal() {
  isExcluirModalOpen.value = false
  produtosParaExcluir.value = []
}

function handleExcluirSuccess() {
  const deletedIds = new Set(produtosParaExcluir.value.map((p) => p.id))
  selectedProductIds.value = selectedProductIds.value.filter((id) => !deletedIds.has(id))
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
          @click="handleOpenNovoProduto"
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
          class="btn btn-outline-danger btn-sm"
          @click="handlePromptDeleteSelected"
          title="Excluir produtos selecionados"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="3 6 5 6 21 6" />
            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
          </svg>
          <span>Excluir selecionados</span>
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

        <div class="catalogo-toolbar">
          <div class="view-mode-toggle" role="group" aria-label="Modo de exibição">
            <button
              type="button"
              class="btn-toggle-view"
              :class="{ active: viewMode === 'cards' }"
              title="Exibição em cards"
              aria-label="Exibição em cards"
              @click="viewMode = 'cards'"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="3" width="7" height="7" rx="1"/>
                <rect x="14" y="3" width="7" height="7" rx="1"/>
                <rect x="14" y="14" width="7" height="7" rx="1"/>
                <rect x="3" y="14" width="7" height="7" rx="1"/>
              </svg>
              <span>Cards</span>
            </button>
            <button
              type="button"
              class="btn-toggle-view"
              :class="{ active: viewMode === 'table' }"
              title="Exibição em tabela"
              aria-label="Exibição em tabela"
              @click="viewMode = 'table'"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="8" y1="6" x2="21" y2="6"/>
                <line x1="8" y1="12" x2="21" y2="12"/>
                <line x1="8" y1="18" x2="21" y2="18"/>
                <line x1="3" y1="6" x2="3.01" y2="6"/>
                <line x1="3" y1="12" x2="3.01" y2="12"/>
                <line x1="3" y1="18" x2="3.01" y2="18"/>
              </svg>
              <span>Tabela</span>
            </button>
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
      </div>

      <div v-if="filteredProdutos.length > 0 && viewMode === 'cards'" class="catalogo-cards-grid">
        <article
          v-for="produto in filteredProdutos"
          :key="produto.id"
          class="product-card"
          :class="{ 'card-selected': selectedProductIds.includes(produto.id) }"
        >
          <div class="card-image-box">
            <img
              v-if="produto.foto && produto.foto.trim()"
              :src="getProductImageUrl(produto.foto)"
              :alt="produto.nome"
              class="card-img"
              loading="lazy"
            />
            <div v-else class="card-img-placeholder" aria-hidden="true" title="Sem imagem cadastrada">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                <circle cx="8.5" cy="8.5" r="1.5"/>
                <polyline points="21 15 16 10 5 21"/>
              </svg>
            </div>

            <label class="card-checkbox-label" :title="`Selecionar ${produto.nome}`">
              <input
                type="checkbox"
                class="custom-checkbox"
                :checked="selectedProductIds.includes(produto.id)"
                @change="toggleProductSelection(produto.id)"
                :aria-label="`Selecionar ${produto.nome}`"
              />
            </label>
          </div>

          <div class="card-body">
            <div class="card-meta-line">
              <span class="card-category">{{ produto.categoriaNome || 'Geral' }}</span>
              <template v-if="produto.marca">
                <span class="card-brand-dot">•</span>
                <span class="card-brand">{{ produto.marca }}</span>
              </template>
            </div>

            <h3 class="card-product-name" :title="produto.nome">
              {{ produto.nome }}
            </h3>

            <div class="card-info-row">
              <strong class="card-price">{{ money(produto.valorVenda) }}</strong>
              <button
                type="button"
                class="stock-pill-simple"
                :class="{ 'stock-low': getEstoqueTotal(produto) <= produto.estoqueMinimo }"
                title="Clique para ver estoque por tamanho e cor"
                @click="handleOpenEstoqueModal(produto)"
              >
                <span>Estoque: <strong>{{ getEstoqueTotal(produto) }}</strong></span>
              </button>
            </div>
          </div>

          <footer class="card-actions-footer">
            <button
              type="button"
              class="card-btn-action btn-card-edit"
              title="Editar informações do produto"
              @click="handleEditProduto(produto)"
            >
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
              </svg>
              <span>Editar</span>
            </button>

            <button
              type="button"
              class="card-btn-action btn-card-etiquetas"
              title="Gerar etiqueta para este produto"
              @click="handlePrintIndividual(produto)"
            >
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M6 2h12a2 2 0 0 1 2 2v16l-8-4-8 4V4a2 2 0 0 1 2-2z" />
              </svg>
              <span>Etiquetas</span>
            </button>

            <button
              type="button"
              class="card-btn-action btn-card-delete"
              title="Excluir produto"
              aria-label="Excluir produto"
              @click="handlePromptDeleteProduto(produto)"
            >
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6" />
                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
              </svg>
              <span>Excluir</span>
            </button>
          </footer>
        </article>
      </div>

      <div v-else-if="filteredProdutos.length > 0 && viewMode === 'table'" class="table-container">
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
                <div class="produto-cell">
                  <div class="produto-thumb-box">
                    <img
                      v-if="produto.foto && produto.foto.trim()"
                      :src="getProductImageUrl(produto.foto)"
                      :alt="produto.nome"
                      class="produto-thumb-img"
                      loading="lazy"
                    />
                    <div v-else class="produto-thumb-placeholder" aria-hidden="true" title="Sem imagem cadastrada">
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
                        <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                        <circle cx="8.5" cy="8.5" r="1.5"/>
                        <polyline points="21 15 16 10 5 21"/>
                      </svg>
                    </div>
                  </div>
                  <div class="produto-info">
                    <span class="produto-nome">{{ produto.nome }}</span>
                    <span v-if="produto.marca" class="produto-marca">{{ produto.marca }}</span>
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
                <button
                  type="button"
                  class="stock-table-btn"
                  :class="{ 'estoque-baixo': getEstoqueTotal(produto) <= produto.estoqueMinimo }"
                  title="Clique para ver estoque separado por tamanho e cor"
                  @click="handleOpenEstoqueModal(produto)"
                >
                  <span class="stock-badge-number">{{ getEstoqueTotal(produto) }} un</span>
                  <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="stock-chevron">
                    <polyline points="9 18 15 12 9 6" />
                  </svg>
                </button>
              </td>
              <td class="td-acoes">
                <div class="acoes-group">
                  <button
                    type="button"
                    class="btn-table-action btn-edit-action"
                    title="Editar informações e imagem do produto"
                    @click="handleEditProduto(produto)"
                  >
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                      <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                    </svg>
                    <span>Editar</span>
                  </button>
                  <button
                    type="button"
                    class="btn-table-action btn-etiquetas-action"
                    title="Gerar etiqueta para este produto"
                    @click="handlePrintIndividual(produto)"
                  >
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M6 2h12a2 2 0 0 1 2 2v16l-8-4-8 4V4a2 2 0 0 1 2-2z" />
                    </svg>
                    <span>Etiquetas</span>
                  </button>
                  <button
                    type="button"
                    class="btn-table-action btn-delete-action"
                    title="Excluir produto"
                    aria-label="Excluir produto"
                    @click="handlePromptDeleteProduto(produto)"
                  >
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <polyline points="3 6 5 6 21 6" />
                      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                    </svg>
                    <span>Excluir</span>
                  </button>
                </div>
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
          @click="handleOpenNovoProduto"
        >
          Cadastrar primeiro produto
        </button>
      </div>
    </section>

    <ModalEstoqueVariacoes
      :open="isEstoqueModalOpen"
      :produto="produtoEstoqueModal"
      @close="handleCloseEstoqueModal"
      @updated="handleVariacaoEstoqueUpdated"
    />

    <ModalNovoProduto
      :open="isNovoProdutoOpen"
      :produto="produtoEmEdicao"
      :categorias="categorias"
      @close="handleCloseModal"
      @success="handleCreated"
      @delete="handleDeleteFromEditModal"
    />

    <ModalNovaCategoria
      :open="isNovaCategoriaOpen"
      @close="isNovaCategoriaOpen = false"
      @success="handleCreated"
    />

    <ModalExcluirProduto
      :open="isExcluirModalOpen"
      :produtos="produtosParaExcluir"
      @close="handleCloseExcluirModal"
      @success="handleExcluirSuccess"
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

.btn-outline-danger {
  background: transparent;
  border: 1px solid #fca5a5;
  color: #dc2626;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.btn-outline-danger:hover {
  background: #fee2e2;
  border-color: #f87171;
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

.catalogo-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.view-mode-toggle {
  display: flex;
  align-items: center;
  background: #f4f1ee;
  border: 1px solid #d8cfca;
  border-radius: 8px;
  padding: 2px;
  gap: 2px;
}

.btn-toggle-view {
  min-height: 32px;
  padding: 0 10px;
  border: 0;
  background: transparent;
  color: #756a65;
  font-size: 0.78rem;
  font-weight: 700;
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-toggle-view:hover:not(.active) {
  color: #25201f;
  background: rgba(0, 0, 0, 0.04);
}

.btn-toggle-view.active {
  background: #ffffff;
  color: #9d3556;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.search-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  min-width: 260px;
  max-width: 360px;
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

.catalogo-cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}

.product-card {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(48, 35, 30, 0.05);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  transition: transform 0.15s ease, box-shadow 0.15s ease, border-color 0.15s ease;
}

.product-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(48, 35, 30, 0.1);
  border-color: #d8cfca;
}

.product-card.card-selected {
  border-color: #f1bdc8;
  background: #fdfafb;
  box-shadow: 0 0 0 2px rgba(179, 63, 98, 0.15);
}

.card-image-box {
  position: relative;
  width: 100%;
  height: 150px;
  background: #faf8f6;
  border-bottom: 1px solid #eee7e3;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.card-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  transition: transform 0.25s ease;
}

.product-card:hover .card-img {
  transform: scale(1.03);
}

.card-img-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #b5aaa5;
  background: #f4f1ee;
}

.card-checkbox-label {
  position: absolute;
  top: 8px;
  left: 8px;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(3px);
  border-radius: 6px;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
  cursor: pointer;
  z-index: 2;
}

.card-body {
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
}

.card-meta-line {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.72rem;
  color: #8b807b;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.card-category {
  color: #9d3556;
}

.card-brand-dot {
  color: #c4b9b3;
}

.card-brand {
  color: #736965;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 110px;
}

.card-product-name {
  margin: 2px 0 6px 0;
  font-size: 0.94rem;
  font-weight: 700;
  color: #25201f;
  line-height: 1.35;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  min-height: 2.6em;
}

.card-info-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-top: auto;
  padding-top: 4px;
}

.card-price {
  font-size: 1.05rem;
  font-weight: 800;
  color: #25201f;
}

.stock-pill-simple {
  background: #f4f1ee;
  border: 1px solid #dfd7d2;
  border-radius: 6px;
  padding: 3px 9px;
  font-size: 0.76rem;
  color: #554b47;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  transition: all 0.15s ease;
}

.stock-pill-simple:hover {
  background: #eae4df;
  border-color: #cbbdb6;
  color: #25201f;
}

.stock-pill-simple.stock-low {
  background: #fff0f2;
  border-color: #fecdd3;
  color: #991b1b;
}

.stock-pill-simple.stock-low strong {
  color: #b91c1c;
}

.card-actions-footer {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 6px;
  padding: 10px 8px;
  background: #faf8f6;
  border-top: 1px solid #eee7e3;
}

.card-btn-action {
  min-height: 32px;
  padding: 0 6px;
  border-radius: 6px;
  font-size: 0.76rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  transition: all 0.15s ease;
  border: 1px solid #d8cfca;
  background: #ffffff;
  white-space: nowrap;
}

.btn-card-edit {
  color: #7b2943;
  border-color: #ecc5ce;
  background: #fdf8f9;
}

.btn-card-edit:hover {
  background: #faeaee;
  color: #5c182d;
  border-color: #df9eaf;
}

.btn-card-etiquetas {
  color: #625955;
  background: #ffffff;
}

.btn-card-etiquetas:hover {
  background: #eee7e3;
  color: #25201f;
  border-color: #bfaea6;
}

.btn-card-delete {
  color: #b91c1c;
  background: #ffffff;
  border-color: #fecaca;
}

.btn-card-delete:hover {
  background: #fef2f2;
  color: #991b1b;
  border-color: #f87171;
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
  min-width: 240px;
}

.produto-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.produto-thumb-box {
  width: 44px;
  height: 44px;
  flex-shrink: 0;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid #e5ddd8;
  background: #faf8f6;
  display: flex;
  align-items: center;
  justify-content: center;
}

.produto-thumb-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.produto-thumb-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #b5aaa5;
  background: #f4f1ee;
}

.produto-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.produto-nome {
  font-weight: 700;
  color: #25201f;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
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
  width: 140px;
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
  width: 110px;
}

.th-estoque {
  width: 120px;
}

.stock-table-btn {
  min-height: 28px;
  padding: 2px 8px;
  border-radius: 5px;
  background: #f4f1ee;
  border: 1px solid #d8cfca;
  color: #25201f;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  transition: all 0.15s ease;
}

.stock-table-btn:hover {
  background: #eee7e3;
  border-color: #bfaea6;
}

.stock-table-btn.estoque-baixo {
  background: #fff0f2;
  color: #991b1b;
  border-color: #fecdd3;
}

.stock-chevron {
  color: #8b807b;
}

.th-acoes,
.td-acoes {
  width: 250px;
  text-align: right;
}

.acoes-group {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  justify-content: flex-end;
}

.btn-table-action {
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
  gap: 5px;
  transition: all 0.15s ease;
  white-space: nowrap;
}

.btn-table-action:hover {
  background: #eee7e3;
  color: #25201f;
  border-color: #bfaea6;
}

.btn-edit-action {
  color: #7b2943;
  border-color: #ecc5ce;
  background: #fdf8f9;
}

.btn-edit-action:hover {
  background: #faeaee;
  color: #5c182d;
  border-color: #df9eaf;
}

.btn-etiquetas-action {
  color: #625955;
}

.btn-delete-action {
  color: #b91c1c;
  border-color: #fecaca;
  background: #ffffff;
}

.btn-delete-action:hover {
  background: #fef2f2;
  color: #991b1b;
  border-color: #f87171;
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

  .catalogo-toolbar {
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

  .catalogo-cards-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 12px;
  }

  .card-image-box {
    height: 130px;
  }

  .card-actions-footer {
    grid-template-columns: 1fr 1fr 1fr;
    gap: 4px;
    padding: 8px 6px;
  }

  .card-btn-action {
    font-size: 0.7rem;
    padding: 0 4px;
    gap: 3px;
  }
}
</style>
