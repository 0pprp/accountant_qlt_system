import { describe, expect, it } from 'vitest'
import {
  adoptMergedTabBaseline,
  buildTabAccessChanges,
  collectTabConflicts,
  dirtyTabCount,
  expectedVersionForTabSave,
  mergeTabDraftWithServer,
  resetTabToRole,
  snapshotFromTabAccess,
  toggleTabDraft,
} from '../tabAccessFormState'
import type {
  UserTabAccessResponse,
  UserTabAccessRow,
} from '@/modules/User/types/tabAccess'

function row(
  partial: Partial<UserTabAccessRow> & { tabKey: string }
): UserTabAccessRow {
  return {
    parentKey: null,
    displayName: partial.tabKey,
    roleDefaultEnabled: true,
    effectiveEnabled: true,
    override: null,
    ...partial,
  }
}

describe('tabAccessFormState', () => {
  it('baseline version لا يتغير عند وصول server أحدث', () => {
    const baseline = {
      version: 4,
      states: { a: { enabled: true, override: null as const } },
    }
    expect(expectedVersionForTabSave(baseline)).toBe(4)
  })

  it('toggle إلى قيمة الدور يعيد inherit', () => {
    const closed = { enabled: false, override: 'deny' as const }
    expect(toggleTabDraft(closed, true)).toEqual({
      enabled: true,
      override: null,
    })
  })

  it('toggle عكس الدور يضع allow/deny', () => {
    const open = { enabled: true, override: null }
    expect(toggleTabDraft(open, true)).toEqual({
      enabled: false,
      override: 'deny',
    })
  })

  it('resetTabToRole يعتمد افتراض الدور', () => {
    expect(resetTabToRole(false)).toEqual({ enabled: false, override: null })
  })

  it('يجمع تعارضات المسودة والخادم', () => {
    const baseline = {
      version: 1,
      states: {
        a: { enabled: true, override: null as const },
        b: { enabled: false, override: null as const },
      },
    }
    const draft = {
      a: { enabled: false, override: 'deny' as const },
      b: { enabled: false, override: null as const },
    }
    const server = {
      version: 2,
      fullName: 'u',
      roleDisplayName: 'r',
      isProtectedAccount: false,
      canEdit: true,
      tabs: [row({ tabKey: 'a', displayName: 'تبويب أ' })],
      states: {
        // نفس enabled لكن تخصيص مختلف عن baseline والمسودة
        a: { enabled: true, override: 'allow' as const },
        b: { enabled: false, override: null as const },
      },
    }
    const conflicts = collectTabConflicts(draft, baseline, server)
    expect(conflicts).toHaveLength(1)
    expect(conflicts[0].tabKey).toBe('a')
  })

  it('buildTabAccessChanges يرسل null للاعتماد و true/false للتخصيص', () => {
    const tabs = [
      row({ tabKey: 'a', roleDefaultEnabled: true, override: 'deny' }),
      row({ tabKey: 'b', roleDefaultEnabled: false, override: null }),
    ]
    const baseline = {
      version: 3,
      states: {
        a: { enabled: false, override: 'deny' as const },
        b: { enabled: false, override: null as const },
      },
    }
    const draft = {
      a: { enabled: true, override: null as const },
      b: { enabled: true, override: 'allow' as const },
    }
    expect(dirtyTabCount(draft, baseline)).toBe(2)
    expect(buildTabAccessChanges(draft, baseline, tabs)).toEqual([
      { tabKey: 'a', enabled: null },
      { tabKey: 'b', enabled: true },
    ])
  })

  it('merge + adopt يبقي المسودة مع إصدار الخادم', () => {
    const baseline = {
      version: 1,
      states: { a: { enabled: true, override: null as const } },
    }
    const draft = { a: { enabled: false, override: 'deny' as const } }
    const server = {
      version: 5,
      fullName: 'u',
      roleDisplayName: 'r',
      isProtectedAccount: false,
      canEdit: true,
      tabs: [row({ tabKey: 'a' })],
      states: { a: { enabled: true, override: null as const } },
    }
    const merged = mergeTabDraftWithServer(draft, baseline, server, {})
    expect(merged.ok).toBe(true)
    if (merged.ok === false) return
    const adopted = adoptMergedTabBaseline(server, merged.states)
    expect(adopted.baseline.version).toBe(5)
    expect(adopted.draft.a.enabled).toBe(false)
  })

  it('snapshotFromTabAccess يبني الحالات من الفعّال', () => {
    const data: UserTabAccessResponse = {
      userId: 9,
      fullName: 'أحمد',
      roleDisplayName: 'مدير',
      isProtectedAccount: false,
      permissionsVersion: 2,
      canEdit: true,
      tabs: [
        row({
          tabKey: 'branch',
          effectiveEnabled: false,
          override: 'deny',
          roleDefaultEnabled: true,
        }),
      ],
    }
    const snap = snapshotFromTabAccess(data)
    expect(snap.states.branch).toEqual({ enabled: false, override: 'deny' })
    expect(snap.roleDisplayName).toBe('مدير')
  })
})
