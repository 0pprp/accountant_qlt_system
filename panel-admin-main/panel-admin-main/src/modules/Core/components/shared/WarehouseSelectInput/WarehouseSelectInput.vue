<template>
  <div>
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <AutoComplete
      :suggestions="filteredWarehouses"
      @complete="search"
      @select="onWarehouseSelect"
      v-model="selectedWarehouse"
      optionLabel="name"
      :unstyled="false"
      optionValue="id"
      :required
      :forceSelection="true"
      :autoHighlight="true"
      :completeOnFocus="true"
      :disabled="readonly || disabled"
      class="!block"
      :placeholder="$t(placeholder)"
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs py-2"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AutoComplete, Message } from 'primevue'
import type { Props } from './WarehouseSelectInput.types'
import { useBranchWarehousesDataQuery } from '@/modules/Branch/requests/queries'
import type { Warehouse } from '@/modules/Warehouse/types/model'
import useAuthStore from '@/modules/Auth/store'

defineProps<Props>()

const warehouseId = defineModel<Props['modelValue']>('modelValue')

const filteredWarehouses = ref<Array<Warehouse>>([])
const selectedWarehouse = ref<Warehouse | null>(null)

const warehouses = ref<Array<Warehouse>>()

const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch!.id)

const { data } = useBranchWarehousesDataQuery(branchId)

watch(
  data,
  (newData) => {
    if (Array.isArray(newData)) {
      warehouses.value = newData
    } else {
      warehouses.value = []
    }

    if (warehouseId.value) {
      selectedWarehouse.value =
        warehouses.value.find(
          (warehouse) => warehouse.id === warehouseId.value
        ) || null
    }
  },
  { immediate: true }
)

async function search(event: { query: string }) {
  // TODO searchable with searchTerm in queries after add backend this
  if (warehouses.value) {
    setTimeout(() => {
      if (!event.query.trim().length) {
        filteredWarehouses.value = [...warehouses.value!]
      } else {
        filteredWarehouses.value = warehouses.value!.filter((warehouse) => {
          return warehouse.name
            .toLowerCase()
            .startsWith(event.query.toLowerCase())
        })
      }
    }, 20)
  }
}

watch(selectedWarehouse, (newWarehouse) => {
  warehouseId.value = newWarehouse ? newWarehouse.id : null
})

function onWarehouseSelect(event: { value: Warehouse }) {
  selectedWarehouse.value = event.value
}
</script>
