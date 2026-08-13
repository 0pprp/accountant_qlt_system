<template>
  <div class="rounded-2xl bg-white border-2 border-line p-6 shadow-sm h-full">
    <div class="flex flex-col gap-4 items-center h-full">
      <div class="flex items-center gap-2 justify-between w-full">
        <p class="text-base font-medium text-gray-700 w-full text-right">
          {{ $t('dashboard.user.title') }}
        </p>

        <SvgIcon name="userOutlined" class="text-gray-700 w-6 h-6" />
      </div>

      <div class="flex flex-col gap-4 items-center py-8 h-full">
        <div
          class="w-24 h-24 rounded-2xl bg-surface-3 flex items-center justify-center overflow-hidden"
        >
          <img
            :src="user.avatarUrl ?? '/images/ProfilePic.png'"
            :alt="user.fullName"
            class="w-full h-full object-cover"
          />
        </div>

        <p class="text-xl font-semibold text-gray-900 text-center">
          {{ user.fullName }}
        </p>

        <div class="flex items-center gap-2">
          <SvgIcon name="medalStarCircle" class="w-5 h-5 text-gray-700" />
          <span class="text-sm text-gray-700">{{ user.role }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { DashboardUser } from '@/modules/Core/types/model/dashboard'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

defineProps<{
  user: DashboardUser
}>()
</script>
