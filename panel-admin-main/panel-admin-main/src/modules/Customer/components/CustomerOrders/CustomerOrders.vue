<template>
  <div class="flex flex-col gap-4 px-6">
    <!-- Orders List View -->
    <template v-if="!isViewingDetails">
      <div v-if="isPending" class="flex justify-center w-full py-20">
        <ProgressSpinner class="!w-10 !h-10" :unstyled="false" />
      </div>

      <div
        v-else-if="!ordersData || ordersData.length === 0"
        class="flex justify-center w-full py-20"
      >
        <NoData />
      </div>

      <div v-else class="flex flex-col gap-4">
        <Card
          v-for="order in ordersData"
          :key="order.id"
          class="rounded-lg bg-white p-4 shadow-sm"
        >
          <template #content>
            <div class="">
              <!-- Header Section -->
              <div class="flex justify-between items-start mb-4">
                <div class="flex flex-col gap-2">
                  <div class="flex items-center gap-2">
                    <span class="text-lg font-semibold text-gray-900">
                      {{ $t('customer.orders.shoppingCart') }} : {{ order.id }}
                    </span>
                  </div>
                  <span class="text-sm text-gray-600">
                    {{ $t('customer.orders.branchSafe') }}
                  </span>
                </div>

                <div class="flex flex-col gap-2 items-end">
                  <span class="text-sm text-gray-700">
                    {{ order.orderItems.length }}
                    {{ $t('customer.orders.items') }}
                  </span>
                  <span class="text-sm text-gray-600">
                    {{ formattedDate(order.createdAt) }}
                  </span>
                </div>
              </div>

              <hr class="my-4 border-line border !border-t-0" />

              <!-- Items Table -->
              <div class="!h-[152px] overflow-auto">
                <div
                  class="flex justify-between align-middle text-gray-300 mb-2"
                >
                  <span class="flex-1 text-right">{{
                    $t('customer.orders.item')
                  }}</span>
                  <span class="w-20 text-center">{{
                    $t('customer.orders.quantity')
                  }}</span>
                  <span class="w-24 text-center">{{
                    $t('customer.orders.price')
                  }}</span>
                  <span class="w-24 text-center">{{
                    $t('customer.orders.salePrice')
                  }}</span>
                </div>

                <div
                  v-for="item in order.orderItems"
                  :key="item.id"
                  class="flex justify-between align-middle py-2"
                >
                  <span class="flex-1 text-right text-gray-700">
                    {{ item.productName }}
                  </span>
                  <span class="w-20 text-center text-gray-900">
                    {{ item.quantity }}
                  </span>
                  <span class="w-24 text-center text-gray-900">
                    {{ formattedPrice(item.buyAmount) }}
                    <small class="text-gray-700 text-xs mr-1">
                      {{ $t('words.dinar') }}
                    </small>
                  </span>
                  <span class="w-24 text-center text-gray-900">
                    {{ formattedPrice(item.sellAmount) }}
                    <small class="text-gray-700 text-xs mr-1">
                      {{ $t('words.dinar') }}
                    </small>
                  </span>
                </div>
              </div>

              <hr class="my-4 border-line border !border-t-0" />

              <!-- Total Amount -->
              <div class="flex justify-between align-middle mb-4">
                <strong class="text-gray-900">
                  {{ $t('customer.orders.totalAmount') }}
                </strong>
                <span class="font-semibold text-gray-900">
                  {{ formattedPrice(order.sellAmount) }}
                  <small class="text-gray-700 text-sm mr-1">
                    {{ $t('words.dinar') }}
                  </small>
                </span>
              </div>

              <!-- Action Buttons -->
              <div class="flex justify-end gap-3 pt-4">
                <Button
                  class="!text-white !px-6 !py-2 !rounded-xl !bg-primary !border-none"
                  :unstyled="false"
                  @click="handleShowDetails(order.id)"
                >
                  {{ $t('customer.orders.customerPayments') }}
                </Button>
              </div>
            </div>
          </template>
        </Card>
      </div>
    </template>

    <!-- Order Details View -->
    <template v-else>
      <div
        class="flex justify-between items-center border border-line rounded-xl bg-white p-2"
      >
        <strong class="text-lg font-semibold text-center text-gray-900">
          {{ $t('customer.orders.shoppingCart') }}
        </strong>

        <Button
          class="!text-primary !px-4 !py-2 !rounded-lg !bg-transparent !border-none"
          :unstyled="false"
          @click="handleBack"
        >
          {{ $t('customer.orders.back') }}

          <i class="pi pi-angle-left text-primary text-lg"></i>
        </Button>
      </div>
      <div class="flex gap-4">
        <!-- Summary Card -->
        <div class="w-1/3">
          <Card class="rounded-lg bg-white p-6 shadow-sm sticky top-5">
            <template #content>
              <div v-if="isSummaryPending" class="flex justify-center py-10">
                <ProgressSpinner class="!w-8 !h-8" :unstyled="false" />
              </div>
              <div v-else-if="summaryData" class="flex flex-col gap-4">
                <!-- Summary Items -->
                <div
                  v-for="(item, index) in summaryItems"
                  :key="index"
                  class="flex justify-between items-center p-3 rounded-lg bg-gray-50"
                >
                  <div class="flex items-center gap-2">
                    <span
                      v-if="item.hasCircle"
                      :class="[
                        'w-4 h-4 rounded-full border border-line',
                        item.circleColor || 'bg-gray-300',
                      ]"
                    ></span>
                    <span class="text-gray-700">{{ item.label }}</span>
                  </div>
                  <span class="font-semibold text-gray-900">
                    {{ formattedPrice(item.value) }}
                    <small class="text-gray-600 text-sm mr-1">
                      {{ $t('words.dinar') }}
                    </small>
                  </span>
                </div>

                <!-- Donut Chart -->
                <div
                  v-if="chartData"
                  class="flex justify-center items-center mt-4 relative"
                >
                  <div class="w-full relative" style="height: 300px">
                    <Chart
                      type="doughnut"
                      :data="chartData"
                      :options="chartOptions"
                    />
                  </div>
                </div>
              </div>
            </template>
          </Card>
        </div>

        <!-- Installment Payments Table -->
        <div class="flex-1">
          <CustomDataTable
            :data="(paymentsData || []) as OrderInstallmentPayment[]"
            v-model:selection="selectedPayments"
            :columns="columns"
            :pageSize="paymentsData?.length || 0"
            selectable
            tableStyle="min-width: 100%"
            class="custom-table custom-border"
            :loading="isPaymentsPending"
          >
            <template #body-date="{ data }">
              {{ formattedDate(data.date) }}
            </template>

            <template #body-amount="{ data }">
              <span v-if="data.hasPayment && data.amount !== null">
                {{ formattedPrice(data.amount) }}
              </span>
              <span v-else class="text-gray-400">-</span>
              <span
                v-if="data.hasPayment && data.amount !== null"
                class="text-gray-700 px-1"
              >
                {{ $t('words.dinar') }}
              </span>
            </template>

            <template #body-description="{ data }">
              <span v-if="data.description">{{ data.description }}</span>
              <span v-else class="text-gray-400">-</span>
            </template>

            <template #body-hasPayment="{ data }">
              <span
                v-if="data.hasPayment"
                class="px-2 py-1 rounded-xl text-xs bg-success-50 border-success text-success border"
              >
                {{ $t('customer.orders.paid') }}
              </span>
              <span
                v-else
                class="px-2 py-1 rounded-xl text-xs bg-gray-50 text-gray-400 border-gray-400 border"
              >
                {{ $t('customer.orders.unpaid') }}
              </span>
            </template>
          </CustomDataTable>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { Button, Card, ProgressSpinner } from 'primevue'
