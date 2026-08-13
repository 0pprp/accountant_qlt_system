<template>
  <div
    class="flex flex-wrap gap-4 mb-4 p-4 rounded-xl border-line border bg-white mt-4"
  >
    <div class="min-w-[200px]">
      <SelectField
        v-model="localActivityType"
        :options="activityTypeOptions"
        label="activityLog.filters.activityType.label"
        clearable
        placeholder="activityLog.filters.activityType.placeholder"
      />
    </div>

    <div class="min-w-[200px]">
      <SelectField
        v-model="localTargetEntityType"
        :options="targetEntityTypeOptions"
        label="activityLog.filters.targetEntityType.label"
        clearable
        placeholder="activityLog.filters.targetEntityType.placeholder"
      />
    </div>

    <div class="min-w-[200px]">
      <SelectField
        v-model="localUserId"
        :options="userFilterOptions"
        label="activityLog.filters.user.label"
        clearable
        placeholder="activityLog.filters.user.placeholder"
        :loading="isUsersLoading"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'
import {
  ACTIVITY_TYPE_KEYS,
  TARGET_ENTITY_TYPE_KEYS,
} from '@/modules/ActivityLog/constants/enums'
import { buildEnumSelectOptions } from '@/modules/ActivityLog/utils/labels'
import { useUserDataQuery } from '@/modules/User/requests/queries'
import type { User } from '@/modules/User/types/model'
import useAuthStore from '@/modules/Auth/store'

const localActivityType = defineModel<number | null>('activityType', {
  default: null,
})

const localTargetEntityType = defineModel<number | null>('targetEntityType', {
  default: null,
})

const localUserId = defineModel<number | null>('userId', { default: null })

function getUserOptionValue(user: User): number | null {
  const id = user.Id ?? user.id
  const numericId = Number(id)
  return Number.isFinite(numericId) ? numericId : null
}

function getUserOptionLabel(user: User): string {
  const fullName = user.FullName ?? user.fullName
  if (typeof fullName === 'string' && fullName.trim()) {
    return fullName
  }

  const id = getUserOptionValue(user)
  return id !== null ? String(id) : '-'
}

const { t } = useI18n()
const authStore = useAuthStore()

const pageIndex = computed(() => 1)
const pageSize = computed(() => 100)
const branchId = computed(() => authStore.selectedBranch?.id ?? 0)
const searchTerm = computed(() => null)
const roleIds = computed(() => null)

const { data: userListData, isFetching: isUsersLoading } = useUserDataQuery(
  pageIndex,
  pageSize,
  roleIds,
  branchId,
  searchTerm
)

const activityTypeOptions = computed(() =>
  buildEnumSelectOptions(
    ACTIVITY_TYPE_KEYS,
    t,
    'activityLog.filters.activityType.all'
  )
)

const targetEntityTypeOptions = computed(() =>
  buildEnumSelectOptions(
    TARGET_ENTITY_TYPE_KEYS,
    t,
    'activityLog.filters.targetEntityType.all'
  )
)

const userFilterOptions = computed(() => {
  const options: Array<{ value: number | null; label: string }> = [
    {
      value: null,
      label: t('activityLog.filters.user.all'),
    },
  ]

  userListData.value?.paginatedUsers.items.forEach((user) => {
    const value = getUserOptionValue(user)
    if (value === null) {
      return
    }

    options.push({
      value,
      label: getUserOptionLabel(user),
    })
  })

  return options
})
</script>
