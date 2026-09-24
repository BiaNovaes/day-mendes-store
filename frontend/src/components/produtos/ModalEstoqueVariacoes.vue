<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { api, calcularEstoqueTotal, getProductImageUrl, type Produto, type VariacaoProduto } from '../../api'

const props = defineProps<{
  open: boolean
  produto: Produto | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'updated', data: { produtoId: number; variacaoId: number; novaQuantidade: number }): void
}>()

const editingVariacaoId = ref<number | null>(null)
const editQtyValue = ref<number>(0)
const isSaving = ref(false)
const saveError = ref('')

function handleBackdropClick(e: MouseEvent) {
  if (e.target === e.currentTarget) {
    emit('close')
  }
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    if (editingVariacaoId.value !== null) {
      cancelarEdicao()
    } else {
      emit('close')
    }
  }
}

const totalEstoque = computed(() => {
  if (!props.produto) return 0
  return calcularEstoqueTotal(props.produto)
})

const activeVariacoes = computed<VariacaoProduto[]>(() => {
  if (!props.produto?.variacoes) return []
  return props.produto.variacoes.filter((v) => (v.status as number) !== 3)
})

const hasMultipleColors = computed(() => {
  if (activeVariacoes.value.length === 0) return false
  const meaningfulColors = activeVariacoes.value
    .map((v) => (v.cor ?? '').trim())
    .filter((c) => c && c !== '-' && c.toLowerCase() !== 'padrão' && c.toLowerCase() !== 'padrao')
  return meaningfulColors.length > 0
})

const isEstoqueBaixo = computed(() => {
  if (!props.produto) return false
  return totalEstoque.value <= props.produto.estoqueMinimo
})

function iniciarEdicao(variacao: VariacaoProduto) {
  saveError.value = ''
  editingVariacaoId.value = variacao.id
  editQtyValue.value = Number(variacao.quantidadeEstoque) || 0
  nextTick(() => {
    const el = document.getElementById(`input-qty-${variacao.id}`) as HTMLInputElement | null
    if (el) {
      el.focus()
      el.select()
    }
  })
}

function cancelarEdicao() {
  editingVariacaoId.value = null
  saveError.value = ''
}

async function salvarEdicao(variacao: VariacaoProduto) {
  if (editQtyValue.value === null || editQtyValue.value === undefined || editQtyValue.value < 0) {
    saveError.value = 'A quantidade não pode ser negativa.'
    return
  }

  isSaving.value = true
  saveError.value = ''

  try {
    const novaQuantidade = Math.floor(Number(editQtyValue.value))
    await api.atualizarEstoqueVariacao(variacao.id, novaQuantidade)

    variacao.quantidadeEstoque = novaQuantidade

    if (props.produto) {
      const total = calcularEstoqueTotal(props.produto)
      props.produto.estoqueTotal = total
      props.produto.quantidadeEstoque = total
      props.produto.estoqueBaixo = total <= props.produto.estoqueMinimo
    }

    emit('updated', {
      produtoId: variacao.produtoId,
      variacaoId: variacao.id,
      novaQuantidade,
    })

    editingVariacaoId.value = null
  } catch (err) {
    saveError.value = err instanceof Error ? err.message : 'Erro ao atualizar a quantidade em estoque.'
  } finally {
    isSaving.value = false
  }
}

