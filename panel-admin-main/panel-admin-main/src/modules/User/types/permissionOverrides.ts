export type PermissionOverrideMode = 'inherit' | 'allow' | 'deny'

export type PermissionOverrideRow = {
  permissionId: number
  name: string
  displayName: string
  scope: string
  scopeDisplayName: string
  roleAllows: boolean
  mode: PermissionOverrideMode
  effectiveAllowed: boolean
}

export type UserPermissionOverridesResponse = {
  userId: number
  fullName: string
  roleId: number
  roleName: string
  roleDisplayName: string
  permissionsVersion: number
  isProtectedAccount: boolean
  canEditOverrides: boolean
  rolePermissions: Array<{ id: number; name: string }>
  allowOverrides: Array<{ id: number; name: string }>
  denyOverrides: Array<{ id: number; name: string }>
  effectivePermissions: Array<{ id: number; name: string }>
  rows: PermissionOverrideRow[]
}

export type PermissionOverrideChange = {
  permissionId: number
  mode: PermissionOverrideMode
}

export type PatchPermissionOverridesPayload = {
  expectedVersion: number
  changes: PermissionOverrideChange[]
}
