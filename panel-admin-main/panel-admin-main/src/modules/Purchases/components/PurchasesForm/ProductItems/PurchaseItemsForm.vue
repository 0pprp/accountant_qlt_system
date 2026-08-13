<template>
  <div class="space-y-4">
    <div
      v-for="(item, index) in localItems"
      :key="item.key"
      class="grid grid-cols-12 gap-4 align-middle items-center"
    >
      <div class="col-span-12 md:col-span-1 text-center pt-4 text-gray-700">
        {{ index + 1 }}
      </div>

      <div class="col-span-12 md:col-span-6">
        <TextField
          v-if="item.isForeignLine"
          v-model="item.productName"
          :readonly="formState === FormsState.View"
          :label="index === 0 ? 'purchases.form.productName.label' : ''"
          placeholder="purchases.form.productName.placeholder"
          @update:model-value="() => handleForeignNameChange(index)"
        />
        <AllProductSelectInput
          v-else
          v-model="item.productId"
          :canCreate="true"
          :readonly="formState === FormsState.View"
          :label="index === 0 ? 'purchases.form.productName.label' : ''"
          placeholder="purchases.form.productName.placeholder"
          @update:product-name="(name) => handleProductNameUpdate(index, name)"
          @update:amount="(amount) => handleAmountUpdate(index, amount)"
        />
      </div>

      <div class="col-span-12 md:col-span-2">
        <NumberField
          v-model="item.quantity"
          :readonly="formState === FormsState.View"
          placeholder="purchases.form.quantity.placeholder"
          :label="index === 0 ? 'purchases.form.quantity.label' : ''"
          @update:model-value="handleQuantityChange(index)"
        />
      </div>

      <div class="col-span-12 md:col-span-2">
        <NumberField
          v-model="item.totalAmount"
          :readonly="formState === FormsState.View"
          placeholder="purchases.form.amount.placeholder"
          :label="index === 0 ? 'purchases.form.amount.label' : ''"
          @update:model-value="(value) => handleTotalAmountChange(index, value)"
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
import { ref, watch, nextTick } from 'vue'
import type { Props, Emits } from './PurchaseItemsForm.types'
import type { PurchaseItemModel } from '@/modules/Purchases/types/model'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import AllProductSelectInput from '@/modules/Core/components/shared/AllProductSelectInput/AllProductSelectInput.vue'
import { FormsState } from '@/modules/Core/types/model/forms'

interface LocalPurchaseItem extends Omit<PurchaseItemModel, 'product'> {
  key: number
  productId: number | undefined
  productName: string
  totalAmount: number
  unitPrice: number
  hasInitialAmount: boolean
  isProductChanged: boolean
  isForeignLine: boolean
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const localItems = ref<Array<LocalPurchaseItem>>([])
const itemKeyCounter = ref(0)
const isInitialLoad = ref(true)

function mapToLocalItem(item: PurchaseItemModel): LocalPurchaseItem {
  const unitPrice =
    item.quantity > 0 ? item.amount / item.quantity : item.amount
  const productId = item.product?.id ?? item.productId ?? undefined
  const productName = item.product?.name ?? item.foreignProductName ?? ''

  return {
    id: item.id,
    quantity: item.quantity,
    amount: item.amount,
    unitPrice,
    productId: productId || undefined,
    totalAmount: item.amount,
    productName,
    hasInitialAmount: item.amount > 0,
    isProductChanged: false,
    isForeignLine: !productId && !!item.foreignProductName,
    key: itemKeyCounter.value++,
  }
}

function initializeItems() {
  if (props.purchaseItems && props.purchaseItems.length > 0) {
    localItems.value = props.purchaseItems.map(mapToLocalItem)
  } else {
    localItems.value = [createEmptyItem()]
  }

  nextTick(() => {
    isInitialLoad.value = false
  })
}

function createEmptyItem(): LocalPurchaseItem {
  return {
    id: 0,
    quantity: 1,
    amount: 0,
    unitPrice: 0,
    productId: undefined,
    totalAmount: 0,
    productName: '',
    hasInitialAmount: false,
    isProductChanged: false,
    isForeignLine: false,
    key: itemKeyCounter.value++,
  }
}

function addNewItem() {
  localItems.value.push(createEmptyItem())
  emitChanges()
}

function deleteItem(index: number) {
  if (localItems.value.length > 1) {
    localItems.value.splice(index, 1)
  }
  emitChanges()
}

function handleAmountUpdate(index: number, amount: number) {
  const item = localItems.value[index]

  if (isInitialLoad.value && item.hasInitialAmount) {
    return
  }

  if (item.productId && !item.isProductChanged && item.hasInitialAmount) {
    return
  }

  item.unitPrice = amount
  item.amount = amount
  item.totalAmount = amount * item.quantity

  if (item.isProductChanged) {
    item.isProductChanged = false
    item.hasInitialAmount = false
  }

  emitChanges()
}

function handleQuantityChange(index: number) {
  const item = localItems.value[index]
  item.totalAmount = item.unitPrice * item.quantity
  emitChanges()
}

function handleProductNameUpdate(index: number, name: string) {
  const item = localItems.value[index]

  if (item.productName !== name) {
    item.isProductChanged = true
    item.isForeignLine = false
  }

  item.productName = name
  emitChanges()
}

function handleForeignNameChange(index: number) {
  const item = localItems.value[index]
  item.productName = item.productName.slice(0, 200)
  emitChanges()
}

function handleTotalAmountChange(index: number, totalAmount: number | null) {
  if (totalAmount === null) return

  const item = localItems.value[index]
  item.totalAmount = totalAmount

  if (item.quantity > 0) {
    item.unitPrice = totalAmount / item.quantity
    item.amount = item.unitPrice
  } else {
    item.unitPrice = totalAmount
    item.amount = totalAmount
  }

  item.hasInitialAmount = false

  emitChanges()
}

function emitChanges() {
  const formItems: PurchaseItemModel[] = localItems.value.map((item) => {
    const base = {
      id: item.id || undefined,
      quantity: item.quantity,
      amount: item.totalAmount,
    }

    // Create (POST): productId only — never foreignProductName
    if (props.formState === FormsState.Create) {
      return {
        ...base,
        productId: item.productId || 0,
      }
    }

    // Update (PUT): warehouse line vs foreign line
    if (item.productId) {
      return {
        ...base,
        productId: item.productId,
      }
    }

    if (item.isForeignLine || item.productName.trim()) {
      return {
        ...base,
        foreignProductName: item.productName.trim().slice(0, 200),
      }
    }

    return {
      ...base,
      productId: undefined,
    }
  })

  emit('update', formItems)
}

watch(
  () => props.purchaseItems,
  (newItems) => {
    if (newItems && newItems.length > 0) {
      isInitialLoad.value = true
      localItems.value = newItems.map(mapToLocalItem)

      nextTick(() => {
        isInitialLoad.value = false
      })
    }
  },
  { deep: true }
)

watch(
  () => localItems.value.map((item) => item.productId),
  (newIds, oldIds) => {
    if (!oldIds || isInitialLoad.value) return

    newIds.forEach((newId, index) => {
      if (oldIds[index] !== undefined && oldIds[index] !== newId) {
        localItems.value[index].isProductChanged = true
        if (newId) {
          localItems.value[index].isForeignLine = false
        }
      }
    })
  },
  { deep: true }
)

initializeItems()
</script>
