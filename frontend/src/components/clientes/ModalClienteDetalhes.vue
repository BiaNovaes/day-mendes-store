<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { api, getProductImageUrl, type Cliente, type Venda } from '../../api'
import { formatFriendlyDate, formatMoney, getPaymentConfig } from '../vendas/vendaUtils'

const props = defineProps<{
  open: boolean
  cliente: Cliente | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'updated', cliente: Cliente): void
}>()

const loadingVendas = ref(false)
const vendasCliente = ref<Venda[]>([])
const errorVendas = ref('')

const isEditing = ref(false)
const isSaving = ref(false)
const editError = ref('')
const currentCliente = ref<Cliente | null>(null)

const editForm = reactive({
  nome: '',
  apelido: '',
  email: '',
  telefone: '',
  endereco: '',
})

watch(
  () => props.cliente,
  (newVal) => {
    if (newVal) {
      currentCliente.value = { ...newVal }
    } else {
      currentCliente.value = null
    }
  },
  { immediate: true, deep: true }
)

const displayCliente = computed(() => currentCliente.value ?? props.cliente)

function handleBackdropClick(e: MouseEvent) {
  if (e.target === e.currentTarget) {
    emit('close')
  }
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') {
    if (isEditing.value) {
      cancelarEdicao()
    } else {
      emit('close')
    }
  }
}

const initials = computed(() => {
  if (!displayCliente.value?.nome) return 'CL'
  const parts = displayCliente.value.nome.trim().split(/\s+/)
  const first = parts[0]
  if (!first) return 'CL'
  if (parts.length === 1) return first.substring(0, 2).toUpperCase()
  const last = parts[parts.length - 1]
  const firstChar = first[0] ?? ''
  const lastChar = last ? (last[0] ?? '') : ''
  return (firstChar + lastChar).toUpperCase() || 'CL'
})

function iniciarEdicao() {
  if (!displayCliente.value) return
  editForm.nome = displayCliente.value.nome || ''
  editForm.apelido = displayCliente.value.apelido || ''
  editForm.email = displayCliente.value.email || ''
  editForm.telefone = displayCliente.value.telefone || ''
  editForm.endereco = displayCliente.value.endereco || ''
  editError.value = ''
  isEditing.value = true
}

function cancelarEdicao() {
  isEditing.value = false
  editError.value = ''
}

async function salvarEdicao() {
  if (!displayCliente.value) return
  const nomeTrim = editForm.nome.trim()
  if (!nomeTrim) {
    editError.value = 'O nome do cliente é obrigatório.'
    return
  }

  isSaving.value = true
  editError.value = ''

  try {
    const updated = await api.atualizarCliente(displayCliente.value.id, {
      nome: nomeTrim,
      apelido: editForm.apelido.trim() || undefined,
      email: editForm.email.trim() || undefined,
      telefone: editForm.telefone.trim() || undefined,
      endereco: editForm.endereco.trim() || undefined,
    })

    currentCliente.value = { ...currentCliente.value, ...updated }
    emit('updated', currentCliente.value)
    isEditing.value = false
  } catch (err) {
    editError.value = err instanceof Error ? err.message : 'Não foi possível atualizar os dados do cliente.'
  } finally {
    isSaving.value = false
  }
}

async function carregarHistoricoCompras(clienteId: number) {
  loadingVendas.value = true
  errorVendas.value = ''
  try {
    const res = await api.vendas({ clienteId, pageSize: 50 })
    vendasCliente.value = res.items || []
  } catch (err) {
    errorVendas.value = err instanceof Error ? err.message : 'Não foi possível carregar o histórico de compras.'
  } finally {
    loadingVendas.value = false
  }
}

watch(
  () => [props.open, props.cliente?.id],
  ([isOpen, id]) => {
    isEditing.value = false
    editError.value = ''
    if (isOpen && id) {
      carregarHistoricoCompras(Number(id))
    } else if (!isOpen) {
      vendasCliente.value = []
      errorVendas.value = ''
    }
  }
)

