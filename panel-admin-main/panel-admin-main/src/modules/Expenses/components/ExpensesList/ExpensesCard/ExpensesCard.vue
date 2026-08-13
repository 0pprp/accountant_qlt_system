<template>
  <Card class="rounded-lg bg-white p-4">
    <template #content>
      <div class="">
        <div class="flex justify-between align-middle">
          <div class="flex flex-col gap-2">
            <strong>
              {{ $t('expenses.form.factorNumber.label') }}
              {{ expense.factorNumber }}
            </strong>
            <span>
              {{
                expense.safeType === SafeType.Branch
                  ? $t('expenses.safeType.branch')
                  : $t('expenses.safeType.main')
              }}
            </span>
          </div>

          <div class="flex flex-col gap-2">
            <Chip
              :label="`${expense.expenseItemsCount}  ${$t('warehouse.warehouse')}`"
              variant="tonal"
              severity="secondary"
              :unstyled="false"
              class="text-center mx-auto"
            />
            <span>
              {{ formattedDate(expense.createdAt) }}
            </span>
          </div>
        </div>

        <hr class="my-4 border-line border" />

        <div class="!h-[152px] overflow-auto">
          <div class="flex justify-between align-middle text-gray-300">
            <span> {{ $t('warehouse.warehouse') }} </span>
            <span> {{ $t('expenses.form.quantity.label') }} </span>
            <span> {{ $t('expenses.form.amount.label') }} </span>
          </div>

          <div
            v-for="item in expense.expenseItems"
            :key="item.id"
            class="flex py-2 justify-between align-middle"
          >
            <span class="text-gray-700"> {{ item.name }} </span>
            <span class="text-gray-900"> {{ item.quantity }} </span>
            <span class="text-gray-900"
              >{{ formattedPrice(item.amount) }}
              <small class="text-gray-700">{{ $t('words.dinar') }}</small></span
            >
          </div>
        </div>

        <hr class="my-4 border-line border" />

        <div class="flex justify-between align-middle">
          <strong>
            {{ $t('expenses.allAmount') }}
          </strong>
          <span class="font-stretch-50%">
            {{ formattedPrice(expense.totalAmount) }}
            <small class="text-gray-700">{{ $t('words.dinar') }}</small>
          </span>
        </div>

        <div class="flex justify-between align-middle pt-5 gap-3">
          <Button
            v-if="canUpdateExpense"
            class="bg-transparent shadow"
            severity="warning"
            :loading="isLoading"
            :disabled="isLoading"
            raised
            @click="handleEdit(expense.id)"
          >
            <i v-if="isLoading" class="pi pi-spin pi-spinner"></i>
            <SvgIcon name="pen" class="mt-2 text-warning cursor-pointer" />
          </Button>

          <Button
            v-if="canReadExpense"
            class="text-primary shadow bg-surface-3 w-100"
            :loading="isLoading"
            :disabled="isLoading"
            @click="handleView(expense.id)"
          >
            <i v-if="isLoading" class="pi pi-spin pi-spinner"></i>
            {{ $t('expenses.viewDetails') }}
          </Button>
        </div>
      </div>

      <CustomDialog
        v-model="isOpen"
        title="expenses.updateExpenses"
        @update:is-open="toggleIsOpen"
      >
        <template #content>
          <ExpensesForm
            :loading="isUpdating"
            :initialValues="selectedItem"
            :formState
            @submit="onSubmit"
            @cancel="toggleIsOpen"
          />
        </template>
      </CustomDialog>
    </template>
  </Card>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Button, Card, Chip } from 'primevue'
import type { Props } from './ExpensesCard.types'
import ExpensesForm from '../../ExpensesForm/ExpensesForm.vue'
import { formattedDate } from '@/modules/Core/utils/time'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import { FormsState } from '@/modules/Core/types/model/forms'
import { useUpdateExpensesMutation } from '@/modules/Expenses/requests/mutations'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { useToastStore } from '@/modules/Core/store'
import { useExpensesByIdDataQuery } from '@/modules/Expenses/requests/queries'
import {
  SafeType,
  type ExpensesForm as ExpensesFormType,
  type SingleExpense,
} from '@/modules/Expenses/types/model'
import { formattedPrice } from '@/modules/Core/utils'
import { usePermission } from '@/modules/Core/composable/usePermission'

defineProps<Props>()

function toggleIsOpen() {
  isOpen.value = !isOpen.value

  if (!isOpen.value) {
    selectedItemId.value = 0
    selectedItem.value = undefined
  }
}
const { can } = usePermission()
const canReadExpense = can('Expense', 'Read')
const canUpdateExpense = can('Expense', 'Update')

const isOpen = ref(false)

const formState = ref<FormsState>(FormsState.Update)
const selectedItem = ref<SingleExpense>()
const selectedItemId = ref<number>(0)

const { data, isLoading } = useExpensesByIdDataQuery(selectedItemId, {
  enabled: computed(() => selectedItemId.value > 0),
})

const { isPending: isUpdating, mutateAsync: updateExpenses } =
  useUpdateExpensesMutation()

const toastStore = useToastStore()
watch([isUpdating], () => {
  if (isUpdating.value) {
    toastStore.setMassage({
      title: 'branch.updatePending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

watch(data, (newData) => {
  if (newData) {
    selectedItem.value = newData
    toggleIsOpen()
  }
})

async function onSubmit(values: ExpensesFormType) {
  await updateExpenses({ id: selectedItem.value!.id, payload: values })
  toggleIsOpen()
}

function handleEdit(id: number) {
  formState.value = FormsState.Update
  selectedItemId.value = id
}

function handleView(id: number) {
  formState.value = FormsState.View
  selectedItemId.value = id
}
</script>
