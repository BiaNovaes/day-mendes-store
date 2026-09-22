<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { api, type Venda } from '../../api'
import {
  formatMoney,
  formatFullDate,
  getStatusConfig,
  getPaymentConfig,
} from './vendaUtils'

const props = defineProps<{
  venda: Venda | null
  open: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'updated'): void
}>()

const confirmingCancel = ref(false)
const cancelMotivo = ref('')
const actionLoading = ref(false)
const actionError = ref('')
const actionSuccess = ref('')

const status = computed(() => (props.venda ? getStatusConfig(props.venda.statusVenda) : null))
const payment = computed(() => (props.venda ? getPaymentConfig(props.venda.formaPagamento) : null))
const motivoCancelamento = computed(() => {
  if (!props.venda) return ''
  return props.venda.motivoCancelamento || (props.venda as Record<string, any>).motivo || ''
})

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      confirmingCancel.value = false
      cancelMotivo.value = ''
      actionLoading.value = false
      actionError.value = ''
      actionSuccess.value = ''
    }
  }
)

function handleBackdropClick(e: MouseEvent) {
  if (e.target === e.currentTarget) {
    emit('close')
  }
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    emit('close')
  }
}

async function handleCancelarVenda() {
  if (!props.venda) return
  actionLoading.value = true
  actionError.value = ''
  actionSuccess.value = ''

  try {
    const trimmedMotivo = cancelMotivo.value.trim()
    const updated = await api.cancelarVenda(props.venda.id, trimmedMotivo || undefined)
    actionSuccess.value = 'Venda cancelada com sucesso! O estoque foi estornado.'
    confirmingCancel.value = false
    if (props.venda) {
      props.venda.statusVenda = 3
      props.venda.motivoCancelamento = updated?.motivoCancelamento ?? (trimmedMotivo || undefined)
    }
    emit('updated')
  } catch (err) {
    actionError.value = err instanceof Error ? err.message : 'Erro ao cancelar a venda.'
  } finally {
    actionLoading.value = false
  }
}

