<template>
  <AppSelect
    v-if="!multiple"
    :model-value="singleOrderId"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :options="orderOptions"
    :error="error"
    :required="required"
    :disabled="readonly || disabled"
    @update:model-value="onSingleOrder"
  />
  <div
    v-else
    class="q-field"
  >
    <span
      class="q-field__label"
      :class="{ 'is-required': required }"
    >
      {{ $t(label) }}
    </span>
    <MultiSelect
      v-model="selectedOrderList"
      :options="filteredOrdersList"
      optionLabel="name"
      optionValue="id"
      class="select"
      :required="required"
      :disabled="readonly || disabled"
      :placeholder="$t(placeholder)"
    />
    <span
      v-if="error"
      class="q-field__error"
      role="alert"
    >
      {{ error }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted } from 'vue'
import { MultiSelect } from 'primevue'
import { AppSelect } from '@/modules/Core/components/ui'
import type { Props } from './OrderListSelectInput.types'
import type { Order } from '@/modules/Order/types/model'
import { useOrderDataQuery } from '@/modules/Order/requests/queries'
import useAuthStore from '@/modules/Auth/store'

const props = defineProps<Props>()

const orderListId = defineModel<Props['modelValue']>('modelValue')

const filteredOrdersList = computed(() => ordersList.value || [])
const selectedOrderList = ref<number | Array<number> | null>(null)
const singleOrderId = ref<number | null>(null)

const pageIndex = ref(1)
const pageSize = ref(100)

const ordersList = ref<Array<Order>>([])
const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const { data } = useOrderDataQuery(pageIndex, pageSize, branchId)

const orderOptions = computed(() =>
  filteredOrdersList.value.map((order) => {
    const name = (order as Order & { name?: string }).name
    return { label: name ?? '', value: order.id }
  }),
)

function onSingleOrder(value: string | number | null | boolean) {
  if (typeof value === 'number' && Number.isFinite(value)) {
    singleOrderId.value = value
    return
  }
  if (value == null || value === '') {
    singleOrderId.value = null
    return
  }
  const n = Number(value)
  singleOrderId.value = Number.isFinite(n) ? n : null
}

function initializeSelection() {
  if (!orderListId.value) {
    selectedOrderList.value = null
    singleOrderId.value = null
    return
  }

  if (props.multiple) {
    selectedOrderList.value = Array.isArray(orderListId.value)
      ? orderListId.value
      : [orderListId.value]
  } else {
    singleOrderId.value = Array.isArray(orderListId.value)
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
  { immediate: true },
)

watch(
  orderListId,
  () => {
    initializeSelection()
  },
  { immediate: true },
)

watch(selectedOrderList, (newSelection) => {
  if (!props.multiple) return
  orderListId.value = newSelection
    ? Array.isArray(newSelection)
      ? newSelection
      : [newSelection]
    : null
})

watch(singleOrderId, (newSelection) => {
  if (props.multiple) return
  if (orderListId.value === newSelection) return
  orderListId.value = newSelection
})

onMounted(() => {
  initializeSelection()
})
</script>
