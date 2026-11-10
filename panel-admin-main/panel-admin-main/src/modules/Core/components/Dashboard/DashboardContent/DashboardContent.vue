<template>
  <div class="flex flex-col gap-4 px-4 md:px-6 py-4">
    <PermissionGate :requirement="profileRequirement" silent>
      <div class="grid grid-cols-12 gap-4">
        <div v-if="userData" class="col-span-12 lg:col-span-4">
          <DashboardUserCard :user="userData" />
        </div>
        <div
          v-if="canReadReports && monthlyChartData"
          class="col-span-12 lg:col-span-8"
        >
          <DashboardMonthlyChart :data="monthlyChartData" />
        </div>
      </div>
    </PermissionGate>

    <PermissionGate :requirement="reportRequirement" silent>
      <DashboardStatsCards v-if="statsData" :stats="statsData" />

      <div class="grid grid-cols-12 gap-4">
        <div v-if="financialRows" class="col-span-12 lg:col-span-8">
          <DashboardFinancialTable :rows="financialRows" />
        </div>
        <div v-if="productsReportData" class="col-span-12 lg:col-span-4">
          <DashboardWarehouseItems
            :productsCount="productsReportData.productsCount"
          />
        </div>
      </div>
    </PermissionGate>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import DashboardUserCard from '../DashboardUserCard/DashboardUserCard.vue'
import DashboardMonthlyChart from '../DashboardMonthlyChart/DashboardMonthlyChart.vue'
import DashboardStatsCards from '../DashboardStatsCards/DashboardStatsCards.vue'
import DashboardWarehouseItems from '../DashboardWarehouseItems/DashboardWarehouseItems.vue'
import DashboardFinancialTable from '../DashboardFinancialTable/DashboardFinancialTable.vue'
import PermissionGate from '@/modules/Core/components/access/PermissionGate.vue'
import {
  useDashboardUsersReportQuery,
  useProductsReportQuery,
  useSafeReportQuery,
  useUserProfileQuery,
  useYearlyFinancialReportQuery,
} from '@/modules/Core/requests/queries'
import { useAuthStore } from '@/modules/Auth/store'
import { usePermission } from '@/modules/Core/composable/usePermission'
import type { AccessRequirement } from '@/modules/Core/access/access.types'
import {
  mapProfileToDashboardUser,
  mapSafeReportToFinancialRows,
  mapUsersReportToStats,
  mapYearlyFinancialToChartData,
} from '@/modules/Core/utils/dashboardMappers'

const { t } = useI18n()
const authStore = useAuthStore()
const { can } = usePermission()

const profileRequirement: AccessRequirement = {
  type: 'single',
  permission: { module: 'User', action: 'Read' },
}

const reportRequirement: AccessRequirement = {
  type: 'single',
  permission: { module: 'Report', action: 'Read' },
}

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)
const canReadReports = computed(() => can('Report', 'Read'))

const { data: profileData } = useUserProfileQuery(branchId)
const { data: yearlyFinancialData } = useYearlyFinancialReportQuery(branchId)
const { data: usersReportData } = useDashboardUsersReportQuery(branchId)
const { data: safeReportData } = useSafeReportQuery(branchId)
const { data: productsReportData } = useProductsReportQuery(branchId)

const userData = computed(() =>
  profileData.value ? mapProfileToDashboardUser(profileData.value) : null
)

const monthlyChartData = computed(() =>
  yearlyFinancialData.value
    ? mapYearlyFinancialToChartData(yearlyFinancialData.value)
    : null
)

const statsData = computed(() =>
  usersReportData.value ? mapUsersReportToStats(usersReportData.value) : null
)

const financialRows = computed(() =>
  safeReportData.value
    ? mapSafeReportToFinancialRows(safeReportData.value, {
        today: t('dashboard.financialTable.periods.today'),
        yesterday: t('dashboard.financialTable.periods.yesterday'),
        lastWeek: t('dashboard.financialTable.periods.lastWeek'),
        lastMonth: t('dashboard.financialTable.periods.lastMonth'),
        lastYear: t('dashboard.financialTable.periods.lastYear'),
      })
    : null
)
</script>
