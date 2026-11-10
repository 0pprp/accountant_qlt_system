import { describe, expect, it } from 'vitest'

/**
 * عقد البوابة الموثّق — الاستثناءات والنطاق.
 * الاختبار السلوكي الحقيقي: branchVisitGate.component.test.ts (Router + API).
 */
function evaluateBranchVisitGate(input: {
  sectionId: string
  tabId: string
  canProbeVisit: boolean
  loading: boolean
  error: boolean
  selectedBranchId: number | null
  openVisitBranchId: number | null
}) {
  const isBranch = input.sectionId === 'branch'
  const isVisitWorkspace = input.tabId === 'branchVisitWorkspace'
  if (!isBranch) {
    return { allowOpsContent: true, needsVisitRedirect: false, exempt: true as const }
  }
  if (isVisitWorkspace) {
    return { allowOpsContent: true, needsVisitRedirect: false, exempt: false as const }
  }

  const matching =
    input.openVisitBranchId != null &&
    input.selectedBranchId != null &&
    input.openVisitBranchId === input.selectedBranchId

  if (!input.canProbeVisit) {
    return { allowOpsContent: false, needsVisitRedirect: true, exempt: false as const }
  }
  if (input.loading) {
    return { allowOpsContent: false, needsVisitRedirect: false, exempt: false as const }
  }
  if (input.error || !matching) {
    return { allowOpsContent: false, needsVisitRedirect: true, exempt: false as const }
  }
  return { allowOpsContent: true, needsVisitRedirect: false, exempt: false as const }
}

describe('branch visit gate contract', () => {
  it('requires matching visit for branch ops; Read-less probe still blocks', () => {
    expect(
      evaluateBranchVisitGate({
        sectionId: 'branch',
        tabId: 'customers',
        canProbeVisit: false,
        loading: false,
        error: false,
        selectedBranchId: 1,
        openVisitBranchId: null,
      }).allowOpsContent
    ).toBe(false)
  })

  it('does not gate visit-workspace itself', () => {
    expect(
      evaluateBranchVisitGate({
        sectionId: 'branch',
        tabId: 'branchVisitWorkspace',
        canProbeVisit: true,
        loading: false,
        error: false,
        selectedBranchId: 1,
        openVisitBranchId: null,
      })
    ).toEqual({
      allowOpsContent: true,
      needsVisitRedirect: false,
      exempt: false,
    })
  })

  it('documents exemptions: oversight + delegate reports are out of gate', () => {
    expect(
      evaluateBranchVisitGate({
        sectionId: 'oversight',
        tabId: 'summary',
        canProbeVisit: true,
        loading: false,
        error: false,
        selectedBranchId: 1,
        openVisitBranchId: null,
      }).exempt
    ).toBe(true)
    expect(
      evaluateBranchVisitGate({
        sectionId: 'delegate',
        tabId: 'branchVisits',
        canProbeVisit: true,
        loading: false,
        error: false,
        selectedBranchId: 1,
        openVisitBranchId: null,
      }).exempt
    ).toBe(true)
  })

  it('visit for branch A does not open ops for selected branch B', () => {
    expect(
      evaluateBranchVisitGate({
        sectionId: 'branch',
        tabId: 'sales',
        canProbeVisit: true,
        loading: false,
        error: false,
        selectedBranchId: 2,
        openVisitBranchId: 1,
      }).allowOpsContent
    ).toBe(false)
  })

  it('blocks ops while loading or on error', () => {
    expect(
      evaluateBranchVisitGate({
        sectionId: 'branch',
        tabId: 'payments',
        canProbeVisit: true,
        loading: true,
        error: false,
        selectedBranchId: 1,
        openVisitBranchId: 1,
      }).allowOpsContent
    ).toBe(false)
    expect(
      evaluateBranchVisitGate({
        sectionId: 'branch',
        tabId: 'payments',
        canProbeVisit: true,
        loading: false,
        error: true,
        selectedBranchId: 1,
        openVisitBranchId: 1,
      }).allowOpsContent
    ).toBe(false)
  })
})
