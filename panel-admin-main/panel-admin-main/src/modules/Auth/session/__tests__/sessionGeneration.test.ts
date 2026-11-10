import { describe, expect, it } from 'vitest'
import {
  isStaleGeneration,
  nextGeneration,
} from '../sessionGeneration'
import {
  clearIntendedNavigation,
  consumeIntendedNavigation,
  rememberIntendedNavigation,
} from '../intendedNavigation'

describe('sessionGeneration', () => {
  it('و) نتيجة قديمة تُرفض بعد زيادة الجيل', () => {
    const started = 3
    const afterReplace = nextGeneration(started)
    expect(isStaleGeneration(started, afterReplace)).toBe(true)
    expect(isStaleGeneration(afterReplace, afterReplace)).toBe(false)
  })
})

describe('intendedNavigation', () => {
  it('يحتفظ بالوجهة ويعيدها مرة واحدة', () => {
    clearIntendedNavigation()
    rememberIntendedNavigation({
      name: 'PaymentListRoute',
      fullPath: '/payment',
      params: {},
      query: {},
    })
    expect(consumeIntendedNavigation()?.fullPath).toBe('/payment')
    expect(consumeIntendedNavigation()).toBeNull()
  })
})
