/** دورة صلاحيات الجلسة — لا تُستنتج الجاهزية من localStorage وحده. */
export type PermissionsLifecycleStatus = 'loading' | 'ready' | 'error'

export type SessionEndReason =
  | 'logout'
  | 'refresh_failed'
  | 'replace_account'
  | 'bootstrap_failed'

export interface IntendedNavigation {
  name: string | null | undefined
  fullPath: string
  params: Record<string, unknown>
  query: Record<string, unknown>
}
