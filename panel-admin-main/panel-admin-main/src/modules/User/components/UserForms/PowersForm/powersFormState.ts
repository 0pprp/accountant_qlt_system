import type {
  PermissionOverrideMode,
  PermissionOverrideRow,
  UserPermissionOverridesResponse,
} from '@/modules/User/types/permissionOverrides'

export type PowersBaseline = {
  version: number
  modes: Record<number, PermissionOverrideMode>
}

export type PowersServerSnapshot = {
  version: number
  modes: Record<number, PermissionOverrideMode>
  rows: PermissionOverrideRow[]
  fullName: string
  roleDisplayName: string
  isProtectedAccount: boolean
  canEditOverrides: boolean
}

export type ConflictResolution = 'draft' | 'server'

export type PowersConflict = {
  permissionId: number
  displayName: string
  draft: PermissionOverrideMode
  baseline: PermissionOverrideMode
  server: PermissionOverrideMode
}

export function modesFromRows(
  rows: PermissionOverrideRow[]
): Record<number, PermissionOverrideMode> {
  const modes: Record<number, PermissionOverrideMode> = {}
  for (const row of rows) {
    modes[row.permissionId] = row.mode
  }
  return modes
}

export function snapshotFromResponse(
  data: UserPermissionOverridesResponse
): PowersServerSnapshot {
  return {
    version: data.permissionsVersion,
    modes: modesFromRows(data.rows),
    rows: data.rows,
    fullName: data.fullName,
    roleDisplayName: data.roleDisplayName,
    isProtectedAccount: data.isProtectedAccount,
    canEditOverrides: data.canEditOverrides,
  }
}

export function collectConflicts(
  draft: Record<number, PermissionOverrideMode>,
  baseline: PowersBaseline,
  server: PowersServerSnapshot
): PowersConflict[] {
  const byId = new Map(server.rows.map((r) => [r.permissionId, r]))
  const ids = new Set([
    ...Object.keys(draft).map(Number),
    ...Object.keys(baseline.modes).map(Number),
    ...Object.keys(server.modes).map(Number),
  ])

  const conflicts: PowersConflict[] = []
  for (const id of ids) {
    const d = draft[id] ?? 'inherit'
    const b = baseline.modes[id] ?? 'inherit'
    const s = server.modes[id] ?? 'inherit'
    const locallyChanged = d !== b
    const serverChanged = s !== b
    if (locallyChanged && serverChanged && d !== s) {
      conflicts.push({
        permissionId: id,
        displayName: byId.get(id)?.displayName ?? String(id),
        draft: d,
        baseline: b,
        server: s,
      })
    }
  }
  return conflicts
}

/** الحفظ يستخدم إصدار baseline المعتمد فقط — لا يُشتق من serverLatest. */
export function expectedVersionForSave(baseline: PowersBaseline): number {
  return baseline.version
}

export function unresolvedConflictIds(
  conflicts: PowersConflict[],
  resolutions: Record<number, ConflictResolution>
): number[] {
  return conflicts
    .filter((c) => resolutions[c.permissionId] !== 'draft' && resolutions[c.permissionId] !== 'server')
    .map((c) => c.permissionId)
}

/**
 * دمج تعديلات المستخدم المحلية فوق نسخة الخادم.
 * - عمليات لم يعدّلها المستخدم ← قيمة الخادم
 * - عمليات عدّلها المستخدم دون تعارض ← قيمة المسودة
 * - تعارضات ← تتطلب حسمًا صريحًا في resolutions
 */
export function mergeDraftWithServer(
  draft: Record<number, PermissionOverrideMode>,
  baseline: PowersBaseline,
  server: PowersServerSnapshot,
  resolutions: Record<number, ConflictResolution>
):
  | { ok: true; modes: Record<number, PermissionOverrideMode> }
  | { ok: false; unresolved: number[] } {
  const conflicts = collectConflicts(draft, baseline, server)
  const unresolved = unresolvedConflictIds(conflicts, resolutions)
  if (unresolved.length > 0) {
    return { ok: false, unresolved }
  }

  const ids = new Set([
    ...Object.keys(draft).map(Number),
    ...Object.keys(baseline.modes).map(Number),
    ...Object.keys(server.modes).map(Number),
  ])

  const modes: Record<number, PermissionOverrideMode> = {}
  for (const id of ids) {
    const d = draft[id] ?? 'inherit'
    const b = baseline.modes[id] ?? 'inherit'
    const s = server.modes[id] ?? 'inherit'
    const locallyChanged = d !== b
    const serverChanged = s !== b

    if (locallyChanged && serverChanged && d !== s) {
      modes[id] = resolutions[id] === 'draft' ? d : s
    } else if (locallyChanged) {
      modes[id] = d
    } else {
      modes[id] = s
    }
  }

  return { ok: true, modes }
}

/** بعد الاعتماد: baseline = نسخة الخادم (إصدار + أوضاع)، المسودة = نتيجة الدمج. */
export function adoptMergedBaseline(
  server: PowersServerSnapshot,
  mergedModes: Record<number, PermissionOverrideMode>
): { baseline: PowersBaseline; draft: Record<number, PermissionOverrideMode> } {
  return {
    baseline: {
      version: server.version,
      modes: { ...server.modes },
    },
    draft: { ...mergedModes },
  }
}
