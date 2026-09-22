export const currency = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

export function formatMoney(val?: number): string {
  return currency.format(val ?? 0)
}

export function formatFriendlyDate(dateStr?: string): string {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return dateStr

  const now = new Date()
  const isToday =
    date.getDate() === now.getDate() &&
    date.getMonth() === now.getMonth() &&
    date.getFullYear() === now.getFullYear()

  const yesterday = new Date(now)
  yesterday.setDate(now.getDate() - 1)
  const isYesterday =
    date.getDate() === yesterday.getDate() &&
    date.getMonth() === yesterday.getMonth() &&
    date.getFullYear() === yesterday.getFullYear()

  const time = date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })

  if (isToday) return `Hoje, ${time}`
  if (isYesterday) return `Ontem, ${time}`

  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()
  return `${day}/${month}/${year}, ${time}`
}

export function formatFullDate(dateStr?: string): string {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return dateStr

  return date.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export interface StatusConfig {
  label: string
  badgeClass: string
  bg: string
  color: string
  dotColor: string
}

export function getStatusConfig(status: number): StatusConfig {
  switch (status) {
    case 2:
      return {
        label: 'Finalizada',
        badgeClass: 'status-finalizada',
        bg: '#e6f7ef',
        color: '#065f46',
        dotColor: '#10b981',
      }
    case 3:
      return {
        label: 'Cancelada',
        badgeClass: 'status-cancelada',
        bg: '#fee2e2',
        color: '#991b1b',
        dotColor: '#ef4444',
      }
    case 1:
      return {
        label: 'Rascunho',
        badgeClass: 'status-rascunho',
        bg: '#fef3c7',
        color: '#92400e',
        dotColor: '#f59e0b',
      }
    default:
      return {
        label: 'Pendente',
        badgeClass: 'status-pendente',
        bg: '#f3f4f6',
        color: '#4b5563',
        dotColor: '#9ca3af',
      }
  }
}

export interface PaymentConfig {
  label: string
  bg: string
  color: string
}

export function getPaymentConfig(forma?: string): PaymentConfig {
  const norm = (forma ?? '').trim().toLowerCase()
  if (norm.includes('pix')) {
    return {
      label: 'Pix',
      bg: '#fdf2f5',
      color: '#b33f62',
    }
  }
  if (norm.includes('debito') || norm.includes('débito')) {
    return {
      label: 'Débito',
      bg: '#f4f1ee',
      color: '#49403d',
    }
  }
  if (norm.includes('credito') || norm.includes('crédito')) {
    return {
      label: 'Crédito',
      bg: '#f4f1ee',
      color: '#49403d',
    }
  }
  if (norm.includes('dinheiro')) {
    return {
      label: 'Dinheiro',
      bg: '#e6f7ef',
      color: '#065f46',
    }
  }
  return {
    label: forma || 'Outro',
    bg: '#f4f1ee',
    color: '#49403d',
  }
}

