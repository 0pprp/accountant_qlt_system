<template>
  <div>
    <TheHeader :canSearch="false">
      <template #back>
        <RouterLink :to="{ name: 'UserListRoute' }">
          <SvgIcon name="boldArrowLeft" />
        </RouterLink>
      </template>

      <template #calendar>
        <HeaderDatePicker />
      </template>
    </TheHeader>

    <UserDailyReportList v-if="canReadUser" />
  </div>
</template>

<script setup lang="ts">
import UserDailyReportList from '../components/UserDailyReportList/UserDailyReportList.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { can } = usePermission()
const canReadUser = can('User', 'Read')
</script>
