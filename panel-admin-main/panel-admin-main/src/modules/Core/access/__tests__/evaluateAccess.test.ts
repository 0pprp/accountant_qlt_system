import { describe, expect, it } from 'vitest'
import { canAccess, evaluateAccess } from '../evaluateAccess'
import type { AccessRequirement } from '../access.types'
import type { PermissionAction, PermissionModule } from '@/modules/Auth/types/model'

function makeHas(owned: Array<[PermissionModule, PermissionAction]>) {
  const set = new Set(owned.map(([m, a]) => `${m}:${a}`))
  return (module: PermissionModule, action: PermissionAction) =>
    set.has(`${module}:${action}`)
}

describe('evaluateAccess', () => {
  it('returns loading while permissions are not ready (never allows)', () => {
    const decision = evaluateAccess({
      permissionsReady: false,
      isAuthenticated: true,
      hasPermission: () => true,
      requirement: { type: 'publicAuthenticated' },
    })
    expect(decision).toBe('loading')
    expect(
      canAccess({
        permissionsReady: false,
        isAuthenticated: true,
        hasPermission: () => true,
        requirement: { type: 'publicAuthenticated' },
      })
    ).toBe(false)
  })

  it('allows publicAuthenticated for signed-in ready users', () => {
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: () => false,
        requirement: { type: 'publicAuthenticated' },
      })
    ).toBe('allowed')
  })

  it('denies missing requirement on protected evaluation', () => {
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: () => true,
        requirement: null,
      })
    ).toBe('denied')
  })

  it('denies invalid requirement shapes', () => {
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: () => true,
        requirement: { type: 'single' } as unknown as AccessRequirement,
      })
    ).toBe('denied')
  })

  it('evaluates single permission present and missing', () => {
    const has = makeHas([['OrderList', 'Read']])
    const req: AccessRequirement = {
      type: 'single',
      permission: { module: 'OrderList', action: 'Read' },
    }
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: has,
        requirement: req,
      })
    ).toBe('allowed')

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: makeHas([]),
        requirement: req,
      })
    ).toBe('denied')
  })

  it('evaluates allOf requiring every permission', () => {
    const req: AccessRequirement = {
      type: 'allOf',
      permissions: [
        { module: 'User', action: 'Read' },
        { module: 'User', action: 'Update' },
      ],
    }
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: makeHas([
          ['User', 'Read'],
          ['User', 'Update'],
        ]),
        requirement: req,
      })
    ).toBe('allowed')
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: makeHas([['User', 'Read']]),
        requirement: req,
      })
    ).toBe('denied')
  })

  it('evaluates anyOf requiring at least one permission', () => {
    const req: AccessRequirement = {
      type: 'anyOf',
      permissions: [
        { module: 'Safe', action: 'Read' },
        { module: 'Report', action: 'Read' },
      ],
    }
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: makeHas([['Report', 'Read']]),
        requirement: req,
      })
    ).toBe('allowed')
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: makeHas([]),
        requirement: req,
      })
    ).toBe('denied')
  })
})