function formatSaleDateHeader(dateStr?: string): string {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  if (isNaN(d.getTime())) return dateStr
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  return `${day}/${month}`
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open && displayCliente"
      class="modal-backdrop"
      role="dialog"
      aria-modal="true"
      :aria-labelledby="`modal-cliente-title-${displayCliente.id}`"
      @click="handleBackdropClick"
      @keydown="handleKeydown"
    >
      <div class="modal-card">
        <header class="modal-header">
          <div class="header-client-profile">
            <div class="avatar-circle">
              {{ initials }}
            </div>
            <div class="header-profile-texts">
              <div class="client-title-row">
                <h2 :id="`modal-cliente-title-${displayCliente.id}`" class="client-name">
                  {{ displayCliente.nome }}
                </h2>
                <span v-if="displayCliente.apelido" class="client-nickname-pill">
                  "{{ displayCliente.apelido }}"
                </span>
                <span
                  class="status-pill"
                  :class="displayCliente.status === 1 ? 'status-active' : 'status-inactive'"
                >
                  {{ displayCliente.status === 1 ? 'Ativo' : 'Inativo' }}
                </span>
              </div>
              <p class="client-since">
                Cliente desde {{ new Date(displayCliente.createdAt || Date.now()).toLocaleDateString('pt-BR') }}
              </p>
            </div>
          </div>

          <button
            type="button"
            class="btn-close"
            title="Fechar"
            aria-label="Fechar janela de detalhes do cliente"
            @click="emit('close')"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </header>

        <div class="modal-body">
          <div class="summary-metrics-grid">
            <div class="metric-card">
              <span class="metric-label">Total gasto</span>
              <strong class="metric-value">{{ formatMoney(displayCliente.valorTotalComprado) }}</strong>
            </div>
            <div class="metric-card">
              <span class="metric-label">Compras realizadas</span>
              <strong class="metric-value">{{ displayCliente.totalCompras || 0 }}</strong>
            </div>
            <div class="metric-card">
              <span class="metric-label">Última compra</span>
              <strong class="metric-value metric-small">
                {{ formatFriendlyDate(displayCliente.ultimaCompraEm) }}
              </strong>
            </div>
          </div>

          <section class="section-box">
            <div class="section-box-header">
              <span class="section-badge-title">Dados do cliente</span>
              <button
                v-if="!isEditing"
                type="button"
                class="btn-edit-client"
                title="Editar dados cadastrais deste cliente"
                @click="iniciarEdicao"
              >
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                </svg>
                <span>Editar</span>
              </button>
            </div>

            <form v-if="isEditing" class="client-edit-form" @submit.prevent="salvarEdicao">
              <div v-if="editError" class="edit-error-banner">
                {{ editError }}
              </div>

              <div class="edit-fields-grid">
                <div class="edit-field">
                  <label for="edit-cli-nome">Nome *</label>
                  <input
                    id="edit-cli-nome"
                    v-model="editForm.nome"
                    type="text"
                    required
                    placeholder="Nome completo"
                    class="edit-input"
                    :disabled="isSaving"
                  />
                </div>

                <div class="edit-field">
                  <label for="edit-cli-apelido">Apelido</label>
                  <input
                    id="edit-cli-apelido"
                    v-model="editForm.apelido"
                    type="text"
                    placeholder="Como prefere ser chamado"
                    class="edit-input"
                    :disabled="isSaving"
                  />
                </div>

                <div class="edit-field">
                  <label for="edit-cli-email">E-mail</label>
                  <input
                    id="edit-cli-email"
                    v-model="editForm.email"
                    type="email"
                    placeholder="email@exemplo.com"
                    class="edit-input"
                    :disabled="isSaving"
                  />
                </div>

                <div class="edit-field">
                  <label for="edit-cli-telefone">Telefone</label>
                  <input
                    id="edit-cli-telefone"
                    v-model="editForm.telefone"
                    type="text"
                    placeholder="(00) 00000-0000"
                    class="edit-input"
                    :disabled="isSaving"
                  />
                </div>

                <div class="edit-field edit-field-full">
                  <label for="edit-cli-endereco">Endereço</label>
                  <textarea
                    id="edit-cli-endereco"
                    v-model="editForm.endereco"
                    rows="2"
                    placeholder="Rua, número, complemento, bairro, cidade..."
                    class="edit-textarea"
                    :disabled="isSaving"
                  ></textarea>
                </div>
              </div>

              <div class="edit-actions-row">
                <button
                  type="button"
                  class="btn-edit-cancel"
                  :disabled="isSaving"
                  @click="cancelarEdicao"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  class="btn-edit-save"
                  :disabled="isSaving || !editForm.nome.trim()"
                >
                  <span v-if="isSaving" class="spinner-small"></span>
                  <span>{{ isSaving ? 'Salvando...' : 'Salvar alterações' }}</span>
                </button>
              </div>
            </form>

            <div v-else class="client-details-grid">
              <div class="detail-item">
                <span class="detail-label">Nome completo</span>
                <span class="detail-value">{{ displayCliente.nome }}</span>
              </div>

              <div class="detail-item">
                <span class="detail-label">Apelido</span>
                <span class="detail-value">{{ displayCliente.apelido || 'Não informado' }}</span>
              </div>

              <div class="detail-item">
                <span class="detail-label">E-mail</span>
                <span class="detail-value" :class="{ 'text-muted': !displayCliente.email }">
                  <a v-if="displayCliente.email" :href="`mailto:${displayCliente.email}`" class="detail-link">
                    {{ displayCliente.email }}
                  </a>
                  <span v-else>Não informado</span>
                </span>
              </div>

              <div class="detail-item">
                <span class="detail-label">Telefone</span>
                <span class="detail-value" :class="{ 'text-muted': !displayCliente.telefone }">
                  {{ displayCliente.telefone || 'Não informado' }}
                </span>
              </div>

              <div class="detail-item detail-full-width">
                <span class="detail-label">Endereço</span>
                <span class="detail-value" :class="{ 'text-muted': !displayCliente.endereco }">
                  {{ displayCliente.endereco || 'Não informado' }}
                </span>
              </div>
            </div>
          </section>

          <section class="section-box">
            <div class="section-box-header">
              <div class="section-title-wrap">
                <span class="section-badge-title">Histórico de compras</span>
                <span v-if="!loadingVendas" class="badge-count">
                  {{ vendasCliente.length }} {{ vendasCliente.length === 1 ? 'compra' : 'compras' }}
                </span>
              </div>
              <button
                type="button"
                class="btn-reload-history"
                :disabled="loadingVendas"
                title="Recarregar histórico"
                @click="carregarHistoricoCompras(displayCliente.id)"
              >
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" :class="{ spinning: loadingVendas }">
                  <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2" />
                </svg>
                <span>Atualizar</span>
              </button>
            </div>

            <div v-if="loadingVendas" class="loading-state">
              <div class="spinner"></div>
              <span>Carregando compras do cliente...</span>
            </div>

            <div v-else-if="errorVendas" class="error-state">
              <p>{{ errorVendas }}</p>
              <button type="button" class="btn-retry" @click="carregarHistoricoCompras(displayCliente.id)">
                Tentar novamente
              </button>
            </div>

            <div v-else-if="vendasCliente.length === 0" class="empty-state">
              <p class="empty-title">Nenhuma compra realizada ainda</p>
              <p class="empty-desc">
                Assim que este cliente realizar uma compra no PDV, os produtos e valores aparecerão aqui.
              </p>
            </div>

            <div v-else class="sales-history-list">
              <article
                v-for="(venda, index) in vendasCliente"
                :key="venda.id"
                class="sale-card"
                :class="{
                  'sale-most-recent': index === 0,
                  'sale-canceled': venda.statusVenda === 3,
                }"
              >
                <header class="sale-header">
                  <div class="sale-header-left">
                    <div class="sale-title-row">
                      <span class="sale-date-badge">
                        Compra realizada — {{ formatSaleDateHeader(venda.dataVenda) }}
                      </span>
                      <span v-if="index === 0" class="tag-recent">
                        Compra mais recente
                      </span>
                    </div>
                    <span class="sale-full-date">
                      {{ formatFriendlyDate(venda.dataVenda) }} • Venda #{{ venda.id }}
                    </span>
                  </div>

                  <div class="sale-header-right">
                    <span
                      v-if="venda.statusVenda === 3"
                      class="status-pill-sale status-estornada"
                    >
                      Estornada
                    </span>
                    <span
                      v-else-if="venda.statusVenda === 2"
                      class="status-pill-sale status-finalizada"
                    >
                      Finalizada
                    </span>
                    <span
                      v-else
                      class="status-pill-sale status-rascunho"
                    >
                      Rascunho
                    </span>
                    <strong class="sale-total-value">
                      {{ formatMoney(venda.valorTotal) }}
                    </strong>
                  </div>
                </header>

                <div v-if="venda.statusVenda === 3" class="canceled-box">
                  <span class="canceled-label">Compra cancelada / estornada</span>
                  <p v-if="venda.motivoCancelamento" class="canceled-reason">
                    Motivo: {{ venda.motivoCancelamento }}
                  </p>
                </div>

                <div class="sale-content">
                  <div class="sale-payment-info">
                    <span class="payment-pill" :style="{ backgroundColor: getPaymentConfig(venda.formaPagamento).bg, color: getPaymentConfig(venda.formaPagamento).color }">
                      {{ getPaymentConfig(venda.formaPagamento).label }}
                    </span>
                    <span class="items-count-label">
                      {{ venda.itens?.length || 0 }} {{ (venda.itens?.length === 1) ? 'item' : 'itens' }}
                    </span>
                  </div>

                  <div class="sale-items-table-wrapper">
                    <div
                      v-for="item in venda.itens"
                      :key="item.id"
                      class="sale-item-row"
                    >
                      <div class="sale-item-thumb-box">
                        <img
                          v-if="item.produtoFoto && item.produtoFoto.trim()"
                          :src="getProductImageUrl(item.produtoFoto)"
                          :alt="item.produtoNome"
                          class="sale-item-img"
                        />
                        <div v-else class="sale-item-placeholder" aria-hidden="true">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                            <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                            <circle cx="8.5" cy="8.5" r="1.5"/>
                            <polyline points="21 15 16 10 5 21"/>
                          </svg>
                        </div>
                      </div>

                      <div class="sale-item-info">
                        <span class="item-name">{{ item.produtoNome }}</span>
                        <div class="item-variation-row">
                          <span v-if="item.tamanho" class="item-var-badge">
                            Tam: {{ item.tamanho }}
                          </span>
                          <span v-if="item.cor && item.cor.trim() !== '-'" class="item-var-color">
                            {{ item.cor }}
                          </span>
                          <span class="item-qty-price">
                            {{ item.quantidade }}x {{ formatMoney(item.valorUnitario) }}
                          </span>
                        </div>
                      </div>

                      <div class="sale-item-subtotal">
                        <strong>{{ formatMoney(item.subtotal) }}</strong>
                      </div>
                    </div>
                  </div>
                </div>
              </article>
            </div>
          </section>
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
  box-shadow: 0 24px 60px rgba(48, 35, 30, 0.2);
  width: 100%;
  max-width: 620px;
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

