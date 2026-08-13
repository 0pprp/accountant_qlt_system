<template>
  <div>
    <CustomDataTable
      :data="data?.items || []"
      v-model:selection="selectedPayments"
      :columns="columns"
      :rows="pageSize"
      selectable
      :pageSize="pageSize"
      paginatorEnabled
      :totalRecords="data?.totalCount"
      :paginatorLabel="$t('payment.payments')"
      tableStyle="min-width: 100%"
      class="custom-table custom-border mt-4"
      :loading="isLoading"
      @page-change="onPageChange"
      @sort-change="handleSortChange"
    >
      <template #body-date="{ data }">
        {{ formattedDate(data.date) }}
      </template>

      <template #body-amount="{ data }">
        <span>
          {{ data.amount }}
        </span>
        <span class="text-gray-700 px-1">
          {{ $t('words.dinar') }}
        </span>
      </template>

      <template #body-edit="{ data }" v-if="canUpdateInstallmentPayment">
        <SvgIcon
          name="boldPencil"
          class="w-7.5 h-7.5 text-gray-300 cursor-pointer hover:text-secondary transition-colors"
          @click="handleUpdate(data.id)"
        />
      </template>

      <template #body-delete="{ data }" v-if="canDeleteInstallmentPayment">
        <SvgIcon
          name="boldTrash"
          class="w-7.5 h-7.5 text-gray-300 cursor-pointer hover:text-error transition-colors"
          @click="handleDelete(data.id)"
        />
      </template>
    </CustomDataTable>

    <BaseConfirmationDialog
      v-model="isOpen"
      icon="boldPencil"
      :title="getDialogTitle()"
      color="warning"
      :disabled="!isAmountValid && currentAction === 'UPDATE'"
      :submitText="getSubmitText()"
      @update:is-open="handleModalUpdate"
      @on-submit="handleSubmit()"
    >
      <template #content>
        <div v-if="currentAction === 'UPDATE'">
          <NumberField
            v-model="amount"
            :suffix="$t('words.dinar')"
            required
            :error="errors['amount']"
            :placeholder="amountPlaceholder"
            label="payment.form.amount.label"
          />

          <TextField
            v-model="description"
            :error="errors['description']"
            placeholder="payment.form.description.placeholder"
            label="payment.form.description.label"
          />
        </div>
      </template>
    </BaseConfirmationDialog>

    <BaseConfirmationDialog
      v-model="isDeleteDialogOpen"
      icon="delete"
      title="words.delete"
      color="error"
      submitText="words.delete"
      @update:is-open="isDeleteDialogOpen = !isDeleteDialogOpen"
      @on-submit="confirmDelete()"
    >
      <template #content>
        <div class="text-gray-700 text-center">
          {{ $t('payment.deleteConfirmation') }}
        </div>
      </template>
    </BaseConfirmationDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import type { Payment } from '@/modules/Payment/types/model'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { formattedDate } from '@/modules/Core/utils/time'
import BaseConfirmationDialog from '@/modules/Core/components/base/BaseConfirmationDialog/BaseConfirmationDialog.vue'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import {
  useUpdatePaymentMutation,
  useDeletePaymentMutation,
} from '@/modules/Payment/requests/mutations'
import { usePaymentListDataQuery } from '@/modules/Payment/requests/queries'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import useAuthStore from '@/modules/Auth/store'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { t } = useI18n()
const route = useRoute()

const selectedPayments = ref<Payment>()
const isOpen = ref(false)
const isDeleteDialogOpen = ref(false)
const currentAction = ref<'UPDATE' | null>(null)
const currentPaymentId = ref<number | null>(null)
const amount = ref<number | null>(null)
const description = ref<string>()
const errors = ref<Record<string, string>>({})

const amountPlaceholder = computed(() => {
  if (!currentPaymentId.value || !data.value?.items) {
    return 'payment.form.amount.placeholder'
  }

  const paymentItem = data.value.items.find(
    (item) => item.id === currentPaymentId.value
  )
  if (paymentItem && paymentItem.dailyInstallmentAmount) {
    return String(paymentItem.dailyInstallmentAmount)
  }

  return 'payment.form.amount.placeholder'
})

const orderListId = computed(() => {
  const id = route.params.orderListId
  return typeof id === 'string' ? parseInt(id, 10) : Number(id)
})

function toggleIsOpen() {
  isOpen.value = !isOpen.value

  if (!isOpen.value) {
    resetDialogState()
  }
}

function handleModalUpdate(value: boolean) {
  isOpen.value = value
  if (!value) {
    resetDialogState()
  }
}

