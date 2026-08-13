<template>
  <CustomTab v-if="saleInfo" :tabs="steps" v-model:modelValue="activeTab">
    <template #content>
      <form
        novalidate
        @submit.prevent="onSubmit"
        class="mt-10 flex flex-col gap-6"
      >
        <InstallmentDetailsForm
          v-show="activeTab === CreateSaleSteps.installmentDetails"
          ref="installmentDetailsFormRef"
          :orderItems="saleInfo.orderInfo.orderItems"
          :saleFormMode="formMode"
          :customerId="customerId"
          :attachments="saleInfo.orderInfo.attachments"
        />

        <SellerDataForm
          v-show="activeTab === CreateSaleSteps.sellerData"
          :sellerFormMode="formMode"
          ref="sellerDataFormRef"
          :creationAddress="sellerFieldProps.creationAddress"
          :saleDate="sellerFieldProps.saleDate"
          :saleTime="sellerFieldProps.saleTime"
          :sellerId="sellerFieldProps.sellerId"
          :orderListId="sellerFieldProps.orderListId"
        />

        <div
          v-if="formMode !== FormMode.IsView"
          class="py-4 w-full flex flex-row gap-4 justify-center"
        >
          <Button
            v-if="canSubmitSave"
            :disabled="hasError || isUpdateSale"
            :loading="isUpdateSale"
            class="py-4 w-1/4 btn-primary"
            type="submit"
          >
            {{ $t('words.save') }}
          </Button>
        </div>
      </form>
    </template>
  </CustomTab>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import {
  type SaleFormWithFiles,
  type SellerInfoForm,
  CreateSaleSteps,
} from '../../types/model'
import { type Props } from './SaleTabs.types'
import InstallmentDetailsForm from '../SaleForms/InstallmentDetailsForm/InstallmentDetailsForm.vue'
import SellerDataForm from '../SaleForms/SellerDataForm/SellerDataForm.vue'
import { useUpdateSaleMutation } from '../../requests/mutations'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import CustomTab from '@/modules/Core/components/shared/CustomTab/CustomTab.vue'
import type { TabItem } from '@/modules/Core/components/shared/CustomTab/CustomTab.types'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'

const props = defineProps<Props>()
const { t } = useI18n()
const router = useRouter()
const route = useRoute()

const activeTab = ref<CreateSaleSteps>(CreateSaleSteps.installmentDetails)

const installmentDetailsFormRef = ref<InstanceType<
  typeof InstallmentDetailsForm
> | null>(null)

const sellerDataFormRef = ref<InstanceType<typeof SellerDataForm> | null>(null)

const steps = computed<TabItem[]>(() => [
  {
    title: t('sale.saleCreateSteps.installmentDetails.stepTitle'),
    icon: 'userPlus',
    value: CreateSaleSteps.installmentDetails,
    enTitle: 'installmentDetails',
    isLastTab: false,
  },
  {
    title: t('sale.saleCreateSteps.sellerInfo.stepTitle'),
    icon: 'dashboard',
    value: CreateSaleSteps.sellerData,
    enTitle: 'sellerData',
    isLastTab: true,
  },
])

const customerId = computed(() => {
  const fromQuery = route.query.customerId ? Number(route.query.customerId) : 0
  if (fromQuery) return fromQuery
  return props.saleInfo?.orderInfo?.customerId ?? 0
})

const sellerFieldProps = computed(() => {
  const s = props.saleInfo.sellerInfo
  return {
    creationAddress: s.creationAddress,
    saleDate: s.saleDate,
    saleTime: s.saleTime.slice(0, 5),
    sellerId: s.seller.id,
    orderListId: s.orderList.id,
  }
})

const canSubmitSave = computed(
  () =>
    activeTab.value === CreateSaleSteps.installmentDetails ||
    activeTab.value === CreateSaleSteps.sellerData
)

function goToNextTab() {
  if (activeTab.value === CreateSaleSteps.installmentDetails) {
    activeTab.value = CreateSaleSteps.sellerData
  }
}

const hasError = computed(() => {
  if (activeTab.value === CreateSaleSteps.sellerData) {
    return sellerDataFormRef.value?.hasError ?? false
  }
  if (activeTab.value === CreateSaleSteps.installmentDetails) {
    return installmentDetailsFormRef.value?.hasError ?? false
  }
  return false
})

const { isPending: isUpdateSale, mutateAsync: updateSale } =
  useUpdateSaleMutation()

const sellerInfoForm = ref<SellerInfoForm>({
  hasError: false,
  creationAddress: '',
  saleDate: '',
  saleTime: '',
  sellerId: 0,
  orderListId: 0,
})

const toastStore = useToastStore()
const submittingStep = ref<CreateSaleSteps | null>(null)
watch(isUpdateSale, (loading) => {
  if (loading && submittingStep.value === CreateSaleSteps.sellerData) {
    toastStore.setMassage({
      title: 'user.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

const saleId = computed(() => Number(route.params.saleId))

const formMode = computed(() => {
  if (route.name === 'SaleCreateRoute') return FormMode.IsCreate
  if (route.name === 'SaleUpdateRoute') return FormMode.IsUpdate
  return FormMode.IsView
})

function syncSellerInfoFromDataForm() {
  const values = sellerDataFormRef.value!.formValues
  Object.assign(sellerInfoForm.value, {
    creationAddress: values.creationAddress,
    saleDate: values.saleDate,
    saleTime: values.saleTime,
    sellerId: values.sellerId,
    orderListId: values.orderListId,
  })
}

async function onSubmit() {
  submittingStep.value = activeTab.value

  if (activeTab.value === CreateSaleSteps.installmentDetails) {
    const saleFormData: SaleFormWithFiles =
      installmentDetailsFormRef.value!.formValues

    syncSellerInfoFromDataForm()

    await updateSale({
      id: saleId.value,
      payload: { ...saleFormData, ...sellerInfoForm.value },
    })
  } else if (activeTab.value === CreateSaleSteps.sellerData) {
    syncSellerInfoFromDataForm()

    await updateSale({
      id: saleId.value,
      payload: {
        ...sellerInfoForm.value,
        ...installmentDetailsFormRef.value!.formValues,
      },
    })

    router.push({ name: 'SaleListRoute' })
  }
  goToNextTab()
  submittingStep.value = null
}
</script>
