<template>
  <section class="dtb" dir="rtl" aria-labelledby="dtb-title">
    <header class="dtb-head">
      <div>
        <h2 id="dtb-title" class="dfb-title">مهامي</h2>
        <p class="dfb-subtitle">المهام المرسلة إليك مرتبة حسب الأولوية ثم تاريخ الاستحقاق · {{ connectionLabel }}</p>
      </div>
      <input
        v-model="searchInput"
        type="search"
        class="dfb-search dtb-search"
        placeholder="ابحث بالعنوان أو الاسم…"
        aria-label="بحث في المهام"
        autocomplete="off"
      >
    </header>

    <div class="dfb-filter-chips" role="group" aria-label="تصفية حسب الحالة">
      <button
        v-for="f in DELEGATE_FILTERS"
        :key="f.key"
        type="button"
        class="dfb-chip"
        :aria-pressed="filter === f.key"
        @click="filter = f.key"
      >
        {{ f.label }} <strong>{{ countFor(f.key) }}</strong>
      </button>
    </div>

    <p v-if="!canEnter" class="dfb-error" role="alert">لا تملك صلاحية عرض المهام.</p>
    <p v-else-if="listError" class="dfb-error" role="alert">تعذر تحميل المهام.</p>

    <div v-else class="dtb-layout">
      <div class="dtb-list" aria-label="قائمة المهام">
        <p v-if="listLoading" class="dfb-state" aria-live="polite">جاري التحميل…</p>
        <p v-else-if="!visibleItems.length" class="dfb-state">لا توجد مهام معيّنة حالياً</p>
        <ul v-else class="dtb-cards">
          <li v-for="item in visibleItems" :key="item.id">
            <button
              type="button"
              class="dtb-card"
              :class="{ 'is-active': item.id === selectedTaskId }"
              :aria-current="item.id === selectedTaskId ? 'true' : undefined"
              @click="select(item.id)"
            >
              <span class="dtb-card-top">
                <span class="dtb-card-title">{{ item.title }}</span>
                <span class="dtb-pill" :class="`tone-${DELEGATE_PRIORITY_VIEW[item.priority].tone}`">
                  {{ DELEGATE_PRIORITY_VIEW[item.priority].label }}
                </span>
              </span>
              <span class="dtb-card-chips">
                <span class="dtb-pill" :class="`tone-${statusOf(item).tone}`">{{ statusOf(item).label }}</span>
                <span v-if="item.isOverdue" class="dtb-pill tone-danger">تجاوزت المدة</span>
              </span>
              <span class="dtb-card-row">
                <span>تاريخ الاستحقاق</span>
                <strong class="dfb-num">{{ formatDue(item) }}</strong>
              </span>
              <span v-if="item.branchName" class="dtb-card-row">
                <span>الفرع</span>
                <strong>{{ item.branchName }}</strong>
              </span>
              <span v-if="descriptionPreview(item.description)" class="dtb-card-desc">
                {{ descriptionPreview(item.description) }}
              </span>
              <span v-if="item.rejectNote" class="dtb-card-reject">
                <strong>ملاحظة الرفض:</strong> {{ item.rejectNote }}
              </span>
            </button>
          </li>
        </ul>
        <p v-if="truncated" class="dfb-hint">يُعرض أول {{ visibleItems.length }} مهمة؛ استخدم البحث لتضييق النتائج.</p>
      </div>

      <DelegateTaskDetail
        class="dtb-detail"
        :detail="detail"
        :loading="detailQuery.isLoading.value"
        :error="detailQuery.isError.value"
        :status="detailTask ? statusOf(detailTask) : null"
        :actions="detailActions"
        :busy="actionMutation.isPending.value"
        :downloading-id="downloadingId"
        :can-download="canReadAttachment"
        @action="onAction"
        @download="onDownload"
      />
    </div>

    <NeuDialog
      :visible="confirmAction != null"
      :title="confirmView.title"
      size="sm"
      :busy="actionMutation.isPending.value"
      @close="confirmAction = null"
    >
      <p class="dtb-dialog-text">{{ confirmView.message }}</p>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="actionMutation.isPending.value" @click="confirmAction = null">إلغاء</button>
        <button type="button" class="dfb-button is-primary" :disabled="actionMutation.isPending.value" @click="submitConfirm">
          {{ actionMutation.isPending.value ? 'جاري التنفيذ…' : confirmView.title }}
        </button>
      </template>
    </NeuDialog>

    <NeuDialog
      :visible="rejectOpen"
      title="رفض المهمة"
      subtitle="ملاحظة الرفض مطلوبة."
      size="sm"
      :busy="actionMutation.isPending.value"
      @close="rejectOpen = false"
    >
      <label class="dtb-field">
        <span>الملاحظة *</span>
        <textarea
          v-model="rejectNote"
          class="dfb-search dtb-textarea"
          rows="4"
          maxlength="1000"
          :aria-invalid="rejectTouched && !rejectNote.trim()"
          :disabled="actionMutation.isPending.value"
        />
      </label>
      <p v-if="rejectTouched && !rejectNote.trim()" class="dfb-error" role="alert">ملاحظة الرفض مطلوبة</p>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="actionMutation.isPending.value" @click="rejectOpen = false">إلغاء</button>
        <button type="button" class="dfb-button is-primary dtb-danger" :disabled="actionMutation.isPending.value" @click="submitReject">رفض</button>
      </template>
    </NeuDialog>

    <NeuDialog
      :visible="transferOpen"
      title="نقل إلى إدارة الفرع"
      :subtitle="detailTask?.branchName ?? undefined"
      size="sm"
      :busy="actionMutation.isPending.value"
      @close="transferOpen = false"
    >
      <p v-if="managersQuery.isLoading.value" class="dfb-state">جاري تحميل مستخدمي إدارة الفرع…</p>
      <p v-else-if="managersQuery.isError.value" class="dfb-error" role="alert">تعذر تحميل مستخدمي إدارة الفرع.</p>
      <p v-else-if="!managerOptions.length" class="dfb-state">لا يوجد مستخدمو إدارة فرع نشطون</p>
      <ul v-else class="dtb-pick" role="radiogroup" aria-label="مستخدم إدارة الفرع">
        <li v-for="m in managerOptions" :key="m.userId">
          <button
            type="button"
            role="radio"
            class="dtb-pick-item"
            :aria-checked="transferTarget === m.userId"
            @click="transferTarget = m.userId"
          >
            <strong>{{ m.displayName }}</strong>
            <small v-if="m.username">{{ m.username }}</small>
          </button>
        </li>
      </ul>
      <label class="dtb-field">
        <span>ملاحظة (اختياري)</span>
        <textarea v-model="transferNote" class="dfb-search dtb-textarea" rows="3" maxlength="1000" :disabled="actionMutation.isPending.value" />
      </label>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="actionMutation.isPending.value" @click="transferOpen = false">إلغاء</button>
        <button
          type="button"
          class="dfb-button is-primary"
          :disabled="actionMutation.isPending.value || transferTarget == null"
          @click="submitTransfer"
        >
          {{ actionMutation.isPending.value ? 'جاري التنفيذ…' : 'نقل' }}
        </button>
      </template>
    </NeuDialog>
  </section>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/vue-query'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { useAuthStore } from '@/modules/Auth/store'
