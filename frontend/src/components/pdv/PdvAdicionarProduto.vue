<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from 'vue'
import type { Produto, VariacaoProduto } from '../../api'
import { generatedBarcode, money } from './pdvUtils'

const props = defineProps<{
  availableVariations: Array<{ produto: Produto; variacao: VariacaoProduto }>
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'add-item', payload: { produto: Produto; variacao: VariacaoProduto; quantidade: number }): void
}>()

const scanCode = ref('')
const scanInputRef = ref<HTMLInputElement | null>(null)
const scanError = ref('')
const scanSuccessMsg = ref('')

const isManualOpen = ref(false)
const productSearchTerm = ref('')
const manualSearchInputRef = ref<HTMLInputElement | null>(null)
const selectedVariationId = ref('')
const selectedQuantity = ref(1)

const filteredVariations = computed(() => {
  const term = productSearchTerm.value.trim().toLowerCase()
  if (!term) return []

  return props.availableVariations.filter(({ produto, variacao }) => {
    const nome = (produto.nome ?? '').toLowerCase()
    const marca = (produto.marca ?? '').toLowerCase()
    const tamanho = (variacao.tamanho ?? '').toLowerCase()
    const cor = (variacao.cor ?? '').toLowerCase()
    const barcodeGerado = generatedBarcode(produto, variacao).toLowerCase()
    const barcodeProprio = (variacao.codigoBarras ?? '').toLowerCase()

    return (
      nome.includes(term) ||
      marca.includes(term) ||
      tamanho.includes(term) ||
      cor.includes(term) ||
      barcodeGerado.includes(term) ||
      barcodeProprio.includes(term)
    )
  })
})

let successTimeout: ReturnType<typeof setTimeout> | null = null

function focusScanInput() {
  nextTick(() => {
    scanInputRef.value?.focus()
  })
}

function handleScan() {
  const code = scanCode.value.trim().toUpperCase()
  scanError.value = ''
  scanSuccessMsg.value = ''

  if (!code) {
    focusScanInput()
    return
  }

  const found = props.availableVariations.find(
    ({ produto, variacao }) =>
      generatedBarcode(produto, variacao) === code || variacao.codigoBarras === code,
  )

  if (!found) {
    scanError.value = `Código "${scanCode.value}" não encontrado ou produto sem estoque.`
    scanCode.value = ''
    focusScanInput()
    return
  }

  emit('add-item', {
    produto: found.produto,
    variacao: found.variacao,
    quantidade: 1,
  })

  scanSuccessMsg.value = `"${found.produto.nome} (${found.variacao.tamanho}/${found.variacao.cor})" adicionado!`
  scanCode.value = ''

  if (successTimeout) clearTimeout(successTimeout)
  successTimeout = setTimeout(() => {
    scanSuccessMsg.value = ''
  }, 2500)

  focusScanInput()
}

function handleSelectProduct(item: { produto: Produto; variacao: VariacaoProduto }) {
  emit('add-item', {
    produto: item.produto,
    variacao: item.variacao,
    quantidade: 1,
  })

  scanSuccessMsg.value = `"${item.produto.nome} (${item.variacao.tamanho}/${item.variacao.cor})" adicionado!`
  if (successTimeout) clearTimeout(successTimeout)
  successTimeout = setTimeout(() => {
    scanSuccessMsg.value = ''
  }, 2500)

  productSearchTerm.value = ''
  nextTick(() => {
    manualSearchInputRef.value?.focus()
  })
}

function handleManualSelectSubmit() {
  if (!selectedVariationId.value) return

  const found = props.availableVariations.find(
    ({ variacao }) => variacao.id === Number(selectedVariationId.value),
  )

  if (!found) return

  const qtd = Math.max(1, Math.floor(selectedQuantity.value || 1))

  emit('add-item', {
    produto: found.produto,
    variacao: found.variacao,
    quantidade: qtd,
  })

  selectedVariationId.value = ''
  selectedQuantity.value = 1

  scanSuccessMsg.value = `"${found.produto.nome} (${found.variacao.tamanho}/${found.variacao.cor})" adicionado!`
  if (successTimeout) clearTimeout(successTimeout)
  successTimeout = setTimeout(() => {
    scanSuccessMsg.value = ''
  }, 2500)

  focusScanInput()
}

