<template>
  <div>
    <TheHeader>
      <template #create>
        <Button
          v-if="canCreateProductCategory && canCreateProduct"
          class="bg-primary text-white"
          @click="handleCreateProduct()"
        >
          {{ $t('words.add') }}
          <SvgIcon name="addCircle" />
        </Button>
      </template>
    </TheHeader>

    <ProductCategoryList
      v-if="canReadProductCategory"
      @edit-item="handleEdit"
      @update-product="handleUpdateProduct"
      @delete-product="handleDeleteProduct"
    />

    <CustomDialog
      v-model="isOpen"
      :title="getDialogTitle()"
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <ProductCategoryForm
          v-if="productFormState === null"
          :loading="isUpdating"
          :selectedProductCategory
          @submit="onSubmit"
        />

        <ProductForm
          v-else
          :loading="isCreatingProduct || isUpdatingProduct"
          :categoryId
          :warehouseId
          :selectedProduct
          :formState="productFormState"
          @submit="onProductSubmit"
        />
      </template>
    </CustomDialog>

    <BaseConfirmationDialog
      v-if="productFormState === FormsState.Delete"
      v-model="isOpenDeleteConfirmation"
      icon="delete"
      title="warehouse.deleteProduct.title"
      color="error"
      submitText="words.delete"
      @update:is-open="toggleDeleteConfirmation"
      @on-submit="confirmDeleteProduct"
    >
      <template #content>
        <div class="text-gray-700 text-center">
          {{ $t('warehouse.deleteProduct.description') }}
        </div>
        <div class="text-center font-semibold">
          {{ selectedProduct?.name }}
        </div>
      </template>
    </BaseConfirmationDialog>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Button } from 'primevue'
import ProductCategoryForm from '../components/ProductCategoryForm/ProductCategoryForm.vue'
import ProductCategoryList from '../components/WarehouseList/WarehouseList.vue'
import {
  useCreateProductMutation,
  useDeleteProductMutation,
  useUpdateProductCategoryMutation,
  useUpdateProductMutation,
} from '../requests/mutations'
import { useProductCategoryDataQuery } from '../requests/queries'
import ProductForm from '../components/ProductForm/ProductForm.vue'
import { useBranchWarehousesDataQuery } from '@/modules/Branch/requests/queries'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import type {
  Product,
  ProductCategory,
  ProductCategoryForm as ProductCategoryFormType,
  ProductForm as ProductFormType,
} from '@/modules/Warehouse/types/model'
import { FormsState } from '@/modules/Core/types/model/forms'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import useAuthStore from '@/modules/Auth/store'
import BaseConfirmationDialog from '@/modules/Core/components/base/BaseConfirmationDialog/BaseConfirmationDialog.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'

const isOpen = ref(false)
const isOpenDeleteConfirmation = ref(false)
const productFormState = ref<FormsState | null>(null)

const { can } = usePermission()
const canReadProductCategory = can('ProductCategory', 'Read')
const canCreateProductCategory = can('ProductCategory', 'Create')
const canCreateProduct = can('Product', 'Create')

function toggleIsOpen() {
  isOpen.value = !isOpen.value

  if (!isOpen.value) {
    resetFormState()
  }
}

function toggleDeleteConfirmation() {
  isOpenDeleteConfirmation.value = !isOpenDeleteConfirmation.value

  if (!isOpenDeleteConfirmation.value) {
    resetFormState()
  }
}

function resetFormState() {
  productFormState.value = null
  selectedProduct.value = undefined
  selectedProductCategory.value = undefined
  categoryId.value = undefined
  warehouseId.value = undefined
}

const selectedProductCategory = ref<ProductCategory>()
const selectedProduct = ref<Product>()
const categoryId = ref<number>()
const warehouseId = ref<number>()

const authStore = useAuthStore()
const branchId = authStore.selectedBranch!.id

// Get categories for lookup
const categoryPageIndex = ref(1)
const categoryPageSize = ref(100)
const branchIdRef = ref(authStore.selectedBranch!.id)