watch(
  () => [props.open, props.produto?.id],
  () => {
    cancelarEdicao()
  }
)
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open && produto"
      class="modal-backdrop"
      role="dialog"
      aria-modal="true"
      :aria-labelledby="`modal-estoque-title-${produto.id}`"
      @click="handleBackdropClick"
      @keydown="handleKeydown"
    >
      <div class="modal-card">
        <header class="modal-header">
          <div class="header-product-info">
            <div class="header-thumb-box">
              <img
                v-if="produto.foto && produto.foto.trim()"
                :src="getProductImageUrl(produto.foto)"
                :alt="produto.nome"
                class="header-thumb-img"
              />
              <div v-else class="header-thumb-placeholder" aria-hidden="true">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                  <circle cx="8.5" cy="8.5" r="1.5"/>
                  <polyline points="21 15 16 10 5 21"/>
                </svg>
              </div>
            </div>

            <div class="header-texts">
              <span class="category-badge">{{ produto.categoriaNome || 'Geral' }}</span>
              <h2 :id="`modal-estoque-title-${produto.id}`" class="product-title">{{ produto.nome }}</h2>
              <div class="header-meta">
                <span class="stock-summary-pill" :class="{ 'stock-low': isEstoqueBaixo }">
                  Total: {{ totalEstoque }} {{ totalEstoque === 1 ? 'unidade' : 'unidades' }}
                </span>
                <span v-if="isEstoqueBaixo" class="low-stock-warning">
                  Estoque mínimo: {{ produto.estoqueMinimo }} un
                </span>
              </div>
            </div>
          </div>

          <button
            type="button"
            class="btn-close"
            title="Fechar"
            aria-label="Fechar janela de estoque"
            @click="emit('close')"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </header>

        <div class="modal-body">
          <div class="section-title-row">
            <span class="section-title">Estoque por variação</span>
            <span class="variations-count">
              {{ activeVariacoes.length }} {{ activeVariacoes.length === 1 ? 'variação' : 'variações' }}
            </span>
          </div>

          <div v-if="activeVariacoes.length > 0" class="variations-list">
            <div
              v-for="variacao in activeVariacoes"
              :key="variacao.id"
              class="variation-item"
              :class="{
                'item-out-of-stock': variacao.quantidadeEstoque <= 0,
                'is-editing': editingVariacaoId === variacao.id,
              }"
            >
              <div class="variation-main">
                <span class="size-tag">{{ variacao.tamanho || 'U' }}</span>
                <span v-if="hasMultipleColors && variacao.cor && variacao.cor.trim() !== '-'" class="color-text">
                  {{ variacao.cor }}
                </span>
                <span v-if="variacao.codigoBarras" class="barcode-subtle" :title="`Código: ${variacao.codigoBarras}`">
                  {{ variacao.codigoBarras }}
                </span>
              </div>

              <div v-if="editingVariacaoId !== variacao.id" class="variation-stock-actions">
                <div class="variation-stock">
                  <span
                    class="stock-number-pill"
                    :class="{
                      'pill-available': variacao.quantidadeEstoque > 0,
                      'pill-empty': variacao.quantidadeEstoque <= 0,
                    }"
                  >
                    <span class="stock-qty">{{ variacao.quantidadeEstoque }}</span>
                    <span class="stock-unit">un</span>
                  </span>
                  <span v-if="variacao.quantidadeEstoque <= 0" class="empty-badge">Esgotado</span>
                </div>

                <button
                  type="button"
                  class="btn-edit-qty"
                  title="Editar quantidade em estoque"
                  @click="iniciarEdicao(variacao)"
                >
                  <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                  </svg>
                  <span>Editar</span>
                </button>
              </div>

              <div v-else class="variation-edit-box">
                <form class="variation-edit-form" @submit.prevent="salvarEdicao(variacao)">
                  <div class="edit-qty-input-group">
                    <label :for="`input-qty-${variacao.id}`" class="edit-qty-label">Quantidade:</label>
                    <input
                      :id="`input-qty-${variacao.id}`"
                      v-model.number="editQtyValue"
                      type="number"
                      min="0"
                      step="1"
                      class="edit-qty-input"
                      :disabled="isSaving"
                      required
                      @keydown.esc.prevent="cancelarEdicao"
                    />
                  </div>

                  <div class="edit-qty-btn-group">
                    <button
                      type="button"
                      class="btn-qty-cancel"
                      :disabled="isSaving"
                      @click="cancelarEdicao"
                    >
                      Cancelar
                    </button>
                    <button
                      type="submit"
                      class="btn-qty-save"
                      :disabled="isSaving || editQtyValue === null || editQtyValue === undefined || editQtyValue < 0"
                    >
                      <span v-if="isSaving" class="spinner-small-white"></span>
                      <span v-else>Salvar</span>
                    </button>
                  </div>
                </form>
                <p v-if="saveError && editingVariacaoId === variacao.id" class="edit-qty-error-msg">
                  {{ saveError }}
                </p>
              </div>
            </div>
          </div>

          <div v-else class="empty-variations">
            <p>Nenhuma variação ativa cadastrada para este produto.</p>
          </div>
        </div>

        <footer class="modal-footer">
          <button type="button" class="btn-close-modal" @click="emit('close')">
            Fechar
          </button>
        </footer>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(37, 32, 31, 0.45);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 1000;
  animation: fadeIn 0.15s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.modal-card {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  box-shadow: 0 20px 50px rgba(48, 35, 30, 0.18);
  width: 100%;
  max-width: 440px;
  max-height: calc(100vh - 32px);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: slideUp 0.15s ease;
}