function toggleManual() {
  isManualOpen.value = !isManualOpen.value
  if (isManualOpen.value) {
    nextTick(() => {
      manualSearchInputRef.value?.focus()
    })
  } else {
    productSearchTerm.value = ''
    selectedVariationId.value = ''
    focusScanInput()
  }
}

onMounted(() => {
  focusScanInput()
})
</script>

<template>
  <div class="step-card step-card-add">
    <div class="step-header">
      <div class="step-badge">1</div>
      <div class="step-title-group">
        <h2 class="step-title">Adicionar produto</h2>
        <span class="step-subtitle">Bipe a etiqueta ou adicione manualmente</span>
      </div>
    </div>

    <div class="scan-area">
      <form class="scan-form" @submit.prevent="handleScan">
        <label class="scan-label" for="pdv-scan-input">
          <span class="scan-label-text">Bipe o código de barras do produto</span>
          <span class="scan-label-hint">
            Use o leitor de código de barras ou digite o código e pressione Enter
          </span>
        </label>

        <div class="scan-input-wrapper">
          <span class="barcode-icon" aria-hidden="true">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M3 5v14" />
              <path d="M8 5v14" />
              <path d="M12 5v14" />
              <path d="M17 5v14" />
              <path d="M21 5v14" />
            </svg>
          </span>

          <input
            id="pdv-scan-input"
            ref="scanInputRef"
            v-model="scanCode"
            type="text"
            class="scan-input"
            placeholder="Aproxime o leitor ou digite o código..."
            autocomplete="off"
            :disabled="loading"
          />

          <button
            type="submit"
            class="btn-scan"
            :disabled="loading || !scanCode.trim()"
            title="Adicionar produto bipado"
          >
            <span class="btn-scan-text">Adicionar</span>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
          </button>
        </div>
      </form>

      <transition name="fade">
        <div v-if="scanError" class="scan-feedback error-feedback" role="alert">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
          <span>{{ scanError }}</span>
          <button type="button" class="btn-dismiss" @click="scanError = ''" title="Fechar" aria-label="Fechar">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>
      </transition>

      <transition name="fade">
        <div v-if="scanSuccessMsg" class="scan-feedback success-feedback" role="status">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="20 6 9 17 4 12" />
          </svg>
          <span>{{ scanSuccessMsg }}</span>
        </div>
      </transition>
    </div>

    <div class="secondary-divider">
      <span class="divider-text">Não tem código de barras?</span>
      <button
        type="button"
        class="btn-toggle-manual"
        :class="{ active: isManualOpen }"
        @click="toggleManual"
      >
        <span>Adicionar produto manualmente</span>
        <svg
          class="chevron-icon"
          :class="{ rotated: isManualOpen }"
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2.5"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </button>
    </div>

    <transition name="slide-collapse">
      <div v-if="isManualOpen" class="manual-box">
        <div class="manual-search-group">
          <label for="pdv-manual-search" class="manual-label">
            Buscar produto por nome ou código
          </label>
          <div class="manual-search-wrapper">
            <span class="manual-search-icon" aria-hidden="true">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="11" cy="11" r="8" />
                <line x1="21" y1="21" x2="16.65" y2="16.65" />
              </svg>
            </span>
            <input
              id="pdv-manual-search"
              ref="manualSearchInputRef"
              v-model="productSearchTerm"
              type="text"
              class="manual-search-input"
              placeholder="Buscar produto por nome ou código..."
              autocomplete="off"
            />
            <button
              v-if="productSearchTerm"
              type="button"
              class="btn-clear-search"
              title="Limpar pesquisa"
              aria-label="Limpar pesquisa"
              @click="productSearchTerm = ''"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
          </div>
        </div>

        <div v-if="!productSearchTerm.trim()" class="manual-search-hint">
        </div>

        <div v-else class="manual-results-wrapper">
          <div v-if="filteredVariations.length" class="manual-results-list">
            <button
              v-for="item in filteredVariations"
              :key="item.variacao.id"
              type="button"
              class="manual-result-item"
              :disabled="loading"
              title="Clique para adicionar ao carrinho"
              @click="handleSelectProduct(item)"
            >
              <div class="manual-result-info">
                <strong class="manual-result-name">{{ item.produto.nome }}</strong>
                <div class="manual-result-meta">
                  <span class="meta-tag">{{ item.variacao.tamanho }} / {{ item.variacao.cor }}</span>
                  <span class="meta-stock">Estoque: {{ item.variacao.quantidadeEstoque }}</span>
                  <span class="meta-code">{{ generatedBarcode(item.produto, item.variacao) }}</span>
                </div>
              </div>
              <div class="manual-result-action">
                <span class="manual-result-price">{{ money(item.produto.valorVenda) }}</span>
                <span class="manual-result-badge">+ Adicionar</span>
              </div>
            </button>
          </div>

          <div v-else class="manual-no-results">
            <span>Nenhum produto encontrado para "{{ productSearchTerm }}".</span>
          </div>
        </div>

        <div class="manual-select-section">
          <div class="manual-divider-row">
            <span class="manual-divider-label">Ou selecione na lista completa</span>
          </div>

          <form class="manual-select-form" @submit.prevent="handleManualSelectSubmit">
            <div class="manual-fields-row">
              <div class="manual-field-product">
                <label for="pdv-manual-product" class="manual-label">Selecionar produto</label>
                <select
                  id="pdv-manual-product"
                  v-model="selectedVariationId"
                  class="manual-select"
                  required
                >
                  <option value="">Selecione um produto da lista...</option>
                  <option
                    v-for="item in availableVariations"
                    :key="item.variacao.id"
                    :value="item.variacao.id"
                  >
                    {{ item.produto.nome }} — {{ item.variacao.tamanho }} / {{ item.variacao.cor }} (Estoque: {{ item.variacao.quantidadeEstoque }}) • {{ money(item.produto.valorVenda) }}
                  </option>
                </select>
              </div>

              <div class="manual-field-qty">
                <label for="pdv-manual-qty" class="manual-label">Qtd.</label>
                <input
                  id="pdv-manual-qty"
                  v-model.number="selectedQuantity"
                  type="number"
                  min="1"
                  class="manual-input-qty"
                  required
                />
              </div>
            </div>

            <div class="manual-actions">
              <button
                type="submit"
                class="btn-manual-submit"
                :disabled="!selectedVariationId || loading"
              >
                Adicionar ao carrinho
              </button>
            </div>
          </form>
        </div>
      </div>
    </transition>
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
}