watch(
  () => authStore.selectedBranch?.id,
  (newId) => {
    if (newId) {
      branchIdRef.value = newId
    }
  }
)

const { data: categoryData } = useProductCategoryDataQuery(
  categoryPageIndex,
  categoryPageSize,
  branchIdRef
)

const { data: warehouseData } = useBranchWarehousesDataQuery(branchIdRef)

const { isPending: isUpdating, mutateAsync: updateProductCategory } =
  useUpdateProductCategoryMutation()

const { isPending: isCreatingProduct, mutateAsync: createProduct } =
  useCreateProductMutation()

const { isPending: isUpdatingProduct, mutateAsync: updateProduct } =
  useUpdateProductMutation()

const { isPending: isDeletingProduct, mutateAsync: deleteProduct } =
  useDeleteProductMutation()

function handleEdit(item: ProductCategory) {
  selectedProductCategory.value = item
  productFormState.value = null
  toggleIsOpen()
}

async function onSubmit(values: ProductCategoryFormType) {
  await updateProductCategory({
    id: selectedProductCategory.value!.id,
    payload: values,
  })
  toggleIsOpen()
}

function handleCreateProduct() {
  productFormState.value = FormsState.Create
  selectedProduct.value = undefined
  categoryId.value = undefined
  warehouseId.value = undefined
  toggleIsOpen()
}

function handleUpdateProduct(product: Product) {
  productFormState.value = FormsState.Update
  selectedProduct.value = product
  // Look up categoryId from categoryName
  const categories = categoryData.value?.items.items || []
  const matchedCategory = categories.find(
    (cat) => cat.name === product.categoryName
  )
  categoryId.value = matchedCategory?.id

  // Look up warehouseId from warehouseName
  const warehouses = warehouseData.value || []
  const matchedWarehouse = warehouses.find(
    (wh) => wh.name === product.warehouseName
  )
  warehouseId.value = matchedWarehouse?.id
  toggleIsOpen()
}

function handleDeleteProduct(product: Product) {
  productFormState.value = FormsState.Delete
  selectedProduct.value = product
  categoryId.value = undefined
  isOpenDeleteConfirmation.value = true
}

async function onProductSubmit(values: ProductFormType) {
  const updatedValues = {
    ...values,
    branchId: branchId,
  }

  if (productFormState.value === FormsState.Create) {
    await createProduct(updatedValues)
  } else if (productFormState.value === FormsState.Update) {
    await updateProduct({
      id: selectedProduct.value!.id,
      payload: updatedValues,
    })
  }
  toggleIsOpen()
}

async function confirmDeleteProduct() {
  if (!selectedProduct.value) return

  await deleteProduct({ id: selectedProduct.value.id })
  toggleDeleteConfirmation()
}

function getDialogTitle() {
  switch (productFormState.value) {
    case FormsState.Create:
      return 'warehouse.createProduct'
    case FormsState.Update:
      return 'warehouse.updateProductCategory'
    case FormsState.Delete:
      return 'warehouse.deleteProduct.title'
    default:
      return 'warehouse.updateProductCategory'
  }
}

const toastStore = useToastStore()

watch(
  [isUpdating, isCreatingProduct, isUpdatingProduct, isDeletingProduct],
  () => {
    if (isUpdating.value || isUpdatingProduct.value) {
      toastStore.setMassage({
        title: 'warehouse.updatePending',
        description: undefined,
        dialogState: DialogState.Loading,
        isOpen: true,
      })
    }
    if (isCreatingProduct.value) {
      toastStore.setMassage({
        title: 'warehouse.createPending',
        description: undefined,
        dialogState: DialogState.Loading,
        isOpen: true,
      })
    }
    if (isDeletingProduct.value) {
      toastStore.setMassage({
        title: 'warehouse.deletePending',
        description: undefined,
        dialogState: DialogState.Loading,
        isOpen: true,
      })
    }
  }
)
</script>