import { DELEGATE_SUMMARY_KEY, useDelegateBranchId } from '@/modules/DelegateManager/composables/useDelegateSummary'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { ManagementTaskEndpoints } from '../domain/endpoints'
import {
  MANAGEMENT_TASK_BRANCH_MANAGER_AUDIENCE,
  ManagementTaskStatus,
  type ManagementTaskAttachment,
  type ManagementTaskListItem,
  type ManagementTaskPaginated,
  type ManagementTaskSimpleAction,
} from '../domain/types'
import { useManagementTaskAccess } from '../composables/useManagementTaskAccess'
import {
  DELEGATE_FILTERS,
  DELEGATE_PRIORITY_VIEW,
  delegateStatusView,
  descriptionPreview,
  formatDue,
  type DelegateTaskFilter,
} from '../composables/delegateTaskView'
import { MANAGEMENT_TASKS_LIST_KEY } from '../requests/queries/useManagementTasksQuery'
import { useManagementTaskDetailQuery } from '../requests/queries/useManagementTaskDetailQuery'
import { useManagementTaskAssigneesQuery } from '../requests/queries/useManagementTaskAssigneesQuery'
import { useManagementTaskActionMutation } from '../requests/mutations/useManagementTaskActionMutation'
import { downloadManagementTaskAttachment } from '../requests/mutations/downloadManagementTaskAttachment'
import { useManagementTaskRealtime } from '../realtime/managementTaskRealtime'
import DelegateTaskDetail, { type DelegateTaskActions } from './DelegateTaskDetail.vue'

