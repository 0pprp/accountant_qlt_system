<template>
  <div class="flex flex-col">
    <span
      v-if="label"
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <AutoComplete
      :suggestions="filteredProductCategories"
      v-model:modelValue="selectedProductCategory"
      optionLabel="name"
      :unstyled="false"
      optionValue="id"
      class="!block"
      :class="{ 'input--clearable': clearable }"
      :placeholder="$t(placeholder)"
      :required
      :showClear="clearable"
      :forceSelection="true"
      :autoHighlight="true"
      :completeOnFocus="true"
      :disabled="readonly || disabled"
      :loading="isLoading || isCreating"
      @complete="search"
      @select="onProductCategorySelect"
      @clear="onClear"
      @focus="onFocus"
    >
      <template #dropdown="{}" v-if="canCreate && canCreateProductCategory">
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
            @click="toggleIsOpen"
          />
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
      title="warehouse.createProductCategory"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <form
          novalidate
          @submit.prevent="onProductCategorySubmit"
          class="mt-6 flex flex-col gap-6"
        >
          <div class="flex flex-col gap-1">
            <span class="input__label required">
              {{ $t('warehouse.productCategoryForm.name.label') }}
            </span>

            <InputText
              v-model="newCategoryName"
              :placeholder="
                $t('warehouse.productCategoryForm.name.placeholder')
              "
              name="name"
              type="text"
            />

            <Message
              v-if="!newCategoryName && categorySubmitAttempted"
              severity="error"
              class="text-error text-xs"
              variant="simple"
            >
              {{ $t('warehouse.productCategoryForm.name.validation.required') }}
            </Message>
          </div>

          <div class="w-full flex justify-center pt-4">
            <Button
              :disabled="!newCategoryName || isCreating"
              class="btn-primary py-4 w-1/2"
              type="submit"
            >
              {{ $t('words.save') }}
              <i v-if="isCreating" class="pi pi-spin pi-spinner"></i>
            </Button>
          </div>
        </form>
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AutoComplete, Divider, Message } from 'primevue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import type { Props } from './CategorySelectInput.types'
import SvgIcon from '../../SvgIcon/SvgIcon.vue'
import CustomDialog from '../CustomDialog/CustomDialog.vue'
import type {
  ProductCategory,
  ProductCategoryForm,
} from '@/modules/Warehouse/types/model'
import { useProductCategoryDataQuery } from '@/modules/Warehouse/requests/queries'
import { useCreateProductCategoryMutation } from '@/modules/Warehouse/requests/mutations'
import useAuthStore from '@/modules/Auth/store'
import { usePermission } from '@/modules/Core/composable/usePermission'

defineProps<Props>()

const categoryId = defineModel<Props['modelValue']>('modelValue')

const { can } = usePermission()
const canCreateProductCategory = can('ProductCategory', 'Create')

const filteredProductCategories = ref<Array<ProductCategory>>([])
const selectedProductCategory = ref<ProductCategory | null>(null)

const pageIndex = ref(1)
const pageSize = ref(100)
const searchTerm = ref<Array<string> | null>(null)
const searchQuery = ref<string>('')
let debounceTimer: ReturnType<typeof setTimeout> | null = null

const productCategories = ref<Array<ProductCategory>>()
const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch!.id)

const { data, isLoading } = useProductCategoryDataQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm
)

watch(
  data,
  (newData) => {
    productCategories.value = newData?.items.items || []
    filteredProductCategories.value = productCategories.value

    if (categoryId.value) {
      selectedProductCategory.value =
        productCategories.value.find(
          (productCategory) => productCategory.id === categoryId.value
        ) || null
    }
  },
  { immediate: true }
)

const newCategoryName = ref<string | null>(null)
const isOpen = ref(false)
const categorySubmitAttempted = ref(false)

async function search(event: { query: string }) {
  searchQuery.value = event.query
  newCategoryName.value = event.query

  // Clear previous timer
  if (debounceTimer) {
    clearTimeout(debounceTimer)
  }

  // Set new timer for 5 seconds
  debounceTimer = setTimeout(() => {
    if (searchQuery.value.trim().length) {
      searchTerm.value = [searchQuery.value]
    } else {
      searchTerm.value = null
    }
  }, 5000)
}

watch(selectedProductCategory, (newProductCategory) => {
  categoryId.value = newProductCategory ? newProductCategory.id : null
})

function onProductCategorySelect(event: { value: ProductCategory }) {
  selectedProductCategory.value = event.value
}

function onClear() {
  selectedProductCategory.value = null
  categoryId.value = null
  searchTerm.value = null
  searchQuery.value = ''
  if (debounceTimer) {
    clearTimeout(debounceTimer)
    debounceTimer = null
  }
}

function onFocus() {
  // Show all items on focus
  searchTerm.value = null
  searchQuery.value = ''
  if (debounceTimer) {
    clearTimeout(debounceTimer)
    debounceTimer = null
  }
  if (productCategories.value && productCategories.value.length > 0) {
    filteredProductCategories.value = [...productCategories.value]
  }
}

const { isPending: isCreating, mutateAsync: createProductCategory } =
  useCreateProductCategoryMutation()

const newProductCategory = ref<ProductCategoryForm>()

function toggleIsOpen() {
  isOpen.value = !isOpen.value
  if (!isOpen.value) {
    // Reset form when closing
    newCategoryName.value = null
    categorySubmitAttempted.value = false
  }
}

async function onProductCategorySubmit() {
  categorySubmitAttempted.value = true

  if (newCategoryName.value) {
    newProductCategory.value = { name: newCategoryName.value }

    const result = await createProductCategory(newProductCategory.value)
    categoryId.value = result.data.id

    // Close the dialog after successful creation
    toggleIsOpen()
  }
}

const iconClass = computed(() => {
  return (filteredProductCategories.value &&
    filteredProductCategories.value.length === 0) ||
    !categoryId.value
    ? '!text-success'
    : '!text-gray-300'
})
</script>

<style scoped>
.input--clearable :deep(.p-autocomplete-clear-icon) {
  margin-inline-end: 8px;
}
</style>
