<template>
  <div>
    <TheHeader searchPlaceholder="notification.searchPlaceholder">
      <template #create>
        <Button
          v-if="canUpdateNotification"
          class="!bg-background !text-gray-700 !border !border-line !rounded-lg"
          :loading="isMarkingAll"
          @click="handleMarkAllAsRead"
        >
          <SvgIcon name="noteCheckList" class="w-6 h-6 text-gray-700" />
          {{ $t('notification.markAllAsRead') }}
        </Button>
      </template>

      <template #calendar>
        <HeaderDatePicker v-if="canReadNotification" />
      </template>
    </TheHeader>

    <NotificationList v-if="canReadNotification" />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Button } from 'primevue'
import NotificationList from '../components/NotificationList/NotificationList.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import useAuthStore from '@/modules/Auth/store'
import { useMarkAllNotificationsReadMutation } from '@/modules/Notification/requests/mutations'

const { can } = usePermission()
const authStore = useAuthStore()

const canReadNotification = can('Notification', 'Read')
const canUpdateNotification = can('Notification', 'Update')

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const { mutate: markAllAsRead, isPending: isMarkingAll } =
  useMarkAllNotificationsReadMutation()

function handleMarkAllAsRead() {
  if (branchId.value > 0) {
    markAllAsRead(branchId.value)
  }
}
</script>
