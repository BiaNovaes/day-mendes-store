<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { api, getProductImageUrl, type Categoria, type Produto } from '../../api'

const props = defineProps<{
  open: boolean
  categorias: Categoria[]
  produto?: Produto | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success'): void
  (e: 'delete', produto: Produto): void
}>()

const isEditing = computed(() => Boolean(props.produto && props.produto.id))

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
const imageError = ref('')

const fileInputRef = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const previewUrl = ref('')
const currentFoto = ref('')
const removeFoto = ref(false)

function cleanupPreview() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = ''
  }
}

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      cleanupPreview()
      selectedFile.value = null
      removeFoto.value = false
      imageError.value = ''
      error.value = ''
      loading.value = false

      if (props.produto) {
        Object.assign(form, {
          nome: props.produto.nome ?? '',
          categoriaId: props.produto.categoriaId ? String(props.produto.categoriaId) : '',
          marca: props.produto.marca ?? '',
          descricao: props.produto.descricao ?? '',
          valorCompra: props.produto.valorCompra !== undefined ? String(props.produto.valorCompra) : '',
          valorVenda: props.produto.valorVenda !== undefined ? String(props.produto.valorVenda) : '',
          estoqueMinimo: props.produto.estoqueMinimo !== undefined ? String(props.produto.estoqueMinimo) : '1',
          tamanho: '',
          cor: '',
          quantidadeEstoque: '0',
        })
        currentFoto.value = props.produto.foto ?? ''
      } else {
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
        currentFoto.value = ''
      }
    } else {
      cleanupPreview()
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

function triggerFileInput() {
  fileInputRef.value?.click()
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function handleFileChange(event: Event) {
  imageError.value = ''
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return

  const validExtensions = ['jpg', 'jpeg', 'png', 'webp']
  const ext = file.name.split('.').pop()?.toLowerCase() ?? ''
  const validMimes = ['image/jpeg', 'image/png', 'image/webp']

  if (!validMimes.includes(file.type) && !validExtensions.includes(ext)) {
    imageError.value = 'Formato inválido. Formatos permitidos: JPG, PNG ou WEBP.'
    input.value = ''
    return
  }

  const maxSizeInBytes = 10 * 1024 * 1024
  if (file.size > maxSizeInBytes) {
    imageError.value = 'A imagem é muito grande. O tamanho máximo permitido é 10 MB.'
    input.value = ''
    return
  }

  cleanupPreview()
  selectedFile.value = file
  previewUrl.value = URL.createObjectURL(file)
  removeFoto.value = false
  input.value = ''
}

function handleRemoveSelectedImage() {
  cleanupPreview()
  selectedFile.value = null
  imageError.value = ''
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
  }
}

function handleRemoveExistingImage() {
  handleRemoveSelectedImage()
  removeFoto.value = true
}

function handleUndoRemoveExistingImage() {
  removeFoto.value = false
}

async function handleSubmit() {
  if (!form.nome.trim() || !form.categoriaId) return
  loading.value = true
  error.value = ''
  imageError.value = ''

  try {
    let fotoUrl: string | null = null

    if (selectedFile.value) {
      const uploadRes = await api.uploadFoto(selectedFile.value)
      fotoUrl = uploadRes.url
    } else if (removeFoto.value) {
      fotoUrl = ''
    } else if (currentFoto.value) {
      fotoUrl = currentFoto.value
    }

    if (isEditing.value && props.produto) {
      await api.atualizarProduto(props.produto.id, {
        categoriaId: Number(form.categoriaId),
        nome: form.nome.trim(),
        marca: form.marca.trim() || null,
        descricao: form.descricao.trim() || null,
        valorCompra: Number(form.valorCompra) || 0,
        valorVenda: Number(form.valorVenda) || 0,
        estoqueMinimo: Number(form.estoqueMinimo) || 0,
        foto: fotoUrl,
      })
    } else {
      await api.salvarProduto({
        categoriaId: Number(form.categoriaId),
        nome: form.nome.trim(),
        marca: form.marca.trim() || null,
        descricao: form.descricao.trim() || null,
        valorCompra: Number(form.valorCompra) || 0,
        valorVenda: Number(form.valorVenda) || 0,
        estoqueMinimo: Number(form.estoqueMinimo) || 0,
        foto: fotoUrl || null,
        variacoes: [
          {
            tamanho: form.tamanho.trim() || 'U',
            cor: form.cor.trim() || 'Padrão',
            quantidadeEstoque: Number(form.quantidadeEstoque) || 0,
          },
        ],
      })
    }

    emit('success')
    emit('close')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Erro ao salvar produto.'
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
            <h2 id="modal-produto-title" class="modal-title">
              {{ isEditing ? 'Editar produto' : 'Novo produto' }}
            </h2>
            <p class="modal-subtitle">
              {{ isEditing ? 'Atualize as informações e a imagem do catálogo' : 'Preencha as informações para cadastrar no catálogo' }}
            </p>
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
            <span class="section-title">Imagem do produto</span>
          </div>

          <div class="image-upload-area">
            <input
              ref="fileInputRef"
              type="file"
              accept=".jpg,.jpeg,.png,.webp"
              class="hidden-file-input"
              @change="handleFileChange"
            />

            <div v-if="previewUrl" class="image-card image-preview-card">
              <div class="image-thumb-box">
                <img :src="previewUrl" alt="Prévia da nova imagem" class="image-thumb-img" />
              </div>
              <div class="image-meta-box">
                <span class="image-filename" :title="selectedFile?.name">
                  {{ selectedFile?.name }}
                </span>
                <span class="image-subtext">
                  <span class="tag-pending">Nova imagem</span>
                  <span v-if="selectedFile">{{ formatFileSize(selectedFile.size) }}</span>
                </span>
              </div>
              <div class="image-actions-box">
                <button
                  type="button"
                  class="btn-text-danger"
                  title="Remover imagem selecionada"
                  @click="handleRemoveSelectedImage"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="3 6 5 6 21 6" />
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                  </svg>
                  <span>Remover</span>
                </button>
              </div>
            </div>

            <div v-else-if="removeFoto" class="image-card image-removed-card">
              <div class="removed-icon-box" aria-hidden="true">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <circle cx="12" cy="12" r="10" />
                  <line x1="15" y1="9" x2="9" y2="15" />
                  <line x1="9" y1="9" x2="15" y2="15" />
                </svg>
              </div>
              <div class="image-meta-box">
                <span class="image-filename">Imagem marcada para exclusão</span>
                <span class="image-subtext">A remoção será concluída ao salvar</span>
              </div>
              <div class="image-actions-box">
                <button
                  type="button"
                  class="btn-action-sm btn-ghost-sm"
                  @click="handleUndoRemoveExistingImage"
                >
                  Desfazer
                </button>
                <button
                  type="button"
                  class="btn-action-sm btn-secondary-sm"
                  @click="triggerFileInput"
                >
                  Nova imagem
                </button>
              </div>
            </div>

            <div v-else-if="currentFoto" class="image-card image-existing-card">
              <div class="image-thumb-box">
                <img :src="getProductImageUrl(currentFoto)" alt="Imagem atual do produto" class="image-thumb-img" />
              </div>
              <div class="image-meta-box">
                <span class="image-filename">Imagem atual cadastrada</span>
                <span class="image-subtext">Visível no catálogo</span>
              </div>
              <div class="image-actions-box">
                <button
                  type="button"
                  class="btn-action-sm btn-secondary-sm"
                  title="Selecionar outra imagem para substituir"
                  @click="triggerFileInput"
                >
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                    <polyline points="17 8 12 3 7 8" />
                    <line x1="12" y1="3" x2="12" y2="15" />
                  </svg>
                  <span>Substituir</span>
                </button>
                <button
                  type="button"
                  class="btn-text-danger"
                  title="Excluir imagem do produto"
                  @click="handleRemoveExistingImage"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="3 6 5 6 21 6" />
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                  </svg>
                  <span>Remover</span>
                </button>
              </div>
            </div>

            <div v-else class="image-empty-box">
              <button
                type="button"
                class="btn-select-image"
                @click="triggerFileInput"
              >
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                  <circle cx="8.5" cy="8.5" r="1.5"/>
                  <polyline points="21 15 16 10 5 21"/>
                </svg>
                <span>Selecionar imagem</span>
              </button>
              <span class="image-hint-text">Formatos permitidos: JPG, PNG ou WEBP (até 10 MB)</span>
            </div>

            <p v-if="imageError" class="image-field-error">{{ imageError }}</p>
          </div>

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

          <template v-if="!isEditing">
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
          </template>

          <footer class="modal-footer">
            <button
              v-if="isEditing && produto"
              type="button"
              class="btn-delete-link"
              :disabled="loading"
              @click="emit('delete', produto)"
            >
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6" />
                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
              </svg>
              <span>Excluir produto</span>
            </button>

            <div class="footer-end-actions">
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
                {{ loading ? 'Salvando...' : (isEditing ? 'Salvar alterações' : 'Cadastrar produto') }}
              </button>
            </div>
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

.image-upload-area {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.hidden-file-input {
  display: none;
}

.image-empty-box {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 12px 14px;
  background: #faf8f6;
  border: 1.5px dashed #d8cfca;
  border-radius: 8px;
  flex-wrap: wrap;
}

.btn-select-image {
  min-height: 38px;
  padding: 0 14px;
  border-radius: 6px;
  background: #ffffff;
  border: 1.5px solid #d8cfca;
  color: #25201f;
  font-size: 0.82rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: all 0.15s ease;
}

.btn-select-image:hover {
  background: #eee7e3;
  border-color: #bfaea6;
}

.image-hint-text {
  font-size: 0.78rem;
  color: #8b807b;
}

.image-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 10px 14px;
  background: #faf8f6;
  border: 1px solid #e5ddd8;
  border-radius: 8px;
}

.image-preview-card {
  background: #fdfaf9;
  border-color: #e5ddd8;
}

.image-existing-card {
  background: #faf8f6;
  border-color: #e5ddd8;
}

.image-removed-card {
  background: #fff8f8;
  border: 1px dashed #f1bdc8;
}

.image-thumb-box {
  width: 56px;
  height: 56px;
  flex-shrink: 0;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid #d8cfca;
  background: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-thumb-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.removed-icon-box {
  width: 44px;
  height: 44px;
  flex-shrink: 0;
  border-radius: 6px;
  background: #fee2e2;
  color: #991b1b;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-meta-box {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.image-filename {
  font-size: 0.84rem;
  font-weight: 700;
  color: #25201f;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.image-subtext {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.76rem;
  color: #8b807b;
}

.tag-pending {
  background: #fdf2f4;
  color: #b33f62;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: 4px;
  border: 1px solid #fecdd3;
}

.image-actions-box {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.btn-action-sm {
  min-height: 32px;
  padding: 0 10px;
  border-radius: 6px;
  font-size: 0.78rem;
  font-weight: 700;
  cursor: pointer;
  border: 1px solid transparent;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: all 0.15s ease;
}

.btn-secondary-sm {
  background: #ffffff;
  border-color: #d8cfca;
  color: #625955;
}

.btn-secondary-sm:hover {
  background: #eee7e3;
  color: #25201f;
}

.btn-ghost-sm {
  background: transparent;
  border-color: #d8cfca;
  color: #625955;
}

.btn-ghost-sm:hover {
  background: #eee7e3;
}

.btn-text-danger {
  min-height: 32px;
  padding: 0 8px;
  border: none;
  background: transparent;
  color: #991b1b;
  font-size: 0.78rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  border-radius: 4px;
  transition: background-color 0.15s ease;
}

.btn-text-danger:hover {
  background: #fee2e2;
}

.image-field-error {
  margin: 0;
  font-size: 0.78rem;
  font-weight: 600;
  color: #991b1b;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-top: 6px;
  padding-top: 16px;
  border-top: 1px solid #eee7e3;
  flex-wrap: wrap;
}

.footer-end-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-left: auto;
}

.btn-delete-link {
  background: transparent;
  border: 1.5px solid #fecaca;
  color: #dc2626;
  padding: 0 14px;
  min-height: 42px;
  border-radius: 8px;
  font-size: 0.84rem;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-delete-link:hover:not(:disabled) {
  background: #fef2f2;
  border-color: #f87171;
}

.btn-delete-link:disabled {
  opacity: 0.5;
  cursor: not-allowed;
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

  .image-card,
  .image-empty-box {
    flex-direction: column;
    align-items: flex-start;
  }

  .image-actions-box {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>
