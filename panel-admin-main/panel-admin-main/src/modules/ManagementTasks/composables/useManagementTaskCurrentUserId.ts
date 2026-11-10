import { computed } from 'vue'
import { useAuthStore } from '@/modules/Auth/store'

const USER_ID_CLAIMS = [
  'nameid',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
  'sub',
]

function decodePayload(token: string): Record<string, unknown> | null {
  const part = token.split('.')[1]
  if (!part) return null
  try {
    const base64 = part.replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=')
    const binary = atob(padded)
    const json = decodeURIComponent(
      Array.from(binary)
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join('')
    )
    return JSON.parse(json) as Record<string, unknown>
  } catch {
    return null
  }
}

/**
 * معرّف المستخدم الحالي من claim في التوكن — للعرض فقط (إخفاء أزرار غير منطقية).
 * الخادم هو المرجع في التحقق من حامل المهمة.
 */
export function useManagementTaskCurrentUserId() {
  const auth = useAuthStore()
  return computed<number | null>(() => {
    if (!auth.accessToken) return null
    const payload = decodePayload(auth.accessToken)
    if (!payload) return null
    for (const key of USER_ID_CLAIMS) {
      const raw = payload[key]
      const id = Number(raw)
      if (raw != null && Number.isInteger(id) && id > 0) return id
    }
    return null
  })
}