.header-client-profile {
  display: flex;
  align-items: center;
  gap: 14px;
  min-width: 0;
}

.avatar-circle {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #b33f62;
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.1rem;
  letter-spacing: 0.5px;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(179, 63, 98, 0.25);
}

.header-profile-texts {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.client-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.client-name {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 800;
  color: #25201f;
  line-height: 1.25;
}

.client-nickname-pill {
  font-size: 0.78rem;
  font-weight: 700;
  color: #832742;
  background: #fdf2f5;
  border: 1px solid #f9ccd7;
  padding: 1px 7px;
  border-radius: 12px;
}

.status-pill {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 2px 7px;
  border-radius: 4px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.status-active {
  background: #e6f7ef;
  color: #065f46;
}

.status-inactive {
  background: #f3f4f6;
  color: #6b7280;
}

.client-since {
  margin: 0;
  font-size: 0.76rem;
  color: #8b807b;
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
  gap: 16px;
}

.summary-metrics-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
}

.metric-card {
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 8px;
  padding: 10px 14px;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.metric-label {
  font-size: 0.72rem;
  font-weight: 800;
  color: #8b807b;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.metric-value {
  font-size: 1.12rem;
  font-weight: 800;
  color: #25201f;
}

.metric-small {
  font-size: 0.88rem;
  font-weight: 700;
  color: #49403d;
}

.section-box {
  background: #ffffff;
  border: 1px solid #eee7e3;
  border-radius: 8px;
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section-box-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 4px;
  border-bottom: 1px solid #f4f1ee;
}

.section-title-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-badge-title {
  font-size: 0.78rem;
  font-weight: 800;
  color: #625955;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.badge-count {
  font-size: 0.72rem;
  font-weight: 700;
  color: #832742;
  background: #fdf2f5;
  padding: 2px 7px;
  border-radius: 10px;
}

.btn-edit-client {
  background: #fdf2f5;
  border: 1px solid #f9ccd7;
  color: #832742;
  font-size: 0.74rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 3px 9px;
  border-radius: 4px;
  min-height: auto;
  transition: all 0.15s ease;
}

.btn-edit-client:hover {
  background: #f9ccd7;
  color: #66182f;
}

.client-edit-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
  animation: fadeIn 0.15s ease;
}

.edit-error-banner {
  padding: 8px 12px;
  background: #fff0f2;
  border: 1px solid #f1bdc8;
  border-radius: 6px;
  color: #9b1c3f;
  font-size: 0.8rem;
  font-weight: 600;
}

.edit-fields-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px 14px;
}

