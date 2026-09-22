<script setup lang="ts">
import { computed } from 'vue'
import type { Venda } from '../../api'
import {
  formatMoney,
  formatFriendlyDate,
  getStatusConfig,
  getPaymentConfig,
} from './vendaUtils'

const props = defineProps<{
  venda: Venda
}>()

const emit = defineEmits<{
  (e: 'click', venda: Venda): void
}>()

const status = computed(() => getStatusConfig(props.venda.statusVenda))
const payment = computed(() => getPaymentConfig(props.venda.formaPagamento))

const totalItensText = computed(() => {
  const qtd = props.venda.totalItens || props.venda.itens?.reduce((acc, i) => acc + i.quantidade, 0) || 0
  return qtd === 1 ? '1 item' : `${qtd} itens`
})

function handleClick() {
  emit('click', props.venda)
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' || e.key === ' ') {
    e.preventDefault()
    emit('click', props.venda)
  }
}
</script>

<template>
  <tr
    class="venda-row"
    tabindex="0"
    role="button"
    :aria-label="`Ver detalhes da venda #${venda.id}`"
    @click="handleClick"
    @keydown="handleKeydown"
  >
    <td class="col-id">
      <div class="id-wrapper">
        <span class="venda-id">#{{ venda.id }}</span>
        <span class="venda-itens-count">{{ totalItensText }}</span>
      </div>
    </td>

    <td class="col-date">
      <span class="date-main">{{ formatFriendlyDate(venda.dataVenda) }}</span>
    </td>

    <td class="col-client">
      <div class="client-info">
        <span class="client-name" :class="{ 'client-anonymous': !venda.clienteNome }">
          {{ venda.clienteNome || 'Venda avulsa' }}
        </span>
        <span v-if="venda.clienteTelefone" class="client-phone">
          {{ venda.clienteTelefone }}
        </span>
      </div>
    </td>

    <td class="col-payment">
      <span
        class="payment-badge"
        :style="{
          backgroundColor: payment.bg,
          color: payment.color,
        }"
      >
        {{ payment.label }}
      </span>
    </td>

    <td class="col-status">
      <span
        class="status-pill"
        :style="{
          backgroundColor: status.bg,
          color: status.color,
        }"
      >
        <span class="status-dot" :style="{ backgroundColor: status.dotColor }"></span>
        {{ status.label }}
      </span>
    </td>

    <td class="col-total">
      <span class="total-value">{{ formatMoney(venda.valorTotal) }}</span>
    </td>

    <td class="col-action">
      <span class="mobile-action-label">Ver detalhes</span>
      <svg
        class="action-chevron"
        viewBox="0 0 24 24"
        width="15"
        height="15"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
      >
        <polyline points="9 18 15 12 9 6" />
      </svg>
    </td>
  </tr>
</template>

<style scoped>
.venda-row {
  cursor: pointer;
  transition: background-color 0.15s ease;
  border-bottom: 1px solid #f2ede9;
  outline: none;
}

.venda-row:last-child {
  border-bottom: none;
}

.venda-row:hover {
  background-color: #faf7f5;
}

.venda-row:focus-visible {
  background-color: #fdf5f7;
  box-shadow: inset 0 0 0 2px #da5c81;
}

td {
  padding: 13px 16px;
  vertical-align: middle;
}

.col-id {
  white-space: nowrap;
}

.id-wrapper {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.venda-id {
  font-weight: 800;
  font-size: 0.95rem;
  color: #b33f62;
}

.venda-itens-count {
  font-size: 0.72rem;
  color: #8b807b;
}

.col-date {
  white-space: nowrap;
}

.date-main {
  font-size: 0.85rem;
  color: #524844;
}

.col-client {
  min-width: 170px;
}

.client-info {
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.client-name {
  font-size: 0.88rem;
  font-weight: 600;
  color: #25201f;
}

.client-name.client-anonymous {
  color: #8b807b;
  font-style: italic;
  font-weight: 400;
}

.client-phone {
  font-size: 0.74rem;
  color: #8b807b;
}

.col-payment {
  white-space: nowrap;
}

.payment-badge {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 0.76rem;
  font-weight: 600;
}

.col-status {
  white-space: nowrap;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 9px;
  border-radius: 12px;
  font-size: 0.76rem;
  font-weight: 700;
}

.status-dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  flex-shrink: 0;
}

.col-total {
  white-space: nowrap;
  text-align: right;
}

.total-value {
  font-size: 0.96rem;
  font-weight: 800;
  color: #25201f;
}

.col-action {
  width: 36px;
  text-align: right;
  padding-right: 16px;
}

.action-chevron {
  color: #a39793;
  transition: color 0.15s ease;
}

.venda-row:hover .action-chevron {
  color: #b33f62;
}

.mobile-action-label {
  display: none;
}

@media (max-width: 768px) {
  .venda-row {
    display: grid;
    grid-template-columns: 1fr auto;
    grid-template-areas:
      "id status"
      "client client"
      "date payment"
      "total action";
    gap: 8px 12px;
    background: #ffffff;
    border: 1px solid #e5ddd8;
    border-radius: 12px;
    padding: 14px 16px;
    margin-bottom: 10px;
  }

  td {
    padding: 0;
  }

  .col-id {
    grid-area: id;
    align-self: center;
  }

  .col-status {
    grid-area: status;
    justify-self: end;
    align-self: center;
  }

  .col-client {
    grid-area: client;
    padding: 2px 0;
  }

  .col-date {
    grid-area: date;
    align-self: center;
  }

  .col-payment {
    grid-area: payment;
    justify-self: end;
    align-self: center;
  }

  .col-total {
    grid-area: total;
    text-align: left;
    padding-top: 8px;
    border-top: 1px dashed #eee7e3;
    align-self: center;
  }

  .col-action {
    grid-area: action;
    width: auto;
    padding-top: 8px;
    border-top: 1px dashed #eee7e3;
    justify-self: end;
    align-self: center;
    display: flex;
    align-items: center;
    gap: 4px;
  }

  .mobile-action-label {
    display: inline;
    font-size: 0.78rem;
    font-weight: 700;
    color: #b33f62;
  }
}
</style>
