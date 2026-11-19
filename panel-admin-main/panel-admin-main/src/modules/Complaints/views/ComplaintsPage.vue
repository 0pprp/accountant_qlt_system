<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { isAxiosError } from 'axios'
import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import { formatDateTime } from '@/modules/Core/utils/formatDisplay'
import { DELEGATE_SUMMARY_KEY, useDelegateBranchId } from '@/modules/DelegateManager/composables/useDelegateSummary'
import { complaintRelativeTime, complaintRoleLabel, complaintSourceLabel } from '@/modules/Complaints/domain/complaintFormat'

interface Row {
  id: number
  sourceApp: string
  senderUserId: number
  senderDisplayName: string
  senderRole: string
  branchId: number | null
  branchName: string | null
  subject: string
  body: string
  status: string
  createdAtUtc: string
  readAtUtc: string | null
  isUnread: boolean
  expectedVersion: number
}

interface ListResponse {
  items: Row[]
  totalCount: number
  unreadCount: number
  page: number
  pageSize: number
}

const PAGE_SIZE = 20

const auth = useAuthStore()
const route = useRoute()
const qc = useQueryClient()
const branchId = useDelegateBranchId()
const canRead = computed(() => auth.hasPermission('Complaint', 'Read') === true)
const canMark = computed(() => auth.hasPermission('Complaint', 'MarkRead') === true)

const unreadOnly = ref(false)
const searchInput = ref('')
const search = ref('')
const selected = ref<Row | null>(null)
const actionError = ref<string | null>(null)
const notice = ref<string | null>(null)
const confirmAll = ref(false)
const autoMarked = new Set<number>()
let searchTimer: ReturnType<typeof setTimeout> | undefined

watch(searchInput, (value) => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    search.value = value.trim()
  }, 300)
})

onBeforeUnmount(() => clearTimeout(searchTimer))

watch(branchId, () => {
  selected.value = null
  notice.value = null
})

const listQuery = useInfiniteQuery({
  queryKey: computed(() => ['complaints', auth.sessionGeneration, branchId.value, unreadOnly.value, search.value]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  initialPageParam: 1,
  queryFn: async ({ pageParam, signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/Complaints', {
      signal,
      params: {
        page: pageParam,
        pageSize: PAGE_SIZE,
        unreadOnly: unreadOnly.value || undefined,
        search: search.value || undefined,
        branchId: branchId.value ?? undefined,
      },
    })
    return data
  },
  getNextPageParam: (last) => (last.page * last.pageSize < last.totalCount ? last.page + 1 : undefined),
})

const pages = computed(() => listQuery.data.value?.pages ?? [])
const items = computed(() => {
  const seen = new Set<number>()
  const out: Row[] = []
  for (const p of pages.value) {
    for (const row of p.items) {
      if (seen.has(row.id)) continue
      seen.add(row.id)
      out.push(row)
    }
  }
  return out
})
const totalCount = computed(() => pages.value[0]?.totalCount ?? 0)
const unreadCount = computed(() => pages.value[0]?.unreadCount ?? 0)

watch(items, (list) => {
  const current = selected.value
  if (!current) return
  const fresh = list.find((r) => r.id === current.id)
  if (fresh && fresh.expectedVersion >= current.expectedVersion) selected.value = fresh
})

function isConflict(error: unknown): boolean {
  if (!isAxiosError(error)) return false
  if (error.response?.status === 409) return true
  const errors = (error.response?.data as { errors?: { code?: string }[] } | undefined)?.errors
  return Array.isArray(errors) && errors.some((e) => e?.code === 'Complaint.Concurrency')
}

async function refreshAll() {
  await Promise.all([
    qc.invalidateQueries({ queryKey: ['complaints'] }),
    qc.invalidateQueries({ queryKey: [DELEGATE_SUMMARY_KEY] }),
  ])
}

