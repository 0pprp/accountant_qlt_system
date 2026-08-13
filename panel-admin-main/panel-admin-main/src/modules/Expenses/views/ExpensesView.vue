<template>
  <div>
    <TheHeader>
      <template #create>
        <RouterLink
          v-if="!canReadPurchase"
          :to="{ name: 'PurchasesListRoute' }"
          class="bg-background border border-line px-4 py-2 rounded-2xl"
        >
          {{ $t('expenses.viewExpenses') }}
        </RouterLink>

        <Button
          v-if="canCreateExpense"
          class="bg-primary text-white"
          @click="handleCreate()"
        >
          {{ $t('words.add') }}
          {{ $t('expenses.expenses') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>

      <template #export>
        <ExcelReport
          v-if="canReadExpense"
          endpoint="admin/Expenses/excel-report"
          :filename="`expenses-report-${new Date().toISOString().split('T')[0]}.xlsx`"
          :params="excelParams"
        />
      </template>

      <template #calendar>
        <HeaderDatePicker v-if="canReadExpense" />
      </template>
    </TheHeader>

    <ExpensesList ref="customerListRef" :isLoading />

    <CustomDialog
      v-model="isOpen"
      title="expenses.createExpenses"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <ExpensesForm
          :loading="isCreating"
          :initialValues="selectedItem"
          :formState
          @submit="onSubmit"
          @cancel="toggleIsOpen"
        />
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { Button } from 'primevue'
import type {
  ExpensesForm as ExpensesFormType,
  SingleExpense,
} from '../types/model'
import ExpensesForm from '../components/ExpensesForm/ExpensesForm.vue'
import { useCreateExpensesMutation } from '../requests/mutations'
import { useExpensesByIdDataQuery } from '../requests/queries'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import ExcelReport from '@/modules/Core/components/base/ExcelReport/ExcelReport.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import ExpensesList from '@/modules/Expenses/components/ExpensesList/ExpensesList.vue'
import useAuthStore from '@/modules/Auth/store'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import { FormsState } from '@/modules/Core/types/model/forms'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { usePermission } from '@/modules/Core/composable/usePermission'

const isOpen = ref(false)

const formState = ref<FormsState>(FormsState.Create)

const { can } = usePermission()
const canReadExpense = can('Expense', 'Read')
const canReadPurchase = can('Purchase', 'Read')
const canCreateExpense = can('Expense', 'Create')

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const authStore = useAuthStore()
const route = useRoute()

const excelParams = computed(() => ({
  branchId: authStore.selectedBranch?.id,
  searchTerm: Array.isArray(route.query.s)
    ? (route.query.s[0] ?? '')
    : (route.query.s ?? ''),
  startDate:
    typeof route.query.startDate === 'string' ? route.query.startDate : '',
  endDate: typeof route.query.endDate === 'string' ? route.query.endDate : '',
}))

const { isPending: isCreating, mutateAsync: createExpenses } =
  useCreateExpensesMutation()

const selectedItemId = ref<number>(0)

const { data, isLoading } = useExpensesByIdDataQuery(selectedItemId, {
  enabled: computed(() => selectedItemId.value > 0),
})

watch(data, (newData) => {
  if (newData) {
    selectedItem.value = newData
    toggleIsOpen()
  }
})

const toastStore = useToastStore()

watch([isCreating], () => {
  if (isCreating.value) {
    toastStore.setMassage({
      title: 'branch.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

async function onSubmit(values: ExpensesFormType) {
  if (formState.value === FormsState.Create) {
    await createExpenses(values)
  }

  toggleIsOpen()
}
const selectedItem = ref<SingleExpense>()

function handleCreate() {
  formState.value = FormsState.Create
  selectedItem.value = undefined
  toggleIsOpen()
}
</script>
