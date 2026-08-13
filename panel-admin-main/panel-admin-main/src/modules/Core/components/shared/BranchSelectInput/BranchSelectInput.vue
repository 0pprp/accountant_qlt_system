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

    <MultiSelect
      v-if="multiple"
      :options="filteredBranches"
      v-model="selectedBranch"
      :disabled="readonly"
      optionLabel="name"
      :unstyled="false"
      optionValue="id"
      class="select"
      :required
      :readonly
      :placeholder="$t(placeholder)"
      @select="onBranchSelect"
    />

    <Select
      v-else
      :options="filteredBranches"
      v-model="selectedBranch"
      :disabled="readonly"
      optionLabel="name"
      :unstyled="false"
      optionValue="id"
      class="select"
      :required
      :placeholder="$t(placeholder)"
      @select="onBranchSelect"
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Select, Message, MultiSelect } from 'primevue'
import type { Props } from './BranchSelectInput.types'
import type { Branch } from '@/modules/Branch/types/model'
import { useBranchDataQuery } from '@/modules/Branch/requests/queries'

const props = defineProps<Props>()

const branchId = defineModel<Props['modelValue']>('modelValue')

const filteredBranches = computed(() => branches.value)
const selectedBranch = ref<number | Array<number> | null>(null)

const pageIndex = ref(1)
const pageSize = ref(100)

const branches = ref<Array<Branch>>()

const { data } = useBranchDataQuery(pageIndex, pageSize)

watch(
  data,
  (newData) => {
    branches.value = newData?.data.items || []

    if (branchId.value !== null && branchId.value !== undefined) {
      if (props.multiple) {
        selectedBranch.value = Array.isArray(branchId.value)
          ? branchId.value
          : [branchId.value]
      } else {
        selectedBranch.value = Array.isArray(branchId.value)
          ? branchId.value[0]
          : branchId.value
      }
    }
  },
  { immediate: true }
)

watch(selectedBranch, (newBranch) => {
  if (props.multiple) {
    branchId.value =
      newBranch && Array.isArray(newBranch) && newBranch.length > 0
        ? newBranch
        : null
  } else {
    branchId.value =
      newBranch && typeof newBranch === 'number' ? newBranch : null
  }
})
function onBranchSelect(event: { value: Branch | Array<Branch> }) {
  if (Array.isArray(event.value)) {
    selectedBranch.value = event.value.map((branch) => branch.id)
  } else {
    selectedBranch.value = event.value.id
  }
}
</script>
