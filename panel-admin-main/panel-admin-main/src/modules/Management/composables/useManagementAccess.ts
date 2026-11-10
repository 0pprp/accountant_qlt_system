import { computed } from 'vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import {
  MANAGEMENT_TABS,
  getReadyTabsForSection,
  getVisibleManagementSections,
  type ManagementSectionId,
  type ManagementTabDefinition,
} from '../domain/sections'

export function useManagementAccess() {
  const { decide, can } = usePermission()

  function canOpenTab(tab: ManagementTabDefinition): boolean {
    if (tab.implementationStatus !== 'ready') return false
    return decide(tab.requirement) === 'allowed'
  }

  /** جاهز بلا صلاحية = مقفول؛ غير منفَّذ لا يُعرض هنا أصلًا */
  function isTabLocked(tab: ManagementTabDefinition): boolean {
    if (tab.implementationStatus !== 'ready') return true
    return decide(tab.requirement) !== 'allowed'
  }

  const visibleSections = computed(() =>
    getVisibleManagementSections()
      .filter((section) => {
        const ready = getReadyTabsForSection(section.id)
        return ready.some((tab) => canOpenTab(tab))
      })
      .sort((a, b) => a.displayOrder - b.displayOrder)
  )

  function visibleTabs(sectionId: ManagementSectionId) {
    // أظهر الجاهز فقط؛ غير المنفَّذ ليس قفل صلاحية
    return getReadyTabsForSection(sectionId)
  }

  function firstAllowedTab(
    sectionId: ManagementSectionId
  ): ManagementTabDefinition | undefined {
    return getReadyTabsForSection(sectionId).find((t) => canOpenTab(t))
  }

  function firstAllowedAnywhere(): ManagementTabDefinition | undefined {
    for (const section of getVisibleManagementSections()) {
      const tab = firstAllowedTab(section.id)
      if (tab) return tab
    }
    // director alias tabs (delegateFollowup)
    return getReadyTabsForSection('director').find((t) => canOpenTab(t))
  }

  const canOpenShell = computed(() => firstAllowedAnywhere() != null)

  return {
    can,
    decide,
    canOpenTab,
    isTabLocked,
    visibleSections,
    visibleTabs,
    firstAllowedTab,
    firstAllowedAnywhere,
    canOpenShell,
    allTabs: MANAGEMENT_TABS,
  }
}
