<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppSearchField, AppSelect, DataState } from '@/modules/Core/components/ui'
import { formatDateTime, formatNumber } from '@/modules/Core/utils/formatDisplay'
import {
  useAppNoteDetailQuery,
  useAppNotesQuery,
} from '../requests/queries/useAppNotesQuery'
import {
  useMarkAllAppNotesReadMutation,
  useMarkAppNoteReadMutation,
} from '../requests/mutations/useAppNoteMutations'
import '../components/app-notes.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const sourceAppOptions = [
  { label: 'كل المصادر', value: null as string | null },
  { label: 'متابع', value: 'Follower' },
  { label: 'مندوب', value: 'Delegate' },
  { label: 'موظف مبيعات', value: 'SalesEmployee' },
  { label: 'مدير فرع', value: 'BranchManager' },
]

const canRead = computed(() => auth.hasPermission('AppNote', 'Read') === true)

const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const pageSize = ref(20)
const unreadOnly = computed(() => route.query.unread === '1')
const sourceApp = computed(() => String(route.query.sourceApp ?? '').trim())
const searchInput = ref(String(route.query.q ?? ''))
const search = ref(searchInput.value)
const selectedId = computed(() => {
  const n = Number(route.query.id)
  return Number.isInteger(n) && n > 0 ? n : null
})

let searchTimer: ReturnType<typeof setTimeout> | null = null
watch(searchInput, (v) => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    search.value = v.trim()
    patchQuery({ q: search.value || null, page: '1' })
  }, 300)
})

const listQuery = useAppNotesQuery({
  page,
  pageSize,
  unreadOnly,
  sourceApp,
  search,
  enabled: canRead,
})
const detailQuery = useAppNoteDetailQuery({
  id: selectedId,
  enabled: canRead,
})
const markReadMut = useMarkAppNoteReadMutation()
const markAllMut = useMarkAllAppNotesReadMutation()

const data = computed(() => listQuery.data.value)
const unreadCount = computed(() =>
  listQuery.isError.value ? null : (data.value?.unreadCount ?? null)
)

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'ManagementAppNotesRoute',
    query: q,
  })
}

function selectRow(id: number) {
  patchQuery({ id: String(id) })
}

async function markSelectedRead() {
  if (selectedId.value == null) return
  await markReadMut.mutateAsync(selectedId.value)
}

async function markAllRead() {
  await markAllMut.mutateAsync()
}

function sourceLabel(v: string) {
  switch (v) {
    case 'Follower':
      return 'متابع'
    case 'Delegate':
      return 'مندوب'
    case 'SalesEmployee':
    case '4':
      return 'موظف مبيعات'
    case 'BranchManager':
      return 'مدير فرع'
    default:
      return v
  }
}

function statusLabel(isUnread: boolean) {
  return isUnread ? 'غير مقروءة' : 'مقروءة'
}

function fmt(v: string | null | undefined) {
  return formatDateTime(v)
}
</script>

