<script setup lang="ts">
import { reactive, ref } from 'vue'
import { api, setAuthToken, type Loja } from '../../api'

const emit = defineEmits<{
  (e: 'login-success', loja: Loja): void
}>()

const isSetup = ref(false)
const loading = ref(false)
const error = ref('')

const loginForm = reactive({
  email: '',
  senha: '',
})

const setupForm = reactive({
  nome: 'Day Mendes Store',
  email: '',
  senha: '',
  cnpj: '',
})

function getErrorMessage(err: unknown, fallback: string): string {
  if (err instanceof Error) {
    const raw = err.message
    try {
      const parsed = JSON.parse(raw)
      if (parsed && typeof parsed === 'object') {
        if (typeof parsed.message === 'string' && parsed.message.trim()) {
          return parsed.message.trim()
        }
        if (typeof parsed.detail === 'string' && parsed.detail.trim()) {
          return parsed.detail.trim()
        }
      }
    } catch {
    }
    return raw || fallback
  }
  return fallback
}

async function handleLogin() {
  loading.value = true
  error.value = ''
  try {
    const response = await api.login(loginForm.email, loginForm.senha)
    setAuthToken(response.token)
    emit('login-success', response.loja)
  } catch (err) {
    error.value = getErrorMessage(err, 'E-mail ou senha inválidos.')
  } finally {
    loading.value = false
  }
}

async function handleSetup() {
  loading.value = true
  error.value = ''
  try {
    await api.setupLoja(setupForm)
    const response = await api.login(setupForm.email, setupForm.senha)
    setAuthToken(response.token)
    emit('login-success', response.loja)
  } catch (err) {
    error.value = getErrorMessage(err, 'Não foi possível configurar a loja.')
  } finally {
    loading.value = false
  }
}

function toggleMode(toSetup: boolean) {
  isSetup.value = toSetup
  error.value = ''
}
</script>

<template>
  <section class="form-container">
    <div class="form-card">
      <div class="form-header">
        <h1 class="title">{{ isSetup ? 'Configurar loja' : 'Bem-vinda!' }}</h1>
        <p class="subtitle">
          {{
            isSetup
              ? 'Informe os dados da sua loja para iniciar o sistema.'
              : 'Entre na sua conta para acessar sua loja.'
          }}
        </p>
      </div>

      <div v-if="error" class="alert-box error" role="alert">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="8" x2="12" y2="12" />
          <line x1="12" y1="16" x2="12.01" y2="16" />
        </svg>
        <span>{{ error }}</span>
      </div>

      <form v-if="!isSetup" class="fields-stack" @submit.prevent="handleLogin">
        <div class="input-group">
          <label for="login-email">E-mail</label>
          <input
            id="login-email"
            v-model="loginForm.email"
            type="email"
            placeholder="seuemail@exemplo.com"
            required
            autocomplete="email"
          />
        </div>

        <div class="input-group">
          <label for="login-senha">Senha</label>
          <input
            id="login-senha"
            v-model="loginForm.senha"
            type="password"
            placeholder="Sua senha de acesso"
            required
            autocomplete="current-password"
          />
        </div>

        <button type="submit" class="btn-primary" :disabled="loading">
          <span v-if="loading" class="spinner"></span>
          <span>{{ loading ? 'Entrando...' : 'Entrar' }}</span>
        </button>

        <div class="footer-action">
          <button type="button" class="btn-link" @click="toggleMode(true)">
            Configurar loja
          </button>
        </div>
      </form>

      <form v-else class="fields-stack" @submit.prevent="handleSetup">
        <div class="input-group">
          <label for="setup-nome">Nome da loja</label>
          <input
            id="setup-nome"
            v-model="setupForm.nome"
            type="text"
            placeholder="Nome do seu negócio"
            required
          />
        </div>

        <div class="input-group">
          <label for="setup-email">E-mail</label>
          <input
            id="setup-email"
            v-model="setupForm.email"
            type="email"
            placeholder="seuemail@exemplo.com"
            required
          />
        </div>

        <div class="input-group">
          <label for="setup-senha">Senha</label>
          <input
            id="setup-senha"
            v-model="setupForm.senha"
            type="password"
            placeholder="Crie uma senha segura"
            required
          />
        </div>

        <div class="input-group">
          <label for="setup-cnpj">CNPJ <small class="optional">(opcional)</small></label>
          <input
            id="setup-cnpj"
            v-model="setupForm.cnpj"
            type="text"
            placeholder="00.000.000/0000-00"
          />
        </div>

        <button type="submit" class="btn-primary" :disabled="loading">
          <span v-if="loading" class="spinner"></span>
          <span>{{ loading ? 'Salvando...' : 'Salvar e entrar' }}</span>
        </button>

        <div class="footer-action">
          <button type="button" class="btn-link" @click="toggleMode(false)">
            Voltar para o login
          </button>
        </div>
      </form>
    </div>
  </section>
</template>

<style scoped>
.form-container {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 32px;
  background: #ffffff;
}

.form-card {
  width: 100%;
  max-width: 420px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-header {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.eyebrow-pill {
  align-self: flex-start;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.8px;
  background: #fdf2f4;
  color: #da5c81;
}

.title {
  margin: 0;
  font-size: 1.85rem;
  font-weight: 800;
  color: #25201f;
  letter-spacing: -0.5px;
}

.subtitle {
  margin: 0;
  font-size: 0.94rem;
  color: #756a65;
  line-height: 1.45;
}

.alert-box {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  border-radius: 10px;
  font-size: 0.88rem;
  font-weight: 500;
}

.alert-box.error {
  background: #fff0f2;
  border: 1px solid #f8c9d2;
  color: #9d1c3e;
}

.alert-box svg {
  flex-shrink: 0;
}

.fields-stack {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.input-group label {
  font-size: 0.86rem;
  font-weight: 700;
  color: #3b3330;
  display: flex;
  align-items: center;
  gap: 4px;
}

.optional {
  font-size: 0.76rem;
  font-weight: 400;
  color: #8b807b;
}

.input-group input {
  width: 100%;
  min-height: 44px;
  padding: 10px 14px;
  font-size: 0.92rem;
  color: #25201f;
  background: #ffffff;
  border: 1.5px solid #dcd4cf;
  border-radius: 10px;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

.input-group input:hover {
  border-color: #c2b7b0;
}

.input-group input:focus {
  outline: none;
  border-color: #da5c81;
  box-shadow: 0 0 0 3.5px rgba(218, 92, 129, 0.14);
}

.btn-primary {
  width: 100%;
  min-height: 46px;
  margin-top: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  background: #b33f62;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s ease;
  box-shadow: 0 4px 14px rgba(179, 63, 98, 0.25);
}

.btn-primary:hover:not(:disabled) {
  background: #9d3556;
  transform: translateY(-1px);
  box-shadow: 0 6px 18px rgba(179, 63, 98, 0.32);
}

.btn-primary:active:not(:disabled) {
  transform: translateY(0);
}

.btn-primary:disabled {
  opacity: 0.65;
  cursor: not-allowed;
  transform: none;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.35);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.footer-action {
  display: flex;
  justify-content: center;
  margin-top: 4px;
}

.btn-link {
  background: none;
  border: none;
  padding: 6px 10px;
  color: #756a65;
  font-size: 0.86rem;
  font-weight: 600;
  cursor: pointer;
  border-radius: 6px;
  transition: all 0.15s ease;
}

.btn-link:hover {
  color: #b33f62;
  background: #fdf2f4;
}

@media (max-width: 900px) {
  .form-container {
    padding: 32px 20px 48px;
  }
  .title {
    font-size: 1.6rem;
  }
}
</style>