.step-title {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 800;
  color: #25201f;
  letter-spacing: -0.01em;
}

.step-subtitle {
  font-size: 0.8rem;
  color: #736965;
}

.scan-area {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.scan-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.scan-label {
  display: flex;
  flex-direction: column;
  gap: 4px;
  cursor: pointer;
}

.scan-label-text {
  font-size: 0.95rem;
  font-weight: 700;
  color: #25201f;
}

.scan-label-hint {
  font-size: 0.8rem;
  color: #736965;
}

.scan-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  gap: 8px;
}

.barcode-icon {
  position: absolute;
  left: 14px;
  color: #b33f62;
  display: flex;
  align-items: center;
  pointer-events: none;
}

.scan-input {
  flex: 1;
  min-height: 48px;
  padding: 12px 14px 12px 46px;
  font-size: 0.95rem;
  background: #fbf9f8;
  border: 1.5px solid #d8cfca;
  border-radius: 8px;
  color: #25201f;
  transition: all 0.2s ease;
  font-weight: 500;
}

.scan-input:focus {
  outline: none;
  background: #ffffff;
  border-color: #b33f62;
  box-shadow: 0 0 0 3px rgba(179, 63, 98, 0.12);
}

.scan-input::placeholder {
  color: #9c918c;
  font-weight: 400;
}

