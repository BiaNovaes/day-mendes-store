<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { api, getProductImageUrl, type Produto } from '../../api'
import { money } from './etiquetasUtils'

const props = defineProps<{
  open: boolean
  produtos: Produto[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
}>()

const loading = ref(false)
const error = ref('')
const successMessage = ref('')
const hasSalesError = ref(false)

const isMultiple = computed(() => props.produtos.length > 1)
const singleProduto = computed<Produto | null>(() => (props.produtos.length === 1 ? props.produtos[0] ?? null : null))

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      error.value = ''
      successMessage.value = ''
      loading.value = false
      hasSalesError.value = false
    }
  }
)

function handleBackdropClick(e: MouseEvent) {
  if (e.target === e.currentTarget && !loading.value) {
    emit('close')
  }
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape' && !loading.value) {
    emit('close')
  }
}

async function handleConfirmDelete() {
  if (props.produtos.length === 0) return

  loading.value = true
  error.value = ''
  hasSalesError.value = false

  try {
    for (const prod of props.produtos) {
      await api.excluirProduto(prod.id)
    }

    emit('success')
    emit('close')
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'Erro ao excluir produto.'
    error.value = msg
    if (msg.toLowerCase().includes('venda') || msg.toLowerCase().includes('inative')) {
      hasSalesError.value = true
    }
  } finally {
    loading.value = false
  }
}

