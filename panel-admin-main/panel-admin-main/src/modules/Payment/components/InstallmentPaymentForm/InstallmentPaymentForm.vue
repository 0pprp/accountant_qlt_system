<template>
  <form @submit.prevent="submit">
    <div class="grid grid-cols-12 gap-4 pt-4">
      <div class="col-span-12">
        <CustomerSelectInput
          v-model="customerId"
          label="payment.form.customer.label"
          placeholder="payment.form.customer.placeholder"
          required
          :error="errors['customerId']"
          @update:model-value="handleCustomerChange"
        />
      </div>

      <div v-if="customerId && customerId > 0" class="col-span-12">
        <OrderSelectInput
          v-model="orderId"
          :customerId="customerId"
          inProgressOnly
          label="payment.form.order.label"
          placeholder="payment.form.order.placeholder"
          required
          :error="errors['orderId']"
        />
      </div>

      <div v-if="orderId" class="col-span-12 md:col-span-4">
        <CalendarField
          v-model="date"
          label="payment.form.date.label"
          placeholder="payment.form.date.placeholder"
          required
          :error="errors['date']"
        />
      </div>

      <div v-if="orderId" class="col-span-12 md:col-span-4">
        <NumberField
          v-model="amount"
          label="payment.form.amount.label"
          :placeholder="amountPlaceholder"
          required
          :error="errors['amount']"
        />
      </div>

      <div v-if="orderId" class="col-span-12 md:col-span-4">
        <TextField
          v-model="descriptionValue"
          label="payment.form.description.label"
          placeholder="payment.form.description.placeholder"
          :error="errors['description']"
        />
      </div>
    </div>

    <div class="flex justify-center items-center gap-4 mt-8">
      <Button
        class="w-100 !rounded-xl !bg-primary py-3 !border-none text-white"
        type="submit"
        :loading="loading"
        :unstyled="false"
      >
        {{ $t('words.save') }}
      </Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Button } from 'primevue'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { number, object, string } from 'yup'
import type { Emits, Props } from './InstallmentPaymentForm.types'
import type { PaymentCreatePayload } from '@/modules/Payment/types/api'
import CustomerSelectInput from '@/modules/Core/components/shared/CustomerSelectInput/CustomerSelectInput.vue'
import OrderSelectInput from '@/modules/Core/components/shared/OrderSelectInput/OrderSelectInput.vue'
import CalendarField from '@/modules/Core/components/base/Fields/CalendarField/CalendarField.vue'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import { useOrdersByCustomerQuery } from '@/modules/Order/requests/queries'
import type { Order } from '@/modules/Order/types/model'
import { isOrderInProgress } from '@/modules/Order/utils/orderStatus'
import useAuthStore from '@/modules/Auth/store'

defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object().shape({
  customerId: number()
    .required(t('payment.form.customer.validation.required'))
    .min(1, t('payment.form.customer.validation.required')),
  orderId: number()
    .nullable()
    .required(t('payment.form.order.validation.required'))
    .min(1, t('payment.form.order.validation.required')),
  date: string().required(t('payment.form.date.validation.required')),
  amount: number()
    .required(t('payment.form.amount.validation.required'))
    .min(1, t('payment.form.amount.validation.required')),
  description: string().nullable(),
})

interface FormValues {
  customerId: number
  orderId: number | null
  date: string
  amount: number
  description: string | null
}

const { defineField, handleSubmit, errors, setFieldValue } =
  useForm<FormValues>({
    validationSchema,
  })

const [customerId] = defineField('customerId')
const [orderId] = defineField('orderId')
const [date] = defineField('date')
const [amount] = defineField('amount')
const [description] = defineField('description')

// Computed property for description to handle null to undefined conversion
const descriptionValue = computed({
  get: () => description.value ?? undefined,
  set: (value: string | undefined) => {
    description.value = value ?? null
  },
})

// Store for orders and selected order
const authStore = useAuthStore()
const branchId = computed(() => authStore.selectedBranch?.id ?? 0)
const pageIndex = ref(1)
const pageSize = ref(100)

const { data: ordersData } = useOrdersByCustomerQuery(
  pageIndex,
  pageSize,
  branchId,
  computed(() => customerId.value ?? null)
)

const orders = computed(() => {
  const items = ordersData.value?.paginatedOrders?.items || []
  return items
    .filter((item: Record<string, unknown>) => isOrderInProgress(item))
    .map((item: Record<string, unknown>) => ({
      id: (item.id as number) || (item.Id as number) || 0,
      dailyInstallmentAmount:
        (item.dailyInstallmentAmount as number) ||
        (item.DailyInstallmentAmount as number) ||
        0,
    })) as Array<Pick<Order, 'id' | 'dailyInstallmentAmount'>>
})

const selectedOrder = ref<Pick<Order, 'id' | 'dailyInstallmentAmount'> | null>(
  null
)

const amountPlaceholder = computed(() => {
  if (selectedOrder.value && selectedOrder.value.dailyInstallmentAmount) {
    return String(selectedOrder.value.dailyInstallmentAmount)
  }
  return 'payment.form.amount.placeholder'
})

watch(orderId, (newOrderId) => {
  if (newOrderId && orders.value) {
    const foundOrder = orders.value.find((order) => order.id === newOrderId)
    selectedOrder.value = foundOrder || null

    if (foundOrder) {
      setFieldValue('amount', foundOrder.dailyInstallmentAmount)
    }
  } else {
    selectedOrder.value = null
  }
})

watch(orders, () => {
  if (orderId.value && orders.value) {
    const foundOrder = orders.value.find((order) => order.id === orderId.value)
    selectedOrder.value = foundOrder || null

    if (foundOrder) {
      setFieldValue('amount', foundOrder.dailyInstallmentAmount)
    }
  }
})

function handleCustomerChange() {
  // Reset order when customer changes
  setFieldValue('orderId', null as number | null)
  selectedOrder.value = null
}

const submit = handleSubmit((values) => {
  const payload: PaymentCreatePayload = {
    orderId: values.orderId!,
    date: values.date!,
    amount: values.amount!,
    description: values.description || null,
  }
  emit('submit', payload)
})
</script>
