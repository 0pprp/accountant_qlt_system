<template>
  <!-- نفس تصميم المنصب الوظيفي: AppSelect فقط، بدون Prime Select متداخل -->
  <AppSelect
    v-if="!multiple"
    :model-value="singleBranchId"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :options="branchOptions"
    :error="error"
    :required="required"
    :disabled="readonly || disabled"
    @update:model-value="onSingleBranch"
  />
  <AppSelect
    v-else
    :model-value="multiFirstId"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :options="branchOptions"
    :error="error"
    :required="required"
    :disabled="readonly || disabled"
    @update:model-value="onMultiAsSingle"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AppSelect } from '@/modules/Core/components/ui'
import type { Props } from './BranchSelectInput.types'
import type { Branch } from '@/modules/Branch/types/model'
import { useBranchDataQuery } from '@/modules/Branch/requests/queries'

const props = defineProps<Props>()

const branchId = defineModel<Props['modelValue']>('modelValue')

const singleBranchId = ref<number | null>(null)
const pageIndex = ref(1)
const pageSize = ref(100)
const branches = ref<Array<Branch>>()

const { data } = useBranchDataQuery(pageIndex, pageSize)

const branchOptions = computed(() =>
  (branches.value ?? []).map((b) => ({ label: b.name, value: b.id })),
)

const multiFirstId = computed(() => {
  const v = branchId.value
  if (Array.isArray(v) && v.length > 0) return v[0] ?? null
  return typeof v === 'number' ? v : null
})

function applyBranchModel(value: Props['modelValue']) {
  if (props.multiple) return
  const next = value == null ? null : Array.isArray(value) ? (value[0] ?? null) : value
  if (singleBranchId.value !== next) singleBranchId.value = next
}

function onSingleBranch(value: string | number | null | boolean) {
  if (typeof value === 'number' && Number.isFinite(value)) {
    singleBranchId.value = value
    return
  }
  if (value == null || value === '') {
    singleBranchId.value = null
    return
  }
  const n = Number(value)
  singleBranchId.value = Number.isFinite(n) ? n : null
}

function onMultiAsSingle(value: string | number | null | boolean) {
  if (typeof value === 'number' && Number.isFinite(value)) {
    branchId.value = [value]
    return
  }
  if (value == null || value === '') {
    branchId.value = null
    return
  }
  const n = Number(value)
  branchId.value = Number.isFinite(n) ? [n] : null
}

watch(
  data,
  (newData) => {
    branches.value = newData?.data.items || []
    applyBranchModel(branchId.value)
  },
  { immediate: true },
)

watch(
  () => branchId.value,
  (value) => {
    applyBranchModel(value)
  },
)

watch(singleBranchId, (newBranch) => {
  if (props.multiple) return
  const next = typeof newBranch === 'number' ? newBranch : null
  if (branchId.value !== next) branchId.value = next
})
</script>
