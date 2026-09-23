<script setup lang="ts">
import { computed } from 'vue'
import type { Cliente, Produto, VariacaoProduto } from '../../api'
import { generatedBarcode, money } from './pdvUtils'

export interface CartItem {
  produto: Produto
  variacao: VariacaoProduto
  quantidade: number
}

const props = defineProps<{
  cart: CartItem[]
  clientes: Cliente[]
  loading?: boolean
  clienteId: string
  formaPagamento: string
}>()

const emit = defineEmits<{
  (e: 'update:clienteId', val: string): void
  (e: 'update:formaPagamento', val: string): void
  (e: 'remove-item', variationId: number): void
  (e: 'update-quantity', payload: { variacaoId: number; quantidade: number }): void
  (e: 'finalizar'): void
}>()

const totalItens = computed(() =>
  props.cart.reduce((total, item) => total + item.quantidade, 0),
)

const totalValor = computed(() =>
  props.cart.reduce(
    (total, item) => total + item.produto.valorVenda * item.quantidade,
    0,
  ),
)

function handleClienteChange(e: Event) {
  const target = e.target as HTMLSelectElement
  emit('update:clienteId', target.value)
}

function handlePagamentoChange(e: Event) {
  const target = e.target as HTMLSelectElement
  emit('update:formaPagamento', target.value)
}

function handleIncrement(item: CartItem) {
  if (item.quantidade >= item.variacao.quantidadeEstoque) {
    return
  }
  emit('update-quantity', {
    variacaoId: item.variacao.id,
    quantidade: item.quantidade + 1,
  })
}

function handleDecrement(item: CartItem) {
  if (item.quantidade <= 1) {
    emit('remove-item', item.variacao.id)
    return
  }
  emit('update-quantity', {
    variacaoId: item.variacao.id,
    quantidade: item.quantidade - 1,
  })
}
</script>

