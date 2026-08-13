<template>
  <div>
    <TheHeader>
      <template #create>
        <Button
          v-if="canCreateInstallmentPayment"
          class="bg-primary text-white"
          @click="handleCreate()"
        >
          {{ $t('payment.actions.newInstallment') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>
      <template #calendar> <HeaderDatePicker /> </template
    ></TheHeader>

    <InstallmentPaymentList v-if="canReadInstallmentPayment" />

    <CustomDialog
      v-model="isOpen"
      title="payment.actions.newInstallment"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <InstallmentPaymentForm
          :loading="isCreating"
          @submit="onSubmit"
          @cancel="toggleIsOpen"
        />
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Button } from 'primevue'
import InstallmentPaymentList from '../components/InstallmentPaymentList/InstallmentPaymentList.vue'
import InstallmentPaymentForm from '../components/InstallmentPaymentForm/InstallmentPaymentForm.vue'
import { useCreateInstallmentPaymentDirectMutation } from '../requests/mutations'
import type { PaymentCreatePayload } from '../types/api'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

const { can } = usePermission()
const canReadInstallmentPayment = can('InstallmentPayment', 'Read')
const canCreateInstallmentPayment = can('InstallmentPayment', 'Create')

const route = useRoute()
const router = useRouter()

const isOpen = ref(false)
const { isPending: isCreating, mutateAsync: createPayment } =
  useCreateInstallmentPaymentDirectMutation()

function formatDateToString(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function getYesterdayDate(): Date {
  const yesterday = new Date()
  yesterday.setDate(yesterday.getDate() - 1)
  return yesterday
}

function setDefaultDateFilter() {
  const startDate = route.query.startDate
  const endDate = route.query.endDate

  if (!startDate && !endDate) {
    const yesterday = getYesterdayDate()
    const yesterdayString = formatDateToString(yesterday)

    router.push({
      query: {
        ...route.query,
        startDate: yesterdayString,
        endDate: yesterdayString,
      },
    })
  }
}

onMounted(() => {
  setDefaultDateFilter()
})

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

function handleCreate() {
  toggleIsOpen()
}

async function onSubmit(values: PaymentCreatePayload) {
  await createPayment(values)
  toggleIsOpen()
}
</script>