import Chart from 'primevue/chart'
import { useI18n } from 'vue-i18n'
import type { Props } from './CustomerOrders.types'
import {
  useCustomerOrdersQuery,
  useOrderSummaryQuery,
} from '@/modules/Customer/requests/queries'
import { useOrderInstallmentPaymentsQuery } from '@/modules/Order/requests/queries'
import { formattedDate } from '@/modules/Core/utils/time'
import { formattedPrice } from '@/modules/Core/utils/price'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import NoData from '@/modules/Core/components/shared/NoData/NoData.vue'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import type { OrderInstallmentPayment } from '@/modules/Order/types/model'

const props = defineProps<Props>()
const { t } = useI18n()

const customerId = computed(() => props.customerId)
const selectedOrderId = ref<number | null>(null)
const isViewingDetails = computed(() => selectedOrderId.value !== null)

const { data: ordersData, isPending } = useCustomerOrdersQuery(customerId)

const orderId = computed(() => selectedOrderId.value ?? 0)

const { data: summaryData, isPending: isSummaryPending } = useOrderSummaryQuery(
  orderId,
  {
    enabled: () => isViewingDetails.value,
  }
)

const { data: paymentsData, isPending: isPaymentsPending } =
  useOrderInstallmentPaymentsQuery(orderId, {
    enabled: () => isViewingDetails.value,
  })

