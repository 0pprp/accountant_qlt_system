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
  questionAr: string
  videoUrl: string
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

const canRead = computed(() => auth.hasPermission('Tutorial', 'Read') === true)
const canCreate = computed(() => auth.hasPermission('Tutorial', 'Create') === true)
const canUpdate = computed(() => auth.hasPermission('Tutorial', 'Update') === true)
const canDelete = computed(() => auth.hasPermission('Tutorial', 'Delete') === true)

const appKey = computed(() => String(route.query.appKey ?? ''))
const enabledFilter = computed(() => {
  if (route.query.enabled === '1') return true
  if (route.query.enabled === '0') return false
  return null
})
const dialog = ref(false)
const formError = ref<string | null>(null)
const submitting = ref(false)
const editing = ref<Row | null>(null)
const form = ref({
  appKey: 'delegate',
  questionAr: '',
  videoUrl: '',
  isEnabled: true,
})

const listQuery = useQuery({
  queryKey: computed(() => [
    'tutorial-videos',
    appKey.value,
    enabledFilter.value,
    auth.sessionGeneration,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/TutorialVideos', {
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
    appKey: appKey.value || 'delegate',
    questionAr: '',
    videoUrl: '',
    isEnabled: true,
  }
  formError.value = null
  dialog.value = true
}

function openEdit(row: Row) {
  editing.value = row
  form.value = {
    appKey: row.appKey,
    questionAr: row.questionAr,
    videoUrl: row.videoUrl,
    isEnabled: row.isEnabled,
  }
  formError.value = null
  dialog.value = true
}

const saveMut = useMutation({
  mutationFn: async () => {
    const body = {
      appKey: form.value.appKey,
      questionAr: form.value.questionAr,
      videoUrl: form.value.videoUrl,
      isEnabled: form.value.isEnabled,
    }
    if (editing.value) {
      await axiosInstance.put(`/admin/TutorialVideos/${editing.value.id}`, {
        questionAr: body.questionAr,
        videoUrl: body.videoUrl,
        isEnabled: body.isEnabled,
        expectedVersion: editing.value.expectedVersion,
      })
    } else {
      await axiosInstance.post('/admin/TutorialVideos', body)
    }
  },
  onSuccess: async () => {
    dialog.value = false
    await qc.invalidateQueries({ queryKey: ['tutorial-videos'] })
  },
})

const deleteMut = useMutation({
  mutationFn: async (row: Row) => {
    await axiosInstance.delete(`/admin/TutorialVideos/${row.id}`, {
      params: { expectedVersion: row.expectedVersion },
    })
  },
  onSuccess: async () => {
    await qc.invalidateQueries({ queryKey: ['tutorial-videos'] })
  },
})

async function submit() {
  formError.value = null
  submitting.value = true
  try {
    await saveMut.mutateAsync()
  } catch {
    formError.value = 'تعذر الحفظ. تحقق من الرابط والصلاحية.'
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
    class="tv-page"
    dir="rtl"
  >
    <header>
      <h1 class="tv-title">الفيديوهات التعليمية</h1>
      <p class="tv-sub">
        أسئلة مع روابط فيديو خارجية للتطبيقات الميدانية — بدون رفع ملف أو تتبع مشاهدة.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="tv-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة الفيديوهات التعليمية.
    </div>
    <template v-else>
      <div class="tv-toolbar">
        <select
          :value="appKey"
          aria-label="الجمهور"
          @change="patch({ appKey: ($event.target as HTMLSelectElement).value || null })"
        >
          <option value="">كل التطبيقات</option>
          <option value="delegate">المندوب</option>
          <option value="branch-manager">إدارة الفرع</option>
          <option value="follower">المتابع / مسؤول اللجنة</option>
        </select>
        <select
          :value="enabledFilter === null ? '' : enabledFilter ? '1' : '0'"
          aria-label="الحالة"
          @change="patch({ enabled: ($event.target as HTMLSelectElement).value || null })"
        >
          <option value="">الكل</option>
          <option value="1">مفعّل</option>
          <option value="0">معطّل</option>
        </select>
        <button
          v-if="canCreate"
          type="button"
          class="tv-btn primary"
          @click="openCreate"
        >
          إضافة فيديو
        </button>
        <button
          v-if="listQuery.isError.value"
          type="button"
          class="tv-btn"
          @click="listQuery.refetch()"
        >
          إعادة المحاولة
        </button>
      </div>
      <p class="tv-meta">
        الإجمالي:
        {{ listQuery.data.value?.totalCount ?? (listQuery.isError.value ? '—' : '…') }}
      </p>
      <p
        v-if="listQuery.isError.value"
        class="tv-err"
        role="alert"
      >
        تعذر تحميل القائمة.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="tv-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="tv-state"
      >
        لا فيديوهات تعليمية.
      </div>
      <div
        v-else
        class="tv-table-wrap"
      >
        <table class="tv-table">
          <thead>
            <tr>
              <th>الترتيب</th>
              <th>السؤال</th>
              <th>الجمهور</th>
              <th>الحالة</th>
              <th>آخر تحديث</th>
              <th>إجراء</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in listQuery.data.value?.items ?? []"
              :key="row.id"
            >
              <td>{{ row.sortOrder }}</td>
              <td>
                <strong>{{ row.questionAr }}</strong>
                <div class="tv-url">{{ row.videoUrl }}</div>
              </td>
              <td>{{ row.appKeyLabel }}</td>
              <td>
                <span :class="row.isEnabled ? 'ok' : 'off'">
                  {{ row.isEnabled ? 'مفعّل' : 'معطّل' }}
                </span>
              </td>
              <td>{{ row.updatedByName ?? '—' }} · {{ fmtDate(row.updatedAtUtc) }}</td>
              <td class="tv-actions">
                <a
                  :href="row.videoUrl"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="tv-btn"
                >معاينة</a>
                <button
                  v-if="canUpdate"
                  type="button"
                  class="tv-btn"
                  @click="openEdit(row)"
                >
                  تعديل
                </button>
                <button
                  v-if="canDelete"
                  type="button"
                  class="tv-btn"
                  :disabled="deleteMut.isPending.value"
                  @click="deleteMut.mutate(row)"
                >
                  حذف
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <div
      v-if="dialog"
      class="tv-backdrop"
      @click.self="dialog = false"
    >
      <div
        class="tv-dialog"
        role="dialog"
        aria-modal="true"
      >
        <h2>{{ editing ? 'تعديل فيديو' : 'إضافة فيديو' }}</h2>
        <label
          v-if="!editing"
          class="tv-field"
        >الجمهور
          <select v-model="form.appKey">
            <option value="delegate">المندوب</option>
            <option value="branch-manager">إدارة الفرع</option>
            <option value="follower">المتابع / مسؤول اللجنة</option>
          </select>
        </label>
        <label class="tv-field">السؤال
          <input
            v-model="form.questionAr"
            type="text"
            maxlength="400"
          >
        </label>
        <label class="tv-field">رابط الفيديو (http/https)
          <input
            v-model="form.videoUrl"
            type="url"
            maxlength="1024"
            dir="ltr"
          >
        </label>
        <label class="tv-check">
          <input
            v-model="form.isEnabled"
            type="checkbox"
          >
          مفعّل
        </label>
        <p
          v-if="formError"
          class="tv-err"
        >
          {{ formError }}
        </p>
        <div class="tv-actions">
          <button
            type="button"
            class="tv-btn"
            @click="dialog = false"
          >
            إلغاء
          </button>
          <button
            type="button"
            class="tv-btn primary"
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
.tv-page {
  --c-primary: #006a64;
  --c-ink: #00201e;
  --c-field: #f8f8f8;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: var(--c-ink);
  padding: 1rem 1.25rem 2rem;
  max-width: 1100px;
  margin: 0 auto;
}
.tv-title {
  margin: 0;
  font-size: 1.55rem;
  color: var(--c-primary);
}
.tv-sub {
  margin: 0.35rem 0 1rem;
  color: #445;
  font-size: 0.95rem;
}
.tv-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}
.tv-toolbar select,
.tv-field input,
.tv-field select {
  background: var(--c-field);
  border: 1px solid #d0d5d4;
  border-radius: 8px;
  padding: 0.45rem 0.65rem;
  font: inherit;
  color: inherit;
}
.tv-btn {
  border: 1px solid #c5cecc;
  background: #fff;
  border-radius: 8px;
  padding: 0.4rem 0.75rem;
  font: inherit;
  cursor: pointer;
  color: var(--c-ink);
  text-decoration: none;
  display: inline-block;
}
.tv-btn.primary {
  background: var(--c-primary);
  border-color: var(--c-primary);
  color: #fff;
}
.tv-meta {
  margin: 0 0 0.5rem;
  font-size: 0.9rem;
}
.tv-state {
  padding: 1.5rem;
  text-align: center;
  background: var(--c-field);
  border-radius: 12px;
}
.tv-err {
  color: #9b1c1c;
  margin: 0.5rem 0;
}
.tv-table-wrap {
  overflow-x: auto;
}
.tv-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.92rem;
}
.tv-table th,
.tv-table td {
  border-bottom: 1px solid #e3e8e7;
  padding: 0.55rem 0.4rem;
  text-align: right;
  vertical-align: top;
}
.tv-url {
  font-size: 0.78rem;
  color: #667;
  direction: ltr;
  word-break: break-all;
  margin-top: 0.2rem;
}
.ok {
  color: #0a6b3c;
}
.off {
  color: #8a4b00;
}
.tv-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}
.tv-backdrop {
  position: fixed;
  inset: 0;
  background: rgb(0 32 30 / 45%);
  display: grid;
  place-items: center;
  padding: 1rem;
  z-index: 40;
}
.tv-dialog {
  background: #fff;
  border-radius: 14px;
  padding: 1.1rem 1.2rem;
  width: min(480px, 100%);
  max-height: 90vh;
  overflow: auto;
}
.tv-field {
  display: grid;
  gap: 0.3rem;
  margin: 0.65rem 0;
  font-size: 0.9rem;
}
.tv-check {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin: 0.5rem 0;
}
@media (max-width: 768px) {
  .tv-page {
    padding: 0.75rem;
  }
}
</style>