async function handleInativarFallback() {
  if (props.produtos.length === 0) return

  loading.value = true
  error.value = ''

  try {
    for (const prod of props.produtos) {
      await api.inativarProduto(prod.id)
    }

    emit('success')
    emit('close')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erro ao inativar produto.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="modal-backdrop"
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-excluir-title"
      @click="handleBackdropClick"
      @keydown="handleKeydown"
    >
      <div class="modal-card">
        <header class="modal-header">
          <div class="header-icon-badge">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6" />
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
              <line x1="10" y1="11" x2="10" y2="17" />
              <line x1="14" y1="11" x2="14" y2="17" />
            </svg>
          </div>
          <div class="header-titles">
            <h2 id="modal-excluir-title" class="modal-title">
              {{ isMultiple ? `Excluir ${produtos.length} produtos` : 'Excluir produto' }}
            </h2>
            <p class="modal-subtitle">
              Confirme a exclusão do catálogo
            </p>
          </div>
          <button
            type="button"
            class="btn-close"
            title="Fechar"
            aria-label="Fechar"
            :disabled="loading"
            @click="emit('close')"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </header>

        <div class="modal-body">
          <div v-if="error" class="alert-box alert-error">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="alert-icon">
              <circle cx="12" cy="12" r="10" />
              <line x1="12" y1="8" x2="12" y2="12" />
              <line x1="12" y1="16" x2="12.01" y2="16" />
            </svg>
            <div class="alert-content">
              <strong>Não foi possível excluir:</strong>
              <p>{{ error }}</p>
              <p v-if="hasSalesError" class="alert-hint">
                Dica: Você pode <strong>inativar</strong> este produto para que ele pare de aparecer no catálogo e no PDV sem perder o registro das vendas já realizadas.
              </p>
            </div>
          </div>

          <div v-if="singleProduto" class="produto-preview-card">
            <div class="preview-thumb-box">
              <img
                v-if="singleProduto.foto && singleProduto.foto.trim()"
                :src="getProductImageUrl(singleProduto.foto)"
                :alt="singleProduto.nome"
                class="preview-img"
              />
              <div v-else class="preview-placeholder">
                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6">
                  <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                  <circle cx="8.5" cy="8.5" r="1.5"/>
                  <polyline points="21 15 16 10 5 21"/>
                </svg>
              </div>
            </div>
            <div class="preview-info">
              <span class="preview-name">{{ singleProduto.nome }}</span>
              <div class="preview-meta">
                <span class="preview-badge">{{ singleProduto.categoriaNome || 'Geral' }}</span>
                <span v-if="singleProduto.marca" class="preview-brand">{{ singleProduto.marca }}</span>
                <strong class="preview-price">{{ money(singleProduto.valorVenda) }}</strong>
              </div>
            </div>
          </div>

          <div v-else-if="isMultiple" class="multi-produtos-list">
            <p class="multi-produtos-instruction">
              Os seguintes <strong>{{ produtos.length }} produtos</strong> serão excluídos:
            </p>
            <ul class="produtos-scroll-list">
              <li v-for="item in produtos" :key="item.id" class="multi-item-row">
                <span class="multi-item-name">{{ item.nome }}</span>
                <span class="multi-item-cat">{{ item.categoriaNome || 'Geral' }}</span>
                <span class="multi-item-price">{{ money(item.valorVenda) }}</span>
              </li>
            </ul>
          </div>

          <div class="warning-explanation">
            <p v-if="singleProduto">
              Tem certeza que deseja excluir <strong>"{{ singleProduto.nome }}"</strong>?
            </p>
            <p v-else>
              Tem certeza que deseja excluir os <strong>{{ produtos.length }} produtos selecionados</strong>?
            </p>
            <p class="warning-sub">
              Esta ação removerá o produto do catálogo e das opções de venda.
            </p>
          </div>
        </div>

        <footer class="modal-footer">
          <button
            type="button"
            class="btn btn-secondary"
            :disabled="loading"
            @click="emit('close')"
          >
            Cancelar
          </button>

          <button
            v-if="hasSalesError"
            type="button"
            class="btn btn-warning-action"
            :disabled="loading"
            @click="handleInativarFallback"
          >
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10" />
              <line x1="4.93" y1="4.93" x2="19.07" y2="19.07" />
            </svg>
            <span>{{ loading ? 'Inativando...' : 'Inativar produto' }}</span>
          </button>

          <button
            type="button"
            class="btn btn-danger"
            :disabled="loading"
            @click="handleConfirmDelete"
          >
            <svg v-if="!loading" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6" />
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
            </svg>
            <span>{{ loading ? 'Excluindo...' : (isMultiple ? 'Sim, excluir produtos' : 'Sim, excluir produto') }}</span>
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
  background: rgba(26, 22, 21, 0.55);
  backdrop-filter: blur(3px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 16px;
  animation: fadeIn 0.15s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal-card {
  background: #ffffff;
  border: 1px solid #f0dadf;
  border-radius: 12px;
  width: 100%;
  max-width: 500px;
  box-shadow: 0 18px 45px rgba(131, 39, 66, 0.15), 0 4px 14px rgba(0, 0, 0, 0.08);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: popIn 0.15s ease-out;
}

@keyframes popIn {
  from {
    transform: scale(0.96);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

.modal-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 18px 20px;
  border-bottom: 1px solid #f3ece9;
  background: #fdfafb;
}

.header-icon-badge {
  width: 42px;
  height: 42px;
  border-radius: 10px;
  background: #fee2e2;
  color: #dc2626;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.header-titles {
  flex: 1;
}

.modal-title {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 800;
  color: #1f1b1a;
}

.modal-subtitle {
  margin: 2px 0 0;
  font-size: 0.8rem;
  color: #8b807b;
}

.btn-close {
  background: transparent;
  border: 0;
  color: #8b807b;
  cursor: pointer;
  padding: 4px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s ease;
}

.btn-close:hover {
  background: #f3ede9;
  color: #1f1b1a;
}

.modal-body {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-height: 60vh;
  overflow-y: auto;
}

.alert-box {
  padding: 12px 14px;
  border-radius: 8px;
  font-size: 0.84rem;
  display: flex;
  align-items: flex-start;
  gap: 10px;
}

.alert-error {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #991b1b;
}

.alert-icon {
  flex-shrink: 0;
  margin-top: 1px;
}

.alert-content p {
  margin: 4px 0 0;
  line-height: 1.4;
}

.alert-hint {
  font-size: 0.78rem;
  color: #7f1d1d;
  background: #fee2e2;
  padding: 6px 8px;
  border-radius: 6px;
  margin-top: 6px !important;
}

.produto-preview-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 12px 14px;
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 8px;
}

.preview-thumb-box {
  width: 52px;
  height: 52px;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid #e5ddd8;
  background: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.preview-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.preview-placeholder {
  color: #b5aaa5;
}

.preview-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.preview-name {
  font-size: 0.92rem;
  font-weight: 700;
  color: #25201f;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.preview-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.preview-badge {
  font-size: 0.72rem;
  font-weight: 600;
  background: #eee7e3;
  color: #625955;
  padding: 2px 6px;
  border-radius: 4px;
}

.preview-brand {
  font-size: 0.76rem;
  color: #8b807b;
}

.preview-price {
  font-size: 0.86rem;
  color: #9d3556;
}

.multi-produtos-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.multi-produtos-instruction {
  margin: 0;
  font-size: 0.86rem;
  color: #49403d;
}

.produtos-scroll-list {
  list-style: none;
  margin: 0;
  padding: 0;
  max-height: 160px;
  overflow-y: auto;
  border: 1px solid #eee7e3;
  border-radius: 6px;
  background: #faf8f6;
}

.multi-item-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  border-bottom: 1px solid #f4f1ee;
  font-size: 0.82rem;
  gap: 8px;
}

.multi-item-row:last-child {
  border-bottom: none;
}

.multi-item-name {
  font-weight: 600;
  color: #25201f;
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.multi-item-cat {
  font-size: 0.74rem;
  color: #8b807b;
}

.multi-item-price {
  font-weight: 700;
  color: #9d3556;
}

.warning-explanation {
  background: #fff8f8;
  border: 1px dashed #fca5a5;
  border-radius: 8px;
  padding: 12px 14px;
}

.warning-explanation p {
  margin: 0;
  font-size: 0.88rem;
  color: #49403d;
  line-height: 1.4;
}

.warning-sub {
  margin-top: 4px !important;
  font-size: 0.78rem !important;
  color: #881337 !important;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
  padding: 14px 20px;
  border-top: 1px solid #f3ece9;
  background: #faf8f6;
}

.btn {
  min-height: 38px;
  padding: 0 16px;
  border-radius: 8px;
  font-size: 0.84rem;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
  border: 1px solid transparent;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #ffffff;
  border-color: #d8cfca;
  color: #49403d;
}

.btn-secondary:hover:not(:disabled) {
  background: #f4f1ee;
  border-color: #bfaea6;
}

.btn-danger {
  background: #dc2626;
  color: #ffffff;
  box-shadow: 0 2px 8px rgba(220, 38, 38, 0.25);
}

.btn-danger:hover:not(:disabled) {
  background: #b91c1c;
}

.btn-warning-action {
  background: #fef3c7;
  border-color: #fde68a;
  color: #92400e;
}

.btn-warning-action:hover:not(:disabled) {
  background: #fde68a;
  color: #78350f;
}
</style>
