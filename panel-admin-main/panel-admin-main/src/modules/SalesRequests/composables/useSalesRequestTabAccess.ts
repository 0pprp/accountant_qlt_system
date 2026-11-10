import { computed } from 'vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import {
  EXTRA_VIEWS,
  TAB_PERMISSION_ACTIONS,
  WORKFLOW_PERMISSION_ACTIONS,
  WORKFLOW_TABS,
  tabByKey,
  type SalesRequestTabKey,
  type TabDefinition,
} from '../domain/tabs'

/**
 * صلاحيات التبويبات — مستقلة عن أكشنات Assign/Reject.
 * القفل لا يشغّل query. «الكل» يتبع صلاحيات سير العمل فقط (لا العروض الإضافية).
 */
export function useSalesRequestTabAccess() {
  const { can } = usePermission()

  const canOpenWorkspace = computed(() =>
    TAB_PERMISSION_ACTIONS.some((action) => can('SalesRequest', action))
  )

  const canReadAnyWorkflowTab = computed(() =>
    WORKFLOW_PERMISSION_ACTIONS.some((action) => can('SalesRequest', action))
  )

  function canReadTab(key: string): boolean {
    const def = tabByKey(key)
    if (!def) return false
    if (def.implementationStatus === 'not_implemented') return false
    if (def.key === 'all') return canReadAnyWorkflowTab.value
    if (!def.permissionAction) return false
    return can('SalesRequest', def.permissionAction)
  }

  const allowedWorkflowTabs = computed(() =>
    WORKFLOW_TABS.filter((t) => canReadTab(t.key))
  )

  const allowedExtraViews = computed(() =>
    EXTRA_VIEWS.filter((t) => t.key !== 'all' && canReadTab(t.key))
  )

  const visibleExtraViews = computed(() =>
    EXTRA_VIEWS.filter((t) => {
      if (t.key === 'all') return canReadAnyWorkflowTab.value
      return true
    })
  )

  function firstAllowedTab(): SalesRequestTabKey {
    const first = allowedWorkflowTabs.value[0] ?? allowedExtraViews.value[0]
    return first?.key ?? 'unassigned'
  }

  function resolveInitialTab(preferred?: string | null): SalesRequestTabKey {
    if (preferred && canReadTab(preferred)) {
      return preferred as SalesRequestTabKey
    }
    return firstAllowedTab()
  }

  function isLocked(tab: TabDefinition): boolean {
    if (tab.implementationStatus === 'not_implemented') return true
    return !canReadTab(tab.key)
  }

  return {
    canOpenWorkspace,
    canReadAnyWorkflowTab,
    canReadTab,
    allowedWorkflowTabs,
    allowedExtraViews,
    visibleExtraViews,
    firstAllowedTab,
    resolveInitialTab,
    isLocked,
    canTimeline: computed(() => can('SalesRequest', 'TimelineRead')),
    canAssign: computed(() => can('SalesRequest', 'Assign')),
    canReject: computed(() => can('SalesRequest', 'Reject')),
    canException: computed(() => can('SalesRequest', 'RequestException')),
    canMatchRead: computed(() => can('SalesRequest', 'MatchRead')),
    canDecideException: computed(() => can('SalesRequest', 'ExceptionDecide')),
    canSoftDelete: computed(() => can('SalesRequest', 'SoftDelete')),
    canRestore: computed(() => can('SalesRequest', 'Restore')),
  }
}
