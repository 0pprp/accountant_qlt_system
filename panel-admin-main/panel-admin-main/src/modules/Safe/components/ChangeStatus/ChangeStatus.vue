<template>
  <div class="flex gap-2">
    <SvgIcon
      name="checkSquare"
      :class="getIconClasses(props.status)"
      @click="openModal(TransactionStatus.Approved)"
    />

    <SvgIcon
      name="closeSquare"
      :class="getIconClasses(props.status)"
      @click="openModal(TransactionStatus.Rejected)"
    />

    <CustomDialog
      v-model="isOpen"
      class="!w-[420px]"
      title="safe.changeStatusTitle"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <div class="pt-4 flex flex-col gap-4">
          <SelectField
            v-model="status"
            :options="statusOptions"
            placeholder="safe.changeStatusForm.status.placeholder"
            readonly
            disabled
            label="safe.changeStatusForm.status.label"
          />
          <TextareaField
            v-model="statusDescription"
            :rows="3"
            placeholder="safe.changeStatusForm.statusDescription.placeholder"
            label="safe.changeStatusForm.statusDescription.label"
          />
          <Button class="btn-primary py-4" @click="changeStatus">
            {{ $t('words.save') }}
            <i v-if="isChanging" class="pi pi-spin pi-spinner"></i>
          </Button>
        </div>
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { Button } from 'primevue'
import { useI18n } from 'vue-i18n'
import { computed, ref } from 'vue'
import { TransactionStatus } from '../../types/model'
import type { Props } from './ChangeStatus.types'
import { useChangeStatusTransactionMutation } from '../../requests/mutations'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import TextareaField from '@/modules/Core/components/base/Fields/TextareaField/TextareaField.vue'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'

const props = defineProps<Props>()

const { t } = useI18n()
const status = ref()
const statusDescription = ref()

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const statusOptions = computed(() => {
  return [
    {
      value: TransactionStatus.Approved,
      label: t('safe.status.approved'),
    },
    {
      value: TransactionStatus.Rejected,
      label: t('safe.status.rejected'),
    },
  ]
})

const isOpen = ref(false)

function openModal(transactionStatus: TransactionStatus) {
  if (props.status !== TransactionStatus.Pending) {
    return
  } else {
    status.value = transactionStatus
    toggleIsOpen()
  }
}
const { isPending: isChanging, mutateAsync: changeStatusMutation } =
  useChangeStatusTransactionMutation()

async function changeStatus() {
  const values = {
    status: status.value,
    statusDescription: statusDescription.value,
  }

  await changeStatusMutation({ id: props.id, payload: values })
  toggleIsOpen()
}

function getIconClasses(status: TransactionStatus) {
  return status === TransactionStatus.Pending
    ? 'text-gray-700 cursor-pointer'
    : 'text-gray-300 cursor-not-allowed '
}
</script>
