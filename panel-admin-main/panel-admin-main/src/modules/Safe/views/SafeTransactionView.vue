<template>
  <div>
    <TheHeader>
      <template #create>
        <RouterLink
          v-if="canReadSafe"
          :to="{
            name:
              route.query.main === 'true'
                ? 'SafeMainMainRoute'
                : 'SafeMainRoute',
          }"
          class="bg-background rounded-2xl text-gray-900 inline-flex items-center gap-2 px-4 py-2"
        >
          {{ $t('safe.sellers') }}
        </RouterLink>

        <Button
          v-if="canCreateTransaction"
          class="bg-primary rounded-2xl text-white items-center gap-2 px-4 py-2"
          @click="toggleIsOpen"
        >
          {{ $t('safe.createSafe') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>

      <template #export>
        <ExcelReport
          :endpoint="`admin/Safes/${safeStore.safeId}/transactions/excel-report`"
          :filename="`safes-report-${new Date().toISOString().split('T')[0]}.xlsx`"
          :params="excelParams"
        />
      </template>
    </TheHeader>

    <SafeTransactionsList
      v-if="canReadTransaction"
      ref="safeTransactionsListRef"
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
import { Button } from 'primevue'
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import SafeTransactionsList from '../components/SafeTransactionsList/SafeTransactionsList.vue'
import TransferForm from '../components/TransferForm/TransferForm.vue'
import { useCreateTransferMutation } from '../requests/mutations'
import useSafeStore from '../store'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import ExcelReport from '@/modules/Core/components/base/ExcelReport/ExcelReport.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { type TransferForm as TransferFormType } from '@/modules/Safe/types/model'
import { usePermission } from '@/modules/Core/composable/usePermission'

const route = useRoute()

const excelParams = computed(() => ({}))

const isOpen = ref(false)

const safeStore = useSafeStore()

const { can } = usePermission()
const canReadTransaction = can('Transaction', 'Read')
const canCreateTransaction = can('Transaction', 'Create')
const canReadSafe = can('Safe', 'Read')

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