const markMut = useMutation({
  mutationFn: async (row: Row) => {
    const { data } = await axiosInstance.post<Row>(`/admin/Complaints/${row.id}/mark-read`, {
      expectedVersion: row.expectedVersion,
    })
    return data
  },
  onSuccess: async (data) => {
    actionError.value = null
    if (selected.value?.id === data.id) selected.value = data
    await refreshAll()
  },
  onError: async (error) => {
    if (isConflict(error)) {
      actionError.value = 'تم تحديث الرسالة من جهة أخرى، أعدنا تحميلها.'
      await refreshAll()
      return
    }
    actionError.value = 'تعذر تحديد الرسالة كمقروءة.'
  },
})

const markAllMut = useMutation({
  mutationFn: async () => {
    const { data } = await axiosInstance.post<{ updated: number }>('/admin/Complaints/mark-all-read', null, {
      params: branchId.value == null ? undefined : { branchId: branchId.value },
    })
    return data
  },
  onSuccess: async (data) => {
    confirmAll.value = false
    actionError.value = null
    notice.value = data.updated > 0 ? `تم تحديد ${data.updated} رسالة كمقروءة.` : 'لا توجد رسائل غير مقروءة.'
    const current = selected.value
    if (current?.isUnread) selected.value = { ...current, isUnread: false, status: 'Read', readAtUtc: new Date().toISOString() }
    await refreshAll()
  },
  onError: async (error) => {
    confirmAll.value = false
    notice.value = null
    actionError.value = isConflict(error)
      ? 'تم تعديل بعض الرسائل من جهة أخرى، أعدنا التحميل. حاول مرة أخرى.'
      : 'تعذر تحديد جميع الرسائل كمقروءة.'
    if (isConflict(error)) await refreshAll()
  },
})

function open(row: Row) {
  selected.value = row
  actionError.value = null
  notice.value = null
  if (row.isUnread && canMark.value && !autoMarked.has(row.id) && !markMut.isPending.value) {
    autoMarked.add(row.id)
    markMut.mutate(row)
  }
}

function subtitle(row: Row): string {
  const parts = [row.senderDisplayName || 'غير محدد']
  if (row.branchName) parts.push(row.branchName)
  parts.push(complaintSourceLabel(row.sourceApp, row.senderRole))
  return parts.join(' • ')
}

function setUnreadOnly(value: boolean) {
  if (unreadOnly.value === value) return
  unreadOnly.value = value
}
</script>

