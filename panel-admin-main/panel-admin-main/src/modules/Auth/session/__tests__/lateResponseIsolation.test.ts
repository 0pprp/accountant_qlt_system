import { describe, expect, it } from 'vitest'
import {
  isStaleGeneration,
  nextGeneration,
} from '../sessionGeneration'

/**
 * يثبت منطق عزل الاستجابات المتأخرة دون HTTP حقيقي.
 * سيناريو: refresh/طلب بدأ للحساب A يكتمل بعد دخول B.
 */
describe('عزل الاستجابات المتأخرة بين الحسابات', () => {
  it('و) لا تُطبَّق حمولة الحساب A بعد استبدال الجلسة بـ B', () => {
    let sessionGeneration = 1
    const refreshStartedForA = sessionGeneration

    // دخول الحساب B
    sessionGeneration = nextGeneration(sessionGeneration)
    const accountBToken = 'token-B'

    const lateRefreshFromA = {
      accessToken: 'token-A-late',
      refreshToken: 'refresh-A-late',
    }

    const shouldApply = !isStaleGeneration(
      refreshStartedForA,
      sessionGeneration
    )

    expect(shouldApply).toBe(false)

    let appliedToken = accountBToken
    if (shouldApply) {
      appliedToken = lateRefreshFromA.accessToken
    }

    expect(appliedToken).toBe('token-B')
  })

  it('ز) تبديل الفرع يزيد جيل الفرع دون جيل الجلسة', () => {
    let sessionGeneration = 5
    let branchGeneration = 2
    const requestBranchGen = branchGeneration

    branchGeneration = nextGeneration(branchGeneration)
    // هوية الحساب ثابتة
    expect(sessionGeneration).toBe(5)
    expect(isStaleGeneration(requestBranchGen, branchGeneration)).toBe(true)
  })

  it('هـ) إنهاء الجلسة يزيد الجيل فيرفض الطابور القديم', () => {
    let sessionGeneration = 10
    const queuedRequestGen = sessionGeneration

    sessionGeneration = nextGeneration(sessionGeneration) // endSession

    const canReplayQueued = !isStaleGeneration(
      queuedRequestGen,
      sessionGeneration
    )
    expect(canReplayQueued).toBe(false)
  })
})
