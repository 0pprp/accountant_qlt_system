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
      :options="filteredSafe"
      v-model="selectedSafe"
      :disabled="readonly || disabled"
      optionLabel="name"
      :unstyled="false"
      class="select"
      :required="required"
      :placeholder="$t(placeholder)"
      @change="onOrderListChange"
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
import { computed, ref, watch, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { Select, Message } from 'primevue'
import type { Props } from './SafeSelectInput.types'
import { useSafeDataQuery } from '@/modules/Safe/requests/queries'
import type { Safe } from '@/modules/Safe/types/model'

const props = defineProps<Props>()

const safeId = defineModel<Props['modelValue']>('modelValue')

type SafeOption = Safe | { id: null; name: string; branchId: null }

const { t } = useI18n()
const { data } = useSafeDataQuery()

const filteredSafe = computed<SafeOption[]>(() => {
  const baseList = props.items || safeList.value || []
  let result = [...baseList]

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
      (item) => item.id === -1 || (item.id === null && item.branchId === null)
    )
    if (!hasForeign) {
      result = [foreignSafe, ...result]
    }
  }

  return result
})

const selectedSafe = ref<SafeOption | null>(null)

const safeList = ref<Array<Safe>>([])

function findSafeById(id: number | null): SafeOption | null {
  if (id === null || id === -1) {
    if (props.hasForeignSafe) {
      return (
        filteredSafe.value.find(
          (safe) =>
            safe.id === -1 || (safe.id === null && safe.branchId === null)
        ) || null
      )
    }
    return null
  }
  return filteredSafe.value.find((safe) => safe.id === id) || null
}

function initializeSelection() {
  if (safeId.value === null || safeId.value === undefined) {
    selectedSafe.value = null
    return
  }

  const id = Array.isArray(safeId.value) ? safeId.value[0] : safeId.value
  selectedSafe.value = findSafeById(id)
}

watch(
  [data, () => props.items],
  ([newData, newItems]) => {
    if (newItems) {
      safeList.value = []
    } else {
      safeList.value = newData || []
    }
    initializeSelection()
  },
  { immediate: true }
)

watch(
  safeId,
  (newId) => {
    // اگر newId تعریف نشده، selectedSafe را null کن
    if (newId === undefined) {
      if (selectedSafe.value !== null) {
        selectedSafe.value = null
      }
      return
    }

    const id = newId === null ? null : Array.isArray(newId) ? newId[0] : newId
    const newSelection = findSafeById(id)

    if (JSON.stringify(selectedSafe.value) !== JSON.stringify(newSelection)) {
      selectedSafe.value = newSelection
    }
  },
  { immediate: true }
)

watch(selectedSafe, (newSelection) => {
  const newModelValue: Props['modelValue'] = newSelection
    ? newSelection.id
    : null

  if (JSON.stringify(safeId.value) !== JSON.stringify(newModelValue)) {
    safeId.value = newModelValue
  }
})

function onOrderListChange(event: { value: SafeOption }) {
  selectedSafe.value = event.value
}

onMounted(() => {
  initializeSelection()
})
</script>
