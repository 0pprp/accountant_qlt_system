<template>
  <div class="grid grid-cols-12 gap-6 px-6">
    <div class="col-span-12">
      <ProductsForm
        ref="productsFormRef"
        :formState="saleFormMode"
        :orderItems="orderItems"
      />
    </div>

    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('sale.saleCreateSteps.installmentDetails.salesDocumentsTitle') }}
    </span>

    <div class="col-span-12">
      <DocumentsForm
        ref="documentsRef"
        :saleFormMode="saleFormMode"
        :attachments
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import ProductsForm from './ProductsForm/ProductsForm.vue'
import type { Props } from './InstallmentDetailsForm.types'
import DocumentsForm, {
  type AttachmentForm,
} from './DocumentsForm/DocumentsForm.vue'
import type { SaleFormWithFiles } from '@/modules/Sale/types/model'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import useAuthStore from '@/modules/Auth/store'

const props = defineProps<Props>()

const productsFormRef = ref<InstanceType<typeof ProductsForm> | null>(null)

const documentsRef = ref<{
  documentValue: Array<AttachmentForm>
  hasError: boolean
}>()

const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch!.id)

const formValues = computed<SaleFormWithFiles>(() => ({
  orderItems: productsFormRef.value?.items || [],
  attachments: documentsRef.value?.documentValue || [],
  branchId: branchId.value,
  customerId: props.customerId || 0,
}))

const requiresCustomer = computed(
  () => props.saleFormMode === FormMode.IsCreate
)

const hasError = computed(
  () =>
    productsFormRef.value?.hasError ||
    documentsRef.value?.hasError ||
    !productsFormRef.value?.items ||
    productsFormRef.value?.items.length === 0 ||
    (requiresCustomer.value && !props.customerId)
)

defineExpose({ formValues, hasError })
</script>
