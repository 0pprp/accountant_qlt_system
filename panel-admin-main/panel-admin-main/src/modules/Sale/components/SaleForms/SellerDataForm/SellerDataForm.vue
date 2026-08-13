<template>
  <div class="grid grid-cols-12 gap-6 px-6">
    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('sale.saleCreateSteps.sellerInfo.stepTitle') }}
    </span>

    <div class="col-span-6">
      <UsersSelectInput
        v-model="sellerId"
        placeholder="sale.sellerInfoForm.sellerId.placeholder"
        required
        :branchId
        :roleId="sellerRoleIds"
        :readonly="sellerFormMode === FormMode.IsView"
        :loading="loading"
        label="sale.sellerInfoForm.sellerId.label"
        :error="errors['sellerId']"
      />
    </div>

    <div class="col-span-6">
      <OrderListSelectInput
        v-model="orderListId"
        placeholder="sale.sellerInfoForm.orderListId.placeholder"
        required
        :readonly="sellerFormMode === FormMode.IsView"
        label="sale.sellerInfoForm.orderListId.label"
        :error="errors['orderListId']"
      />
    </div>

    <div class="col-span-12">
      <TextField
        v-model="creationAddress"
        placeholder="sale.sellerInfoForm.creationAddress.placeholder"
        required
        :readonly="sellerFormMode === FormMode.IsView"
        label="sale.sellerInfoForm.creationAddress.label"
        :error="errors['creationAddress']"
      />
    </div>

    <div class="col-span-6">
      <CalendarField
        v-model="saleDate"
        placeholder="sale.sellerInfoForm.saleDate.placeholder"
        :readonly="sellerFormMode === FormMode.IsView"
        label="sale.sellerInfoForm.saleDate.label"
        :error="errors['saleDate']"
      />
    </div>

    <div class="col-span-6">
      <CalendarField
        v-model="saleTime"
        :timeOnly="true"
        placeholder="sale.sellerInfoForm.saleTime.placeholder"
        :readonly="sellerFormMode === FormMode.IsView"
        label="sale.sellerInfoForm.saleTime.label"
        :error="errors['saleTime']"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { computed } from 'vue'
import { object, string } from 'yup'
import type { Props } from './SellerDataForm.types'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import CalendarField from '@/modules/Core/components/base/Fields/CalendarField/CalendarField.vue'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import useAuthStore from '@/modules/Auth/store'
import UsersSelectInput from '@/modules/Core/components/shared/UsersSelectInput/UsersSelectInput.vue'
import OrderListSelectInput from '@/modules/Core/components/shared/OrderListSelectInput/OrderListSelectInput.vue'
import type { SellerInfoForm } from '@/modules/Sale/types/model'
import { useRoleDataQuery } from '@/modules/Role/requests/queries'

const props = defineProps<Props>()

const { t } = useI18n()

const authStore = useAuthStore()
const branchId = computed(() => authStore.selectedBranch!.id)

const validationSchema = object({
  sellerId: string().required(
    t('sale.sellerInfoForm.sellerId.validation.required')
  ),
  orderListId: string().required(
    t('sale.sellerInfoForm.orderListId.validation.required')
  ),

  creationAddress: string().required(
    t('sale.sellerInfoForm.creationAddress.validation.required')
  ),
  saleDate: string()
    .optional()
    .transform((value) => value || undefined),
  saleTime: string()
    .optional()
    .transform((value) => value || undefined),
})

const { defineField, errors, values } = useForm<SellerInfoForm>({
  validationSchema,
  initialValues: {
    creationAddress: props.creationAddress || '',
    saleDate: props.saleDate ?? undefined,
    saleTime: props.saleTime ?? undefined,
    sellerId: props.sellerId || 0,
    orderListId: props.orderListId || 0,
  },
})

const [creationAddress] = defineField('creationAddress')
const [saleDate] = defineField('saleDate')
const [saleTime] = defineField('saleTime')
const [sellerId] = defineField('sellerId')
const [orderListId] = defineField('orderListId')

const formValues = computed(() => values)

const hasError = computed(() => {
  return (
    Object.keys(errors.value).length > 0 ||
    !creationAddress.value ||
    !saleDate.value ||
    !saleTime.value ||
    !sellerId.value ||
    !orderListId.value ||
    !branchId.value
  )
})

const { data: roleData, isLoading: loading } = useRoleDataQuery()

const sellerRoleIds = computed(() => {
  if (!roleData.value) return []

  const roleIds = []

  const mandobRole = roleData.value.find((role) => role.name === 'Mandob')
  if (mandobRole?.id) {
    roleIds.push(mandobRole.id)
  }

  const motabaRole = roleData.value.find((role) => role.name === 'Motaba')
  if (motabaRole?.id) {
    roleIds.push(motabaRole.id)
  }

  return roleIds
})

defineExpose({ formValues, hasError })
</script>
