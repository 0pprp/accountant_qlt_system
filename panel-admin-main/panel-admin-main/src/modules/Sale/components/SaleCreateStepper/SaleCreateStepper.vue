<template>
  <CustomStepper :steps="steps" v-model:modelValue="activeStep">
    <template #content>
      <form
        novalidate
        @submit.prevent="onSubmit"
        class="mt-10 flex flex-col gap-6"
      >
        <CustomerDataForm
          v-show="activeStep === CreateSaleSteps.customerData"
          :customerFormMode="FormMode.IsCreate"
          ref="customerDataFormRef"
          @update:attachments="handleCustomerAttachments"
        />

        <CustomerDocumentsForm
          v-if="canReadAttachment"
          v-show="activeStep === CreateSaleSteps.customerDocuments"
          ref="customerDocumentsFormRef"
          :customerId="customerId"
          :customerFormMode="customerDocumentsFormMode"
          :attachments="customerAttachments"
        />

        <InstallmentDetailsForm
          v-show="activeStep === CreateSaleSteps.installmentDetails"
          :saleFormMode="FormMode.IsCreate"
          :customerId="customerId"
          ref="installmentDetailsFormRef"
        />

        <SellerDataForm
          v-show="activeStep === CreateSaleSteps.sellerData"
          :sellerFormMode="FormMode.IsCreate"
          ref="sellerDataFormRef"
        />

        <div class="py-4 w-full flex flex-row gap-4 justify-center">
          <Button
            v-if="
              activeStep === CreateSaleSteps.customerData ||
              activeStep === CreateSaleSteps.installmentDetails ||
              activeStep === CreateSaleSteps.sellerData
            "
            :disabled="isSubmitDisabled"
            :loading="isSubmitLoading"
            class="py-4 w-1/4"
            :class="'btn-primary'"
            type="submit"
          >
            {{ $t('words.save') }}
            <i v-if="isSubmitLoading" class="pi pi-spin pi-spinner"></i>
          </Button>

          <Button
            v-if="activeStep === CreateSaleSteps.customerDocuments"
            :disabled="hasError"
            class="btn-text-primary py-4 w-1/6"
            @click.prevent="nextStep()"
          >
            {{ $t('words.next') }}
            <i class="pi pi-angle-left" style="font-size:"></i>
          </Button>
        </div>
      </form>
    </template>
  </CustomStepper>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useCreateSaleMutation } from '../../requests/mutations'
import InstallmentDetailsForm from '../SaleForms/InstallmentDetailsForm/InstallmentDetailsForm.vue'
import CustomerDataForm from '../SaleForms/CustomerDataForm/CustomerDataForm.vue'
import CustomerDocumentsForm from '../SaleForms/CustomerDocumentsForm/CustomerDocumentsForm.vue'
import {
  CreateSaleSteps,
  type SaleFormWithFiles,
  type SellerInfoForm,
} from '../../types/model'
import { useUpdateSellerInfoMutation } from '../../requests/mutations/useUpdateSellerInfoMutation'
import SellerDataForm from '../SaleForms/SellerDataForm/SellerDataForm.vue'
import CustomStepper from '@/modules/Core/components/shared/CustomStepper/CustomStepper.vue'
import type { StepItem } from '@/modules/Core/components/shared/CustomStepper/CustomStepper.types'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import type { CustomerForm } from '@/modules/Customer/types/model'
import { useCreateCustomerMutation } from '@/modules/Customer/requests/mutations'
import type { Attachment } from '@/modules/User/types/model'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { t } = useI18n()

const router = useRouter()

const { can } = usePermission()
const canCreateAttachment = can('Attachment', 'Create')
const canReadAttachment = can('Attachment', 'Read')

const customerDocumentsFormMode = computed(() => {
  return canCreateAttachment ? FormMode.IsUpdate : FormMode.IsView
})

const activeStep = ref<CreateSaleSteps>(CreateSaleSteps.customerData)

const customerDataFormRef = ref<InstanceType<typeof CustomerDataForm> | null>(
  null
)

const installmentDetailsFormRef = ref<InstanceType<
  typeof InstallmentDetailsForm
> | null>(null)

const sellerDataFormRef = ref<InstanceType<typeof SellerDataForm> | null>(null)

const customerDocumentsFormRef = ref<{ hasError: boolean }>()

const allSteps = ref<StepItem[]>([
  {
    title: t('sale.saleCreateSteps.customerData.stepTitle'),
    icon: 'securityShieldUser',
    value: CreateSaleSteps.customerData,
    enTitle: 'customerData',
    isLastStep: false,
  },
  {
    title: t('sale.saleCreateSteps.customerDocuments.stepTitle'),
    icon: 'userCard',
    value: CreateSaleSteps.customerDocuments,
    enTitle: 'customerDocuments',
    isLastStep: false,
    permission: 'Attachment',
  },
  {
    title: t('sale.saleCreateSteps.installmentDetails.stepTitle'),
    icon: 'userPlus',
    value: CreateSaleSteps.installmentDetails,
    enTitle: 'installmentDetails',
    isLastStep: false,
  },

  {
    title: t('sale.saleCreateSteps.sellerInfo.stepTitle'),
    icon: 'dashboard',
    value: CreateSaleSteps.sellerData,
    enTitle: 'sellerData',
    isLastStep: true,
  },
])

const steps = computed(() => {
  return allSteps.value
    .filter((step) => {
      if (!step.permission) return true
      if (step.permission === 'Attachment') return canReadAttachment
      return true
    })
    .map((step, index, array) => ({
      ...step,
      isLastStep: index === array.length - 1,
    }))
})

