<template>
  <div>
    <TheHeader>
      <template #create>
        <RouterLink
          v-if="canReadExpense"
          :to="{ name: 'ExpensesListRoute' }"
          class="bg-background border border-line px-4 py-2 rounded-2xl"
        >
          {{ $t('purchases.viewExpenses') }}
        </RouterLink>

        <Button
          v-if="canCreatePurchase"
          class="bg-primary text-white"
          @click="handleCreate()"
        >
          {{ $t('words.add') }}
          {{ $t('purchases.purchases') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>

      <template #export>
        <ExcelReport
          endpoint="admin/Purchases/excel-report"
          :filename="`purchases-report-${new Date().toISOString().split('T')[0]}.xlsx`"
          :params="excelParams"
        />
      </template>

      <template #calendar>
        <HeaderDatePicker />
      </template>
    </TheHeader>

    <PurchasesList v-if="canReadPurchase" ref="customerListRef" :isLoading />

    <CustomDialog
      v-model="isOpen"
      title="purchases.createPurchases"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <PurchasesForm
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
  PurchasesForm as PurchasesFormType,
  SinglePurchase,
} from '../types/model'
import PurchasesForm from '../components/PurchasesForm/PurchasesForm.vue'
import { useCreatePurchasesMutation } from '../requests/mutations'
import { usePurchasesByIdDataQuery } from '../requests/queries'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import ExcelReport from '@/modules/Core/components/base/ExcelReport/ExcelReport.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import PurchasesList from '@/modules/Purchases/components/PurchasesList/PurchasesList.vue'
import useAuthStore from '@/modules/Auth/store'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import { FormsState } from '@/modules/Core/types/model/forms'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { usePermission } from '@/modules/Core/composable/usePermission'

const isOpen = ref(false)

const formState = ref<FormsState>(FormsState.Create)

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const { can } = usePermission()
const canReadPurchase = can('Purchase', 'Read')
const canReadExpense = can('Expense', 'Read')
const canCreatePurchase = can('Purchase', 'Create')

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

const { isPending: isCreating, mutateAsync: createPurchases } =
  useCreatePurchasesMutation()

const selectedItemId = ref<number>(0)

const { data, isLoading } = usePurchasesByIdDataQuery(selectedItemId, {
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

async function onSubmit(values: PurchasesFormType) {
  if (formState.value === FormsState.Create) {
    await createPurchases(values)
  }

  toggleIsOpen()
}
const selectedItem = ref<SinglePurchase>()

function handleCreate() {
  formState.value = FormsState.Create
  selectedItem.value = undefined
  toggleIsOpen()
}
</script>
