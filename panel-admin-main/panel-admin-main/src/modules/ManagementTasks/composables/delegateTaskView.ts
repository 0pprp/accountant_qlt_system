import {
  ManagementTaskPriority,
  ManagementTaskStatus,
  type ManagementTaskDetail,
  type ManagementTaskListItem,
  type ManagementTaskPriorityValue,
} from '../domain/types'

export type DelegateTaskTone = 'info' | 'muted' | 'teal' | 'warn' | 'danger' | 'good'

export type DelegateTaskFilter =
  | 'all'
  | 'sent'
  | 'accepted'
  | 'awaiting'
  | 'transferred'
  | 'returned'
  | 'completed'

export interface DelegateTaskStatusView {
  key: Exclude<DelegateTaskFilter, 'all'>
  label: string
  tone: DelegateTaskTone
}

export const DELEGATE_PRIORITY_VIEW: Record<
  ManagementTaskPriorityValue,
  { label: string; tone: DelegateTaskTone }
> = {
  [ManagementTaskPriority.High]: { label: 'عالية', tone: 'danger' },
  [ManagementTaskPriority.Medium]: { label: 'متوسطة', tone: 'warn' },
  [ManagementTaskPriority.Low]: { label: 'ضعيفة', tone: 'muted' },
}

export const DELEGATE_FILTERS: ReadonlyArray<{ key: DelegateTaskFilter; label: string }> = [
  { key: 'all', label: 'الكل' },
  { key: 'sent', label: 'مرسلة' },
  { key: 'accepted', label: 'مقبولة' },
  { key: 'awaiting', label: 'بانتظار تصديقك' },
  { key: 'transferred', label: 'منقولة' },
  { key: 'returned', label: 'معادة' },
  { key: 'completed', label: 'منجزة' },
]

export function isHeldByBranch(task: ManagementTaskListItem, me: number | null): boolean {
  if (task.transferredAtUtc == null) return false
  if (me != null && task.currentHolderUserId === me) return false
  return (
    task.status === ManagementTaskStatus.Transferred ||
    task.status === ManagementTaskStatus.Accepted
  )
}

export function delegateStatusView(
  task: ManagementTaskListItem,
  me: number | null
): DelegateTaskStatusView {
  if (isHeldByBranch(task, me)) return { key: 'transferred', label: 'منقولة', tone: 'muted' }
  switch (task.status) {
    case ManagementTaskStatus.Sent:
      return { key: 'sent', label: 'مرسلة', tone: 'info' }
    case ManagementTaskStatus.Transferred:
      return { key: 'transferred', label: 'منقولة', tone: 'muted' }
    case ManagementTaskStatus.Accepted:
      return { key: 'accepted', label: 'مقبولة', tone: 'teal' }
    case ManagementTaskStatus.AwaitingDelegatedConfirm:
      return { key: 'awaiting', label: 'بانتظار تصديقك', tone: 'warn' }
    case ManagementTaskStatus.Returned:
      return { key: 'returned', label: 'معادة', tone: 'danger' }
    default:
      return { key: 'completed', label: 'منجزة', tone: 'good' }
  }
}

export function descriptionPreview(text: string | null | undefined, max = 90): string {
  const value = (text ?? '').trim()
  if (value.length <= max) return value
  return `${value.slice(0, max)}…`
}

const BAGHDAD = 'Asia/Baghdad'

function baghdadParts(d: Date) {
  const parts = new Intl.DateTimeFormat('en-US', {
    timeZone: BAGHDAD,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }).formatToParts(d)
  const get = (type: string) => parts.find((p) => p.type === type)?.value ?? '00'
  return {
    year: get('year'),
    month: get('month'),
    day: get('day'),
    hour: Number(get('hour')) % 24,
    minute: get('minute'),
  }
}

export function formatDateTime12h(value: string | null | undefined): string {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const p = baghdadParts(d)
  const suffix = p.hour < 12 ? 'ص' : 'م'
  const hour12 = p.hour % 12 === 0 ? 12 : p.hour % 12
  return `${p.year}/${p.month}/${p.day} ${hour12}:${p.minute} ${suffix}`
}

export function formatDay(value: string | null | undefined): string {
  if (!value) return '—'
  const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(value)
  return m ? `${m[1]}/${m[2]}/${m[3]}` : '—'
}

export function formatDue(task: Pick<ManagementTaskListItem, 'dueAtUtc' | 'dueDate'>): string {
  return task.dueAtUtc ? formatDateTime12h(task.dueAtUtc) : formatDay(task.dueDate)
}

export interface DelegateTimelineEntry {
  key: string
  label: string
  at: string
  by: string | null
  note: string | null
  tone: DelegateTaskTone
}

const ACCEPTED_EVENT = 3
const REJECTED_EVENT = 4
const TRANSFERRED_EVENT = 5

export function buildDelegateTimeline(detail: ManagementTaskDetail): DelegateTimelineEntry[] {
  const task = detail.task
  const entries: DelegateTimelineEntry[] = []
  if (task.createdAt) {
    entries.push({
      key: 'created',
      label: 'أُرسلت المهمة',
      at: task.createdAt,
      by: task.createdByDisplayName,
      note: null,
      tone: 'info',
    })
  }
  for (const e of detail.events) {
    if (e.eventType === ACCEPTED_EVENT) {
      entries.push({ key: `accepted-${e.id}`, label: 'قُبلت', at: e.atUtc, by: e.actorDisplayName, note: null, tone: 'teal' })
    } else if (e.eventType === REJECTED_EVENT) {
      entries.push({ key: `rejected-${e.id}`, label: 'رُفضت', at: e.atUtc, by: e.actorDisplayName, note: e.note, tone: 'danger' })
    }
  }
  if (task.transferredAtUtc) {
    const transfer = [...detail.events].reverse().find((e) => e.eventType === TRANSFERRED_EVENT)
    entries.push({
      key: 'transferred',
      label: transfer?.targetDisplayName ? `نُقلت إلى إدارة الفرع · ${transfer.targetDisplayName}` : 'نُقلت إلى إدارة الفرع',
      at: task.transferredAtUtc,
      by: transfer?.actorDisplayName ?? null,
      note: transfer?.note ?? null,
      tone: 'muted',
    })
  }
  if (task.branchCompletedAtUtc) {
    entries.push({ key: 'branch-completed', label: 'أنجزتها إدارة الفرع', at: task.branchCompletedAtUtc, by: null, note: null, tone: 'warn' })
  }
  if (task.assigneeCompletedAtUtc) {
    entries.push({ key: 'assignee-completed', label: 'أنجزها المفوض', at: task.assigneeCompletedAtUtc, by: task.assigneeDisplayName, note: null, tone: 'good' })
  }
  if (task.confirmedAtUtc) {
    entries.push({ key: 'confirmed', label: 'صدّقها المفوض', at: task.confirmedAtUtc, by: task.assigneeDisplayName, note: null, tone: 'good' })
  }
  entries.sort((a, b) => new Date(a.at).getTime() - new Date(b.at).getTime())
  return entries
}
