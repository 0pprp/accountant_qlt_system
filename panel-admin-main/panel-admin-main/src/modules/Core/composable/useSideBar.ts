import { useI18n } from 'vue-i18n'
import { computed } from 'vue'
import { useAuthStore } from '@/modules/Auth/store'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { getReadyNavigationSections } from '@/modules/Core/navigation/navigation.registry'
import type {
  NavigationSectionDefinition,
  NavigationSidebarItem,
} from '@/modules/Core/navigation/navigation.types'
import { useNotificationUnreadCountQuery } from '@/modules/Notification/requests/queries'

export default function useSideBar() {
  const { t } = useI18n()
  const authStore = useAuthStore()
  const { decide, permissionsReady } = usePermission()

  const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

  const canReadNotifications = computed(() =>
    authStore.hasPermission('Notification', 'Read')
  )

  const { data: unreadCountData } = useNotificationUnreadCountQuery(branchId, {
    enabled: computed(() => branchId.value > 0 && canReadNotifications.value),
  })

  const notificationBadgeCount = computed(
    () => unreadCountData.value?.unreadCount ?? 0
  )

  function toSidebarItem(
    section: NavigationSectionDefinition
  ): NavigationSidebarItem | null {
    const decision = decide(section.requirement)
    const state =
      decision === 'allowed'
        ? 'allowed'
        : decision === 'denied'
          ? 'locked'
          : 'hidden'

    if (state === 'hidden') return null

    const item: NavigationSidebarItem = {
      id: section.id,
      title: t(section.translationKey),
      icon: section.icon,
      value: section.id,
      link: section.routeName,
      query: section.routeQuery ? { ...section.routeQuery } : undefined,
      group: section.group,
      state,
      displayOrder: section.displayOrder,
    }

    if (section.supportsBadge && state === 'allowed') {
      item.badgeCount = notificationBadgeCount.value
    }

    return item
  }

  /**
   * يفتح الحاوية عند وجود ابن جاهز واحد على الأقل.
   * فتح الحاوية لا يمنح صلاحيات بقية التبويبات.
   * not_implemented مُستبعد قبل الوصول هنا عبر getReadyNavigationSections.
   */
  function flattenSections(
    sections: readonly NavigationSectionDefinition[]
  ): NavigationSidebarItem[] {
    const items: NavigationSidebarItem[] = []

    for (const section of sections) {
      if (section.children && section.children.length > 0) {
        const childItems = flattenSections(section.children)
        if (childItems.length === 0) continue

        const parent = toSidebarItem(section)
        if (parent) {
          // حاوية: إن وُجد ابن مسموح اعرض الأب مسموحًا كمجموعة دون منح بقية الأبناء
          if (
            parent.state === 'locked' &&
            childItems.some((c) => c.state === 'allowed')
          ) {
            parent.state = 'allowed'
          }
          parent.children = childItems
          items.push(parent)
        }
        continue
      }

      const item = toSidebarItem(section)
      if (item) items.push(item)
    }

    return items
  }

  const sideBarList = computed<NavigationSidebarItem[]>(() => {
    if (!permissionsReady.value) return []

    return flattenSections(getReadyNavigationSections()).sort(
      (a, b) => a.displayOrder - b.displayOrder
    )
  })

  const isSidebarLoading = computed(
    () =>
      authStore.isAuthenticated &&
      (authStore.permissionsStatus === 'loading' || !permissionsReady.value)
  )

  return {
    sideBarList,
    isSidebarLoading,
  }
}
