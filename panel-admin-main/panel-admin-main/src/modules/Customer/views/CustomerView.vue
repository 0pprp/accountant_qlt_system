<template>
  <div>
    <TheHeader>
      <template #create>
        <RouterLink
          v-if="canCreateCustomer"
          :to="{ name: 'CreateCustomerView' }"
          class="bg-primary rounded-2xl text-white inline-flex items-center gap-2 px-4 py-2"
        >
          {{ $t('customer.createCustomer') }}
          <SvgIcon name="addCircle" />
        </RouterLink>
      </template>

      <template #export>
        <ExcelReport
          v-if="canReadCustomer"
          endpoint="admin/Customers/excel-report"
          :filename="`customers-report-${new Date().toISOString().split('T')[0]}.xlsx`"
          :params="excelParams"
        />
      </template>

      <template #calendar>
        <HeaderDatePicker v-if="canReadCustomer" />
      </template>
    </TheHeader>

    <CustomerList ref="customerListRef" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import ExcelReport from '@/modules/Core/components/base/ExcelReport/ExcelReport.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import CustomerList from '@/modules/Customer/components/CustomerList/CustomerList.vue'
import useAuthStore from '@/modules/Auth/store'
import { usePermission } from '@/modules/Core/composable/usePermission'

const authStore = useAuthStore()
const route = useRoute()

const { can } = usePermission()
const canReadCustomer = can('Customer', 'Read')
const canCreateCustomer = can('Customer', 'Create')

const excelParams = computed(() => ({
  branchId: authStore.selectedBranch?.id,
  searchTerm: Array.isArray(route.query.s)
    ? (route.query.s[0] ?? '')
    : (route.query.s ?? ''),
  startDate:
    typeof route.query.startDate === 'string' ? route.query.startDate : '',
  endDate: typeof route.query.endDate === 'string' ? route.query.endDate : '',
}))
</script>
