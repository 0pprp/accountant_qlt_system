<template>
  <AppSelect
    :model-value="selectedUser"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :options="userOptions"
    :error="error"
    :required="required"
    :disabled="readonly || disabled || isUserPending"
    @update:model-value="onUserSelect"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AppSelect } from '@/modules/Core/components/ui'
import type { Props } from './UsersSelectInput.types'
import { useUserDataQuery } from '@/modules/User/requests/queries'

const props = defineProps<Props>()

const userId = defineModel<Props['modelValue']>('modelValue')

const roleId = computed(() => props.roleId ?? null)
const selectedUser = ref<number | null>(null)
const pageIndex = ref(1)
const pageSize = ref(100)
const branchId = computed(() => props.branchId ?? 0)

const { data: userData, isPending: isUserPending } = useUserDataQuery(
  pageIndex,
  pageSize,
  roleId,
  branchId,
)

const userOptions = computed(() =>
  (userData.value?.paginatedUsers.items ?? []).flatMap((user) => {
    const record = user as typeof user & {
      FullName?: unknown
      fullName?: unknown
      Id?: unknown
    }
    const value = Number(record.Id ?? record.id)
    if (!Number.isFinite(value)) return []
    const label = record.FullName ?? record.fullName ?? ''
    return [{ label: label == null ? '' : String(label), value }]
  }),
)

function onUserSelect(value: string | number | null | boolean) {
  if (typeof value === 'number' && Number.isFinite(value)) {
    selectedUser.value = value
    return
  }
  if (value == null || value === '') {
    selectedUser.value = null
    return
  }
  const n = Number(value)
  selectedUser.value = Number.isFinite(n) ? n : null
}

watch(
  [userData, () => userId.value],
  ([newData, currentUserId]) => {
    if (currentUserId && newData?.paginatedUsers.items?.length) {
      const foundUser = newData.paginatedUsers.items.find((user) => {
        const record = user as typeof user & { Id?: unknown }
        return Number(record.Id ?? record.id) === currentUserId
      })
      selectedUser.value = foundUser
        ? Number((foundUser as typeof foundUser & { Id?: unknown }).Id ?? foundUser.id)
        : null
    } else if (!currentUserId) {
      selectedUser.value = null
    }
  },
  { immediate: true },
)

watch(selectedUser, (newUser) => {
  userId.value = newUser || null
})
</script>