<template>
  <section class="cp" dir="rtl">
    <header class="cp-head">
      <div>
        <h2 class="dfb-title">الرسائل والشكاوى</h2>
        <p class="dfb-subtitle">رسائل وشكاوى التطبيقات ضمن نطاقك، الأحدث أولاً.</p>
      </div>
      <button
        v-if="canRead && canMark"
        type="button"
        class="dfb-button is-small"
        :disabled="markAllMut.isPending.value || unreadCount === 0"
        @click="confirmAll = true"
      >
        تحديد الكل كمقروء
      </button>
    </header>

    <p v-if="!canRead" class="dfb-state" role="alert">ليس لديك صلاحية قراءة الرسائل والشكاوى.</p>

    <template v-else>
      <div class="cp-toolbar">
        <input
          v-model="searchInput"
          type="search"
          class="dfb-search"
          placeholder="بحث في العنوان أو المرسل"
          aria-label="بحث في العنوان أو المرسل"
          maxlength="100"
        >
        <div class="dfb-filter-chips" role="group" aria-label="تصفية الرسائل">
          <button type="button" class="dfb-chip" :aria-pressed="!unreadOnly" @click="setUnreadOnly(false)">
            عرض الكل
          </button>
          <button type="button" class="dfb-chip" :aria-pressed="unreadOnly" @click="setUnreadOnly(true)">
            غير المقروءة فقط <strong>{{ unreadCount }}</strong>
          </button>
        </div>
      </div>

      <p v-if="notice" class="cp-notice" role="status">{{ notice }}</p>
      <p v-if="actionError && !selected" class="dfb-error" role="alert">{{ actionError }}</p>

      <div class="cp-layout" :class="{ 'has-detail': selected }">
        <div class="cp-list-col">
          <div v-if="listQuery.isError.value" class="dfb-error" role="alert">
            تعذر تحميل الرسائل.
            <button type="button" class="dfb-button is-small" @click="listQuery.refetch()">إعادة المحاولة</button>
          </div>
          <p v-else-if="listQuery.isPending.value" class="dfb-state">جاري التحميل…</p>
          <p v-else-if="items.length === 0" class="dfb-state">لا توجد رسائل أو شكاوى</p>
          <template v-else>
            <p class="cp-meta">
              المعروض <span class="dfb-num">{{ items.length }}</span> من <span class="dfb-num">{{ totalCount }}</span>
            </p>
            <ul class="cp-list">
              <li v-for="row in items" :key="row.id">
                <button
                  type="button"
                  class="cp-row"
                  :class="{ 'is-unread': row.isUnread, 'is-active': selected?.id === row.id }"
                  :aria-current="selected?.id === row.id ? 'true' : undefined"
                  @click="open(row)"
                >
                  <span class="cp-row-top">
                    <span class="cp-row-subject">{{ row.subject || 'بدون عنوان' }}</span>
                    <span v-if="row.isUnread" class="dfb-status is-open">جديدة</span>
                  </span>
                  <span class="cp-row-sub">{{ subtitle(row) }}</span>
                  <span class="cp-row-time">{{ complaintRelativeTime(row.createdAtUtc) }}</span>
                </button>
              </li>
            </ul>
            <button
              v-if="listQuery.hasNextPage.value"
              type="button"
              class="dfb-button cp-more"
              :disabled="listQuery.isFetchingNextPage.value"
              @click="listQuery.fetchNextPage()"
            >
              {{ listQuery.isFetchingNextPage.value ? 'جاري التحميل…' : 'تحميل المزيد' }}
            </button>
          </template>
        </div>

        <aside v-if="selected" class="cp-detail" aria-label="تفاصيل الرسالة">
          <div class="cp-detail-head">
            <h3 class="cp-detail-title">{{ selected.subject || 'بدون عنوان' }}</h3>
            <span class="dfb-status" :class="selected.isUnread ? 'is-open' : 'is-sold'">
              {{ selected.isUnread ? 'غير مقروءة' : 'مقروءة' }}
            </span>
          </div>

          <dl class="cp-facts">
            <div><dt>المرسل</dt><dd>{{ selected.senderDisplayName || 'غير محدد' }}</dd></div>
            <div><dt>الصفة</dt><dd>{{ complaintRoleLabel(selected.senderRole) }}</dd></div>
            <div><dt>الفرع</dt><dd>{{ selected.branchName || 'غير محدد' }}</dd></div>
            <div><dt>المصدر</dt><dd>{{ complaintSourceLabel(selected.sourceApp, selected.senderRole) }}</dd></div>
            <div><dt>وقت الإرسال</dt><dd>{{ formatDateTime(selected.createdAtUtc) }}</dd></div>
            <div><dt>تاريخ القراءة</dt><dd>{{ selected.readAtUtc ? formatDateTime(selected.readAtUtc) : 'لم تُقرأ بعد' }}</dd></div>
          </dl>

          <div class="cp-body-card">
            <h4 class="cp-body-title">نص الرسالة</h4>
            <p class="cp-body">{{ selected.body || '-' }}</p>
          </div>

          <p v-if="actionError" class="dfb-error" role="alert">{{ actionError }}</p>

          <div class="cp-actions">
            <button
              v-if="canMark && selected.isUnread"
              type="button"
              class="dfb-button is-primary is-small"
              :disabled="markMut.isPending.value"
              @click="markMut.mutate(selected)"
            >
              {{ markMut.isPending.value ? 'جاري التحديد…' : 'تحديد كمقروءة' }}
            </button>
            <button type="button" class="dfb-button is-small" @click="selected = null; actionError = null">إغلاق</button>
          </div>
        </aside>
      </div>
    </template>

    <NeuDialog
      :visible="confirmAll"
      title="تحديد الكل كمقروء"
      size="sm"
      :busy="markAllMut.isPending.value"
      @close="confirmAll = false"
    >
      <p class="cp-confirm">هل تريد تحديد جميع الرسائل كمقروءة؟</p>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="markAllMut.isPending.value" @click="confirmAll = false">إلغاء</button>
        <button
          type="button"
          class="dfb-button is-primary"
          :disabled="markAllMut.isPending.value"
          @click="markAllMut.mutate()"
        >
          {{ markAllMut.isPending.value ? 'جاري التحديد…' : 'تأكيد' }}
        </button>
      </template>
    </NeuDialog>
  </section>
