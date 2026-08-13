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
      v-model="provinceId"
      :options="provinces"
      optionLabel="name"
      optionValue="id"
      :readonly="readonly"
      :required="required"
      :placeholder="$t(placeholder)"
      :unstyled="false"
      class="select"
    />

    <Message
      v-if="error"
      severity="error"
      variant="simple"
      class="text-error text-xs py-2"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { Select, Message } from 'primevue'
import type { Props } from './ProvinceSelectInput.types'
import { useProvinceDataQuery } from '@/modules/Core/requests/queries'

defineProps<Props>()

const provinceId = defineModel<Props['modelValue']>('modelValue')

const pageIndex = ref(1)
const pageSize = ref(100)

const { data } = useProvinceDataQuery(pageIndex, pageSize)

const provinces = computed(() => data.value?.items || [])
</script>