<template>
  <div class="step-card step-card-cart">
    <div class="step-header">
      <div class="step-badge">2</div>
      <div class="step-title-group">
        <div class="step-title-row">
          <h2 class="step-title">Carrinho</h2>
          <span v-if="cart.length" class="cart-badge-count">
            {{ totalItens }} {{ totalItens === 1 ? 'item' : 'itens' }}
          </span>
        </div>
        <span class="step-subtitle">Confira os produtos adicionados à venda</span>
      </div>
    </div>

    <div class="cart-content">
      <div v-if="!cart.length" class="cart-empty-state">
        <div class="empty-icon-circle">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="9" cy="21" r="1" />
            <circle cx="20" cy="21" r="1" />
            <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
          </svg>
        </div>
        <h3 class="empty-title">O carrinho está vazio</h3>
        <p class="empty-desc">
          Bipe o código de barras de um produto ou adicione manualmente ao lado para começar a venda.
        </p>
      </div>

      <div v-else class="cart-items-list">
        <article
          v-for="item in cart"
          :key="item.variacao.id"
          class="cart-item-row"
        >
          <div class="item-info">
            <span class="item-name">{{ item.produto.nome }}</span>
            <div class="item-tags">
              <span v-if="item.variacao.tamanho" class="item-tag tag-variation">
                Tam: <strong>{{ item.variacao.tamanho }}</strong>
              </span>
              <span v-if="item.variacao.cor" class="item-tag tag-variation">
                Cor: <strong>{{ item.variacao.cor }}</strong>
              </span>
              <span class="item-tag tag-code" title="Código de barras">
                {{ generatedBarcode(item.produto, item.variacao) }}
              </span>
            </div>
          </div>

          <div class="item-quantity-wrapper">
            <div class="qty-stepper">
              <button
                type="button"
                class="qty-btn"
                :title="item.quantidade <= 1 ? 'Remover item' : 'Diminuir quantidade'"
                @click="handleDecrement(item)"
              >
                -
              </button>
              <span class="qty-value">{{ item.quantidade }}</span>
              <button
                type="button"
                class="qty-btn"
                :disabled="item.quantidade >= item.variacao.quantidadeEstoque"
                title="Aumentar quantidade"
                @click="handleIncrement(item)"
              >
                +
              </button>
            </div>
            <span class="unit-price">{{ money(item.produto.valorVenda) }} un.</span>
          </div>

          <div class="item-subtotal-wrapper">
            <span class="item-subtotal-label">Subtotal</span>
            <strong class="item-subtotal-value">
              {{ money(item.produto.valorVenda * item.quantidade) }}
            </strong>
          </div>

          <div class="item-action">
            <button
              type="button"
              class="btn-remove-item"
              title="Remover produto do carrinho"
              aria-label="Remover produto"
              @click="emit('remove-item', item.variacao.id)"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6" />
                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                <line x1="10" y1="11" x2="10" y2="17" />
                <line x1="14" y1="11" x2="14" y2="17" />
              </svg>
            </button>
          </div>
        </article>
      </div>
    </div>

    <div class="checkout-footer">
      <div class="sale-options-grid">
        <div class="sale-option-field">
          <label for="pdv-cliente" class="sale-label">Cliente (opcional)</label>
          <select
            id="pdv-cliente"
            :value="clienteId"
            class="sale-select"
            @change="handleClienteChange"
          >
            <option value="">Venda sem cliente identificado</option>
            <option
              v-for="cliente in clientes"
              :key="cliente.id"
              :value="cliente.id"
            >
              {{ cliente.nome }} {{ cliente.apelido ? `(${cliente.apelido})` : '' }}
            </option>
          </select>
        </div>

        <div class="sale-option-field">
          <label for="pdv-pagamento" class="sale-label">Forma de pagamento</label>
          <select
            id="pdv-pagamento"
            :value="formaPagamento"
            class="sale-select"
            @change="handlePagamentoChange"
          >
            <option value="Pix">Pix</option>
            <option value="Debito">Cartão de Débito</option>
            <option value="Credito">Cartão de Crédito</option>
            <option value="Dinheiro">Dinheiro</option>
          </select>
        </div>
      </div>

      <div class="total-summary-card">
        <div class="total-meta">
          <span class="total-title">Total da venda</span>
          <span class="total-count">
            {{ totalItens }} {{ totalItens === 1 ? 'item adicionado' : 'itens adicionados' }}
          </span>
        </div>
        <div class="total-amount">
          {{ money(totalValor) }}
        </div>
      </div>

      <button
        type="button"
        class="btn-finish-sale"
        :disabled="!cart.length || loading"
        @click="emit('finalizar')"
      >
        <span v-if="loading" class="btn-spinner" aria-hidden="true"></span>
        <svg
          v-else
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2.5"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z" />
          <line x1="3" y1="6" x2="21" y2="6" />
          <path d="M16 10a4 4 0 0 1-8 0" />
        </svg>
        <span class="btn-finish-text">
          {{ loading ? 'Finalizando venda...' : 'Finalizar venda' }}
        </span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.step-card {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(48, 35, 30, 0.05);
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.step-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.step-badge {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: #b33f62;
  color: #ffffff;
  font-size: 0.9rem;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.step-title-group {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
}

.step-title-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.step-title {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 800;
  color: #25201f;
  letter-spacing: -0.01em;
}

.cart-badge-count {
  font-size: 0.76rem;
  font-weight: 700;
  color: #b33f62;
  background: #fbf0f3;
  border: 1px solid #f1bdc8;
  padding: 2px 8px;
  border-radius: 12px;
}

.step-subtitle {
  font-size: 0.8rem;
  color: #736965;
}

.cart-content {
  min-height: 180px;
  display: flex;
  flex-direction: column;
}

.cart-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 40px 16px;
  background: #faf8f6;
  border: 1px dashed #d8cfca;
  border-radius: 10px;
  margin: auto 0;
}

.empty-icon-circle {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: #ffffff;
  border: 1px solid #e5ddd8;
  color: #9c918c;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 12px;
}

.empty-title {
  margin: 0 0 6px;
  font-size: 0.98rem;
  font-weight: 700;
  color: #25201f;
}

.empty-desc {
  margin: 0;
  font-size: 0.82rem;
  color: #736965;
  max-width: 320px;
  line-height: 1.4;
}

.cart-items-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 380px;
  overflow-y: auto;
  padding-right: 4px;
}

.cart-items-list::-webkit-scrollbar {
  width: 6px;
}
.cart-items-list::-webkit-scrollbar-track {
  background: #faf8f6;
  border-radius: 4px;
}
.cart-items-list::-webkit-scrollbar-thumb {
  background: #d8cfca;
  border-radius: 4px;
}

.cart-item-row {
  display: grid;
  grid-template-columns: 1fr auto auto auto;
  align-items: center;
  gap: 16px;
  padding: 12px 14px;
  background: #faf8f6;
  border: 1px solid #eee7e3;
  border-radius: 8px;
  transition: all 0.15s ease;
}

.cart-item-row:hover {
  background: #f7f3f0;
  border-color: #e5ddd8;
}