</template>

<style scoped>
.cp {
  display: flex;
  flex-direction: column;
  gap: 18px;
  min-width: 0;
}

.cp-head {
  align-items: flex-start;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
}

.cp-toolbar {
  align-items: center;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
}

.cp-notice {
  background: var(--dfb-primary-soft);
  border-radius: 12px;
  color: var(--dfb-primary);
  font-weight: 600;
  margin: 0;
  padding: 10px 14px;
}

.cp-layout {
  align-items: start;
  display: grid;
  gap: 20px;
  grid-template-columns: minmax(0, 1fr);
}

.cp-layout.has-detail {
  grid-template-columns: minmax(300px, 380px) minmax(0, 1fr);
}

.cp-list-col {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-width: 0;
}

.cp-meta {
  color: var(--dfb-muted);
  font-size: 13px;
  margin: 0;
}

.cp-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.cp-row {
  background: var(--dfb-raised);
  border: 0;
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  color: inherit;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  font: inherit;
  gap: 4px;
  padding: 12px 16px;
  text-align: start;
  width: 100%;
}

.cp-row:hover { box-shadow: var(--dfb-up-hover); }

.cp-row.is-unread {
  background: var(--dfb-primary-soft);
}

.cp-row.is-active {
  box-shadow: var(--dfb-down);
}

.cp-row-top {
  align-items: center;
  display: flex;
  gap: 8px;
  justify-content: space-between;
}

.cp-row-subject {
  font-weight: 500;
  line-height: 1.4;
  overflow-wrap: anywhere;
}

.cp-row.is-unread .cp-row-subject { font-weight: 700; }

.cp-row-sub {
  color: var(--dfb-muted);
  font-size: 13px;
  line-height: 1.5;
}

.cp-row-time {
  color: var(--dfb-muted);
  font-size: 12px;
}

.cp-more { align-self: center; }

.cp-detail {
  background: var(--dfb-well);
  border-radius: 20px;
  box-shadow: var(--dfb-down);
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
  padding: 18px;
}

.cp-detail-head {
  align-items: flex-start;
  display: flex;
  gap: 12px;
  justify-content: space-between;
}

.cp-detail-title {
  font-size: 18px;
  font-weight: 700;
  line-height: 1.4;
  margin: 0;
  overflow-wrap: anywhere;
}

.cp-facts {
  background: var(--dfb-raised);
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  display: grid;
  gap: 10px 18px;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 180px), 1fr));
  margin: 0;
  padding: 14px 16px;
}

.cp-facts dt {
  color: var(--dfb-muted);
  font-size: 12px;
  font-weight: 600;
}

.cp-facts dd {
  font-weight: 600;
  margin: 2px 0 0;
  overflow-wrap: anywhere;
}

.cp-body-card {
  background: var(--dfb-raised);
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  padding: 14px 16px;
}

.cp-body-title {
  color: var(--dfb-muted);
  font-size: 13px;
  font-weight: 700;
  margin: 0 0 8px;
}

.cp-body {
  line-height: 1.8;
  margin: 0;
  overflow-wrap: anywhere;
  white-space: pre-wrap;
}

.cp-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.cp-confirm {
  line-height: 1.7;
  margin: 0;
}

@media (max-width: 900px) {
  .cp-layout.has-detail {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
