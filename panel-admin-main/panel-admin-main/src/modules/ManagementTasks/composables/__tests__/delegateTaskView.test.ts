import { describe, expect, it } from 'vitest'
import {
  buildDelegateTimeline,
  delegateStatusView,
  descriptionPreview,
  formatDateTime12h,
  formatDue,
} from '../delegateTaskView'
import { ManagementTaskStatus, type ManagementTaskDetail, type ManagementTaskListItem } from '../../domain/types'

function task(overrides: Partial<ManagementTaskListItem> = {}): ManagementTaskListItem {
  return {
    id: 1,
    title: 'مهمة',
    description: '',
    status: ManagementTaskStatus.Sent,
    priority: 1,
    dueDate: '2026-11-20',
    dueAtUtc: null,
    isOverdue: false,
    branchId: 1,
    branchName: 'بغداد',
    createdByUserId: 2,
    createdByDisplayName: 'الإدارة',
    assigneeUserId: 10,
    assigneeDisplayName: 'المفوض',
    currentHolderUserId: 10,
    currentHolderDisplayName: 'المفوض',
    rejectNote: null,
    customerId: null,
    orderId: null,
    expectedVersion: 0,
    createdAt: '2026-11-18T06:00:00Z',
    statusChangedAtUtc: null,
    transferredAtUtc: null,
    branchCompletedAtUtc: null,
    assigneeCompletedAtUtc: null,
    confirmedAtUtc: null,
    ...overrides,
  }
}

describe('delegateTaskView', () => {
  it('labels awaiting confirm for the delegate', () => {
    expect(delegateStatusView(task({ status: ManagementTaskStatus.AwaitingDelegatedConfirm }), 10).label).toBe('بانتظار تصديقك')
  })

  it('shows a task accepted by the branch after transfer as transferred', () => {
    const view = delegateStatusView(
      task({ status: ManagementTaskStatus.Accepted, transferredAtUtc: '2026-11-18T07:00:00Z', currentHolderUserId: 20 }),
      10
    )
    expect(view).toMatchObject({ key: 'transferred', label: 'منقولة', tone: 'muted' })
  })

  it('keeps accepted when the delegate holds the task', () => {
    expect(delegateStatusView(task({ status: ManagementTaskStatus.Accepted }), 10).key).toBe('accepted')
  })

  it('cuts the description preview at 90 characters', () => {
    const long = 'أ'.repeat(120)
    expect(descriptionPreview(long)).toBe(`${'أ'.repeat(90)}…`)
    expect(descriptionPreview('  قصير  ')).toBe('قصير')
  })

  it('formats times in Baghdad with 12h suffix', () => {
    expect(formatDateTime12h('2026-11-18T06:05:00Z')).toBe('2026/11/18 9:05 ص')
    expect(formatDateTime12h('2026-11-18T12:30:00Z')).toBe('2026/11/18 3:30 م')
    expect(formatDue(task())).toBe('2026/11/20')
    expect(formatDue(task({ dueAtUtc: '2026-11-20T09:00:00Z' }))).toBe('2026/11/20 12:00 م')
  })

  it('builds a chronological timeline from stamps and events', () => {
    const detail: ManagementTaskDetail = {
      task: task({
        status: ManagementTaskStatus.Completed,
        transferredAtUtc: '2026-11-18T08:00:00Z',
        branchCompletedAtUtc: '2026-11-18T10:00:00Z',
        confirmedAtUtc: '2026-11-18T11:00:00Z',
      }),
      events: [
        { id: 5, eventType: 3, atUtc: '2026-11-18T07:00:00Z', actorUserId: 10, actorDisplayName: 'المفوض', targetUserId: null, targetDisplayName: null, note: null, previousStatus: 0, newStatus: 2 },
        { id: 6, eventType: 5, atUtc: '2026-11-18T08:00:00Z', actorUserId: 10, actorDisplayName: 'المفوض', targetUserId: 20, targetDisplayName: 'مدير الفرع', note: 'عاجل', previousStatus: 2, newStatus: 1 },
      ],
      attachments: [],
    }
    const keys = buildDelegateTimeline(detail).map((e) => e.key)
    expect(keys).toEqual(['created', 'accepted-5', 'transferred', 'branch-completed', 'confirmed'])
    const transfer = buildDelegateTimeline(detail).find((e) => e.key === 'transferred')
    expect(transfer?.label).toContain('مدير الفرع')
    expect(transfer?.note).toBe('عاجل')
  })
})
