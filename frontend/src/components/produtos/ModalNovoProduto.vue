<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { api, type Categoria } from '../../api'

const props = defineProps<{
  open: boolean
  categorias: Categoria[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
}>()

const form = reactive({
  nome: '',
  categoriaId: '',
  marca: '',
  descricao: '',
  valorCompra: '',
  valorVenda: '',
  estoqueMinimo: '1',
  tamanho: '',
  cor: '',
  quantidadeEstoque: '0',
})

const loading = ref(false)
const error = ref('')

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      Object.assign(form, {
        nome: '',
        categoriaId: '',
        marca: '',
        descricao: '',
        valorCompra: '',
        valorVenda: '',
        estoqueMinimo: '1',
        tamanho: '',
        cor: '',
        quantidadeEstoque: '0',
      })
      error.value = ''
      loading.value = false
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

async function handleSubmit() {
  if (!form.nome.trim() || !form.categoriaId) return
  loading.value = true
  error.value = ''

  try {
    await api.salvarProduto({
      categoriaId: Number(form.categoriaId),
      nome: form.nome.trim(),
      marca: form.marca.trim() || null,
      descricao: form.descricao.trim() || null,
      valorCompra: Number(form.valorCompra) || 0,
      valorVenda: Number(form.valorVenda) || 0,
      estoqueMinimo: Number(form.estoqueMinimo) || 0,
      variacoes: [
        {
          tamanho: form.tamanho.trim() || 'U',
          cor: form.cor.trim() || 'Padrão',
          quantidadeEstoque: Number(form.quantidadeEstoque) || 0,
        },
      ],
    })
    emit('success')
    emit('close')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erro ao cadastrar produto.'
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
      aria-labelledby="modal-produto-title"
      @click="handleBackdropClick"
      @keydown="handleKeydown"
    >
      <div class="modal-card">
        <header class="modal-header">
          <div>
            <h2 id="modal-produto-title" class="modal-title">Novo produto</h2>
            <p class="modal-subtitle">Preencha as informações para cadastrar no catálogo</p>
          </div>
          <button
            type="button"
            class="btn-close"
            title="Fechar"
            aria-label="Fechar"
            @click="emit('close')"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </header>

        <div v-if="error" class="alert-box alert-error">
          <span>{{ error }}</span>
        </div>

        <form class="modal-body" @submit.prevent="handleSubmit">
          <div class="form-row">
            <label class="form-label flex-2">
              <span>Nome do produto <strong class="req">*</strong></span>
              <input
                v-model="form.nome"
                type="text"
                class="form-input"
                placeholder="Ex: Vestido Midi Canelado"
                required
                autofocus
              />
            </label>

            <label class="form-label flex-1">
              <span>Categoria <strong class="req">*</strong></span>
              <select v-model="form.categoriaId" class="form-input" required>
                <option value="">Selecione</option>
                <option v-for="categoria in categorias" :key="categoria.id" :value="categoria.id">
                  {{ categoria.nome }}
                </option>
              </select>
            </label>
          </div>

          <label class="form-label">
            <span>Marca <small class="opt">(opcional)</small></span>
            <input
              v-model="form.marca"
              type="text"
              class="form-input"
              placeholder="Ex: Day Mendes"
            />
          </label>

          <label class="form-label">
            <span>Descrição <small class="opt">(opcional)</small></span>
            <textarea
              v-model="form.descricao"
              class="form-input form-textarea"
              placeholder="Detalhes sobre o produto, caimento, tecido..."
            ></textarea>
          </label>

          <div class="section-divider">
            <span class="section-title">Valores e estoque</span>
          </div>

          <div class="form-row three-cols">
            <label class="form-label">
              <span>Valor de compra <strong class="req">*</strong></span>
              <input
                v-model="form.valorCompra"
                type="number"
                step="0.01"
                min="0"
                class="form-input"
                placeholder="0,00"
                required
              />
            </label>

            <label class="form-label">
              <span>Valor de venda <strong class="req">*</strong></span>
              <input
                v-model="form.valorVenda"
                type="number"
                step="0.01"
                min="0"
                class="form-input"
                placeholder="0,00"
                required
              />
            </label>

            <label class="form-label">
              <span>Estoque mínimo <strong class="req">*</strong></span>
              <input
                v-model="form.estoqueMinimo"
                type="number"
                min="0"
                class="form-input"
                placeholder="1"
                required
              />
            </label>
          </div>

          <div class="section-divider">
            <span class="section-title">Variação inicial</span>
          </div>

          <div class="form-row three-cols">
            <label class="form-label">
              <span>Tamanho <strong class="req">*</strong></span>
              <input
                v-model="form.tamanho"
                type="text"
                class="form-input"
                placeholder="Ex: M, Único, 38..."
                required
              />
            </label>

            <label class="form-label">
              <span>Cor <strong class="req">*</strong></span>
              <input
                v-model="form.cor"
                type="text"
                class="form-input"
                placeholder="Ex: Preto, Marsala..."
                required
              />
            </label>

            <label class="form-label">
              <span>Qtd. em estoque <strong class="req">*</strong></span>
              <input
                v-model="form.quantidadeEstoque"
                type="number"
                min="0"
                class="form-input"
                placeholder="0"
                required
              />
            </label>
          </div>

          <footer class="modal-footer">
            <button
              type="button"
              class="btn btn-secondary"
              @click="emit('close')"
              :disabled="loading"
            >
              Cancelar
            </button>
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="loading || !form.nome.trim() || !form.categoriaId"
            >
              {{ loading ? 'Salvando...' : 'Cadastrar produto' }}
            </button>
          </footer>
        </form>
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
  max-width: 620px;
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

.modal-title {
  margin: 0;
  font-size: 1.2rem;
  font-weight: 800;
  color: #25201f;
}

.modal-subtitle {
  margin: 2px 0 0;
  font-size: 0.8rem;
  color: #736965;
}

.btn-close {
  background: transparent;
  border: none;
  color: #8b807b;
  cursor: pointer;
  padding: 4px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
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
  gap: 14px;
}

.form-row {
  display: flex;
  gap: 12px;
}

.flex-1 {
  flex: 1;
}

.flex-2 {
  flex: 2;
}

.three-cols {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 12px;
}

.form-label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 0.84rem;
  font-weight: 700;
  color: #625955;
}

.req {
  color: #b33f62;
}

.opt {
  font-weight: 400;
  color: #8b807b;
}

.form-input {
  width: 100%;
  border: 1px solid #d8cfca;
  border-radius: 8px;
  background: #ffffff;
  color: #25201f;
  min-height: 40px;
  padding: 8px 12px;
  font-size: 0.88rem;
}

.form-input:focus {
  outline: none;
  border-color: #b33f62;
  box-shadow: 0 0 0 3px rgba(179, 63, 98, 0.1);
}

.form-textarea {
  min-height: 72px;
  resize: vertical;
}

.section-divider {
  display: flex;
  align-items: center;
  margin: 4px 0 0;
  padding-bottom: 6px;
  border-bottom: 1px solid #eee7e3;
}

.section-title {
  font-size: 0.78rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: #8b807b;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 6px;
  padding-top: 16px;
  border-top: 1px solid #eee7e3;
}

.btn {
  min-height: 42px;
  padding: 0 18px;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 800;
  cursor: pointer;
  border: none;
}

.btn-secondary {
  background: #eee7e3;
  color: #625955;
}

.btn-secondary:hover:not(:disabled) {
  background: #e2dbd7;
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

@media (max-width: 560px) {
  .form-row,
  .three-cols {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }
}
</style>
