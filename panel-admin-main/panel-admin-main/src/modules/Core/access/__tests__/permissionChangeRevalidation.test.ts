import { describe, expect, it } from 'vitest'
import { resolveRouteAccess } from '../resolveRouteAccess'

describe('ح) تغيّر الصلاحيات داخل الجلسة', () => {
  it('بعد فقدان الصلاحية يصبح المسار الحالي denied', () => {
    const to = {
      name: 'CustomerMainRoute',
      fullPath: '/customer',
      meta: {
        requiresAuth: true,
        permission: { module: 'Customer', action: 'Read' },
      },
      matched: [
        {
          name: 'CustomerMainRoute',
          meta: {
            requiresAuth: true,
            permission: { module: 'Customer', action: 'Read' },
          },
        },
      ],
    }

    const before = resolveRouteAccess({
      to,
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: (m, a) => m === 'Customer' && a === 'Read',
    })
    expect(before).toEqual({ type: 'allow' })

    const after = resolveRouteAccess({
      to,
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: () => false,
    })
    expect(after.type).toBe('denied')
  })
})
