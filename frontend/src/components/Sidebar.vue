<script setup lang="ts">
export type View = 'dashboard' | 'produtos' | 'clientes' | 'pdv' | 'vendas' | 'relatorios'

interface MenuItem {
  id: View
  label: string
  badge?: string
  icon: string
}

defineProps<{
  currentView: View
  lojaNome: string
  logoUrl?: string
}>()

const emit = defineEmits<{
  (e: 'navigate', view: View): void
  (e: 'logout'): void
}>()

const menuItems: MenuItem[] = [
  { id: 'dashboard', label: 'Painel', icon: 'dashboard' },
  { id: 'produtos', label: 'Produtos', icon: 'produtos' },
  { id: 'clientes', label: 'Clientes', icon: 'clientes' },
  { id: 'pdv', label: 'PDV', icon: 'pdv' },
  { id: 'vendas', label: 'Vendas', icon: 'vendas' },
  { id: 'relatorios', label: 'Relatórios', badge: 'PDF', icon: 'relatorios' },
]
</script>

<template>
  <aside class="app-sidebar">
    <div class="sidebar-brand">
      <div class="logo-wrapper">
        <img :src="logoUrl || '/logo.png'" :alt="lojaNome" />
      </div>
      <div class="brand-text">
        <h2 class="store-name" :title="lojaNome">{{ lojaNome }}</h2>
        <span class="system-status">
          <span class="status-dot"></span>
          Sistema de gestão
        </span>
      </div>
    </div>

    <nav class="sidebar-nav">
      <p class="nav-section-title">Navegação</p>
      <button
        v-for="item in menuItems"
        :key="item.id"
        type="button"
        class="nav-item"
        :class="{ active: currentView === item.id }"
        @click="emit('navigate', item.id)"
      >
        <span class="nav-icon">
          <svg v-if="item.icon === 'dashboard'" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="7" height="7" rx="1.5" />
            <rect x="14" y="3" width="7" height="7" rx="1.5" />
            <rect x="14" y="14" width="7" height="7" rx="1.5" />
            <rect x="3" y="14" width="7" height="7" rx="1.5" />
          </svg>
          <svg v-else-if="item.icon === 'produtos'" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z" />
            <line x1="3" y1="6" x2="21" y2="6" />
            <path d="M16 10a4 4 0 0 1-8 0" />
          </svg>
          <svg v-else-if="item.icon === 'clientes'" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
            <circle cx="9" cy="7" r="4" />
            <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
            <path d="M16 3.13a4 4 0 0 1 0 7.75" />
          </svg>
          <svg v-else-if="item.icon === 'pdv'" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="16" rx="2" />
            <line x1="7" y1="8" x2="7" y2="12" />
            <line x1="10" y1="8" x2="10" y2="12" />
            <line x1="14" y1="8" x2="14" y2="12" />
            <line x1="17" y1="8" x2="17" y2="12" />
            <line x1="7" y1="16" x2="17" y2="16" />
          </svg>
          <svg v-else-if="item.icon === 'vendas'" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="12" y1="1" x2="12" y2="23" />
            <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
          </svg>
          <svg v-else-if="item.icon === 'relatorios'" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
            <polyline points="14 2 14 8 20 8" />
            <line x1="16" y1="13" x2="8" y2="13" />
            <line x1="16" y1="17" x2="8" y2="17" />
            <polyline points="10 9 9 9 8 9" />
          </svg>
        </span>

        <span class="nav-label">{{ item.label }}</span>

        <span v-if="item.badge" class="nav-badge">{{ item.badge }}</span>
      </button>
    </nav>

    <div class="sidebar-footer">
      <button type="button" class="btn-logout" @click="emit('logout')">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
          <polyline points="16 17 21 12 16 7" />
          <line x1="21" y1="12" x2="9" y2="12" />
        </svg>
        <span>Encerrar sessão</span>
      </button>
    </div>
  </aside>
</template>

<style scoped>
.app-sidebar {
  position: sticky;
  top: 0;
  height: 100vh;
  width: 260px;
  min-width: 260px;
  background: linear-gradient(180deg, #241e1d 0%, #1a1615 100%);
  color: #f7f3f1;
  display: flex;
  flex-direction: column;
  border-right: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow: 2px 0 16px rgba(0, 0, 0, 0.12);
  z-index: 50;
  user-select: none;
}

.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 22px 20px 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.logo-wrapper {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: #ffffff;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
}

.logo-wrapper img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.brand-text {
  display: flex;
  flex-direction: column;
  gap: 3px;
  min-width: 0;
}

.store-name {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 800;
  color: #ffffff;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  letter-spacing: -0.2px;
}

.system-status {
  font-size: 0.72rem;
  color: rgba(255, 255, 255, 0.55);
  display: flex;
  align-items: center;
  gap: 6px;
}

.status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background-color: #10b981;
  display: inline-block;
  box-shadow: 0 0 6px rgba(16, 185, 129, 0.6);
}

.sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 20px 14px;
  overflow-y: auto;
}

.nav-section-title {
  margin: 0 0 8px 10px;
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.8px;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.35);
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  min-height: 42px;
  padding: 10px 14px;
  border: 1px solid transparent;
  border-radius: 8px;
  background: transparent;
  color: rgba(255, 255, 255, 0.78);
  font-size: 0.88rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.16s ease;
  text-align: left;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.06);
  color: #ffffff;
  border-color: rgba(255, 255, 255, 0.08);
}

.nav-item.active {
  background: #b33f62;
  color: #ffffff;
  box-shadow: 0 4px 14px rgba(179, 63, 98, 0.35);
  font-weight: 700;
}

.nav-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  flex-shrink: 0;
  color: inherit;
}

.nav-label {
  flex: 1;
}

.nav-badge {
  font-size: 0.65rem;
  font-weight: 800;
  padding: 2px 6px;
  border-radius: 4px;
  background: rgba(255, 255, 255, 0.2);
  color: #ffffff;
  letter-spacing: 0.4px;
}

.nav-item.active .nav-badge {
  background: rgba(255, 255, 255, 0.28);
}

.sidebar-footer {
  padding: 16px 14px 20px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.btn-logout {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  width: 100%;
  min-height: 38px;
  padding: 8px 14px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.16s ease;
}

.btn-logout:hover {
  background: rgba(239, 68, 68, 0.12);
  color: #fca5a5;
  border-color: rgba(239, 68, 68, 0.3);
}

@media (max-width: 768px) {
  .app-sidebar {
    display: none !important;
  }
}
</style>
