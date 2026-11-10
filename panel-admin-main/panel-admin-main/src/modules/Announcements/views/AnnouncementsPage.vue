<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Row {
  id: number
  appKey: string
  appKeyLabel: string
  contentType: string
  contentTypeLabel: string
  textBody: string
  bannerUrl: string | null
  zoomUrl: string | null
  hasLink: boolean
  linkUrl: string | null
  isEnabled: boolean
  sortOrder: number
  updatedByName: string | null
  updatedAtUtc: string
  expectedVersion: number
}

interface ListResponse {
  items: Row[]
  totalCount: number
}

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const qc = useQueryClient()

const canRead = computed(() => auth.hasPermission('Announcement', 'Read') === true)
const canCreate = computed(() => auth.hasPermission('Announcement', 'Create') === true)
const canUpdate = computed(() => auth.hasPermission('Announcement', 'Update') === true)
const canDelete = computed(() => auth.hasPermission('Announcement', 'Delete') === true)

const appKey = computed(() => String(route.query.appKey ?? ''))
const enabledFilter = computed(() => {
  if (route.query.enabled === '1') return true
  if (route.query.enabled === '0') return false
  return null
})
const selectedId = ref<number | null>(null)
const dialog = ref(false)
const formError = ref<string | null>(null)
const submitting = ref(false)
const editing = ref<Row | null>(null)
const form = ref({
  appKey: 'delegate',
  contentType: 'text',
  textBody: '',
  bannerStorageKey: '',
  zoomStorageKey: '',
  hasLink: false,
  linkUrl: '',
  isEnabled: true,
})
const bannerPreview = ref<string | null>(null)
const zoomPreview = ref<string | null>(null)

const listQuery = useQuery({
  queryKey: computed(() => [
    'announcements',
    appKey.value,
    enabledFilter.value,
    auth.sessionGeneration,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/Announcements', {
      signal,
      params: {
        page: 1,
        pageSize: 100,
        appKey: appKey.value || undefined,
        isEnabled: enabledFilter.value === null ? undefined : enabledFilter.value,
      },
    })
    return data
  },
})

const selected = computed(
  () => listQuery.data.value?.items.find((r) => r.id === selectedId.value) ?? null
)

function patch(next: Record<string, string | null | undefined>) {
  const q = { ...(route.query as Record<string, string>) }
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({ name: route.name ?? undefined, query: q })
}

function openCreate() {
  editing.value = null
  form.value = {
    appKey: 'delegate',
    contentType: 'text',
    textBody: '',
    bannerStorageKey: '',
    zoomStorageKey: '',
    hasLink: false,
    linkUrl: '',
    isEnabled: true,
  }
  bannerPreview.value = null
  zoomPreview.value = null
  formError.value = null
  dialog.value = true
}

function openEdit(row: Row) {
  editing.value = row
  form.value = {
    appKey: row.appKey,
    contentType: row.contentType,
    textBody: row.textBody,
    bannerStorageKey: '',
    zoomStorageKey: '',
    hasLink: row.hasLink,
    linkUrl: row.linkUrl ?? '',
    isEnabled: row.isEnabled,
  }
  bannerPreview.value = row.bannerUrl
  zoomPreview.value = row.zoomUrl
  formError.value = null
  dialog.value = true
}

async function upload(kind: 'banner' | 'zoom', file: File | undefined) {
  if (!file) return
  const fd = new FormData()
  fd.append('file', file)
  const { data } = await axiosInstance.post<{ storageKey: string }>(
    '/admin/Announcements/upload-image',
    fd
  )
  if (kind === 'banner') {
    form.value.bannerStorageKey = data.storageKey
    bannerPreview.value = URL.createObjectURL(file)
  } else {
    form.value.zoomStorageKey = data.storageKey
    zoomPreview.value = URL.createObjectURL(file)
  }
}

const saveMut = useMutation({
  mutationFn: async () => {
    const body = {
      appKey: form.value.appKey,
      contentType: form.value.contentType,
      textBody: form.value.textBody,
      bannerStorageKey: form.value.bannerStorageKey || undefined,
      zoomStorageKey: form.value.zoomStorageKey || undefined,
      hasLink: form.value.hasLink,
      linkUrl: form.value.hasLink ? form.value.linkUrl : undefined,
      isEnabled: form.value.isEnabled,
    }
    if (editing.value) {
      await axiosInstance.put(`/admin/Announcements/${editing.value.id}`, {
        ...body,
        expectedVersion: editing.value.expectedVersion,
      })
    } else {
      await axiosInstance.post('/admin/Announcements', body)
    }
  },
  onSuccess: async () => {
    dialog.value = false
    await qc.invalidateQueries({ queryKey: ['announcements'] })
  },
})