const OPEN_PAGE_SIZE = 100
const DONE_PAGE_SIZE = 50

const route = useRoute()
const auth = useAuthStore()
const toast = useToastStore()
const qc = useQueryClient()
const { canRead, canUseInbox, canReadAttachment, currentUserId, actionsFor } = useManagementTaskAccess()

const canEnter = computed(() => canRead.value || canUseInbox.value)
const branchId = useDelegateBranchId()
const filter = ref<DelegateTaskFilter>('all')
const searchInput = ref('')
const search = ref('')

let searchTimer: ReturnType<typeof setTimeout> | undefined
watch(searchInput, (value) => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    search.value = value.trim()
  }, 350)
})
onBeforeUnmount(() => clearTimeout(searchTimer))

function fetchInbox(status: number | null, pageSize: number) {
  return axiosInstance
    .get<ManagementTaskPaginated<ManagementTaskListItem>>(ManagementTaskEndpoints.list, {
      params: {
        pageIndex: 1,
        pageSize,
        inboxForMe: true,
        includeTransferredByMe: true,
        branchId: branchId.value ?? undefined,
        status: status ?? undefined,
        search: search.value || undefined,
      },
    })
    .then((r) => r.data)
}

const queryEnabled = computed(() => canEnter.value && auth.permissionsReady)
const openQuery = useQuery({
  queryKey: computed(() => [MANAGEMENT_TASKS_LIST_KEY, 'delegate-open', auth.sessionGeneration, branchId.value, search.value]),
  enabled: queryEnabled,
  placeholderData: keepPreviousData,
  queryFn: () => fetchInbox(null, OPEN_PAGE_SIZE),
})
const doneQuery = useQuery({
  queryKey: computed(() => [MANAGEMENT_TASKS_LIST_KEY, 'delegate-done', auth.sessionGeneration, branchId.value, search.value]),
  enabled: queryEnabled,
  placeholderData: keepPreviousData,
  queryFn: () => fetchInbox(ManagementTaskStatus.Completed, DONE_PAGE_SIZE),
})

const openItems = computed(() => openQuery.data.value?.items ?? [])
const doneItems = computed(() => doneQuery.data.value?.items ?? [])
const listLoading = computed(() => openQuery.isLoading.value || (filter.value === 'completed' && doneQuery.isLoading.value))
const listError = computed(() => openQuery.isError.value)
const truncated = computed(() =>
  filter.value === 'completed'
    ? (doneQuery.data.value?.totalCount ?? 0) > doneItems.value.length
    : (openQuery.data.value?.totalCount ?? 0) > openItems.value.length
)

function statusOf(task: ManagementTaskListItem) {
  return delegateStatusView(task, currentUserId.value)
}

const openByFilter = computed(() => {
  const map = new Map<DelegateTaskFilter, ManagementTaskListItem[]>()
  for (const item of openItems.value) {
    const key = statusOf(item).key
    map.set(key, [...(map.get(key) ?? []), item])
  }
  return map
})

const visibleItems = computed(() => {
  if (filter.value === 'all') return openItems.value
  if (filter.value === 'completed') return doneItems.value
  return openByFilter.value.get(filter.value) ?? []
})

function countFor(key: DelegateTaskFilter): number {
  if (key === 'all') return openQuery.data.value?.totalCount ?? 0
  if (key === 'completed') return doneQuery.data.value?.totalCount ?? 0
  return openByFilter.value.get(key)?.length ?? 0
}

const selectedTaskId = ref<number | null>(null)
function select(id: number) {
  selectedTaskId.value = id
}

watch(
  visibleItems,
  (items) => {
    if (selectedTaskId.value != null && items.some((i) => i.id === selectedTaskId.value)) return
    selectedTaskId.value = items[0]?.id ?? null
  },
  { immediate: true }
)

const { statusLabel: connectionLabel } = useManagementTaskRealtime({
  enabled: queryEnabled,
  openTaskId: selectedTaskId,
})