const selectedPayments = ref<OrderInstallmentPayment>()

const columns = computed<Column<AllowedTypes>[]>(() => [
  {
    field: 'date',
    header: t('payment.table.date'),
    sortable: true,
  },
  {
    field: 'amount',
    header: t('payment.table.amount'),
    sortable: true,
  },
  {
    field: 'description',
    header: t('payment.table.description'),
    sortable: false,
  },
  {
    field: 'hasPayment',
    header: t('customer.orders.paymentStatus'),
    sortable: false,
  },
])

const summaryItems = computed(() => {
  if (!summaryData.value) return []

  return [
    {
      label: t('customer.orders.summary.salePrice'),
      value: summaryData.value.sellAmount,
      hasCircle: false,
    },
    {
      label: t('customer.orders.summary.dailyInstallment'),
      value: summaryData.value.dailyInstallmentAmount,
      hasCircle: false,
    },
    {
      label: t('customer.orders.summary.receivedAmount'),
      value: summaryData.value.paidAmount,
      hasCircle: true,
      circleColor: 'bg-primary',
    },
    {
      label: t('customer.orders.summary.accumulated'),
      value: summaryData.value.overdueAmount,
      hasCircle: true,
      circleColor: 'bg-warning',
    },
    {
      label: t('customer.orders.summary.remaining'),
      value: summaryData.value.unpaidAmount,
      hasCircle: true,
      circleColor: 'bg-gray-300',
    },
  ]
})

const chartData = computed(() => {
  if (!summaryData.value) return null

  return {
    labels: [
      t('customer.orders.summary.remaining'),
      t('customer.orders.summary.accumulated'),
      t('customer.orders.summary.receivedAmount'),
    ],
    datasets: [
      {
        data: [
          summaryData.value.unpaidAmount,
          summaryData.value.overdueAmount,
          summaryData.value.paidAmount,
        ],
        backgroundColor: ['#CDCDCD', '#ffcc00', '#006a64'],
        borderWidth: 0,
        borderRadius: [8, 8, 8],
      },
    ],
  }
})

const chartOptions = computed(() => {
  return {
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        callbacks: {
          label: (context: { label?: string; parsed?: number }) => {
            const label = context.label || ''
            const value = formattedPrice(context.parsed || 0)
            return `${label}: ${value} ${t('words.dinar')}`
          },
        },
      },
    },
    cutout: '60%',
    maintainAspectRatio: false,
  }
})

function handleShowDetails(orderId: number) {
  selectedOrderId.value = orderId
}

function handleBack() {
  selectedOrderId.value = null
}
</script>