function resetDialogState() {
  currentAction.value = null
  currentPaymentId.value = null
  amount.value = null
  errors.value = {}
  description.value = undefined
}

const { can } = usePermission()
const canUpdateInstallmentPayment = can('InstallmentPayment', 'Update')
const canDeleteInstallmentPayment = can('InstallmentPayment', 'Delete')

const columns = computed<Column<AllowedTypes>[]>(() => {
  const baseColumns: Column<AllowedTypes>[] = [
    {
      field: 'customerFullName',
      header: t('payment.table.customerFullName'),
      sortable: true,
    },
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
  ]

  if (canUpdateInstallmentPayment) {
    baseColumns.push({
      field: 'edit',
      header: t('words.edit'),
      sortable: false,
    })
  }

  if (canDeleteInstallmentPayment) {
    baseColumns.push({
      field: 'delete',
      header: t('words.delete'),
      sortable: false,
    })
  }

  return baseColumns
})

const pageSize = ref(10)
const pageIndex = ref(1)

const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const searchTerm = computed(() => {
  const s = route.query.s
  if (!s) return null
  return Array.isArray(s) ? s.map(String) : [String(s)]
})

const startDate = computed(() => {
  return typeof route.query.startDate === 'string'
    ? route.query.startDate
    : null
})

const endDate = computed(() => {
  return typeof route.query.endDate === 'string' ? route.query.endDate : null
})

const sortCriteria = ref<Array<{
  property: string
  direction: number
}> | null>(null)

watch(
  searchTerm,
  () => {
    pageIndex.value = 1
  },
  { deep: true }
)

watch([startDate, endDate], () => {
  pageIndex.value = 1
})

const { data, isLoading, refetch } = usePaymentListDataQuery(
  orderListId,
  pageIndex,
  pageSize,
  branchId,
  searchTerm,
  startDate,
  endDate,
  sortCriteria
)

function onPageChange(event: { first: number }) {
  pageIndex.value = Math.floor(event.first / pageSize.value) + 1
}

function handleSortChange(event: {
  sortField: string | null
  sortOrder: 1 | -1 | 0
}) {
  if (event.sortField && event.sortOrder !== 0) {
    const direction = event.sortOrder === 1 ? 0 : 1
    sortCriteria.value = [
      {
        property: event.sortField,
        direction: direction,
      },
    ]
  } else {
    sortCriteria.value = null
  }
}

function handleUpdate(id: number) {
  currentAction.value = 'UPDATE'
  currentPaymentId.value = id

  const paymentItem = data.value?.items?.find((item) => item.id === id)
  if (paymentItem) {
    description.value = paymentItem.description || undefined
    amount.value = Number(paymentItem.amount)
  }
  toggleIsOpen()
}

function handleDelete(id: number) {
  currentPaymentId.value = id
  isDeleteDialogOpen.value = true
}

function getDialogTitle(): string {
  switch (currentAction.value) {
    case 'UPDATE':
      return 'words.edit'
    default:
      return ''
  }
}

function getSubmitText(): string {
  switch (currentAction.value) {
    case 'UPDATE':
      return 'words.edit'
    default:
      return 'words.save'
  }
}

const { isPending: isUpdating, mutateAsync: updatePayment } =
  useUpdatePaymentMutation()
const { isPending: isDeleting, mutateAsync: deletePayment } =
  useDeletePaymentMutation()

const toastStore = useToastStore()

const isAmountValid = computed(() => {
  return amount.value !== null && amount.value > 0
})

watch([isUpdating, isDeleting], () => {
  if (isUpdating.value) {
    toastStore.setMassage({
      title: 'branch.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
  if (isDeleting.value) {
    toastStore.setMassage({
      title: 'payment.deletePending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

async function handleSubmit() {
  if (!currentPaymentId.value || !currentAction.value) return

  if (!isAmountValid.value && currentAction.value === 'UPDATE') {
    errors.value.amount = t('payment.form.amount.validation.required')
    return
  } else {
    errors.value.amount = ''
  }

  if (currentAction.value === 'UPDATE') {
    const payload = {
      amount: amount.value!,
      description: description.value || null,
    }

    await updatePayment({
      id: currentPaymentId.value,
      payload,
      orderId: orderListId.value,
      branchId: branchId.value,
    })
  }

  await refetch()
  resetDialogState()
  isOpen.value = false
}

async function confirmDelete() {
  if (!currentPaymentId.value) return

  await deletePayment({
    id: currentPaymentId.value,
    orderId: orderListId.value,
  })

  await refetch()
  isDeleteDialogOpen.value = false
  currentPaymentId.value = null
}
</script>
