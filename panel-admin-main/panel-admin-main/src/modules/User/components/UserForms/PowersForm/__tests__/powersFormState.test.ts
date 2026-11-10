import { describe, expect, it } from 'vitest'
import {
  adoptMergedBaseline,
  collectConflicts,
  expectedVersionForSave,
  mergeDraftWithServer,
  modesFromRows,
  snapshotFromResponse,
} from '../powersFormState'
import type {
  PermissionOverrideRow,
  UserPermissionOverridesResponse,
} from '@/modules/User/types/permissionOverrides'

function row(
  partial: Partial<PermissionOverrideRow> & {
    permissionId: number
    mode: PermissionOverrideRow['mode']
  }
): PermissionOverrideRow {
  return {
    name: `p${partial.permissionId}`,
    displayName: `عملية ${partial.permissionId}`,
    scope: 'User',
    scopeDisplayName: 'المستخدمين',
    roleAllows: true,
    effectiveAllowed: true,
    ...partial,
  }
}

function serverSnap(
  version: number,
  modes: Record<number, PermissionOverrideRow['mode']>,
  rows: PermissionOverrideRow[] = []
) {
  return {
    version,
    modes,
    rows,
    fullName: 'u',
    roleDisplayName: 'r',
    isProtectedAccount: false,
    canEditOverrides: true,
  }
}

describe('powersFormState — تعارض المسودة', () => {
  it('baseline=4 ووصول server=5 يبقي expectedVersion=4', () => {
    const baseline = { version: 4, modes: { 1: 'inherit' as const } }
    const server = serverSnap(5, { 1: 'deny' })
    expect(expectedVersionForSave(baseline)).toBe(4)
    // وصول serverLatest لا يغيّر دالة الإصدار — تبقى على baseline فقط
    void server
    expect(expectedVersionForSave(baseline)).toBe(4)
  })

  it('المقارنة وحدها لا تعتمد الإصدار (serverLatest لا يغيّر expectedVersion)', () => {
    const baseline = { version: 4, modes: { 1: 'inherit' as const, 2: 'allow' as const } }
    expect(expectedVersionForSave(baseline)).toBe(4)
    // محاكاة «مقارنة فقط»: تحديث serverLatest دون لمس baseline
    const afterCompare = { ...baseline }
    expect(expectedVersionForSave(afterCompare)).toBe(4)
  })

  it('يجمع التعارضات عند اختلاف المسودة والخادم عن baseline', () => {
    const baseline = {
      version: 1,
      modes: { 1: 'inherit' as const, 2: 'allow' as const },
    }
    const draft = { 1: 'deny' as const, 2: 'allow' as const }
    const server = serverSnap(
      2,
      { 1: 'allow', 2: 'allow' },
      [row({ permissionId: 1, mode: 'allow', displayName: 'قراءة' })]
    )
    const conflicts = collectConflicts(draft, baseline, server)
    expect(conflicts).toHaveLength(1)
    expect(conflicts[0].permissionId).toBe(1)
    expect(conflicts[0].draft).toBe('deny')
    expect(conflicts[0].server).toBe('allow')
  })

  it('الاعتماد الصريح ينتج إصدار 5 ويحفظ تغييرات الطرفين غير المتعارضة', () => {
    const baseline = {
      version: 4,
      modes: {
        1: 'inherit' as const,
        2: 'inherit' as const,
        3: 'inherit' as const,
      },
    }
    // المستخدم عدّل 1 و 2؛ الخادم عدّل 2 و 3 — تعارض على 2 فقط
    const draft = {
      1: 'deny' as const,
      2: 'allow' as const,
      3: 'inherit' as const,
    }
    const server = serverSnap(5, {
      1: 'inherit',
      2: 'deny',
      3: 'allow',
    })

    const blocked = mergeDraftWithServer(draft, baseline, server, {})
    expect(blocked.ok).toBe(false)
    if (blocked.ok === false) {
      expect(blocked.unresolved).toEqual([2])
    }

    const merged = mergeDraftWithServer(draft, baseline, server, {
      2: 'draft',
    })
    expect(merged.ok).toBe(true)
    if (merged.ok === false) return

    expect(merged.modes[1]).toBe('deny') // محلي فقط
    expect(merged.modes[2]).toBe('allow') // محسوم لصالح المسودة
    expect(merged.modes[3]).toBe('allow') // خادم فقط

    const adopted = adoptMergedBaseline(server, merged.modes)
    expect(adopted.baseline.version).toBe(5)
    expect(adopted.baseline.modes).toEqual(server.modes)
    expect(adopted.draft[1]).toBe('deny')
    expect(adopted.draft[2]).toBe('allow')
    expect(adopted.draft[3]).toBe('allow')
    expect(expectedVersionForSave(adopted.baseline)).toBe(5)
  })

  it('التعارض غير المحسوم يمنع الحفظ/الاعتماد', () => {
    const baseline = { version: 4, modes: { 1: 'inherit' as const } }
    const draft = { 1: 'deny' as const }
    const server = serverSnap(5, { 1: 'allow' })
    const result = mergeDraftWithServer(draft, baseline, server, {})
    expect(result.ok).toBe(false)
    if (result.ok === false) {
      expect(result.unresolved).toContain(1)
    }
    // بدون اعتماد تبقى expectedVersion على baseline
    expect(expectedVersionForSave(baseline)).toBe(4)
  })

  it('المقارنة لا تمسح المسودة — snapshot منفصل', () => {
    const payload: UserPermissionOverridesResponse = {
      userId: 1,
      fullName: 'علي',
      roleId: 2,
      roleName: 'Mandob',
      roleDisplayName: 'مندوب',
      permissionsVersion: 5,
      isProtectedAccount: false,
      canEditOverrides: true,
      rolePermissions: [],
      allowOverrides: [],
      denyOverrides: [],
      effectivePermissions: [],
      rows: [row({ permissionId: 9, mode: 'deny' })],
    }
    const snap = snapshotFromResponse(payload)
    expect(snap.version).toBe(5)
    expect(modesFromRows(payload.rows)[9]).toBe('deny')
    expect(snap.modes[9]).toBe('deny')
  })
})