const deleteMut = useMutation({
  mutationFn: async (row: Row) => {
    await axiosInstance.delete(`/admin/Announcements/${row.id}`, {
      params: { expectedVersion: row.expectedVersion },
    })
  },
  onSuccess: async () => {
    selectedId.value = null
    await qc.invalidateQueries({ queryKey: ['announcements'] })
  },
})

async function submit() {
  formError.value = null
  submitting.value = true
  try {
    await saveMut.mutateAsync()
  } catch {
    formError.value = 'تعذر الحفظ. تحقق من المحتوى والصلاحية.'
  } finally {
    submitting.value = false
  }
}

function fmtDate(iso: string) {
  try {
    return new Intl.DateTimeFormat('ar-IQ', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(iso))
  } catch {
    return iso
  }
}
</script>

<template>
  <div
    class="an-page"
    dir="rtl"
  >
    <header>
      <h1 class="an-title">الإعلانات</h1>
      <p class="an-sub">
        محتوى بانر للتطبيقات الميدانية — ليس ملاحظة وارد ولا إشعار Push.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="an-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة الإعلانات.
    </div>
    <template v-else>
      <div class="an-toolbar">
        <select
          :value="appKey"
          aria-label="الجمهور"
          @change="patch({ appKey: ($event.target as HTMLSelectElement).value || null })"
        >
          <option value="">كل التطبيقات</option>
          <option value="delegate">المندوب</option>
          <option value="follower">المتابع</option>
        </select>
        <select
          :value="enabledFilter === null ? '' : enabledFilter ? '1' : '0'"
          aria-label="الحالة"
          @change="patch({ enabled: ($event.target as HTMLSelectElement).value || null })"
        >
          <option value="">كل الحالات</option>
          <option value="1">مفعّل</option>
          <option value="0">معطّل</option>
        </select>
        <button
          v-if="canCreate"
          type="button"
          class="an-btn primary"
          @click="openCreate"
        >
          إعلان جديد
        </button>
      </div>
      <p
        v-if="listQuery.isError.value"
        class="an-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="an-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="an-state"
      >
        لا إعلانات.
      </div>
      <div
        v-else
        class="an-layout"
      >
        <div class="an-list">
          <button
            v-for="row in listQuery.data.value?.items ?? []"
            :key="row.id"
            type="button"
            class="an-card"
            :class="{ active: selectedId === row.id }"
            @click="selectedId = row.id"
          >
            <div class="an-card-top">
              <strong>{{ row.appKeyLabel }}</strong>
              <span>{{ row.isEnabled ? 'مفعّل' : 'معطّل' }}</span>
            </div>
            <div class="an-muted">
              {{ row.contentTypeLabel }} · ترتيب {{ row.sortOrder }}
            </div>
            <p class="an-snippet">
              {{
                row.contentType === 'text'
                  ? row.textBody
                  : 'إعلان صورة'
              }}
            </p>
          </button>
        </div>
        <aside class="an-detail">
          <div
            v-if="!selected"
            class="an-state"
          >
            اختر إعلانًا للمعاينة.
          </div>
          <template v-else>
            <h2>معاينة المستلم — {{ selected.appKeyLabel }}</h2>
            <p>
              <strong>الحالة:</strong>
              {{ selected.isEnabled ? 'مفعّل (ظاهر)' : 'معطّل (مخفي عن التطبيق)' }}
            </p>
            <p><strong>حدّثه:</strong> {{ selected.updatedByName || '—' }} · {{ fmtDate(selected.updatedAtUtc) }}</p>
            <div
              v-if="selected.contentType === 'text'"
              class="an-preview-text"
            >
              {{ selected.textBody }}
            </div>
            <div
              v-else
              class="an-preview-img"
            >
              <img
                v-if="selected.bannerUrl"
                :src="selected.bannerUrl"
                alt="بانر الإعلان"
              >
              <p class="an-muted">بانر القائمة الرئيسية</p>
            </div>
            <p v-if="selected.hasLink">
              رابط: <a
                :href="selected.linkUrl || '#'"
                target="_blank"
                rel="noopener noreferrer"
              >{{ selected.linkUrl }}</a>
            </p>
            <div class="an-actions">
              <button
                v-if="canUpdate"
                type="button"
                class="an-btn primary"
                @click="openEdit(selected)"
              >
                تعديل
              </button>
              <button
                v-if="canDelete"
                type="button"
                class="an-btn"
                :disabled="deleteMut.isPending.value"
                @click="deleteMut.mutate(selected)"
              >
                حذف
              </button>
            </div>
          </template>
        </aside>
      </div>
    </template>

    <div
      v-if="dialog"
      class="an-backdrop"
      @click.self="dialog = false"
    >
      <div
        class="an-dialog"
        role="dialog"
        aria-modal="true"
      >
        <h2>{{ editing ? 'تعديل إعلان' : 'إعلان جديد' }}</h2>
        <label
          v-if="!editing"
          class="an-field"
        >الجمهور
          <select v-model="form.appKey">
            <option value="delegate">المندوب</option>
            <option value="follower">المتابع</option>
          </select>
        </label>
        <fieldset class="an-field">
          <legend>نوع المحتوى</legend>
          <label><input
            v-model="form.contentType"
            type="radio"
            value="text"
          > نص</label>
          <label><input
            v-model="form.contentType"
            type="radio"
            value="image"
          > صورة</label>
        </fieldset>
        <label
          v-if="form.contentType === 'text'"
          class="an-field"
        >النص
          <textarea
            v-model="form.textBody"
            rows="4"
            maxlength="2000"
          />
        </label>
        <template v-else>
          <label class="an-field">صورة البانر
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp,image/gif"
              @change="upload('banner', ($event.target as HTMLInputElement).files?.[0])"
            >
          </label>
          <img
            v-if="bannerPreview"
            :src="bannerPreview"
            alt="معاينة بانر"
            class="an-thumb"
          >
          <label class="an-field">صورة التكبير
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp,image/gif"
              @change="upload('zoom', ($event.target as HTMLInputElement).files?.[0])"
            >
          </label>
          <img
            v-if="zoomPreview"
            :src="zoomPreview"
            alt="معاينة تكبير"
            class="an-thumb"
          >
        </template>
        <label class="an-check"><input
          v-model="form.hasLink"
          type="checkbox"
        > رابط اختياري</label>
        <label
          v-if="form.hasLink"
          class="an-field"
        >الرابط
          <input
            v-model="form.linkUrl"
            type="url"
            placeholder="https://"
          >
        </label>
        <label class="an-check"><input
          v-model="form.isEnabled"
          type="checkbox"
        > مفعّل (ظاهر للتطبيق)</label>
        <p
          v-if="formError"
          class="an-err"
        >
          {{ formError }}
        </p>
        <div class="an-actions">
          <button
            type="button"
            class="an-btn"
            @click="dialog = false"
          >
            إلغاء
          </button>
          <button
            type="button"
            class="an-btn primary"
            :disabled="submitting"
            @click="submit"
          >
            حفظ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.an-page {
  font-family: 'El Messiri', Tahoma, sans-serif;
  color: #00201e;
  padding: 1rem 1.25rem 2rem;
  max-width: 64rem;
  margin: 0 auto;
}
.an-title {
  margin: 0;
  color: #006a64;
  font-size: 1.75rem;
}
.an-sub {
  margin: 0.35rem 0 0;
  color: #3d524f;
  font-size: 14px;
}
.an-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin: 1.25rem 0 0.75rem;
}
.an-toolbar select,
.an-field input,
.an-field select,
.an-field textarea {
  font-family: inherit;
  font-size: 16px;
  color: #00201e;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  min-height: 44px;
  padding: 0.5rem 0.75rem;
}
.an-btn {
  min-height: 40px;
  padding: 0 0.85rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #fff;
  color: #006a64;
  font-family: inherit;
  cursor: pointer;
}
.an-btn.primary {
  background: #006a64;
  color: #fff;
}
.an-btn:disabled {
  opacity: 0.55;
}
.an-state {
  padding: 1.5rem;
  text-align: center;
  color: #3d524f;
}
.an-err {
  color: #8b1a1a;
}
.an-muted {
  color: #3d524f;
  font-size: 13px;
}
.an-layout {
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 1rem;
}
@media (max-width: 768px) {
  .an-layout {
    grid-template-columns: 1fr;
  }
}
.an-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.an-card {
  text-align: right;
  border: 1px solid #d7e0df;
  border-radius: 10px;
  background: #fff;
  padding: 0.85rem 1rem;
  font-family: inherit;
  cursor: pointer;
}
.an-card.active,
.an-card:hover {
  background: #f3f7f6;
}
.an-card-top {
  display: flex;
  justify-content: space-between;
  gap: 0.5rem;
}
.an-snippet {
  margin: 0.35rem 0 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.an-detail {
  border: 1px solid #d7e0df;
  border-radius: 10px;
  padding: 1rem;
  background: #fff;
}
.an-preview-text {
  margin: 0.75rem 0;
  padding: 0.85rem;
  background: #f8f8f8;
  border-radius: 8px;
  white-space: pre-wrap;
  word-break: break-word;
}
.an-preview-img img,
.an-thumb {
  max-width: 100%;
  border-radius: 8px;
  margin: 0.35rem 0;
}
.an-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.75rem;
}
.an-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  z-index: 40;
  padding: 1rem;
}
.an-dialog {
  width: min(32rem, 100%);
  max-height: 90vh;
  overflow: auto;
  background: #fff;
  border-radius: 12px;
  padding: 1.25rem;
}
.an-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin: 0.75rem 0;
  border: 0;
  padding: 0;
}
.an-check {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0.5rem 0;
}
</style>
