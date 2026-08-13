import { useI18n } from 'vue-i18n'
import { computed } from 'vue'
import type { SideBarList, SideBarItem } from '../types/model/sidebar'
import { useAuthStore } from '@/modules/Auth/store'
import { useNotificationUnreadCountQuery } from '@/modules/Notification/requests/queries'

export default function useSideBar() {
  const { t } = useI18n()
  const authStore = useAuthStore()

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

  const allSideBarItems = computed(() => [
    {
      title: t('sidebar.home'),
      icon: 'home',
      value: 'home',
      link: 'HomeMainRoute',
    },
    {
      title: t('sidebar.branch'),
      icon: 'branch',
      value: 'branch',
      link: 'BranchListRoute',
      permission: {
        module: 'Branch',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.payment'),
      icon: 'payment',
      value: 'payment',
      link: 'PaymentListRoute',
      permission: {
        module: 'OrderList',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.customer'),
      icon: 'customer',
      value: 'customer',
      link: 'CustomerMainRoute',
      permission: {
        module: 'Customer',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.purchases'),
      icon: 'purchases',
      value: 'purchases',
      link: 'PurchasesListRoute',
      permission: {
        module: 'Purchase',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.sale'),
      icon: 'sale',
      value: 'sale',
      link: 'SaleListRoute',
      permission: {
        module: 'Order',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.warehouse'),
      icon: 'warehouse',
      value: 'warehouse',
      link: 'WarehouseListRoute',
      permission: {
        module: 'ProductCategory',
        action: 'Read',
      },
    },

    ...(authStore.isSuperAdmin
      ? [
          {
            title: t('sidebar.safeMain'),
            icon: 'safe',
            value: 'safeMain',
            link: 'SafeMainMainRoute',
            permission: {
              module: 'Safe',
              action: 'Read',
            },
          },
        ]
      : []),

    {
      title: t('sidebar.safe'),
      icon: 'safe',
      value: 'safe',
      link: 'SafeMainRoute',
      permission: {
        module: 'Safe',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.order'),
      icon: 'order',
      value: 'order',
      link: 'OrderListRoute',
      permission: {
        module: 'OrderList',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.user'),
      icon: 'user',
      value: 'user',
      link: 'UserListRoute',
      permission: {
        module: 'User',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.activityLog'),
      icon: 'activityLog',
      value: 'activityLog',
      link: 'ActivityLogListRoute',
      permission: {
        module: 'ActivityLog',
        action: 'Read',
      },
    },
    {
      title: t('sidebar.notifications'),
      icon: 'notification',
      value: 'notifications',
      link: 'NotificationListRoute',
      badgeCount: notificationBadgeCount.value,
      permission: {
        module: 'Notification',
        action: 'Read',
      },
    },
  ])

  const sideBarList = computed<SideBarList>(() => {
    return (allSideBarItems.value as Array<SideBarItem>).filter((item) => {
      if (!item.permission) return true

      return authStore.hasPermission(
        item.permission.module,
        item.permission.action
      )
    })
  })

  return {
    sideBarList,
  }
}