const detailQuery = useManagementTaskDetailQuery(selectedTaskId)
const detail = computed(() =>
  detailQuery.data.value && detailQuery.data.value.task.id === selectedTaskId.value ? detailQuery.data.value : null
)
const detailTask = computed(() => detail.value?.task ?? null)

const detailActions = computed<DelegateTaskActions>(() => {
  const task = detailTask.value
  const none = { accept: false, reject: false, complete: false, transfer: false, confirm: false }
  if (!task) return none
  const a = actionsFor(task)
  return { accept: a.accept, reject: a.reject, complete: a.complete, transfer: a.transfer, confirm: a.confirm }
})

const actionMutation = useManagementTaskActionMutation()
const confirmAction = ref<ManagementTaskSimpleAction | null>(null)
const rejectOpen = ref(false)
const rejectNote = ref('')
const rejectTouched = ref(false)
const transferOpen = ref(false)
const transferTarget = ref<number | null>(null)
const transferNote = ref('')
const downloadingId = ref<number | null>(null)

const confirmView = computed(() => {
  const title = detailTask.value?.title ?? ''
  switch (confirmAction.value) {
    case 'complete':
      return { title: 'تم الإنجاز', message: `تأكيد إنجاز «${title}»؟` }
    case 'confirm':
      return { title: 'تصديق', message: `تأكيد تصديق إنجاز «${title}» بعد إنجاز إدارة الفرع؟` }
    default:
      return { title: 'قبول', message: `هل تريد قبول «${title}»؟` }
  }
})

const transferBranchId = computed(() => detailTask.value?.branchId ?? null)
const managersQuery = useManagementTaskAssigneesQuery(
  transferBranchId,
  transferOpen,
  ref(MANAGEMENT_TASK_BRANCH_MANAGER_AUDIENCE)
)
const managerOptions = computed(() =>
  (managersQuery.data.value ?? []).filter(
    (m) => m.userId !== currentUserId.value && m.userId !== detailTask.value?.currentHolderUserId
  )
)

function onAction(action: keyof DelegateTaskActions) {
  if (actionMutation.isPending.value || !detailTask.value) return
  if (action === 'reject') {
    rejectNote.value = ''
    rejectTouched.value = false
    rejectOpen.value = true
    return
  }
  if (action === 'transfer') {
    transferTarget.value = null
    transferNote.value = ''
    transferOpen.value = true
    return
  }
  confirmAction.value = action
}

function notify(title: string) {
  toast.setMassage({ title, dialogState: DialogState.Success, isOpen: true })
}

function refreshCounters() {
  void qc.invalidateQueries({ queryKey: [DELEGATE_SUMMARY_KEY] })
}

async function run(
  args: Parameters<typeof actionMutation.mutateAsync>[0],
  done: () => void
): Promise<void> {
  try {
    await actionMutation.mutateAsync(args)
    done()
    notify('تم تحديث المهمة')
  } catch {
    void 0
  } finally {
    refreshCounters()
  }
}

async function submitConfirm() {
  const task = detailTask.value
  const action = confirmAction.value
  if (!task || !action || actionMutation.isPending.value) return
  await run({ taskId: task.id, action, expectedVersion: task.expectedVersion }, () => {
    confirmAction.value = null
  })
  confirmAction.value = null
}

async function submitReject() {
  rejectTouched.value = true
  const task = detailTask.value
  const note = rejectNote.value.trim()
  if (!task || !note || actionMutation.isPending.value) return
  await run({ taskId: task.id, action: 'reject', expectedVersion: task.expectedVersion, note }, () => {
    rejectOpen.value = false
  })
}

async function submitTransfer() {
  const task = detailTask.value
  if (!task || transferTarget.value == null || actionMutation.isPending.value) return
  await run(
    {
      taskId: task.id,
      action: 'transfer',
      expectedVersion: task.expectedVersion,
      targetUserId: transferTarget.value,
      note: transferNote.value.trim() || undefined,
    },
    () => {
      transferOpen.value = false
    }
  )
}

async function onDownload(file: ManagementTaskAttachment) {
  const task = detailTask.value
  if (!task || downloadingId.value != null) return
  downloadingId.value = file.id
  try {
    await downloadManagementTaskAttachment(task.id, file.id, file.fileName)
  } catch {
    void 0
  } finally {
    downloadingId.value = null
  }
}

