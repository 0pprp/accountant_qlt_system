<template>
  <Dialog
    v-model:visible="isOpen"
    modal
    dir="rtl"
    class="media-viewer"
    :closable="false"
    :pt="{ mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' } }"
  >
    <template #header>
      <div class="media-viewer__header">
        <bdi class="media-viewer__title">{{ title }}</bdi>
        <div class="media-viewer__actions">
          <a
            v-if="objectUrl"
            class="media-viewer__btn"
            :href="objectUrl"
            :download="title"
          >
            تنزيل
          </a>
          <button type="button" class="media-viewer__btn" @click="close">
            إغلاق
          </button>
        </div>
      </div>
    </template>

    <div class="media-viewer__body">
      <p v-if="loading" class="media-viewer__state">جارٍ التحميل…</p>
      <p v-else-if="failed" class="media-viewer__state">
        تعذّر فتح الملف — قد يكون محذوفاً من مجلّد المستندات
      </p>
      <img
        v-else-if="objectUrl && isImage"
        :src="objectUrl"
        :alt="title"
        class="media-viewer__image"
      />
      <iframe
        v-else-if="objectUrl && isPdf"
        :src="objectUrl"
        :title="title"
        class="media-viewer__frame"
      ></iframe>
      <p v-else class="media-viewer__state">
        نوع الملف غير قابل للعرض هنا — استخدم زر التنزيل
      </p>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import { useAuthedMedia } from '@/modules/Core/composable/useAuthedMedia'

const props = defineProps<{
  /** المسار النسبي على الـAPI، مثل admin/CustomerMedia/documents/12 */
  path: string | null
  title: string
}>()

const isOpen = defineModel<boolean>('modelValue', { default: false })

const { fetchBlob, revoke } = useAuthedMedia()

const objectUrl = ref<string | null>(null)
const mimeType = ref<string>('')
const loading = ref(false)
const failed = ref(false)

const isImage = computed(() => mimeType.value.startsWith('image/'))
const isPdf = computed(() => mimeType.value === 'application/pdf')

function reset() {
  if (objectUrl.value) revoke(objectUrl.value)
  objectUrl.value = null
  mimeType.value = ''
  failed.value = false
}

async function load() {
  if (!props.path) return
  reset()
  loading.value = true
  const blob = await fetchBlob(props.path)
  loading.value = false
  if (!blob) {
    failed.value = true
    return
  }
  objectUrl.value = blob.url
  mimeType.value = blob.type
}

function close() {
  isOpen.value = false
}

watch(
  () => [isOpen.value, props.path] as const,
  ([open]) => {
    if (open) void load()
    else reset()
  },
  { immediate: true },
)
</script>

<style scoped>
.media-viewer__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  width: 100%;
}

.media-viewer__title {
  font-weight: 700;
  color: #00201e;
  overflow-wrap: anywhere;
}

.media-viewer__actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}

.media-viewer__btn {
  padding: 4px 14px;
  border-radius: 999px;
  border: 1px solid #006a64;
  color: #006a64;
  background: #fff;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  text-decoration: none;
  white-space: nowrap;
}

.media-viewer__btn:hover {
  background: #006a64;
  color: #fff;
}

.media-viewer__body {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
}

.media-viewer__state {
  margin: 0;
  color: #50666a;
  font-size: 14px;
}

.media-viewer__image {
  max-width: 100%;
  max-height: 78vh;
  object-fit: contain;
  border-radius: 12px;
}

.media-viewer__frame {
  width: 100%;
  height: 78vh;
  border: 0;
  border-radius: 12px;
  background: #f1f5f4;
}
</style>

<style>
.media-viewer {
  width: min(92vw, 1000px);
  background: #fff;
  border-radius: 20px;
  padding: 18px 20px 20px;
  overflow: auto;
}
</style>
