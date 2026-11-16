import type {
  TabAccessChange,
  TabAccessOverride,
  TabDraftState,
  UserTabAccessResponse,
  UserTabAccessRow,
} from '@/modules/User/types/tabAccess'

export type TabAccessBaseline = {
  version: number
  /** tabKey → draft state at baseline */
  states: Record<string, TabDraftState>
}

export type TabAccessServerSnapshot = {
  version: number
  fullName: string
  roleDisplayName: string
  isProtectedAccount: boolean
  canEdit: boolean
  tabs: UserTabAccessRow[]
  states: Record<string, TabDraftState>
}

export type TabConflictResolution = 'draft' | 'server'

export type TabAccessConflict = {
  tabKey: string
  displayName: string
  draftEnabled: boolean
  baselineEnabled: boolean
  serverEnabled: boolean
}

export function stateFromRow(row: UserTabAccessRow): TabDraftState {
  return {
    enabled: row.effectiveEnabled,
    override: row.override,
  }
}

export function statesFromTabs(
  tabs: UserTabAccessRow[]
): Record<string, TabDraftState> {
  const states: Record<string, TabDraftState> = {}
  for (const tab of tabs) {
    states[tab.tabKey] = stateFromRow(tab)
  }
  return states
}

export function snapshotFromTabAccess(
  data: UserTabAccessResponse
): TabAccessServerSnapshot {
  return {
    version: data.permissionsVersion,
    fullName: data.fullName,
    roleDisplayName: data.roleDisplayName,
    isProtectedAccount: data.isProtectedAccount,
    canEdit: data.canEdit,
    tabs: data.tabs,
    states: statesFromTabs(data.tabs),
  }
}

export function effectiveFromRoleAndOverride(
  roleDefaultEnabled: boolean,
  override: TabAccessOverride
): boolean {
  if (override === 'allow') return true
  if (override === 'deny') return false
  return roleDefaultEnabled
}

function stateSignature(state: TabDraftState): string {
  return `${state.override ?? 'inherit'}:${state.enabled ? 1 : 0}`
}

export function collectTabConflicts(
  draft: Record<string, TabDraftState>,
  baseline: TabAccessBaseline,
  server: TabAccessServerSnapshot
): TabAccessConflict[] {
  const byKey = new Map(server.tabs.map((t) => [t.tabKey, t]))
  const keys = new Set([
    ...Object.keys(draft),
    ...Object.keys(baseline.states),
    ...Object.keys(server.states),
  ])

  const conflicts: TabAccessConflict[] = []
  for (const key of keys) {
    const d = draft[key]
    const b = baseline.states[key]
    const s = server.states[key]
    if (!d || !b || !s) continue
    const dSig = stateSignature(d)
    const bSig = stateSignature(b)
    const sSig = stateSignature(s)
    const locallyChanged = dSig !== bSig
    const serverChanged = sSig !== bSig
    if (locallyChanged && serverChanged && dSig !== sSig) {
      conflicts.push({
        tabKey: key,
        displayName: byKey.get(key)?.displayName ?? key,
        draftEnabled: d.enabled,
        baselineEnabled: b.enabled,
        serverEnabled: s.enabled,
      })
    }
  }
  return conflicts
}

export function expectedVersionForTabSave(baseline: TabAccessBaseline): number {
  return baseline.version
}

export function unresolvedTabConflictKeys(
  conflicts: TabAccessConflict[],
  resolutions: Record<string, TabConflictResolution>
): string[] {
  return conflicts
    .filter((c) => resolutions[c.tabKey] !== 'draft' && resolutions[c.tabKey] !== 'server')
    .map((c) => c.tabKey)
}

export function mergeTabDraftWithServer(
  draft: Record<string, TabDraftState>,
  baseline: TabAccessBaseline,
  server: TabAccessServerSnapshot,
  resolutions: Record<string, TabConflictResolution>
):
  | { ok: true; states: Record<string, TabDraftState> }
  | { ok: false; unresolved: string[] } {
  const conflicts = collectTabConflicts(draft, baseline, server)
  const unresolved = unresolvedTabConflictKeys(conflicts, resolutions)
  if (unresolved.length > 0) return { ok: false, unresolved }

  const keys = new Set([
    ...Object.keys(draft),
    ...Object.keys(baseline.states),
    ...Object.keys(server.states),
  ])
  const states: Record<string, TabDraftState> = {}
  for (const key of keys) {
    const d = draft[key]
    const b = baseline.states[key]
    const s = server.states[key]
    if (!d || !b || !s) continue
    const dSig = stateSignature(d)
    const bSig = stateSignature(b)
    const sSig = stateSignature(s)
    const locallyChanged = dSig !== bSig
    const serverChanged = sSig !== bSig

    if (locallyChanged && serverChanged && dSig !== sSig) {
      states[key] = resolutions[key] === 'draft' ? { ...d } : { ...s }
    } else if (locallyChanged) {
      states[key] = { ...d }
    } else {
      states[key] = { ...s }
    }
  }
  return { ok: true, states }
}

export function adoptMergedTabBaseline(
  server: TabAccessServerSnapshot,
  merged: Record<string, TabDraftState>
): { baseline: TabAccessBaseline; draft: Record<string, TabDraftState> } {
  return {
    baseline: { version: server.version, states: { ...server.states } },
    draft: { ...merged },
  }
}

/**
 * يحسب التغييرات للإرسال.
 * override=null → enabled:null (اعتماد الدور)
 * غير ذلك → enabled:true|false
 */
export function buildTabAccessChanges(
  draft: Record<string, TabDraftState>,
  baseline: TabAccessBaseline,
  tabs: UserTabAccessRow[]
): TabAccessChange[] {
  const changes: TabAccessChange[] = []

  for (const tab of tabs) {
    const d = draft[tab.tabKey]
    const b = baseline.states[tab.tabKey]
    if (!d || !b) continue
    if (d.enabled === b.enabled && d.override === b.override) continue

    changes.push({
      tabKey: tab.tabKey,
      enabled: d.override === null ? null : d.enabled,
    })
  }
  return changes
}

export function toggleTabDraft(
  current: TabDraftState,
  roleDefaultEnabled: boolean
): TabDraftState {
  const nextEnabled = !current.enabled
  if (nextEnabled === roleDefaultEnabled) {
    return { enabled: nextEnabled, override: null }
  }
  return {
    enabled: nextEnabled,
    override: nextEnabled ? 'allow' : 'deny',
  }
}

export function resetTabToRole(
  roleDefaultEnabled: boolean
): TabDraftState {
  return { enabled: roleDefaultEnabled, override: null }
}

export function dirtyTabCount(
  draft: Record<string, TabDraftState>,
  baseline: TabAccessBaseline
): number {
  let n = 0
  for (const key of Object.keys(draft)) {
    const d = draft[key]
    const b = baseline.states[key]
    if (!d || !b) continue
    if (d.enabled !== b.enabled || d.override !== b.override) n += 1
  }
  return n
}
