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
      :suggestions="filteredProducts"
      v-model:modelValue="selectedProduct"
      optionLabel="name"
      :unstyled="false"
      :disabled="readonly || disabled"
      optionValue="id"
      class="!block"
      returnObject
      :placeholder="$t(placeholder)"
      :required
      :loading="isLoading"
      :forceSelection="true"
      :autoHighlight="true"
      :completeOnFocus="true"
      @complete="search"
      @select="onProductSelect"
    >
    </AutoComplete>

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs pt-2"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AutoComplete, Message } from 'primevue'
import type { Props, Emits } from './ProductSelectInput.types'
import type { Product } from '@/modules/Warehouse/types/model'
import { useProductDataQuery } from '@/modules/Warehouse/requests/queries'
import useAuthStore from '@/modules/Auth/store'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const productId = defineModel<Props['modelValue']>('modelValue')

const filteredProducts = ref<Array<Product>>()
const selectedProduct = ref<Product | null>(null)

const categoryId = computed(() => props.productCategoryId || -1)
const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch!.id)
const productPageIndex = ref(1)
const productPageSize = ref(100)

const products = ref<Array<Product>>()

const hasValidCategoryId = computed(
  () =>
    props.productCategoryId !== undefined &&
    props.productCategoryId !== null &&
    props.productCategoryId > 0
)

const { data: productList, isLoading } = useProductDataQuery(
  categoryId,
  branchId,
  productPageIndex,
  productPageSize,
  {
    enabled: hasValidCategoryId,
  }
)

watch(
  productList,
  (newData) => {
    if (newData?.paginatedProducts?.items) {
      products.value = newData.paginatedProducts.items
      filteredProducts.value = [...newData.paginatedProducts.items]
    } else {
      products.value = []
      filteredProducts.value = []
    }

    if (productId.value && products.value.length > 0) {
      selectedProduct.value =
        products.value.find((product) => product.id === productId.value) || null
    } else {
      selectedProduct.value = null
    }
  },
  { immediate: true }
)

const newCategoryName = ref<string | null>(null)

async function search(event: { query: string }) {
  newCategoryName.value = event.query

  // TODO searchable with searchTerm in queries after add backend this
  if (products.value) {
    setTimeout(() => {
      if (!event.query.trim().length) {
        filteredProducts.value = [...products.value!]
      } else {
        filteredProducts.value = products.value!.filter((product) => {
          return product.name
            .toLowerCase()
            .startsWith(event.query.toLowerCase())
        })
      }
    }, 20)
  }
}

watch(selectedProduct, (newProduct) => {
  productId.value = newProduct ? newProduct.id : null
  if (newProduct) {
    emit('update:productName', newProduct.name)
    emit('update:buyAmount', newProduct.buyAmount)
    emit('update:sellAmount', newProduct.sellAmount)
    emit('update:dailyInstallmentAmount', newProduct.dailyInstallmentAmount)
  } else {
    emit('update:productName', '')
    emit('update:buyAmount', 0)
    emit('update:sellAmount', 0)
    emit('update:dailyInstallmentAmount', 0)
  }
})

function onProductSelect(event: { value: Product }) {
  selectedProduct.value = event.value
}
</script>