.edit-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.edit-field label {
  font-size: 0.72rem;
  font-weight: 700;
  color: #8b807b;
  text-transform: uppercase;
  letter-spacing: 0.2px;
}

.edit-field-full {
  grid-column: 1 / -1;
}

.edit-input,
.edit-textarea {
  width: 100%;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  padding: 8px 10px;
  font-size: 0.86rem;
  color: #25201f;
  background: #ffffff;
  outline: none;
  box-sizing: border-box;
  font-family: inherit;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.edit-input:focus,
.edit-textarea:focus {
  border-color: #b33f62;
  box-shadow: 0 0 0 2px rgba(179, 63, 98, 0.12);
}

.edit-input:disabled,
.edit-textarea:disabled {
  background: #f4f1ee;
  color: #8b807b;
  cursor: not-allowed;
}

.edit-textarea {
  min-height: 60px;
  resize: vertical;
}

.edit-actions-row {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 8px;
  padding-top: 4px;
}

.btn-edit-cancel {
  min-height: 32px;
  padding: 0 12px;
  border: 1px solid #d8cfca;
  background: #ffffff;
  color: #625955;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-edit-cancel:hover:not(:disabled) {
  background: #f4f1ee;
  color: #25201f;
}

.btn-edit-save {
  min-height: 32px;
  padding: 0 16px;
  border: 0;
  background: #b33f62;
  color: #ffffff;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: background-color 0.15s ease;
}

.btn-edit-save:hover:not(:disabled) {
  background: #832742;
}

.btn-edit-save:disabled,
.btn-edit-cancel:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner-small {
  width: 12px;
  height: 12px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

.btn-reload-history {
  background: transparent;
  border: 0;
  color: #8b807b;
  font-size: 0.76rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 8px;
  border-radius: 4px;
  min-height: auto;
}

.btn-reload-history:hover:not(:disabled) {
  background: #eee7e3;
  color: #25201f;
}

.client-details-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px 16px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.detail-full-width {
  grid-column: 1 / -1;
}

.detail-label {
  font-size: 0.72rem;
  font-weight: 700;
  color: #8b807b;
  text-transform: uppercase;
  letter-spacing: 0.2px;
}

.detail-value {
  font-size: 0.88rem;
  font-weight: 600;
  color: #25201f;
  word-break: break-word;
}

.detail-link {
  color: #9d3556;
  text-decoration: none;
}

.detail-link:hover {
  text-decoration: underline;
}

.text-muted {
  color: #a89f9a;
  font-style: italic;
  font-weight: 400;
}

.loading-state {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 28px;
  color: #756a65;
  font-size: 0.86rem;
  font-weight: 600;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid #e5ddd8;
  border-top-color: #b33f62;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.spinning {
  animation: spin 0.7s linear infinite;
}

.error-state {
  padding: 16px;
  background: #fff0f2;
  border: 1px solid #f1bdc8;
  border-radius: 6px;
  color: #9b1c3f;
  font-size: 0.84rem;
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: flex-start;
}

.btn-retry {
  min-height: 28px;
  padding: 0 10px;
  border-radius: 4px;
  background: #9b1c3f;
  color: #ffffff;
  font-size: 0.76rem;
  border: 0;
  cursor: pointer;
}

.empty-state {
  padding: 24px 16px;
  text-align: center;
  background: #faf8f6;
  border: 1px dashed #d8cfca;
  border-radius: 6px;
}

.empty-title {
  margin: 0 0 4px 0;
  font-size: 0.88rem;
  font-weight: 700;
  color: #625955;
}

.empty-desc {
  margin: 0;
  font-size: 0.8rem;
  color: #8b807b;
}

.sales-history-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.sale-card {
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 8px;
  padding: 12px 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  transition: all 0.15s ease;
}

.sale-card.sale-most-recent {
  border-color: #f1bdc8;
  background: #fdfafb;
  box-shadow: 0 2px 8px rgba(179, 63, 98, 0.08);
}

.sale-card.sale-canceled {
  border-color: #fca5a5;
  background: #fffafa;
}

.sale-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 10px;
}

.sale-header-left {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.sale-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.sale-date-badge {
  font-size: 0.84rem;
  font-weight: 800;
  color: #25201f;
}

.tag-recent {
  font-size: 0.68rem;
  font-weight: 800;
  color: #832742;
  background: #fdf2f5;
  border: 1px solid #fecdd3;
  padding: 1px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.sale-full-date {
  font-size: 0.74rem;
  color: #8b807b;
}

.sale-header-right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 2px;
}

.status-pill-sale {
  font-size: 0.72rem;
  font-weight: 800;
  padding: 2px 7px;
  border-radius: 4px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.status-estornada {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecdd3;
}

.status-finalizada {
  background: #e6f7ef;
  color: #065f46;
}

.status-rascunho {
  background: #fef3c7;
  color: #92400e;
}

.sale-total-value {
  font-size: 1rem;
  font-weight: 800;
  color: #25201f;
}

.canceled-box {
  background: #fee2e2;
  border-left: 3px solid #ef4444;
  padding: 6px 10px;
  border-radius: 4px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.canceled-label {
  font-size: 0.74rem;
  font-weight: 800;
  color: #991b1b;
  text-transform: uppercase;
}

.canceled-reason {
  margin: 0;
  font-size: 0.78rem;
  color: #7f1d1d;
  font-style: italic;
}

.sale-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
  border-top: 1px solid #f0e9e5;
  padding-top: 8px;
}

.sale-payment-info {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.76rem;
}

.payment-pill {
  padding: 2px 7px;
  border-radius: 4px;
  font-weight: 700;
  font-size: 0.74rem;
}

.items-count-label {
  color: #8b807b;
  font-weight: 600;
}

.sale-items-table-wrapper {
  display: flex;
  flex-direction: column;
  gap: 6px;
  background: #ffffff;
  border: 1px solid #eee7e3;
  border-radius: 6px;
  padding: 8px;
}

.sale-item-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 4px 0;
  border-bottom: 1px solid #faf8f6;
}

.sale-item-row:last-child {
  border-bottom: none;
}

.sale-item-thumb-box {
  width: 32px;
  height: 32px;
  border-radius: 4px;
  overflow: hidden;
  background: #f4f1ee;
  border: 1px solid #e5ddd8;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.sale-item-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.sale-item-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #b5aaa5;
}

.sale-item-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 0;
}

.item-name {
  font-size: 0.82rem;
  font-weight: 700;
  color: #25201f;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-variation-row {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.72rem;
  color: #736965;
  flex-wrap: wrap;
}

.item-var-badge {
  font-weight: 700;
  background: #f4f1ee;
  padding: 1px 5px;
  border-radius: 3px;
  color: #49403d;
}

.item-var-color {
  font-weight: 600;
  color: #625955;
}

.item-qty-price {
  color: #8b807b;
}

.sale-item-subtotal {
  font-size: 0.84rem;
  font-weight: 700;
  color: #25201f;
  text-align: right;
  white-space: nowrap;
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

@media (max-width: 600px) {
  .summary-metrics-grid {
    grid-template-columns: 1fr;
  }

  .client-details-grid {
    grid-template-columns: 1fr;
  }

  .edit-fields-grid {
    grid-template-columns: 1fr;
  }

  .sale-header {
    flex-direction: column;
    align-items: stretch;
  }

  .sale-header-right {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    border-top: 1px dashed #eee7e3;
    padding-top: 6px;
    margin-top: 4px;
  }
}
</style>
