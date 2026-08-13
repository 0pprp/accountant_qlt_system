<template>
  <div>
    <TheHeader>
      <template #create>
        <RouterLink
          v-if="canReadTransaction"
          :to="{
            name: 'SafeTransactionsRoute',
            query: route.name === 'SafeMainMainRoute' ? { main: 'true' } : {},
          }"
          class="bg-background rounded-2xl text-gray-900 inline-flex items-center gap-2 px-4 py-2"
        >
          {{ $t('safe.requests') }}
        </RouterLink>

        <Button
          v-if="cashDeliveriesHeader && canCollectCashTransaction"
          class="bg-primary text-white"
          @click="handleCashDeliveries()"
        >
          {{ $t('safe.someSellerCashDeliveriesBtn') }}
          <SvgIcon name="addCircle" />
        </Button>

        <Button
          v-else-if="canCreateTransaction"
          class="bg-primary rounded-2xl text-white items-center gap-2 px-4 py-2"
          @click="toggleIsOpen"
        >
          {{ $t('safe.createSafe') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>

      <template #export>
        <ExcelReport
          v-if="canReadSafe"
          :endpoint="`admin/Safes/${safeStore.safeId}/sellers/excel-report`"
          :filename="`safes-report-${new Date().toISOString().split('T')[0]}.xlsx`"
          :params="excelParams"
        />
      </template>
    </TheHeader>

    <SafeList
      v-if="canReadSafe"
      ref="safeListRef"
      @update-header="updateHeader"
    />

    <CustomDialog
      v-model="isOpen"
      title="safe.createSafe"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <TransferForm :loading="isCreating" @submit="onSubmit" />
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Button } from 'primevue'
import { useRoute } from 'vue-router'
import TransferForm from '../components/TransferForm/TransferForm.vue'
import { useCreateTransferMutation } from '../requests/mutations'
import useSafeStore from '../store'
import ExcelReport from '@/modules/Core/components/base/ExcelReport/ExcelReport.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import SafeList from '@/modules/Safe/components/SafeList/SafeList.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { useToastStore } from '@/modules/Core/store'
import { type TransferForm as TransferFormType } from '@/modules/Safe/types/model'
import { usePermission } from '@/modules/Core/composable/usePermission'

const route = useRoute()

const safeStore = useSafeStore()

const { can } = usePermission()
const canReadSafe = can('Safe', 'Read')
const canCreateTransaction = can('Transaction', 'Create')
const canReadTransaction = can('Transaction', 'Read')
const canCollectCashTransaction = can('Transaction', 'CollectCash')

const excelParams = computed(() => ({
  searchTerm: Array.isArray(route.query.s)
    ? (route.query.s[0] ?? '')
    : (route.query.s ?? ''),
}))

const cashDeliveriesHeader = ref(false)
const safeListRef = ref<InstanceType<typeof SafeList> | null>(null)

function updateHeader(value: boolean) {
  cashDeliveriesHeader.value = value
}

function handleCashDeliveries() {
  safeListRef.value?.openMultipleDialog()
}

const isOpen = ref(false)

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const { isPending: isCreating, mutateAsync: createTransfer } =
  useCreateTransferMutation()

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

async function onSubmit(values: TransferFormType) {
  await createTransfer(values)
  toggleIsOpen()
}
</script>
