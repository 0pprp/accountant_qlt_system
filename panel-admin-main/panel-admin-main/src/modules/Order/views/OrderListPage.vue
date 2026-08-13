<template>
  <div>
    <TheHeader>
      <template #create>
        <Button
          v-if="canCreateOrderList"
          class="bg-primary text-white"
          @click="handleCreate()"
        >
          {{ $t('order.createOrder') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>
    </TheHeader>

    <OrderList v-if="canReadOrderList" @edit-item="handleEdit" />

    <CustomDialog
      v-model="isOpen"
      title="order.createOrder"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <OrderForm
          :loading="isCreating || isUpdating"
          :selectedItem
          :formState
          @submit="onSubmit"
        />
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Button } from 'primevue'
import OrderForm from '../components/OrderForm/OrderForm.vue'
import OrderList from '../components/OrderList/OrderList.vue'
import {
  useCreateOrderMutation,
  useUpdateOrderMutation,
} from '../requests/mutations'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import type {
  Order,
  OrderForm as OrderFormType,
} from '@/modules/Order/types/model'
import { FormsState } from '@/modules/Core/types/model/forms'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { usePermission } from '@/modules/Core/composable/usePermission'

const isOpen = ref(false)

const formState = ref<FormsState>(FormsState.Create)
const { can } = usePermission()
const canReadOrderList = can('OrderList', 'Read')
const canCreateOrderList = can('OrderList', 'Create')

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const { isPending: isCreating, mutateAsync: createOrder } =
  useCreateOrderMutation()

const { isPending: isUpdating, mutateAsync: updateOrder } =
  useUpdateOrderMutation()

const toastStore = useToastStore()

watch([isCreating, isUpdating], () => {
  if (isCreating.value) {
    toastStore.setMassage({
      title: 'order.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  } else if (isUpdating.value) {
    toastStore.setMassage({
      title: 'order.updatePending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})
async function onSubmit(values: OrderFormType) {
  if (formState.value === FormsState.Create) {
    await createOrder(values)
  } else {
    await updateOrder({ id: selectedItem.value!.id, payload: values })
  }

  toggleIsOpen()
}
const selectedItem = ref<Order>()

function handleCreate() {
  formState.value = FormsState.Create
  selectedItem.value = undefined
  toggleIsOpen()
}

function handleEdit(item: Order) {
  formState.value = FormsState.Update
  selectedItem.value = item
  toggleIsOpen()
}
</script>
