<template>
  <div class="space-y-4">
    <div
      v-for="(item, index) in localItems"
      :key="index"
      class="grid grid-cols-12 gap-4 align-middle items-center"
    >
      <div class="col-span-12 md:col-span-1 text-center pt-4 text-gray-700">
        {{ index + 1 }}
      </div>

      <div class="col-span-12 md:col-span-6">
        <TextField
          v-model="item.name"
          :readonly="formState === FormsState.View"
          placeholder="expenses.form.name.placeholder"
          :label="index === 0 ? 'expenses.form.name.label' : ''"
          @update:model-value="emitChanges"
        />
      </div>

      <div class="col-span-12 md:col-span-2">
        <NumberField
          v-model="item.quantity"
          :readonly="formState === FormsState.View"
          placeholder="expenses.form.quantity.placeholder"
          :label="index === 0 ? 'expenses.form.quantity.label' : ''"
          @update:model-value="emitChanges"
        />
      </div>

      <div class="col-span-12 md:col-span-2">
        <NumberField
          v-model="item.amount"
          :readonly="formState === FormsState.View"
          placeholder="expenses.form.amount.placeholder"
          :label="index === 0 ? 'expenses.form.amount.label' : ''"
          @update:model-value="emitChanges"
        />
      </div>

      <div
        v-if="formState !== FormsState.View"
        class="col-span-12 md:col-span-1 pt-4 flex gap-2"
      >
        <SvgIcon
          name="addCircle"
          class="text-primary cursor-pointer"
          @click="addNewItem"
        />
        <SvgIcon
          v-if="localItems.length > 1"
          name="delete"
          class="text-error cursor-pointer"
          @click="deleteItem(index)"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { Props, Emits } from './ExpenseItemsForm.types'
import type { ExpenseItemModel } from '@/modules/Expenses/types/model'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
// import AllProductSelectInput from '@/modules/Core/components/shared/AllProductSelectInput/AllProductSelectInput.vue'
import { FormsState } from '@/modules/Core/types/model/forms'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const localItems = ref<Array<ExpenseItemModel>>([])

function initializeItems() {
  if (props.expenseItems && props.expenseItems.length > 0) {
    // Ensure each item has the proper product object structure
    localItems.value = props.expenseItems.map((item) => ({
      ...item,
      name: item.name,
    }))
  } else {
    localItems.value = [createEmptyItem()]
  }
}

function createEmptyItem(): ExpenseItemModel {
  return {
    // id: 0,
    quantity: null,
    amount: 0,
    name: '',
  } as ExpenseItemModel
}

function addNewItem() {
  localItems.value.push(createEmptyItem())
  emitChanges()
}

function deleteItem(index: number) {
  if (localItems.value.length > 1) {
    localItems.value.splice(index, 1)
    emitChanges()
  }
}

function emitChanges() {
  const formItems = localItems.value.map((item) => ({
    quantity: item.quantity,
    amount: item.amount,
    name: item.name,
  }))

  emit('update', formItems)
}

watch(
  () => props.expenseItems,
  (newItems) => {
    if (newItems && newItems.length > 0) {
      localItems.value = newItems.map((item) => ({
        ...item,
        name: item.name,
      }))
    }
  },
  { deep: true }
)

initializeItems()
</script>
