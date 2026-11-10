import { describe, expect, it } from 'vitest'
import { resolveRouteAccess, resolveRequirementForRoute } from '../resolveRouteAccess'

function makeTo(partial: {
  name: string
  requiresAuth?: boolean
  permission?: { module: string; action: string }
  matched?: Array<{
    name: string
    requiresAuth?: boolean
    permission?: { module: string; action: string }
  }>
}) {
  const matched = (partial.matched ?? [{ name: partial.name }]).map((m) => ({
    name: m.name,
    meta: {
      requiresAuth: m.requiresAuth ?? partial.requiresAuth,
      permission: m.permission ?? partial.permission,
    },
  }))

  return {
    name: partial.name,
    fullPath: `/${partial.name}`,
    meta: {
      requiresAuth: partial.requiresAuth,
      permission: partial.permission,
    },
    matched,
  }
}

describe('resolveRouteAccess — فجوة التحميل والمسارات', () => {
  it('أ) أثناء loading لمسار محمي → boot مع تذكّر الوجهة (لا سماح بتركيب الصفحة)', () => {
    const outcome = resolveRouteAccess({
      to: makeTo({
        name: 'PaymentListRoute',
        requiresAuth: true,
        permission: { module: 'OrderList', action: 'Read' },
      }),
      isAuthenticated: true,
      permissionsStatus: 'loading',
      hasPermission: () => true,
    })
    expect(outcome).toEqual({ type: 'boot', remember: true })
  })

  it('ب) ready بلا صلاحية → denied دون السماح', () => {
    const outcome = resolveRouteAccess({
      to: makeTo({
        name: 'PaymentListRoute',
        requiresAuth: true,
        permission: { module: 'OrderList', action: 'Read' },
      }),
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: () => false,
    })
    expect(outcome.type).toBe('denied')
  })

  it('ج) ready بصلاحية → allow مرة واحدة للوجهة', () => {
    const outcome = resolveRouteAccess({
      to: makeTo({
        name: 'PaymentListRoute',
        requiresAuth: true,
        permission: { module: 'OrderList', action: 'Read' },
      }),
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: (m, a) => m === 'OrderList' && a === 'Read',
    })
    expect(outcome).toEqual({ type: 'allow' })
  })

  it('د) مسار محمي بلا شرط معلن → denied', () => {
    const outcome = resolveRouteAccess({
      to: makeTo({
        name: 'OrphanProtectedRoute',
        requiresAuth: true,
      }),
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: () => true,
    })
    expect(outcome.type).toBe('denied')
  })

  it('د) مسار فرعي يرث شرط الصلاحية من المطابقة', () => {
    const { requirement, isProtected } = resolveRequirementForRoute(
      makeTo({
        name: 'InstallmentPaymentListRoute',
        matched: [
          {
            name: 'PaymentMainRoute',
          },
          {
            name: 'InstallmentPaymentListRoute',
            requiresAuth: true,
            permission: { module: 'InstallmentPayment', action: 'Read' },
          },
        ],
      })
    )
    expect(isProtected).toBe(true)
    expect(requirement).toEqual({
      type: 'single',
      permission: { module: 'InstallmentPayment', action: 'Read' },
    })
  })

  it('نسيان requiresAuth لا يتجاوز شرط السجل للتنقل', () => {
    const { requirement, isProtected } = resolveRequirementForRoute(
      makeTo({
        name: 'PaymentListRoute',
        // بدون requiresAuth
      })
    )
    expect(isProtected).toBe(true)
    expect(requirement).toEqual({
      type: 'single',
      permission: { module: 'OrderList', action: 'Read' },
    })
  })

  it('فشل التهيئة (error) → boot وليس allow', () => {
    const outcome = resolveRouteAccess({
      to: makeTo({
        name: 'HomeMainRoute',
        requiresAuth: true,
      }),
      isAuthenticated: true,
      permissionsStatus: 'error',
      hasPermission: () => true,
    })
    expect(outcome.type).toBe('boot')
  })

  it('ح) مسار غير معلن بلا meta ولا registry → denied وليس allow', () => {
    const outcome = resolveRouteAccess({
      to: makeTo({
        name: 'RandomBusinessPage',
      }),
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: () => true,
    })
    expect(outcome.type).toBe('denied')
  })

  it('NotFound معلن صراحةً كمسار عام', () => {
    expect(
      resolveRouteAccess({
        to: makeTo({ name: 'NotFoundRoute' }),
        isAuthenticated: true,
        permissionsStatus: 'ready',
        hasPermission: () => false,
      })
    ).toEqual({ type: 'allow' })
  })

  it('المسارات العامة معلنة صراحةً', () => {
    expect(
      resolveRouteAccess({
        to: makeTo({ name: 'AuthLoginRoute' }),
        isAuthenticated: false,
        permissionsStatus: 'ready',
        hasPermission: () => false,
      })
    ).toEqual({ type: 'allow' })

    expect(
      resolveRouteAccess({
        to: makeTo({ name: 'SessionBootRoute' }),
        isAuthenticated: true,
        permissionsStatus: 'loading',
        hasPermission: () => false,
      })
    ).toEqual({ type: 'allow' })
  })
})
