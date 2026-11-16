<template>
  <AppSelect
    :model-value="provinceId ?? null"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :options="provinceOptions"
    :error="error"
    :required="required"
    :disabled="readonly || disabled"
    @update:model-value="onProvince"
  />
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { AppSelect } from '@/modules/Core/components/ui'
import type { Props } from './ProvinceSelectInput.types'
import { useProvinceDataQuery } from '@/modules/Core/requests/queries'

defineProps<Props>()

const provinceId = defineModel<Props['modelValue']>('modelValue')

const pageIndex = ref(1)
const pageSize = ref(100)

const { data } = useProvinceDataQuery(pageIndex, pageSize)

const provinceOptions = computed(() =>
  (data.value?.items || []).map((p) => ({ label: p.name, value: p.id })),
)

function onProvince(value: string | number | null | boolean) {
  if (typeof value === 'number' && Number.isFinite(value)) {
    provinceId.value = value
    return
  }
  if (value == null || value === '') {
    provinceId.value = null
    return
  }
  const n = Number(value)
  provinceId.value = Number.isFinite(n) ? n : null
}
</script>
