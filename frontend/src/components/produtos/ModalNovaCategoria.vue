<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { api } from '../../api'

const props = defineProps<{
  open: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
}>()

const form = reactive({
  nome: '',
  descricao: '',
})

const loading = ref(false)
const error = ref('')

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      form.nome = ''
      form.descricao = ''
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
  if (!form.nome.trim()) return
  loading.value = true
  error.value = ''

  try {
    await api.salvarCategoria({
      nome: form.nome.trim(),
      descricao: form.descricao.trim() || undefined,
    })
    emit('success')
    emit('close')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erro ao cadastrar categoria.'
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
      aria-labelledby="modal-categoria-title"
      @click="handleBackdropClick"
      @keydown="handleKeydown"
    >
      <div class="modal-card">
        <header class="modal-header">
          <div>
            <h2 id="modal-categoria-title" class="modal-title">Nova categoria</h2>
            <p class="modal-subtitle">Organize seus produtos por categorias</p>
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
          <label class="form-label">
            <span>Nome da categoria <strong class="req">*</strong></span>
            <input
              v-model="form.nome"
              type="text"
              class="form-input"
              placeholder="Ex: Vestidos, Blusas, Calças..."
              required
              autofocus
            />
          </label>

          <label class="form-label">
            <span>Descrição <small class="opt">(opcional)</small></span>
            <input
              v-model="form.descricao"
              type="text"
              class="form-input"
              placeholder="Breve descrição da categoria"
            />
          </label>

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
              :disabled="loading || !form.nome.trim()"
            >
              {{ loading ? 'Salvando...' : 'Cadastrar categoria' }}
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
  max-width: 480px;
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
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 0.86rem;
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
  min-height: 42px;
  padding: 10px 12px;
  font-size: 0.9rem;
}

.form-input:focus {
  outline: none;
  border-color: #b33f62;
  box-shadow: 0 0 0 3px rgba(179, 63, 98, 0.1);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 8px;
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
</style>
