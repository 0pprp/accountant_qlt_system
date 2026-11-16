<template>
  <div>
    <TheHeader search-placeholder="header.input.placeholder">
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
    </TheHeader>

    <PaymentRowsTable v-if="canReadOrderList" />

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
import { ref } from 'vue'
import { Button } from 'primevue'
import PaymentRowsTable from '../components/PaymentRowsTable/PaymentRowsTable.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import InstallmentPaymentForm from '../components/InstallmentPaymentForm/InstallmentPaymentForm.vue'
import { useCreateInstallmentPaymentDirectMutation } from '../requests/mutations'
import type { PaymentCreatePayload } from '../types/api'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

const { can } = usePermission()
const canReadOrderList = can('OrderList', 'Read')
const canCreateInstallmentPayment = can('InstallmentPayment', 'Create')

const isOpen = ref(false)
const { isPending: isCreating, mutateAsync: createPayment } =
  useCreateInstallmentPaymentDirectMutation()

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
