<template>
  <AppSelect
    :model-value="selectedSafeId"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :options="safeOptions"
    :error="error"
    :required="required"
    :disabled="readonly || disabled"
    @update:model-value="onPick"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { AppSelect } from '@/modules/Core/components/ui'
import type { Props } from './SafeSelectInput.types'
import { useSafeDataQuery } from '@/modules/Safe/requests/queries'
import type { Safe } from '@/modules/Safe/types/model'

const props = defineProps<Props>()

const safeId = defineModel<Props['modelValue']>('modelValue')

type SafeOption = Safe | { id: number | null; name: string; branchId: null }

const { t } = useI18n()
const { data } = useSafeDataQuery()

const safeList = ref<Array<Safe>>([])

const filteredSafe = computed<SafeOption[]>(() => {
  const baseList = props.items || safeList.value || []
  let result: SafeOption[] = [...baseList]

  if (props.hasMainSafe) {
    const mainSafe = data.value?.find((safe) => safe.branchId === null)
    if (mainSafe) {
      result = [mainSafe, ...result]
    }
  } else {
    result = result.filter((safe) => safe.branchId !== null)
  }

  if (props.hasForeignSafe) {
    const foreignSafe = {
      id: -1,
      name: t('safe.foreignSafe'),
      branchId: null,
    }
    const hasForeign = result.some(
      (item) => item.id === -1 || (item.id === null && item.branchId === null),
    )
    if (!hasForeign) {
      result = [foreignSafe, ...result]
    }
  }

  return result
})

const safeOptions = computed(() =>
  filteredSafe.value.map((s) => ({
    label: s.name,
    value: s.id === null ? -1 : s.id,
  })),
)

const selectedSafeId = computed(() => {
  if (safeId.value === null || safeId.value === undefined) return null
  const id = Array.isArray(safeId.value) ? safeId.value[0] : safeId.value
  if (id === null || id === -1) return -1
  return id
})

function onPick(v: string | number | null | boolean) {
  if (v == null || v === '') {
    safeId.value = null
    return
  }
  const n = typeof v === 'number' ? v : Number(v)
  safeId.value = Number.isFinite(n) ? n : null
}

watch(
  [data, () => props.items],
  ([newData, newItems]) => {
    if (newItems) {
      safeList.value = []
    } else {
      safeList.value = newData || []
    }
  },
  { immediate: true },
)
</script>