const canSubmitCurrentStep = computed(() => {
  if (activeStep.value === CreateSaleSteps.customerData) {
    return true
  }

  if (activeStep.value === CreateSaleSteps.customerDocuments) {
    return canCreateAttachment
  }

  if (activeStep.value === CreateSaleSteps.installmentDetails) {
    return true
  }

  if (activeStep.value === CreateSaleSteps.sellerData) {
    return true
  }

  return false
})

const isSubmitDisabled = computed(() => {
  return hasError.value || !canSubmitCurrentStep.value || isSubmitLoading.value
})

const isSubmitLoading = computed(() => {
  return isCreatingSale.value || isCreatingCustomer.value || isUpdating.value
})

const customerId = ref<number>(-1)

const saleId = ref<number>(-1)

const customerAttachments = ref<Array<Attachment>>([])

function nextStep() {
  const currentIndex = steps.value.findIndex(
    (step) => step.value === activeStep.value
  )

  if (currentIndex < steps.value.length - 1) {
    activeStep.value = steps.value[currentIndex + 1].value as CreateSaleSteps
    localStorage.setItem('activeStep', activeStep.value.toString())
  }
}

const hasError = computed(() => {
  if (activeStep.value === CreateSaleSteps.customerData) {
    return customerDataFormRef.value?.hasError || false
  } else if (activeStep.value === CreateSaleSteps.customerDocuments) {
    return customerDocumentsFormRef.value?.hasError || false
  } else if (activeStep.value === CreateSaleSteps.sellerData) {
    return sellerDataFormRef.value?.hasError || false
  } else if (activeStep.value === CreateSaleSteps.installmentDetails) {
    return installmentDetailsFormRef.value?.hasError || false
  }
  return false
})

const { isPending: isCreatingSale, mutateAsync: createSale } =
  useCreateSaleMutation()

const customerForm = ref<CustomerForm>({
  hasError: false,
  fullName: '',
  motherName: '',
  nationalCode: '',
  birthDate: '',
  phoneNumber: '',
  whatsAppPhoneNumber: '',
  business: {
    name: '',
    address: '',
    nearestKnownLocation: '',
  },
  branchId: 0,
})

const { isPending: isCreatingCustomer, mutateAsync: createCustomer } =
  useCreateCustomerMutation()

function handleCustomerAttachments(
  attachments: Array<Attachment>,
  customerIdFromEmit: number
) {
  customerAttachments.value = attachments
  customerId.value = customerIdFromEmit
}

const sellerInfoForm = ref<SellerInfoForm>({
  hasError: false,
  creationAddress: '',
  saleDate: '',
  saleTime: '',
  sellerId: 0,
  orderListId: 0,
})

const { isPending: isUpdating, mutateAsync: updateSellerInfo } =
  useUpdateSellerInfoMutation()

const toastStore = useToastStore()

watch(isUpdating, () => {
  if (isUpdating.value) {
    toastStore.setMassage({
      title: 'createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

async function onSubmit() {
  if (!canSubmitCurrentStep.value) {
    toastStore.setMassage({
      title: t('errors.noPermission'),
      description: t('errors.noPermissionDescription'),
      dialogState: DialogState.Error,
      isOpen: true,
    })
    return
  }
  if (activeStep.value === CreateSaleSteps.customerData) {
    if (
      customerDataFormRef.value!.formValues.customerId &&
      customerDataFormRef.value!.formValues.customerId !== -1 &&
      customerDataFormRef.value!.formValues.isExistingCustomer
    ) {
      customerId.value = customerDataFormRef.value!.formValues.customerId
    } else {
      customerForm.value.birthDate =
        customerDataFormRef.value!.formValues.birthDate
      customerForm.value.branchId =
        customerDataFormRef.value!.formValues.branchId
      customerForm.value.fullName =
        customerDataFormRef.value!.formValues.fullName
      customerForm.value.motherName =
        customerDataFormRef.value!.formValues.motherName
      customerForm.value.nationalCode =
        customerDataFormRef.value!.formValues.nationalCode
      customerForm.value.phoneNumber =
        customerDataFormRef.value!.formValues.phoneNumber
      customerForm.value.whatsAppPhoneNumber =
        customerDataFormRef.value!.formValues.whatsAppPhoneNumber
      customerForm.value.business =
        customerDataFormRef.value!.formValues.business

      const result = await createCustomer(customerForm.value)
      customerId.value = result.data.id
    }
  } else if (activeStep.value === CreateSaleSteps.installmentDetails) {
    // Get the complete form data with orderItems array
    const saleFormData: SaleFormWithFiles = {
      ...installmentDetailsFormRef.value!.formValues,
      customerId: customerId.value,
    }

    const result = await createSale(saleFormData)
    saleId.value = result.data.id
  } else if (activeStep.value === CreateSaleSteps.sellerData) {
    sellerInfoForm.value.creationAddress =
      sellerDataFormRef.value!.formValues.creationAddress
    sellerInfoForm.value.saleDate = sellerDataFormRef.value!.formValues.saleDate
    sellerInfoForm.value.saleTime = sellerDataFormRef.value!.formValues.saleTime
    sellerInfoForm.value.sellerId = sellerDataFormRef.value!.formValues.sellerId
    sellerInfoForm.value.orderListId =
      sellerDataFormRef.value!.formValues.orderListId

    await updateSellerInfo({ id: saleId.value, payload: sellerInfoForm.value })
    router.push({ name: 'SaleListRoute' })
  }

  nextStep()
}
</script>
