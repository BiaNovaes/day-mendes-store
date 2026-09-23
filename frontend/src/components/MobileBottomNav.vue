<script setup lang="ts">
import type { View } from './Sidebar.vue'

interface NavItem {
  id: View
  label: string
  icon: string
}

defineProps<{
  currentView: View
}>()

const emit = defineEmits<{
  (e: 'navigate', view: View): void
  (e: 'logout'): void
}>()

const navItems: NavItem[] = [
  { id: 'dashboard', label: 'Painel', icon: 'dashboard' },
  { id: 'produtos', label: 'Produtos', icon: 'produtos' },
  { id: 'clientes', label: 'Clientes', icon: 'clientes' },
  { id: 'pdv', label: 'PDV', icon: 'pdv' },
  { id: 'vendas', label: 'Vendas', icon: 'vendas' },
  { id: 'relatorios', label: 'Relatórios', icon: 'relatorios' },
]
</script>

<template>
  <nav class="mobile-bottom-nav" aria-label="Navegação móvel">
    <div class="nav-container">
      <button
        v-for="item in navItems"
        :key="item.id"
        type="button"
        class="nav-tab"
        :class="{ active: currentView === item.id }"
        :aria-current="currentView === item.id ? 'page' : undefined"
        @click="emit('navigate', item.id)"
      >
        <span class="nav-icon-wrapper">
          <svg v-if="item.icon === 'dashboard'" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="7" height="7" rx="1.5" />
            <rect x="14" y="3" width="7" height="7" rx="1.5" />
            <rect x="14" y="14" width="7" height="7" rx="1.5" />
            <rect x="3" y="14" width="7" height="7" rx="1.5" />
          </svg>
          <svg v-else-if="item.icon === 'produtos'" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z" />
            <line x1="3" y1="6" x2="21" y2="6" />
            <path d="M16 10a4 4 0 0 1-8 0" />
          </svg>
          <svg v-else-if="item.icon === 'clientes'" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
          </svg>
          <svg v-else-if="item.icon === 'pdv'" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="16" rx="2" />
            <line x1="7" y1="8" x2="7" y2="12" />
            <line x1="10" y1="8" x2="10" y2="12" />
            <line x1="14" y1="8" x2="14" y2="12" />
            <line x1="17" y1="8" x2="17" y2="12" />
            <line x1="7" y1="16" x2="17" y2="16" />
          </svg>
          <svg v-else-if="item.icon === 'vendas'" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="12" y1="1" x2="12" y2="23" />
            <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
          </svg>
          <svg v-else-if="item.icon === 'relatorios'" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
            <polyline points="14 2 14 8 20 8" />
            <line x1="16" y1="13" x2="8" y2="13" />
            <line x1="16" y1="17" x2="8" y2="17" />
            <polyline points="10 9 9 9 8 9" />
          </svg>
        </span>
        <span class="nav-label">{{ item.label }}</span>
      </button>

      <button
        type="button"
        class="nav-tab nav-tab-logout"
        aria-label="Sair do sistema"
        @click="emit('logout')"
      >
        <span class="nav-icon-wrapper">
          <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
            <polyline points="16 17 21 12 16 7" />
            <line x1="21" y1="12" x2="9" y2="12" />
          </svg>
        </span>
        <span class="nav-label">Sair</span>
      </button>
    </div>
  </nav>
</template>

<style scoped>
.mobile-bottom-nav {
  display: none;
}

@media (max-width: 768px) {
  .mobile-bottom-nav {
    display: block;
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    width: 100%;
    background: #ffffff;
    border-top: 1px solid #f0e4e6;
    box-shadow: 0 -4px 20px rgba(48, 35, 30, 0.08);
    z-index: 1000;
    padding-bottom: env(safe-area-inset-bottom, 0px);
    user-select: none;
    -webkit-tap-highlight-color: transparent;
  }

  .nav-container {
    display: flex;
    align-items: center;
    justify-content: space-around;
    width: 100%;
    padding: 6px 4px 6px;
    gap: 2px;
  }

  .nav-tab {
    flex: 1 1 0;
    min-width: 0;
    min-height: 52px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 3px;
    padding: 4px 2px;
    background: transparent;
    border: none;
    border-radius: 8px;
    color: #736965;
    cursor: pointer;
    transition: all 0.16s ease;
    touch-action: manipulation;
  }

  .nav-icon-wrapper {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 34px;
    height: 26px;
    border-radius: 13px;
    color: inherit;
    transition: all 0.16s ease;
  }

  .nav-label {
    font-size: 0.64rem;
    font-weight: 600;
    line-height: 1.1;
    color: inherit;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 100%;
    letter-spacing: -0.2px;
    transition: color 0.16s ease, font-weight 0.16s ease;
  }

  .nav-tab.active {
    color: #da5c81;
  }

  .nav-tab.active .nav-icon-wrapper {
    background-color: #fbeff1;
    color: #da5c81;
  }

  .nav-tab.active .nav-label {
    color: #da5c81;
    font-weight: 800;
  }

  .nav-tab:active {
    transform: scale(0.96);
  }

  .nav-tab-logout {
    color: #8b807b;
  }

  .nav-tab-logout:active {
    color: #b33f62;
  }

  .nav-tab-logout:active .nav-icon-wrapper {
    background-color: #fbeff1;
    color: #b33f62;
  }
}

@media (max-width: 360px) {
  .nav-tab {
    min-height: 48px;
    padding: 3px 1px;
    gap: 2px;
  }

  .nav-icon-wrapper {
    width: 30px;
    height: 24px;
  }

  .nav-icon-wrapper svg {
    width: 17px;
    height: 17px;
  }

  .nav-label {
    font-size: 0.58rem;
    letter-spacing: -0.4px;
  }
}
</style>
