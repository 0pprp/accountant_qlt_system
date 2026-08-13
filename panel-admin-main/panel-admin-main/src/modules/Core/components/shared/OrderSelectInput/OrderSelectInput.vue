<template>
  <div>
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="label ? $t(label) : ''"
    >
      {{ label ? $t(label) : '' }}
    </span>

    <AutoComplete
      :suggestions="filteredOrders"
      @complete="search"
      @select="onOrderSelect"
      @clear="onClear"
      v-model="selectedOrder"
      :optionLabel="getDisplayLabel"
      :unstyled="false"
      optionValue="id"
      class="!block"
      :forceSelection="true"
      :autoHighlight="true"
      :completeOnFocus="true"
      :required
      :disabled="readonly || disabled || !customerId"
      :placeholder="placeholder ? $t(placeholder) : ''"
      :loading="isLoading"
    >
      <template #option="{ option }">
        <div class="flex flex-col">
          <span class="font-medium">
            {{ option.productsSummary || '' }}
            <span v-if="option.orderListName" class="text-gray-500">
              ({{ option.orderListName }})
            </span>
          </span>
          <small class="text-sm text-gray-500">
            {{ option.customerFullName }} - {{ formattedDate(option.saleDate) }}
          </small>
        </div>
      </template>
    </AutoComplete>

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
import type { Props } from './OrderSelectInput.types'
import { useOrdersByCustomerQuery } from '@/modules/Order/requests/queries'
import type { Order } from '@/modules/Order/types/model'
import {
  getApprovalStatusFromRow,
  getExecutionStatusFromRow,
  isOrderInProgress,
} from '@/modules/Order/utils/orderStatus'
import useAuthStore from '@/modules/Auth/store'
import { formattedDate } from '@/modules/Core/utils/time'

const props = defineProps<Props>()

const orderId = defineModel<Props['modelValue']>('modelValue')

const filteredOrders = ref<Array<Order>>([])
const selectedOrder = ref<Order | null>(null)

const pageIndex = ref(1)
const pageSize = ref(100)
const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)
const customerId = computed(() => props.customerId ?? null)

const orders = ref<Array<Order>>([])

const { data, isLoading } = useOrdersByCustomerQuery(
  pageIndex,
  pageSize,
  branchId,
  customerId
)

watch(
  data,
  (newData) => {
    let items = newData?.paginatedOrders?.items || []

    if (props.inProgressOnly) {
      items = items.filter((item: Record<string, unknown>) =>
        isOrderInProgress(item)
      )
    }

    orders.value = items.map((item: Record<string, unknown>) => ({
      id: (item.id as number) || (item.Id as number) || 0,
      orderListName:
        (item.orderListName as string) ||
        (item.OrderListName as string) ||
        (item.name as string) ||
        '',
      customerFullName:
        (item.customerFullName as string) ||
        (item.CustomerFullName as string) ||
        '',
      saleDate:
        (item.saleDate as string) ||
        (item.SaleDate as string) ||
        (item.createdAt as string) ||
        (item.CreatedAt as string) ||
        '',
      buyAmount: (item.buyAmount as number) || (item.BuyAmount as number) || 0,
      createdAt: (item.createdAt as string) || (item.CreatedAt as string) || '',
      customerNationalCode:
        (item.customerNationalCode as string) ||
        (item.CustomerNationalCode as string) ||
        '',
      customerPhoneNumber:
        (item.customerPhoneNumber as string) ||
        (item.CustomerPhoneNumber as string) ||
        '',
      dailyInstallmentAmount:
        (item.dailyInstallmentAmount as number) ||
        (item.DailyInstallmentAmount as number) ||
        0,
      installmentsCount:
        (item.installmentsCount as number) ||
        (item.InstallmentsCount as number) ||
        0,
      lastInstallmentDate:
        (item.lastInstallmentDate as string) ||
        (item.LastInstallmentDate as string) ||
        '',
      prepaymentAmount:
        (item.prepaymentAmount as number) ||
        (item.PrepaymentAmount as number) ||
        0,
      productsSummary:
        (item.productsSummary as string) ||
        (item.ProductsSummary as string) ||
        '',
      remainingAmount:
        (item.remainingAmount as number) ||
        (item.RemainingAmount as number) ||
        0,
      sellAmount:
        (item.sellAmount as number) || (item.SellAmount as number) || 0,
      sellerName:
        (item.sellerName as string) || (item.SellerName as string) || '',
      executionStatus: getExecutionStatusFromRow(item),
      approvalStatus: getApprovalStatusFromRow(item),
      totalInstallmentsAmount:
        (item.totalInstallmentsAmount as number) ||
        (item.TotalInstallmentsAmount as number) ||
        0,
    })) as Order[]
    filteredOrders.value = [...orders.value]

    if (orderId.value) {
      selectedOrder.value =
        orders.value.find((order) => order.id === orderId.value) || null
    } else {
      selectedOrder.value = null
    }
  },
  { immediate: true }
)

watch(customerId, () => {
  orderId.value = null
  selectedOrder.value = null
  filteredOrders.value = []
})

function getDisplayLabel(order: Order) {
  const summary = order.productsSummary || ''
  const listName = order.orderListName || ''
  if (listName) {
    return `${summary} (${listName})`
  }
  return summary
}

async function search(event: { query: string }) {
  const query = event.query.trim()

  if (orders.value) {
    setTimeout(() => {
      if (!query.length) {
        filteredOrders.value = [...orders.value]
      } else {
        filteredOrders.value = orders.value.filter((order) => {
          const summary = order.productsSummary?.toLowerCase() || ''
          const listName = order.orderListName?.toLowerCase() || ''
          const customerName = order.customerFullName?.toLowerCase() || ''
          return (
            summary.includes(query.toLowerCase()) ||
            listName.includes(query.toLowerCase()) ||
            customerName.includes(query.toLowerCase())
          )
        })
      }
    }, 20)
  }
}

watch(selectedOrder, (newOrder) => {
  if (newOrder && typeof newOrder === 'object' && newOrder.id) {
    orderId.value = newOrder.id
  } else {
    orderId.value = null
  }
})

function onOrderSelect(event: { value: Order }) {
  selectedOrder.value = event.value
}

function onClear() {
  selectedOrder.value = null
  orderId.value = null
}
</script>
