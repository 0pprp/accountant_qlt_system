<template>
  <div class="flex gap-2">
    <SvgIcon
      name="checkSquare"
      class="text-success"
      :class="iconClasses"
      @click="openModal(OrderApprovalStatus.Approved)"
    />

    <SvgIcon
      name="closeSquare"
      class="!text-error"
      :class="iconClasses"
      @click="openModal(OrderApprovalStatus.Rejected)"
    />

    <CustomDialog
      v-model="isOpen"
      class="!w-[420px]"
      title="sale.approvalActions.title"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <div class="pt-4 flex flex-col gap-4">
          <SelectField
            v-model="selectedStatus"
            :options="statusOptions"
            placeholder="sale.approvalActions.statusLabel"
            readonly
            disabled
            label="sale.approvalActions.statusLabel"
          />
          <Button class="btn-primary py-4" @click="submitApproval">
            {{ $t('sale.approvalActions.confirm') }}
            <i v-if="isUpdating" class="pi pi-spin pi-spinner"></i>
          </Button>
        </div>
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { Button } from 'primevue'
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import type { Props } from './OrderApprovalActions.types'
import { useUpdateOrderApprovalStatusMutation } from '../../requests/mutations'
import { OrderApprovalStatus } from '@/modules/Order/types/model'
import { canChangeOrderApproval } from '@/modules/Order/utils/orderStatus'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'

const props = withDefaults(defineProps<Props>(), {
  alwaysEnabled: false,
})

const { t } = useI18n()

const isOpen = ref(false)
const selectedStatus = ref<OrderApprovalStatus>()

const canApprove = computed(
  () => props.alwaysEnabled || canChangeOrderApproval(props.item)
)

const iconClasses = computed(() =>
  canApprove.value
    ? 'text-gray-700 cursor-pointer'
    : '!text-gray-200 cursor-not-allowed'
)

const statusOptions = computed(() => [
  {
    value: OrderApprovalStatus.Approved,
    label: t('sale.approvalActions.approve'),
  },
  {
    value: OrderApprovalStatus.Rejected,
    label: t('sale.approvalActions.reject'),
  },
])

const { isPending: isUpdating, mutateAsync: updateApprovalStatus } =
  useUpdateOrderApprovalStatusMutation()

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

function openModal(status: OrderApprovalStatus) {
  if (!canApprove.value) {
    return
  }

  selectedStatus.value = status
  toggleIsOpen()
}

async function submitApproval() {
  if (selectedStatus.value === undefined) {
    return
  }

  await updateApprovalStatus({
    id: props.id,
    payload: { approvalStatus: selectedStatus.value },
  })

  toggleIsOpen()
}
</script>
