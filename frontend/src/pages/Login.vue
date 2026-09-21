<script setup lang="ts">
import type { Loja } from '../api'
import LoginBranding from '../components/login/LoginBranding.vue'
import LoginForm from '../components/login/LoginForm.vue'

const emit = defineEmits<{
  (e: 'success', loja: Loja): void
}>()
</script>

<template>
  <main class="login-wrapper">
    <div class="login-grid">
      <LoginBranding class="branding-side" />
      <LoginForm class="form-side" @login-success="(loja) => emit('success', loja)" />
    </div>
  </main>
</template>

<style scoped>
.login-wrapper {
  min-height: 100vh;
  width: 100%;
  background: #fdfaf9;
  display: flex;
}

.login-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  width: 100%;
  min-height: 100vh;
}

.branding-side {
  flex: 1;
}

.form-side {
  flex: 1;
}

@media (max-width: 900px) {
  .login-grid {
    grid-template-columns: 1fr;
    min-height: 100vh;
  }
}
</style>
