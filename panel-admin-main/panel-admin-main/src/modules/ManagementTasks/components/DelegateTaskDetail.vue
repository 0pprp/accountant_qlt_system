<template>
  <aside class="dtd" aria-label="تفاصيل المهمة" aria-live="polite">
    <p v-if="loading && !detail" class="dfb-state">جاري تحميل التفاصيل…</p>
    <p v-else-if="error && !detail" class="dfb-error" role="alert">تعذر تحميل تفاصيل المهمة.</p>
    <p v-else-if="!detail" class="dfb-state">اختر مهمة من القائمة لعرض تفاصيلها.</p>

    <template v-else>
      <header class="dtd-head">
        <h3 class="dtd-title">{{ detail.task.title }}</h3>
        <div class="dtd-chips">
          <span class="dtb-pill" :class="`tone-${priority.tone}`">{{ priority.label }}</span>
          <span v-if="status" class="dtb-pill" :class="`tone-${status.tone}`">{{ status.label }}</span>
          <span v-if="detail.task.isOverdue" class="dtb-pill tone-danger">تجاوزت المدة</span>
        </div>
      </header>

      <dl class="dtd-facts">
        <div>
          <dt>تاريخ الاستحقاق</dt>
          <dd class="dfb-num" :class="{ 'is-overdue': detail.task.isOverdue }">{{ formatDue(detail.task) }}</dd>
        </div>
        <div>
          <dt>المرسل</dt>
          <dd>{{ detail.task.createdByDisplayName || '—' }}</dd>
        </div>
        <div>
          <dt>الفرع</dt>
          <dd>{{ detail.task.branchName || '—' }}</dd>
        </div>
        <div v-if="detail.task.currentHolderUserId !== detail.task.assigneeUserId">
          <dt>لدى</dt>
          <dd>{{ detail.task.currentHolderDisplayName }}</dd>
        </div>
      </dl>

      <section class="dtd-card">
        <h4 class="dtd-card-title">وصف المهمة</h4>
        <p v-if="description" class="dtd-desc">{{ description }}</p>
        <p v-else class="dtd-desc is-empty">لا يوجد وصف لهذه المهمة.</p>
      </section>

      <section v-if="detail.task.rejectNote" class="dtd-card dtd-reject">
        <h4 class="dtd-card-title">ملاحظة الرفض</h4>
        <p class="dtd-desc">{{ detail.task.rejectNote }}</p>
      </section>

      <div v-if="hasActions" class="dtd-actions">
        <button v-if="actions.accept" type="button" class="dfb-button is-primary" :disabled="busy" @click="emit('action', 'accept')">قبول</button>
        <button v-if="actions.reject" type="button" class="dfb-button dtd-reject-btn" :disabled="busy" @click="emit('action', 'reject')">رفض</button>
        <button v-if="actions.complete" type="button" class="dfb-button is-primary" :disabled="busy" @click="emit('action', 'complete')">تم الإنجاز</button>
        <button v-if="actions.transfer" type="button" class="dfb-button" :disabled="busy" @click="emit('action', 'transfer')">نقل إلى إدارة الفرع</button>
        <button v-if="actions.confirm" type="button" class="dfb-button is-primary" :disabled="busy" @click="emit('action', 'confirm')">تصديق</button>
      </div>

      <section class="dtd-card">
        <h4 class="dtd-card-title">المراحل</h4>
        <ol class="dtd-timeline">
          <li v-for="entry in timeline" :key="entry.key" :class="`tone-${entry.tone}`">
            <strong>{{ entry.label }}</strong>
            <small class="dfb-num">{{ formatDateTime12h(entry.at) }}<template v-if="entry.by"> · {{ entry.by }}</template></small>
            <p v-if="entry.note">{{ entry.note }}</p>
          </li>
          <li class="tone-warn">
            <strong>الاستحقاق</strong>
            <small class="dfb-num">{{ formatDue(detail.task) }}</small>
          </li>
        </ol>
      </section>

      <section class="dtd-card">
        <h4 class="dtd-card-title">المرفقات</h4>
        <p v-if="!detail.attachments.length" class="dtd-desc is-empty">لا توجد مرفقات</p>
        <ul v-else class="dtd-files">
          <li v-for="file in detail.attachments" :key="file.id">
            <span class="dtd-file-name">{{ file.fileName }}</span>
            <button
              v-if="canDownload"
              type="button"
              class="dfb-button is-small"
              :disabled="downloadingId === file.id"
              @click="emit('download', file)"
            >
              {{ downloadingId === file.id ? 'جاري التنزيل…' : 'تنزيل' }}
            </button>
          </li>
        </ul>
      </section>
    </template>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ManagementTaskAttachment, ManagementTaskDetail } from '../domain/types'