async function handleFinalizarVenda() {
  if (!props.venda) return
  actionLoading.value = true
  actionError.value = ''
  actionSuccess.value = ''

  try {
    await api.finalizarVenda(props.venda.id)
    actionSuccess.value = 'Venda finalizada com sucesso!'
    emit('updated')
  } catch (err) {
    actionError.value = err instanceof Error ? err.message : 'Erro ao finalizar a venda.'
  } finally {
    actionLoading.value = false
  }
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open && venda"
      class="modal-backdrop"
      role="dialog"
      aria-modal="true"
      :aria-labelledby="`modal-title-${venda.id}`"
      @click="handleBackdropClick"
      @keydown="handleKeydown"
    >
      <div class="modal-card">
        <header class="modal-header">
          <div class="header-main">
            <div class="header-title-row">
              <h2 :id="`modal-title-${venda.id}`" class="modal-title">Venda #{{ venda.id }}</h2>
              <span
                v-if="status"
                class="status-pill"
                :style="{ backgroundColor: status.bg, color: status.color }"
              >
                <span class="status-dot" :style="{ backgroundColor: status.dotColor }"></span>
                {{ status.label }}
              </span>
            </div>
            <p class="modal-date">
              {{ formatFullDate(venda.dataVenda) }}
            </p>
          </div>

          <button
            type="button"
            class="btn-close"
            title="Fechar"
            aria-label="Fechar janela"
            @click="emit('close')"
          >
            &times;
          </button>
        </header>

        <div v-if="actionSuccess" class="alert-box alert-success">
          <span>{{ actionSuccess }}</span>
        </div>

        <div v-if="actionError" class="alert-box alert-error">
          <span>{{ actionError }}</span>
        </div>

        <div class="modal-body">
          <div class="info-grid">
            <div class="info-card">
              <span class="info-label">Cliente</span>
              <strong class="info-value">
                {{ venda.clienteNome || 'Venda avulsa' }}
              </strong>
              <span v-if="venda.clienteTelefone" class="info-subtext">
                {{ venda.clienteTelefone }}
              </span>
            </div>

            <div class="info-card">
              <span class="info-label">Forma de Pagamento</span>
              <span
                v-if="payment"
                class="payment-tag"
                :style="{
                  backgroundColor: payment.bg,
                  color: payment.color,
                }"
              >
                {{ payment.label }}
              </span>
            </div>
          </div>

          <div class="items-section">
            <div class="section-title-row">
              <h3 class="section-title">Itens da venda</h3>
              <span class="items-count-text">{{ venda.itens?.length || 0 }} {{ venda.itens?.length === 1 ? 'item' : 'itens' }}</span>
            </div>

            <div class="items-table-wrapper">
              <table class="items-table">
                <thead>
                  <tr>
                    <th>Item</th>
                    <th class="text-center">Qtd.</th>
                    <th class="text-right">Unitário</th>
                    <th class="text-right">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in venda.itens" :key="item.id">
                    <td>
                      <div class="item-name-box">
                        <span class="product-title">{{ item.produtoNome }}</span>
                        <span v-if="item.tamanho || item.cor" class="product-variation">
                          {{ [item.tamanho ? `Tam: ${item.tamanho}` : '', item.cor ? `Cor: ${item.cor}` : ''].filter(Boolean).join(' • ') }}
                        </span>
                      </div>
                    </td>
                    <td class="text-center">
                      <span class="qty-val">{{ item.quantidade }}</span>
                    </td>
                    <td class="text-right text-muted">
                      {{ formatMoney(item.valorUnitario) }}
                    </td>
                    <td class="text-right">
                      <strong class="item-subtotal">{{ formatMoney(item.subtotal) }}</strong>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div class="financial-summary">
            <div class="summary-line">
              <span>Subtotal dos itens</span>
              <span>{{ formatMoney(venda.valorTotal) }}</span>
            </div>
            <div class="summary-total-line">
              <span class="total-label">Total</span>
              <strong class="total-value">{{ formatMoney(venda.valorTotal) }}</strong>
            </div>
          </div>

          <div v-if="venda.statusVenda === 3" class="cancelled-notice">
            <div class="cancelled-notice-header">
              <svg
                class="cancelled-notice-icon"
                viewBox="0 0 24 24"
                width="16"
                height="16"
                fill="none"
                stroke="currentColor"
                stroke-width="2.2"
                stroke-linecap="round"
                stroke-linejoin="round"
              >
                <circle cx="12" cy="12" r="10" />
                <line x1="12" y1="8" x2="12" y2="12" />
                <line x1="12" y1="16" x2="12.01" y2="16" />
              </svg>
              <strong>Venda Cancelada</strong>
            </div>
            <p class="cancelled-desc">Esta venda foi cancelada e os produtos foram estornados ao estoque.</p>
            <div v-if="motivoCancelamento" class="cancelled-motivo-box">
              <span class="cancelled-motivo-label">Motivo do cancelamento:</span>
              <p class="cancelled-motivo-text">{{ motivoCancelamento }}</p>
            </div>
          </div>

          <div v-if="confirmingCancel" class="cancel-confirmation-box">
            <strong>Confirmar cancelamento da venda #{{ venda.id }}?</strong>
            <p class="confirm-desc">
              Esta ação cancelará a venda e devolverá os produtos ao estoque da loja.
            </p>
            <div class="motivo-field">
              <label for="cancel-motivo">Motivo (opcional):</label>
              <input
                id="cancel-motivo"
                v-model="cancelMotivo"
                type="text"
                placeholder="Ex: Devolução pelo cliente..."
                :disabled="actionLoading"
              />
            </div>
            <div class="confirm-actions">
              <button
                type="button"
                class="btn-cancel-back"
                :disabled="actionLoading"
                @click="confirmingCancel = false"
              >
                Voltar
              </button>
              <button
                type="button"
                class="btn-cancel-execute"
                :disabled="actionLoading"
                @click="handleCancelarVenda"
              >
                {{ actionLoading ? 'Cancelando...' : 'Confirmar' }}
              </button>
            </div>
          </div>
        </div>

        <footer class="modal-footer">
          <div class="footer-left">
            <template v-if="!confirmingCancel && venda.statusVenda === 2">
              <button
                type="button"
                class="btn-danger-outline"
                :disabled="actionLoading"
                @click="confirmingCancel = true"
              >
                Cancelar venda
              </button>
            </template>

            <template v-else-if="!confirmingCancel && venda.statusVenda === 1">
              <button
                type="button"
                class="btn-primary-action"
                :disabled="actionLoading"
                @click="handleFinalizarVenda"
              >
                Finalizar venda
              </button>
              <button
                type="button"
                class="btn-danger-outline"
                :disabled="actionLoading"
                @click="confirmingCancel = true"
              >
                Cancelar rascunho
              </button>
            </template>
          </div>

          <div class="footer-right">
            <button
              type="button"
              class="btn-close-modal"
              @click="emit('close')"
            >
              Fechar
            </button>
          </div>
        </footer>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(26, 22, 21, 0.5);
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
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  width: 100%;
  max-width: 580px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.15);
  overflow: hidden;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 18px 22px;
  background: #faf8f6;
  border-bottom: 1px solid #eee7e3;
  gap: 16px;
}