@keyframes slideUp {
  from {
    transform: translateY(8px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.modal-header {
  padding: 16px 20px;
  border-bottom: 1px solid #eee7e3;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
  background: #faf8f6;
}

.header-product-info {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.header-thumb-box {
  width: 46px;
  height: 46px;
  flex-shrink: 0;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #e5ddd8;
  background: #f4f1ee;
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-thumb-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.header-thumb-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #b5aaa5;
}

.header-texts {
  display: flex;
  flex-direction: column;
  gap: 3px;
  min-width: 0;
}

.category-badge {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  color: #832742;
}

.product-title {
  margin: 0;
  font-size: 1rem;
  font-weight: 800;
  color: #25201f;
  line-height: 1.25;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.header-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  margin-top: 2px;
}

.stock-summary-pill {
  font-size: 0.74rem;
  font-weight: 700;
  padding: 2px 7px;
  border-radius: 4px;
  background: #eee7e3;
  color: #49403d;
}

.stock-summary-pill.stock-low {
  background: #fff0f2;
  color: #991b1b;
  border: 1px solid #fecdd3;
}

.low-stock-warning {
  font-size: 0.72rem;
  color: #b91c1c;
  font-weight: 600;
}

.btn-close {
  background: transparent;
  border: 0;
  color: #8b807b;
  cursor: pointer;
  padding: 6px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
  min-height: auto;
}

.btn-close:hover {
  background: #eee7e3;
  color: #25201f;
}

.modal-body {
  padding: 16px 20px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section-title-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 4px;
}

.section-title {
  font-size: 0.78rem;
  font-weight: 800;
  color: #625955;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.variations-count {
  font-size: 0.75rem;
  color: #8b807b;
  font-weight: 600;
}

.variations-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.variation-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 9px 12px;
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 8px;
  transition: background-color 0.15s ease;
}

.variation-item:hover {
  background: #f5f1ee;
}

.variation-item.item-out-of-stock {
  opacity: 0.75;
  background: #fbf9f8;
}

.variation-main {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.size-tag {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 32px;
  padding: 3px 8px;
  background: #ffffff;
  border: 1.5px solid #d8cfca;
  border-radius: 6px;
  font-size: 0.82rem;
  font-weight: 800;
  color: #25201f;
}

.color-text {
  font-size: 0.86rem;
  font-weight: 600;
  color: #3f3633;
}

.barcode-subtle {
  font-size: 0.72rem;
  font-family: monospace;
  color: #9e938d;
  background: #ffffff;
  padding: 1px 6px;
  border-radius: 4px;
  border: 1px solid #ebdcd5;
}

.variation-stock {
  display: flex;
  align-items: center;
  gap: 8px;
}

.stock-number-pill {
  display: inline-flex;
  align-items: baseline;
  gap: 3px;
  padding: 3px 9px;
  border-radius: 6px;
  font-weight: 800;
  font-size: 0.88rem;
}

.pill-available {
  background: #e6f7ef;
  color: #065f46;
}

.pill-empty {
  background: #f3f4f6;
  color: #6b7280;
}

.stock-unit {
  font-size: 0.74rem;
  font-weight: 600;
}

.empty-badge {
  font-size: 0.7rem;
  font-weight: 700;
  color: #991b1b;
  background: #fee2e2;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.empty-variations {
  padding: 20px;
  text-align: center;
  color: #8b807b;
  font-size: 0.85rem;
  background: #faf8f6;
  border: 1px dashed #d8cfca;
  border-radius: 8px;
}

.empty-variations p {
  margin: 0;
}

.modal-footer {
  padding: 12px 20px;
  border-top: 1px solid #eee7e3;
  display: flex;
  justify-content: flex-end;
  background: #faf8f6;
}

.btn-close-modal {
  min-height: 36px;
  padding: 0 16px;
  border-radius: 6px;
  background: #eee7e3;
  color: #49403d;
  font-size: 0.84rem;
  font-weight: 700;
  border: 0;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-close-modal:hover {
  background: #e2dbd7;
  color: #25201f;
}

.variation-item.is-editing {
  background: #ffffff;
  border-color: #b33f62;
  box-shadow: 0 2px 8px rgba(179, 63, 98, 0.08);
}

.variation-stock-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-edit-qty {
  background: #ffffff;
  border: 1px solid #d8cfca;
  color: #625955;
  font-size: 0.72rem;
  font-weight: 700;
  border-radius: 4px;
  padding: 3px 8px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
  min-height: 24px;
}

.btn-edit-qty:hover {
  background: #fdf2f5;
  border-color: #f9ccd7;
  color: #832742;
}

.variation-edit-box {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
}

.variation-edit-form {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.edit-qty-input-group {
  display: flex;
  align-items: center;
  gap: 5px;
}

.edit-qty-label {
  font-size: 0.74rem;
  font-weight: 700;
  color: #625955;
  text-transform: uppercase;
  letter-spacing: 0.2px;
}

.edit-qty-input {
  width: 54px;
  height: 28px;
  text-align: center;
  border: 1.5px solid #b33f62;
  border-radius: 4px;
  font-size: 0.88rem;
  font-weight: 800;
  color: #25201f;
  background: #ffffff;
  outline: none;
  padding: 0 4px;
  font-family: inherit;
  box-sizing: border-box;
}

.edit-qty-input:focus {
  border-color: #832742;
  box-shadow: 0 0 0 2px rgba(179, 63, 98, 0.15);
}

.edit-qty-btn-group {
  display: flex;
  align-items: center;
  gap: 4px;
}

.btn-qty-cancel {
  height: 28px;
  padding: 0 8px;
  border: 1px solid #d8cfca;
  background: #ffffff;
  color: #625955;
  border-radius: 4px;
  font-size: 0.74rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-qty-cancel:hover:not(:disabled) {
  background: #f4f1ee;
  color: #25201f;
}

.btn-qty-save {
  height: 28px;
  padding: 0 10px;
  border: 0;
  background: #b33f62;
  color: #ffffff;
  border-radius: 4px;
  font-size: 0.74rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  transition: background-color 0.15s ease;
}

.btn-qty-save:hover:not(:disabled) {
  background: #832742;
}

.btn-qty-save:disabled,
.btn-qty-cancel:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner-small-white {
  width: 11px;
  height: 11px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

.edit-qty-error-msg {
  margin: 0;
  font-size: 0.72rem;
  color: #991b1b;
  font-weight: 600;
  text-align: right;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 480px) {
  .modal-card {
    max-width: 100%;
  }

  .variation-item {
    padding: 8px 10px;
    flex-wrap: wrap;
    gap: 8px;
  }

  .variation-edit-box {
    width: 100%;
    align-items: stretch;
  }

  .variation-edit-form {
    justify-content: space-between;
    width: 100%;
  }
}
</style>
