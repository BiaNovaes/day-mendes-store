<script setup lang="ts">
import { reactive, ref } from 'vue'
import { api, type Cliente, type Produto, type VariacaoProduto } from '../../api'
import PdvAdicionarProduto from './PdvAdicionarProduto.vue'
import PdvCarrinho, { type CartItem } from './PdvCarrinho.vue'
import { money } from './pdvUtils'

const props = defineProps<{
  clientes: Cliente[]
  availableVariations: Array<{ produto: Produto; variacao: VariacaoProduto }>
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'venda-finalizada'): void
  (e: 'refresh'): void
}>()

const cart = ref<CartItem[]>([])
const vendaForm = reactive({
  clienteId: '',
  formaPagamento: 'Pix',
})

const submitting = ref(false)
const saleError = ref('')
const saleSuccess = ref<{ id: number; total: number; formaPagamento: string } | null>(null)

function handleAddItem(payload: { produto: Produto; variacao: VariacaoProduto; quantidade: number }) {
  saleError.value = ''
  saleSuccess.value = null

  const existing = cart.value.find((item) => item.variacao.id === payload.variacao.id)
  if (existing) {
    const novaQtd = existing.quantidade + payload.quantidade
    existing.quantidade = Math.min(novaQtd, payload.variacao.quantidadeEstoque)
  } else {
    cart.value.push({
      produto: payload.produto,
      variacao: payload.variacao,
      quantidade: Math.min(payload.quantidade, payload.variacao.quantidadeEstoque),
    })
  }
}

function handleRemoveItem(variationId: number) {
  cart.value = cart.value.filter((item) => item.variacao.id !== variationId)
}

function handleUpdateQuantity(payload: { variacaoId: number; quantidade: number }) {
  const item = cart.value.find((i) => i.variacao.id === payload.variacaoId)
  if (!item) return

  if (payload.quantidade <= 0) {
    handleRemoveItem(payload.variacaoId)
    return
  }

  item.quantidade = Math.min(payload.quantidade, item.variacao.quantidadeEstoque)
}

async function handleFinishSale() {
  if (!cart.value.length) return

  saleError.value = ''
  saleSuccess.value = null
  submitting.value = true

  try {
    const payload = {
      clienteId: vendaForm.clienteId ? Number(vendaForm.clienteId) : null,
      formaPagamento: vendaForm.formaPagamento,
      finalizarImediatamente: true,
      itens: cart.value.map((item) => ({
        produtoId: item.produto.id,
        variacaoProdutoId: item.variacao.id,
        quantidade: item.quantidade,
      })),
    }

    const created = await api.criarVenda(payload)

    saleSuccess.value = {
      id: created.id,
      total: created.valorTotal,
      formaPagamento: created.formaPagamento,
    }

    cart.value = []
    vendaForm.clienteId = ''
    vendaForm.formaPagamento = 'Pix'

    emit('venda-finalizada')
  } catch (err) {
    saleError.value = err instanceof Error ? err.message : 'Não foi possível finalizar a venda.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="pdv-wrapper">
    <transition name="fade">
      <div v-if="saleSuccess" class="pdv-banner success-banner" role="status">
        <div class="banner-icon-circle">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="20 6 9 17 4 12" />
          </svg>
        </div>
        <div class="banner-info">
          <strong class="banner-title">Venda #{{ saleSuccess.id }} finalizada com sucesso!</strong>
          <span class="banner-subtitle">
            Total de {{ money(saleSuccess.total) }} pago via {{ saleSuccess.formaPagamento }}. O estoque já foi atualizado.
          </span>
        </div>
        <button
          type="button"
          class="banner-btn-close"
          title="Fechar mensagem"
          aria-label="Fechar mensagem"
          @click="saleSuccess = null"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </div>
    </transition>

    <transition name="fade">
      <div v-if="saleError" class="pdv-banner error-banner" role="alert">
        <div class="banner-icon-circle error-circle">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
        </div>
        <div class="banner-info">
          <strong class="banner-title">Falha ao finalizar venda</strong>
          <span class="banner-subtitle">{{ saleError }}</span>
        </div>
        <button
          type="button"
          class="banner-btn-close"
          title="Fechar mensagem"
          aria-label="Fechar mensagem"
          @click="saleError = ''"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </div>
    </transition>

    <div class="pdv-grid">
      <section class="pdv-col-add" aria-label="Passo 1: Adicionar Produto">
        <PdvAdicionarProduto
          :available-variations="availableVariations"
          :loading="loading || submitting"
          @add-item="handleAddItem"
        />
      </section>

      <section class="pdv-col-cart" aria-label="Passo 2 e Fechamento: Carrinho e Total">
        <PdvCarrinho
          :cart="cart"
          :clientes="clientes"
          :loading="loading || submitting"
          :cliente-id="vendaForm.clienteId"
          :forma-pagamento="vendaForm.formaPagamento"
          @update:cliente-id="(val) => (vendaForm.clienteId = val)"
          @update:forma-pagamento="(val) => (vendaForm.formaPagamento = val)"
          @remove-item="handleRemoveItem"
          @update-quantity="handleUpdateQuantity"
          @finalizar="handleFinishSale"
        />
      </section>
    </div>
  </div>
</template>

<style scoped>
.pdv-wrapper {
  display: flex;
  flex-direction: column;
  gap: 20px;
  width: 100%;
}

.pdv-banner {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 14px 18px;
  border-radius: 10px;
  box-shadow: 0 4px 14px rgba(48, 35, 30, 0.06);
}

.success-banner {
  background: #edf7f1;
  border: 1px solid #bde3cd;
  color: #206e43;
}

.error-banner {
  background: #fff0f2;
  border: 1px solid #f1bdc8;
  color: #9b1c3f;
}

.banner-icon-circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: rgba(45, 122, 79, 0.15);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.error-circle {
  background: rgba(155, 28, 63, 0.15);
}

.banner-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
}

.banner-title {
  font-size: 0.95rem;
  font-weight: 700;
}

.banner-subtitle {
  font-size: 0.82rem;
  opacity: 0.9;
}

.banner-btn-close {
  background: transparent;
  border: none;
  color: inherit;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  opacity: 0.7;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: auto;
}

.banner-btn-close:hover {
  opacity: 1;
  background: rgba(0, 0, 0, 0.05);
}

.pdv-grid {
  display: grid;
  grid-template-columns: minmax(360px, 420px) minmax(420px, 1fr);
  gap: 24px;
  align-items: start;
}

.pdv-col-add,
.pdv-col-cart {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.fade-enter-active,
.fade-leave-active {
  transition: all 0.25s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

@media (max-width: 980px) {
  .pdv-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }
}

@media (max-width: 560px) {
  .pdv-wrapper {
    gap: 14px;
  }

  .pdv-banner {
    padding: 12px 14px;
    gap: 10px;
  }

  .banner-icon-circle {
    width: 30px;
    height: 30px;
  }

  .banner-title {
    font-size: 0.88rem;
  }

  .banner-subtitle {
    font-size: 0.78rem;
  }
}
</style>