.header-main {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.header-title-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.modal-title {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 800;
  color: #25201f;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 9px;
  border-radius: 12px;
  font-size: 0.74rem;
  font-weight: 700;
}

.status-dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
}

.modal-date {
  margin: 0;
  font-size: 0.8rem;
  color: #736965;
}

.btn-close {
  background: transparent;
  border: none;
  font-size: 1.4rem;
  color: #8b807b;
  cursor: pointer;
  padding: 2px 6px;
  border-radius: 6px;
  line-height: 1;
}

.btn-close:hover {
  background: #eee7e3;
  color: #25201f;
}

.alert-box {
  margin: 14px 22px 0;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 0.84rem;
  font-weight: 600;
}

.alert-success {
  background: #ecfdf5;
  color: #065f46;
  border: 1px solid #a7f3d0;
}

.alert-error {
  background: #fff0f2;
  color: #991b1b;
  border: 1px solid #fecdd3;
}

.modal-body {
  padding: 20px 22px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.info-grid {
  display: grid;
  grid-template-columns: 1.3fr 1fr;
  gap: 12px;
}

.info-card {
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 8px;
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.info-label {
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #8b807b;
  letter-spacing: 0.3px;
}

.info-value {
  font-size: 0.9rem;
  color: #25201f;
}

.info-subtext {
  font-size: 0.78rem;
  color: #736965;
}

.payment-tag {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 0.78rem;
  font-weight: 600;
  width: fit-content;
}

.items-section {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.section-title-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.section-title {
  margin: 0;
  font-size: 0.88rem;
  font-weight: 700;
  color: #25201f;
}

.items-count-text {
  font-size: 0.76rem;
  color: #8b807b;
}

.items-table-wrapper {
  border: 1px solid #eee7e3;
  border-radius: 8px;
  overflow: hidden;
}

.items-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.84rem;
}

.items-table th {
  background: #faf8f6;
  color: #736965;
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  padding: 9px 12px;
  border-bottom: 1px solid #eee7e3;
  text-align: left;
}

.items-table td {
  padding: 10px 12px;
  border-bottom: 1px solid #f2ede9;
  vertical-align: middle;
}

.items-table tr:last-child td {
  border-bottom: none;
}

.item-name-box {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.product-title {
  font-weight: 600;
  color: #25201f;
}

.product-variation {
  font-size: 0.73rem;
  color: #736965;
}

.qty-val {
  font-weight: 700;
  color: #49403d;
}

.item-subtotal {
  font-weight: 700;
  color: #25201f;
}

.text-center { text-align: center; }
.text-right { text-align: right; }
.text-muted { color: #8b807b; }

.financial-summary {
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 8px;
  padding: 14px 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.summary-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.84rem;
  color: #736965;
}

.summary-total-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid #e5ddd8;
  padding-top: 8px;
}

.total-label {
  font-size: 0.95rem;
  font-weight: 800;
  color: #25201f;
}

.total-value {
  font-size: 1.15rem;
  font-weight: 800;
  color: #b33f62;
}

.cancelled-notice {
  padding: 13px 15px;
  background: #fef2f2;
  border: 1px solid #fecdd3;
  border-radius: 8px;
  color: #991b1b;
  font-size: 0.82rem;
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.cancelled-notice-header {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #991b1b;
}

.cancelled-notice-header strong {
  font-size: 0.88rem;
  font-weight: 700;
  margin: 0;
}

.cancelled-notice-icon {
  flex-shrink: 0;
  color: #dc2626;
}

.cancelled-desc {
  margin: 0;
  color: #7f1d1d;
  font-size: 0.8rem;
  line-height: 1.4;
}

.cancelled-motivo-box {
  margin-top: 4px;
  padding: 9px 12px;
  background: #ffffff;
  border: 1px solid #fca5a5;
  border-left: 3px solid #dc2626;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.cancelled-motivo-label {
  font-size: 0.72rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  color: #991b1b;
}

.cancelled-motivo-text {
  margin: 0;
  font-size: 0.84rem;
  color: #25201f;
  font-weight: 500;
  word-break: break-word;
  white-space: pre-wrap;
}

.cancel-confirmation-box {
  background: #fff5f7;
  border: 1px solid #f5b0b6;
  border-radius: 8px;
  padding: 14px 16px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.cancel-confirmation-box strong {
  color: #b33f62;
  font-size: 0.9rem;
}

.confirm-desc {
  margin: 0;
  font-size: 0.8rem;
  color: #625955;
}

.motivo-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.motivo-field label {
  font-size: 0.75rem;
  font-weight: 700;
  color: #49403d;
}

.motivo-field input {
  min-height: 36px;
  padding: 6px 10px;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  font-size: 0.84rem;
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.btn-cancel-back {
  min-height: 34px;
  padding: 0 12px;
  background: transparent;
  color: #625955;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
}

.btn-cancel-execute {
  min-height: 34px;
  padding: 0 14px;
  background: #da5c81;
  color: #ffffff;
  border: none;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
}

.modal-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 22px;
  background: #faf8f6;
  border-top: 1px solid #eee7e3;
  gap: 10px;
}

.footer-left {
  display: flex;
  gap: 8px;
}

.btn-danger-outline {
  min-height: 36px;
  padding: 0 12px;
  background: transparent;
  color: #991b1b;
  border: 1px solid #fca5a5;
  border-radius: 6px;
  font-size: 0.82rem;
  font-weight: 700;
  cursor: pointer;
}

.btn-danger-outline:hover {
  background: #fee2e2;
}

.btn-primary-action {
  min-height: 36px;
  padding: 0 16px;
  background: #059669;
  color: #ffffff;
  border: none;
  border-radius: 6px;
  font-size: 0.82rem;
  font-weight: 700;
  cursor: pointer;
}

.btn-close-modal {
  min-height: 36px;
  padding: 0 16px;
  background: #ffffff;
  color: #49403d;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  font-size: 0.82rem;
  font-weight: 700;
  cursor: pointer;
}

.btn-close-modal:hover {
  background: #eee7e3;
}

@media (max-width: 600px) {
  .info-grid {
    grid-template-columns: 1fr;
  }
  .modal-card {
    border-radius: 8px;
  }
  .modal-body {
    padding: 14px;
  }
  .modal-header,
  .modal-footer {
    padding: 12px 14px;
  }
}
</style>