.btn-scan {
  min-height: 48px;
  padding: 0 18px;
  background: #b33f62;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.9rem;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.btn-scan:hover:not(:disabled) {
  background: #9d3556;
}

.btn-scan:active:not(:disabled) {
  background: #852945;
}

.btn-scan:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.scan-feedback {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
}

.error-feedback {
  background: #fff0f2;
  border: 1px solid #f1bdc8;
  color: #9b1c3f;
}

.success-feedback {
  background: #edf7f1;
  border: 1px solid #bde3cd;
  color: #206e43;
}

.btn-dismiss {
  margin-left: auto;
  background: transparent;
  border: none;
  color: inherit;
  cursor: pointer;
  padding: 2px 6px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: auto;
}

.btn-dismiss:hover {
  background: rgba(0, 0, 0, 0.05);
}

.secondary-divider {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding-top: 14px;
  border-top: 1px solid #eee7e3;
  text-align: center;
}

.divider-text {
  font-size: 0.78rem;
  color: #8b807b;
  font-weight: 500;
}

.btn-toggle-manual {
  background: #faf8f6;
  border: 1px solid #e5ddd8;
  color: #625955;
  border-radius: 20px;
  padding: 6px 14px;
  font-size: 0.82rem;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: auto;
}

.btn-toggle-manual:hover {
  background: #f4f1ee;
  color: #25201f;
  border-color: #d8cfca;
}

.btn-toggle-manual.active {
  background: #f4ecee;
  color: #b33f62;
  border-color: #f1bdc8;
}

.chevron-icon {
  transition: transform 0.2s ease;
}

.chevron-icon.rotated {
  transform: rotate(180deg);
}

.manual-box {
  background: #faf8f6;
  border: 1px solid #e5ddd8;
  border-radius: 10px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.manual-search-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.manual-search-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.manual-search-icon {
  position: absolute;
  left: 12px;
  color: #8b807b;
  display: flex;
  align-items: center;
  pointer-events: none;
}

.manual-search-input {
  width: 100%;
  min-height: 40px;
  padding: 8px 36px 8px 36px;
  border: 1.5px solid #d8cfca;
  border-radius: 6px;
  background: #ffffff;
  color: #25201f;
  font-size: 0.86rem;
  font-weight: 500;
  transition: all 0.2s ease;
}

.manual-search-input:focus {
  outline: none;
  border-color: #b33f62;
  box-shadow: 0 0 0 3px rgba(179, 63, 98, 0.1);
}

.btn-clear-search {
  position: absolute;
  right: 8px;
  background: transparent;
  border: none;
  color: #8b807b;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: auto;
}

.btn-clear-search:hover {
  color: #25201f;
  background: rgba(0, 0, 0, 0.05);
}

.manual-search-hint {
  padding: 10px 14px;
  background: #ffffff;
  border: 1px dashed #d8cfca;
  border-radius: 8px;
  font-size: 0.82rem;
  color: #736965;
}

.manual-results-wrapper {
  display: flex;
  flex-direction: column;
}

.manual-results-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 240px;
  overflow-y: auto;
  padding-right: 2px;
}

.manual-results-list::-webkit-scrollbar {
  width: 5px;
}
.manual-results-list::-webkit-scrollbar-track {
  background: #faf8f6;
}
.manual-results-list::-webkit-scrollbar-thumb {
  background: #d8cfca;
  border-radius: 4px;
}

.manual-result-item {
  width: 100%;
  text-align: left;
  background: #ffffff;
  border: 1.5px solid #e5ddd8;
  border-radius: 8px;
  padding: 10px 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  cursor: pointer;
  transition: all 0.15s ease;
  color: inherit;
  min-height: auto;
}