import {
  DELEGATE_PRIORITY_VIEW,
  buildDelegateTimeline,
  formatDateTime12h,
  formatDue,
  type DelegateTaskStatusView,
} from '../composables/delegateTaskView'

export interface DelegateTaskActions {
  accept: boolean
  reject: boolean
  complete: boolean
  transfer: boolean
  confirm: boolean
}

const props = defineProps<{
  detail: ManagementTaskDetail | null
  loading?: boolean
  error?: boolean
  status: DelegateTaskStatusView | null
  actions: DelegateTaskActions
  busy?: boolean
  downloadingId?: number | null
  canDownload?: boolean
}>()

const emit = defineEmits<{
  action: [keyof DelegateTaskActions]
  download: [ManagementTaskAttachment]
}>()

const priority = computed(() =>
  props.detail ? DELEGATE_PRIORITY_VIEW[props.detail.task.priority] : DELEGATE_PRIORITY_VIEW[2]
)
const description = computed(() => props.detail?.task.description.trim() ?? '')
const timeline = computed(() => (props.detail ? buildDelegateTimeline(props.detail) : []))
const hasActions = computed(() => Object.values(props.actions).some(Boolean))
</script>

<style scoped>
.dtd {
  background: var(--dfb-well);
  border-radius: 20px;
  box-shadow: var(--dfb-down);
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-block-size: 320px;
  padding: clamp(14px, 2vw, 22px);
}

.dtd-head {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.dtd-title {
  font-size: 19px;
  font-weight: 700;
  line-height: 1.45;
  margin: 0;
}

.dtd-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.dtd-facts {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 160px), 1fr));
  margin: 0;
}

.dtd-facts > div {
  background: var(--dfb-raised);
  border-radius: 14px;
  box-shadow: var(--dfb-up);
  padding: 10px 14px;
}

.dtd-facts dt {
  color: var(--dfb-muted);
  font-size: 12px;
  font-weight: 600;
}

.dtd-facts dd {
  font-weight: 700;
  margin: 2px 0 0;
}

.dtd-facts dd.is-overdue {
  color: var(--dfb-tier-bad);
}

.dtd-card {
  background: var(--dfb-raised);
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  padding: 14px 16px;
}

.dtd-card-title {
  color: var(--dfb-primary);
  font-size: 15px;
  font-weight: 700;
  margin: 0 0 8px;
}

.dtd-desc {
  line-height: 1.7;
  margin: 0;
  white-space: pre-wrap;
}

.dtd-desc.is-empty {
  color: var(--dfb-muted);
}

.dtd-reject {
  background: var(--dfb-danger-soft);
}

.dtd-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.dtd-reject-btn {
  color: var(--dfb-tier-bad);
}

.dtd-timeline {
  display: flex;
  flex-direction: column;
  gap: 12px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dtd-timeline li {
  border-inline-start: 3px solid var(--dfb-muted);
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding-inline-start: 12px;
}

.dtd-timeline li.tone-info { border-color: var(--dfb-tier-good); }
.dtd-timeline li.tone-teal,
.dtd-timeline li.tone-good { border-color: var(--dfb-tier-excellent); }
.dtd-timeline li.tone-warn { border-color: var(--dfb-tier-weak); }
.dtd-timeline li.tone-danger { border-color: var(--dfb-tier-bad); }

.dtd-timeline small {
  color: var(--dfb-muted);
}

.dtd-timeline p {
  margin: 2px 0 0;
}

.dtd-files {
  display: flex;
  flex-direction: column;
  gap: 8px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dtd-files li {
  align-items: center;
  display: flex;
  gap: 10px;
  justify-content: space-between;
}

.dtd-file-name {
  overflow-wrap: anywhere;
}

.dtb-pill {
  border-radius: 999px;
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
</style>