<template>
  <div
    class="an-page"
    dir="rtl"
  >
    <header>
      <h1 class="an-page__title">ملاحظات التطبيقات</h1>
      <p class="an-page__sub">
        وارد ملاحظات ميدانية إلى الإدارة — قراءة وتعليم مقروء فقط، بلا رد من اللوحة.
      </p>
    </header>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="ليس لديك صلاحية قراءة ملاحظات التطبيقات."
    />

    <template v-else>
      <div class="q-filters">
        <AppSearchField
          v-model="searchInput"
          label="بحث"
          placeholder="بحث في الموضوع أو النص أو المرسل…"
          grow
        />
        <AppSelect
          :model-value="sourceApp || null"
          label="مصدر التطبيق"
          placeholder="كل المصادر"
          :options="sourceAppOptions"
          :filterable="false"
          @update:model-value="
            (v) =>
              patchQuery({
                sourceApp: v ? String(v) : null,
                page: '1',
              })
          "
        />
        <AppButton
          :variant="unreadOnly ? 'primary' : 'secondary'"
          @click="patchQuery({ unread: unreadOnly ? null : '1', page: '1' })"
        >
          غير المقروء فقط
        </AppButton>
        <AppButton
          variant="secondary"
          :disabled="markAllMut.isPending.value || (unreadCount ?? 0) === 0"
          @click="markAllRead"
        >
          تعليم الكل مقروءًا
        </AppButton>
      </div>

      <p class="an-meta">
        غير المقروء:
        <template v-if="unreadCount == null">—</template>
        <template v-else>{{ formatNumber(unreadCount) }}</template>
        · الإجمالي:
        {{ data ? formatNumber(data.totalCount) : (listQuery.isPending.value ? '…' : '—') }}
      </p>

      <DataState
        v-if="listQuery.isError.value"
        kind="error"
        title="تعذر التحميل"
        message="تعذر تحميل الملاحظات."
      />
      <DataState
        v-else-if="listQuery.isPending.value"
        kind="loading"
        title="جاري التحميل"
        message="جاري تحميل الملاحظات."
      />
      <DataState
        v-else-if="(data?.items.length ?? 0) === 0"
        kind="empty"
        title="لا ملاحظات"
        message="لا ملاحظات ضمن الفلاتر الحالية."
      />
      <div
        v-else
        class="an-layout"
      >
        <div class="an-list">
          <button
            v-for="row in data?.items ?? []"
            :key="row.id"
            type="button"
            class="an-row"
            :class="{ active: selectedId === row.id }"
            @click="selectRow(row.id)"
          >
            <div class="an-row__top">
              <span class="an-row__subject">{{ row.subject }}</span>
              <span
                class="an-badge"
                :class="row.isUnread ? 'an-badge--unread' : 'an-badge--read'"
              >
                {{ statusLabel(row.isUnread) }}
              </span>
            </div>
            <div class="an-row__preview">
              {{ sourceLabel(row.sourceApp) }} · {{ row.senderDisplayName }} ·
              {{ fmt(row.createdAtUtc) }}
            </div>
            <div class="an-row__preview">{{ row.bodyPreview }}</div>
          </button>
        </div>

        <aside class="an-detail">
          <div
            v-if="selectedId == null"
            class="an-state"
          >
            اختر ملاحظة لعرض التفاصيل.
          </div>
          <div
            v-else-if="detailQuery.isPending.value"
            class="an-state"
            aria-busy="true"
          >
            جاري تحميل التفاصيل…
          </div>
          <div
            v-else-if="detailQuery.isError.value"
            class="an-error"
            role="alert"
          >
            تعذر تحميل التفاصيل.
          </div>
          <template v-else-if="detailQuery.data.value">
            <h2>{{ detailQuery.data.value.subject }}</h2>
            <dl>
              <dt>الحالة</dt>
              <dd>{{ statusLabel(detailQuery.data.value.isUnread) }}</dd>
              <dt>المصدر</dt>
              <dd>{{ sourceLabel(detailQuery.data.value.sourceApp) }}</dd>
              <dt>المرسل</dt>
              <dd>
                {{ detailQuery.data.value.senderDisplayName }}
                ({{ detailQuery.data.value.senderRole }})
              </dd>
              <dt>الفرع</dt>
              <dd>
                {{
                  detailQuery.data.value.branchName ||
                    (detailQuery.data.value.branchId
                      ? `#${detailQuery.data.value.branchId}`
                      : '—')
                }}
              </dd>
              <dt>الوقت</dt>
              <dd>{{ fmt(detailQuery.data.value.createdAtUtc) }}</dd>
              <dt>قُرئت</dt>
              <dd>{{ fmt(detailQuery.data.value.readAtUtc) }}</dd>
            </dl>
            <p class="an-body">{{ detailQuery.data.value.body }}</p>
            <button
              v-if="detailQuery.data.value.isUnread"
              type="button"
              class="an-btn primary"
              :disabled="markReadMut.isPending.value"
              @click="markSelectedRead"
            >
              تعليم مقروءة
            </button>
          </template>
        </aside>
      </div>

      <div
        v-if="(data?.totalCount ?? 0) > pageSize"
        class="an-pager"
      >
        <button
          type="button"
          class="an-btn"
          :disabled="page <= 1"
          @click="patchQuery({ page: String(page - 1) })"
        >
          السابق
        </button>
        <span>صفحة {{ page }}</span>
        <button
          type="button"
          class="an-btn"
          :disabled="page * pageSize >= (data?.totalCount ?? 0)"
          @click="patchQuery({ page: String(page + 1) })"
        >
          التالي
        </button>
      </div>
    </template>
  </div>
</template>
