<template>
  <div class="flex flex-col">
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <Select
      :options="userData?.paginatedUsers.items"
      v-model="selectedUser"
      optionLabel="FullName"
      :unstyled="false"
      optionValue="Id"
      class="select"
      :required
      :readonly
      :disabled="readonly || disabled"
      :placeholder="$t(placeholder)"
      :loading="isUserPending"
      @update:model-value="onUserSelect"
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs pt-2"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Select, Message } from 'primevue'
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
  branchId
)

watch(
  [userData, () => userId.value],
  ([newData, currentUserId]) => {
    if (currentUserId && newData?.paginatedUsers.items?.length) {
      const foundUser = newData.paginatedUsers.items.find(
        (user) => Number(user.Id) === currentUserId
      )
      selectedUser.value = foundUser ? Number(foundUser.Id) : null
    } else if (!currentUserId) {
      selectedUser.value = null
    }
  },
  { immediate: true }
)

watch(selectedUser, (newUser) => {
  userId.value = newUser || null
})

function onUserSelect(value: number | null) {
  selectedUser.value = value
}
</script>