watch(branchId, () => {
  confirmAction.value = null
  rejectOpen.value = false
  transferOpen.value = false
})
</script>

<style scoped>
.dtb {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.dtb-head {
  align-items: flex-end;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
}

.dtb-search {
  width: min(100%, 320px);
}

.dtb-layout {
  align-items: start;
  display: grid;
  gap: 20px;
  grid-template-columns: minmax(300px, 380px) minmax(0, 1fr);
}

.dtb-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-block-size: calc(100vh - 220px);
  overflow: auto;
  padding: 6px;
}

.dtb-cards {
  display: flex;
  flex-direction: column;
  gap: 12px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dtb-card {
  background: var(--dfb-raised);
  border: 2px solid transparent;
  border-radius: 18px;
  box-shadow: var(--dfb-up);
  color: inherit;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  font: inherit;
  gap: 8px;
  padding: 14px 16px;
  text-align: start;
  transition: box-shadow 0.18s ease;
  width: 100%;
}

.dtb-card:hover {
  box-shadow: var(--dfb-up-hover);
}

.dtb-card.is-active {
  background: var(--dfb-primary-soft);
  border-color: var(--dfb-primary);
  box-shadow: var(--dfb-down);
}

.dtb-card-top {
  align-items: flex-start;
  display: flex;
  gap: 10px;
  justify-content: space-between;
}

.dtb-card-title {
  font-size: 15px;
  font-weight: 700;
  line-height: 1.45;
}

.dtb-card-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.dtb-card-row {
  color: var(--dfb-muted);
  display: flex;
  font-size: 13px;
  gap: 8px;
  justify-content: space-between;
}

.dtb-card-row strong {
  color: var(--dfb-ink);
}

.dtb-card-desc {
  color: var(--dfb-muted);
  font-size: 13px;
  line-height: 1.55;
}

.dtb-card-reject {
  background: var(--dfb-danger-soft);
  border-radius: 12px;
  color: var(--dfb-tier-bad);
  font-size: 13px;
  padding: 6px 10px;
}

.dtb-pill {
  border-radius: 999px;
  flex: none;
  font-size: 12px;
  font-weight: 700;
  line-height: 1;
  padding: 6px 10px;
}

.dtb-pill.tone-info { background: #e3edf8; color: var(--dfb-tier-good); }
.dtb-pill.tone-muted { background: #ecefee; color: var(--dfb-tier-lagging); }
.dtb-pill.tone-teal { background: var(--dfb-primary-soft); color: var(--dfb-tier-excellent); }
.dtb-pill.tone-warn { background: var(--dfb-warn-soft); color: var(--dfb-tier-weak); }
.dtb-pill.tone-danger { background: var(--dfb-danger-soft); color: var(--dfb-tier-bad); }
.dtb-pill.tone-good { background: var(--dfb-good-soft); color: var(--dfb-tier-excellent); }

.dtb-dialog-text {
  line-height: 1.7;
  margin: 0;
}

.dtb-field {
  display: flex;
  flex-direction: column;
  font-size: 14px;
  font-weight: 600;
  gap: 6px;
  margin-top: 12px;
}

.dtb-textarea {
  min-height: 88px;
  padding: 10px 14px;
  resize: vertical;
  width: 100%;
}

.dtb-danger.is-primary {
  background: var(--dfb-tier-bad);
}

.dtb-pick {
  display: flex;
  flex-direction: column;
  gap: 8px;
  list-style: none;
  margin: 0;
  max-block-size: 280px;
  overflow: auto;
  padding: 6px;
}

.dtb-pick-item {
  background: var(--dfb-raised);
  border: 2px solid transparent;
  border-radius: 14px;
  box-shadow: var(--dfb-up);
  color: inherit;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  font: inherit;
  gap: 2px;
  padding: 10px 14px;
  text-align: start;
  width: 100%;
}

.dtb-pick-item small {
  color: var(--dfb-muted);
}

.dtb-pick-item[aria-checked='true'] {
  background: var(--dfb-primary-soft);
  border-color: var(--dfb-primary);
  box-shadow: var(--dfb-down);
}

@media (max-width: 900px) {
  .dtb-layout {
    grid-template-columns: minmax(0, 1fr);
  }

  .dtb-list {
    max-block-size: none;
  }
}
</style>
