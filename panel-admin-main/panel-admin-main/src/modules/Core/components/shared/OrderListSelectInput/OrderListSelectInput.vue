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
      :options="filteredOrdersList"
      v-model="selectedOrderList"
      optionLabel="name"
      :unstyled="false"
      optionValue="id"
      class="select"
      :readonly="readonly"
      :required="required"
      :disabled="readonly || disabled"
      :placeholder="$t(placeholder)"
      @change="onOrderListChange"
    />

    <Select
      v-else
      :options="filteredOrdersList"
      v-model="selectedOrderList"
      :disabled="readonly || disabled"
      optionLabel="name"
      :unstyled="false"
      optionValue="id"
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
import { Select, Message, MultiSelect } from 'primevue'
import type { Props } from './OrderListSelectInput.types'
import type { Order } from '@/modules/Order/types/model'
import { useOrderDataQuery } from '@/modules/Order/requests/queries'
import useAuthStore from '@/modules/Auth/store'

const props = defineProps<Props>()

const orderListId = defineModel<Props['modelValue']>('modelValue')

const filteredOrdersList = computed(() => ordersList.value || [])
const selectedOrderList = ref<number | Array<number> | null>(null)

const pageIndex = ref(1)
const pageSize = ref(100)

const ordersList = ref<Array<Order>>([])
const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const { data } = useOrderDataQuery(pageIndex, pageSize, branchId)

function initializeSelection() {
  if (!orderListId.value) {
    selectedOrderList.value = null
    return
  }

  if (props.multiple) {
    selectedOrderList.value = Array.isArray(orderListId.value)
      ? orderListId.value
      : [orderListId.value]
  } else {
    selectedOrderList.value = Array.isArray(orderListId.value)
      ? orderListId.value[0] || null
      : orderListId.value
  }
}

watch(
  data,
  (newData) => {
    ordersList.value = newData?.items || []
    initializeSelection()
  },
  { immediate: true }
)

watch(
  orderListId,
  (newId) => {
    const currentSelection = selectedOrderList.value

    if (props.multiple) {
      const newSelection = newId
        ? Array.isArray(newId)
          ? newId
          : [newId]
        : null

      if (JSON.stringify(currentSelection) !== JSON.stringify(newSelection)) {
        selectedOrderList.value = newSelection
      }
    } else {
      const newSelection = newId
        ? Array.isArray(newId)
          ? newId[0]
          : newId
        : null

      if (currentSelection !== newSelection) {
        selectedOrderList.value = newSelection
      }
    }
  },
  { immediate: true }
)

watch(selectedOrderList, (newSelection) => {
  let newModelValue: Props['modelValue']

  if (props.multiple) {
    newModelValue = newSelection
      ? Array.isArray(newSelection)
        ? newSelection
        : [newSelection]
      : null
  } else {
    if (Array.isArray(newSelection)) {
      newModelValue = newSelection.length > 0 ? newSelection[0] : null
    } else {
      newModelValue = newSelection
    }
  }

  if (JSON.stringify(orderListId.value) !== JSON.stringify(newModelValue)) {
    orderListId.value = newModelValue
  }
})

function onOrderListChange(event: { value: number | number[] }) {
  selectedOrderList.value = event.value
}

onMounted(() => {
  initializeSelection()
})
</script>