.manual-result-item:hover:not(:disabled) {
  background: #fbf0f3;
  border-color: #b33f62;
  transform: translateY(-1px);
  box-shadow: 0 3px 10px rgba(179, 63, 98, 0.08);
}

.manual-result-item:active:not(:disabled) {
  transform: translateY(0);
}

.manual-result-item:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.manual-result-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.manual-result-name {
  font-size: 0.88rem;
  font-weight: 700;
  color: #25201f;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.manual-result-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.76rem;
  color: #736965;
  flex-wrap: wrap;
}

.meta-tag {
  background: #faf8f6;
  border: 1px solid #e5ddd8;
  padding: 1px 6px;
  border-radius: 4px;
  font-weight: 600;
  color: #625955;
}

.meta-stock {
  color: #206e43;
  font-weight: 600;
}

.meta-code {
  font-family: monospace;
  color: #8b807b;
  font-size: 0.72rem;
}

.manual-result-action {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
  flex-shrink: 0;
}

.manual-result-price {
  font-size: 0.95rem;
  font-weight: 800;
  color: #25201f;
}

.manual-result-badge {
  font-size: 0.72rem;
  font-weight: 700;
  color: #b33f62;
  background: #fbf0f3;
  border: 1px solid #f1bdc8;
  padding: 2px 8px;
  border-radius: 12px;
  transition: all 0.15s ease;
}

.manual-result-item:hover .manual-result-badge {
  background: #b33f62;
  color: #ffffff;
}

.manual-no-results {
  padding: 14px;
  background: #ffffff;
  border: 1px dashed #d8cfca;
  border-radius: 8px;
  font-size: 0.82rem;
  color: #736965;
  text-align: center;
}

.manual-select-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-top: 10px;
  border-top: 1px solid #eee7e3;
}

.manual-divider-row {
  text-align: center;
}

.manual-divider-label {
  font-size: 0.76rem;
  font-weight: 700;
  color: #8b807b;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.manual-select-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.manual-fields-row {
  display: grid;
  grid-template-columns: 1fr 90px;
  gap: 10px;
}

.manual-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 700;
  color: #625955;
  margin-bottom: 5px;
}

.manual-select {
  width: 100%;
  min-height: 42px;
  padding: 8px 12px;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  background: #ffffff;
  color: #25201f;
  font-size: 0.85rem;
  font-weight: 500;
}

.manual-select:focus {
  outline: none;
  border-color: #b33f62;
}

.manual-input-qty {
  width: 100%;
  min-height: 42px;
  padding: 8px 10px;
  border: 1px solid #d8cfca;
  border-radius: 6px;
  background: #ffffff;
  color: #25201f;
  font-size: 0.88rem;
  text-align: center;
  font-weight: 600;
}

.manual-input-qty:focus {
  outline: none;
  border-color: #b33f62;
}

.manual-actions {
  display: flex;
  justify-content: flex-end;
}

.btn-manual-submit {
  background: #625955;
  border: none;
  color: #ffffff;
  border-radius: 6px;
  padding: 0 16px;
  font-size: 0.84rem;
  font-weight: 700;
  min-height: 38px;
  cursor: pointer;
  transition: background 0.2s ease;
  width: 100%;
}

.btn-manual-submit:hover:not(:disabled) {
  background: #25201f;
}

.btn-manual-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-collapse-enter-active,
.slide-collapse-leave-active {
  transition: all 0.25s ease-out;
  overflow: hidden;
}

.slide-collapse-enter-from,
.slide-collapse-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

@media (max-width: 600px) {
  .step-card {
    padding: 16px;
    gap: 16px;
  }

  .scan-input-wrapper {
    flex-direction: column;
    align-items: stretch;
  }

  .scan-input {
    width: 100%;
  }

  .btn-scan {
    width: 100%;
    justify-content: center;
  }

  .manual-fields-row {
    grid-template-columns: 1fr;
  }

  .manual-result-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .manual-result-action {
    width: 100%;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
  }
}
</style>
