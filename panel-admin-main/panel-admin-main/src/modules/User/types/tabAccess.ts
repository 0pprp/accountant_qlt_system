export type TabAccessOverride = 'allow' | 'deny' | null

export type UserTabAccessRow = {
  tabKey: string
  parentKey: string | null
  displayName: string
  roleDefaultEnabled: boolean
  effectiveEnabled: boolean
  override: TabAccessOverride
}

export type UserTabAccessResponse = {
  userId: number
  fullName: string
  roleDisplayName: string
  isProtectedAccount: boolean
  permissionsVersion: number
  canEdit: boolean
  tabs: UserTabAccessRow[]
}

export type TabAccessChange = {
  tabKey: string
  /** true فتح، false قفل، null اعتماد الدور */
  enabled: boolean | null
}

export type PatchTabAccessPayload = {
  expectedVersion: number
  changes: TabAccessChange[]
}

/** حالة المسودة المحلية: enabled فعلي مطلوب في الواجهة */
export type TabDraftState = {
  enabled: boolean
  /** inherit = حسب الدور، allow/deny = مخصص */
  override: TabAccessOverride
}
