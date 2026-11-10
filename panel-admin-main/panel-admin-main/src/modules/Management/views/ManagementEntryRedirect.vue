<template>
  <div class="mgmt-entry-boot" aria-live="polite">
    {{ $t('management.redirecting') }}
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { useManagementAccess } from '../composables/useManagementAccess'
import { appReadyTabs } from '../domain/workspaceNavigation'
import { getTabByIds } from '../domain/sections'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const { firstAllowedTab, canOpenTab } = useManagementAccess()

async function resolveBranchEntry() {
  const visitTab = getTabByIds('branch', 'visit-workspace')
  const canOpenVisit = !!visitTab && canOpenTab(visitTab)
  const canProbe =
    auth.hasPermission('BranchVisit', 'Read') === true ||
    auth.hasPermission('BranchVisit', 'Open') === true
  const selectedId = auth.selectedBranch?.id ?? null

  // نقص BranchVisit.Read لا يمنح تجاوزًا إلى أعمال الفرع.
  if (!canProbe) {
    if (canOpenVisit) {
      void router.replace({ name: visitTab!.routeName })
      return
    }
    void router.replace({
      name: 'AccessDeniedRoute',
      query: { from: 'ManagementBranchEntryRoute' },
    })
    return
  }

  try {
    const { data } = await axiosInstance.get<{
      status?: string
      branchId?: number
    } | null>('/admin/BranchVisits/active')
    const openMatching =
      !!data &&
      String(data.status ?? '').toLowerCase() === 'open' &&
      (selectedId == null || data.branchId === selectedId)
    if (openMatching) {
      const first = appReadyTabs('branch').find(
        (tab) => tab.id !== 'branchVisitWorkspace' && canOpenTab(tab)
      )
      if (first) {
        void router.replace({
          name: first.routeName,
          query: first.defaultQuery ? { ...first.defaultQuery } : undefined,
        })
        return
      }
      if (canOpenVisit) {
        void router.replace({ name: visitTab!.routeName })
        return
      }
      void router.replace({
        name: 'AccessDeniedRoute',
        query: { from: 'ManagementBranchEntryRoute' },
      })
      return
    }
  } catch {
    // فشل الشبكة → صفحة الزيارة إن أمكن؛ لا أعمال.
  }

  if (canOpenVisit) {
    void router.replace({ name: visitTab!.routeName })
    return
  }
  void router.replace({
    name: 'AccessDeniedRoute',
    query: { from: 'ManagementBranchEntryRoute' },
  })
}

onMounted(async () => {
  const section = route.meta.managementSectionId

  // إدارة الفروع فقط — لا تُفرض على المفوض/oversight.
  if (section === 'branch') {
    await resolveBranchEntry()
    return
  }

  const first =
    section === 'delegate'
      ? appReadyTabs(section).find(canOpenTab)
      : firstAllowedTab('oversight')
  if (!first) {
    void router.replace({
      name: 'AccessDeniedRoute',
      query: { from: 'ManagementMainRoute' },
    })
    return
  }
  void router.replace({
    name: first.routeName,
    query: first.defaultQuery ? { ...first.defaultQuery } : undefined,
  })
})
</script>

<style scoped>
.mgmt-entry-boot {
  padding: 1rem;
  color: #526b67;
  font-size: 16px;
}
</style>