.item-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.item-name {
  font-size: 0.92rem;
  font-weight: 700;
  color: #25201f;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-tags {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.item-tag {
  font-size: 0.72rem;
  padding: 2px 6px;
  border-radius: 4px;
}

.tag-variation {
  background: #ffffff;
  border: 1px solid #e5ddd8;
  color: #625955;
}

.tag-variation strong {
  color: #25201f;
}

.tag-code {
  background: #ffffff;
  border: 1px dashed #d8cfca;
  color: #8b807b;
  font-family: monospace;
}

.item-quantity-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
}

.qty-stepper {
  display: flex;
  align-items: center;
  background: #ffffff;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  overflow: hidden;
  height: 32px;
}

.qty-btn {
  width: 28px;
  height: 32px;
  background: transparent;
  border: none;
  color: #625955;
  font-size: 1rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  padding: 0;
  min-height: auto;
  transition: background 0.15s ease;
}

.qty-btn:hover:not(:disabled) {
  background: #eee7e3;
  color: #25201f;
}

.qty-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.qty-value {
  min-width: 28px;
  text-align: center;
  font-size: 0.88rem;
  font-weight: 700;
  color: #25201f;
}

.unit-price {
  font-size: 0.72rem;
  color: #8b807b;
}

.item-subtotal-wrapper {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 2px;
  min-width: 80px;
}

.item-subtotal-label {
  font-size: 0.68rem;
  color: #8b807b;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.item-subtotal-value {
  font-size: 0.95rem;
  font-weight: 800;
  color: #25201f;
}

.btn-remove-item {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  background: transparent;
  border: 1px solid transparent;
  color: #9c918c;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  padding: 0;
  min-height: auto;
  transition: all 0.15s ease;
}

.btn-remove-item:hover {
  background: #fff0f2;
  border-color: #f1bdc8;
  color: #9b1c3f;
}

.checkout-footer {
  display: flex;
  flex-direction: column;
  gap: 16px;
  border-top: 1px solid #eee7e3;
  padding-top: 18px;
  margin-top: auto;
}

.sale-options-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.sale-option-field {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.sale-label {
  font-size: 0.8rem;
  font-weight: 700;
  color: #625955;
}

.sale-select {
  width: 100%;
  min-height: 42px;
  padding: 8px 12px;
  border: 1px solid #d8cfca;
  border-radius: 8px;
  background: #ffffff;
  color: #25201f;
  font-size: 0.88rem;
  font-weight: 500;
}

.sale-select:focus {
  outline: none;
  border-color: #b33f62;
  box-shadow: 0 0 0 3px rgba(179, 63, 98, 0.1);
}

.total-summary-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: #faf8f6;
  border: 1.5px solid #e5ddd8;
  border-radius: 10px;
}

.total-meta {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.total-title {
  font-size: 0.95rem;
  font-weight: 800;
  color: #25201f;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.total-count {
  font-size: 0.78rem;
  color: #736965;
}

.total-amount {
  font-size: 1.7rem;
  font-weight: 900;
  color: #25201f;
  letter-spacing: -0.02em;
}

.btn-finish-sale {
  width: 100%;
  min-height: 50px;
  background: #b33f62;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-size: 1.05rem;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  cursor: pointer;
  box-shadow: 0 4px 14px rgba(179, 63, 98, 0.25);
  transition: all 0.2s ease;
}

.btn-finish-sale:hover:not(:disabled) {
  background: #9d3556;
  box-shadow: 0 6px 18px rgba(179, 63, 98, 0.35);
  transform: translateY(-1px);
}

.btn-finish-sale:active:not(:disabled) {
  background: #852945;
  transform: translateY(0);
}

.btn-finish-sale:disabled {
  background: #c5bcb8;
  color: #ffffff;
  box-shadow: none;
  cursor: not-allowed;
  opacity: 0.7;
}

.btn-spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 680px) {
  .step-card {
    padding: 16px;
    gap: 16px;
  }

  .cart-item-row {
    grid-template-columns: 1fr auto;
    grid-template-rows: auto auto;
    row-gap: 10px;
  }

  .item-info {
    grid-column: 1 / -1;
  }

  .item-quantity-wrapper {
    flex-direction: row;
    gap: 8px;
    align-items: center;
  }

  .item-subtotal-wrapper {
    align-items: flex-end;
  }

  .sale-options-grid {
    grid-template-columns: 1fr;
  }

  .total-summary-card {
    padding: 14px 16px;
  }

  .total-amount {
    font-size: 1.4rem;
  }
}
</style>
