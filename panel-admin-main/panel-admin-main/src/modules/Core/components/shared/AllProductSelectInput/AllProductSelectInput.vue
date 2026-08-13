<template>
  <div class="flex flex-col">
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <AutoComplete
      ref="autoCompleteRef"
      :suggestions="filteredAllProducts"
      v-model="selectedAllProduct"
      optionLabel="name"
      :unstyled="false"
      optionValue="id"
      class="!block"
      :placeholder="$t(placeholder)"
      :required
      :forceSelection="true"
      :autoHighlight="true"
      :completeOnFocus="true"
      :disabled="readonly || disabled"
      @complete="search"
      :loading="isLoading"
      @change="onAllProductChange"
    >
      <template #option="{ option }">
        <div class="flex justify-between items-center w-full">
          <span class="font-medium">{{ option.name }}</span>
          <small class="text-sm">
            {{ option.buyAmount }} {{ $t('words.dinar') }}
          </small>
        </div>
      </template>

      <template #dropdown="{}" v-if="canCreate">
        <div class="flex items-center absolute top-3 left-3">
          <i
            class="pi pi-angle-down text-gray-700 font-thin"
            style="font-size:"
          ></i>
          <Divider layout="vertical" :unstyled="false" />
          <SvgIcon
            name="boldAddCircle"
            class="text-2xl cursor-pointer"
            :class="iconClass"
            style="font-weight: 1.3em"
            @click="onCreateClick"
          />
        </div>
      </template>

      <template #footer v-if="isLoading">
        <div class="flex items-center justify-center p-3">
          <i class="pi pi-spin pi-spinner text-primary mr-2"></i>
          <span class="text-sm text-gray-600"></span>
        </div>
      </template>
    </AutoComplete>

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs pt-2"
      variant="simple"
    >
      {{ error }}
    </Message>

    <CustomDialog
      v-model="isOpen"
      title="warehouse.createProduct"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <ProductForm
          :loading="isCreatingProduct"
          :formState="FormsState.Create"
          @submit="onProductSubmit"
        />
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AutoComplete, Divider, Message } from 'primevue'
import type { Emits, Props } from './AllProductSelectInput.types'
import SvgIcon from '../../SvgIcon/SvgIcon.vue'
import CustomDialog from '../CustomDialog/CustomDialog.vue'
import type {
  AllProductModel,
  ProductForm as ProductFormType,
} from '@/modules/Warehouse/types/model'
import { useAllProductDataQuery } from '@/modules/Warehouse/requests/queries'
import useAuthStore from '@/modules/Auth/store'
import ProductForm from '@/modules/Warehouse/components/ProductForm/ProductOneStepForm.vue'
import { FormsState } from '@/modules/Core/types/model/forms'
import { useCreateProductMutation } from '@/modules/Warehouse/requests/mutations'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const productId = defineModel<Props['modelValue']>('modelValue')

const filteredAllProducts = ref<Array<AllProductModel>>()
const selectedAllProduct = ref<AllProductModel | string | null>(null)
const isSyncingSelected = ref(false)

const pageIndex = ref(1)
const pageSize = ref(100)
const searchTerm = ref()
const isOpen = ref(false)

const allProducts = ref<Array<AllProductModel>>()
const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch!.id)

const { data, isLoading } = useAllProductDataQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm
)

watch(
  data,
  (newData) => {
    allProducts.value = newData?.paginatedProducts?.items || []
    filteredAllProducts.value = [...allProducts.value]

    syncSelectedProduct()
  },
  { immediate: true }
)

watch(productId, () => {
  syncSelectedProduct()
})

watch(
  () => props.displayName,
  () => {
    syncSelectedProduct()
  }
)

function syncSelectedProduct() {
  isSyncingSelected.value = true

  if (productId.value && allProducts.value) {
    const product = allProducts.value.find((p) => p.id === productId.value)
    if (product) {
      selectedAllProduct.value = product
      isSyncingSelected.value = false
      return
    }
  }

  if (!productId.value && props.displayName) {
    // PrimeVue AutoComplete can display a plain string label
    selectedAllProduct.value = props.displayName
    isSyncingSelected.value = false
    return
  }

  if (!productId.value) {
    selectedAllProduct.value = null
  }

  isSyncingSelected.value = false
}

function onAllProductChange(event: { value: AllProductModel | null }) {
  const newProduct = event.value

  if (newProduct) {
    productId.value = newProduct.id
  } else {
    productId.value = null
  }
}

const newAllProductName = ref<string | null>(null)

async function search(event: { query: string }) {
  newAllProductName.value = event.query

  searchTerm.value = event.query

  if (allProducts.value) {
    if (!event.query.trim().length) {
      filteredAllProducts.value = [...allProducts.value!]
    } else {
      filteredAllProducts.value = allProducts.value!.filter((product) => {
        return product.name.toLowerCase().includes(event.query.toLowerCase())
      })
    }
  }
}

watch(selectedAllProduct, (newProduct) => {
  if (isSyncingSelected.value) return

  if (typeof newProduct === 'string') {
    return
  }

  productId.value = newProduct ? newProduct.id : null
  if (typeof newProduct === 'object' && newProduct) {
    emit('update:productName', newProduct.name)
    emit('update:amount', newProduct.buyAmount)
    emit('update:sellAmount', newProduct.sellAmount)
    emit('update:dailyInstallmentAmount', newProduct.dailyInstallmentAmount)
  }
})

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}
const autoCompleteRef = ref()

function onCreateClick() {
  if (autoCompleteRef.value) {
    autoCompleteRef.value.hide()
  }
  toggleIsOpen()
}

const { isPending: isCreatingProduct, mutateAsync: createProduct } =
  useCreateProductMutation()

async function onProductSubmit(values: ProductFormType) {
  const updatedValues = {
    ...values,
    branchId: branchId.value,
  }
  await createProduct(updatedValues)
  toggleIsOpen()
}

const iconClass = computed(() => {
  return (filteredAllProducts.value &&
    filteredAllProducts.value.length === 0) ||
    !productId.value
    ? '!text-success'
    : '!text-gray-300'
})
</script>
