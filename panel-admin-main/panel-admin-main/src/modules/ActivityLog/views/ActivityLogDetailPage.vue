<template>
  <div class="flex flex-col gap-6 p-4">
    <div v-if="isLoading" class="flex justify-center py-12">
      <i class="pi pi-spin pi-spinner text-2xl text-primary"></i>
    </div>

    <div
      v-else-if="isError || !activityLog"
      class="rounded-xl border border-line bg-white p-8 text-center text-gray-500"
    >
      {{ $t('activityLog.notFound') }}
    </div>

    <template v-else>
      <div
        class="flex flex-row justify-between items-center gap-2 border-b border-line pb-4"
      >
        <h1 class="text-xl font-bold text-gray-900 text-center">
          {{ $t('activityLog.detailTitle') }}
        </h1>

        <SvgIcon
          name="arrowLeft"
          class="w-6 h-6 text-gray-700 cursor-pointer hover:text-primary transition-colors duration-300"
          @click="router.back()"
        />
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <DetailSectionCard
          :title="$t('activityLog.detail.activityInfo')"
          icon="noteDocument"
        >
          <DetailField
            :label="$t('activityLog.detail.activityType')"
            :value="getActivityTypeLabel(activityLog.activityType, t)"
          />
          <DetailField
            :label="$t('activityLog.detail.description')"
            :value="
              activityLog.description ||
              getActivityTypeLabel(activityLog.activityType, t)
            "
          />
          <DetailField
            :label="$t('activityLog.detail.createdAt')"
            :value="formattedDate(activityLog.createdAt)"
          />
        </DetailSectionCard>

        <DetailSectionCard
          :title="$t('activityLog.detail.entityInfo')"
          icon="noteCheckList"
        >
          <DetailField
            :label="$t('activityLog.detail.targetEntityType')"
            :value="getTargetEntityTypeLabel(activityLog.targetEntityType, t)"
          />
          <DetailField
            :label="$t('activityLog.detail.branchName')"
            :value="branchDisplayName"
          />
        </DetailSectionCard>

        <DetailSectionCard
          :title="$t('activityLog.detail.userInfo')"
          icon="userCard"
        >
          <DetailField
            :label="$t('activityLog.detail.userName')"
            :value="activityLog.userName"
          />
          <DetailField
            :label="$t('activityLog.detail.userRoles')"
            :value="activityLog.userRoles"
          />
          <DetailField
            :label="$t('activityLog.detail.userId')"
            :value="String(activityLog.userId)"
          />
        </DetailSectionCard>

        <DetailSectionCard
          :title="$t('activityLog.detail.deviceInfo')"
          icon="monitor"
        >
          <DetailField
            :label="$t('activityLog.detail.deviceType')"
            :value="getDeviceTypeLabel(activityLog.deviceType, t)"
          />
          <DetailField
            :label="$t('activityLog.detail.browser')"
            :value="activityLog.browser"
          />
          <DetailField
            :label="$t('activityLog.detail.operatingSystem')"
            :value="activityLog.operatingSystem"
          />
          <DetailField
            :label="$t('activityLog.detail.ipAddress')"
            :value="activityLog.ipAddress"
          />
          <DetailField
            :label="$t('activityLog.detail.userAgent')"
            :value="activityLog.userAgent"
          />
        </DetailSectionCard>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import DetailField from '../components/DetailField/DetailField.vue'
import DetailSectionCard from '../components/DetailSectionCard/DetailSectionCard.vue'
import { useActivityLogByIdQuery } from '../requests/queries'
import { formattedDate } from '@/modules/Core/utils/time.ts'
import {
  getActivityTypeLabel,
  getDeviceTypeLabel,
  getTargetEntityTypeLabel,
} from '@/modules/ActivityLog/utils/labels'
import useAuthStore from '@/modules/Auth/store'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

const route = useRoute()
const { t } = useI18n()
const authStore = useAuthStore()
const router = useRouter()
const activityLogId = computed(() => Number(route.params.id))

const {
  data: activityLog,
  isLoading,
  isError,
} = useActivityLogByIdQuery(activityLogId)

const branchDisplayName = computed(() => {
  if (!activityLog.value) {
    return '-'
  }

  const branch = authStore.branches?.find(
    (item) => item.id === activityLog.value!.branchId
  )

  return branch?.name ?? String(activityLog.value.branchId)
})
</script>
