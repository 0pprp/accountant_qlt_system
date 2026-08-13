<template>
  <div>
    <TheHeader searchPlaceholder="activityLog.searchPlaceholder">
      <template #create>
        <RouterLink
          v-if="canReadUser"
          :to="{ name: 'UserListRoute' }"
          class="bg-primary rounded-2xl text-white inline-flex items-center gap-2 px-4 py-0"
        >
          {{ $t('sidebar.user') }}
        </RouterLink>
      </template>

      <template #calendar>
        <HeaderDatePicker v-if="canReadActivityLog" />
      </template>
    </TheHeader>

    <ActivityLogList v-if="canReadActivityLog" />
  </div>
</template>

<script setup lang="ts">
import ActivityLogList from '../components/ActivityLogList/ActivityLogList.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { can } = usePermission()
const canReadActivityLog = can('ActivityLog', 'Read')
const canReadUser = can('User', 'Read')
</script>
