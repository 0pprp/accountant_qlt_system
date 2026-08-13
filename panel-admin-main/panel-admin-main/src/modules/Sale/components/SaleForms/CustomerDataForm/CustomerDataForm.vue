<template>
  <div class="grid grid-cols-12 gap-6 px-6">
    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('sale.saleForm.customerForm.personalInfoTitle') }}
    </span>

    <div class="col-span-6">
      <CustomerSelectInput
        v-model="customerId"
        placeholder="sale.saleForm.customerForm.customerId.placeholder"
        required
        :readonly="customerFormMode === FormMode.IsView"
        :loading="isLoadingCustomer"
        label="sale.saleForm.customerForm.customerId.label"
        :error="errors['fullName']"
        @update:model-value="handleCustomerSelect"
        @update:customer-name="handleCustomerNameUpdate"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="fullName"
        placeholder="sale.saleForm.customerForm.fullName.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.fullName.label"
        :error="errors['fullName']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="motherName"
        placeholder="sale.saleForm.customerForm.motherName.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.motherName.label"
        :error="errors['motherName']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="nationalCode"
        placeholder="sale.saleForm.customerForm.nationalCode.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.nationalCode.label"
        :error="errors['nationalCode']"
      />
    </div>

    <div class="col-span-6">
      <CalendarField
        v-model="birthDate"
        placeholder="sale.saleForm.customerForm.birthDate.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.birthDate.label"
        :error="errors['birthDate']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="phoneNumber"
        placeholder="sale.saleForm.customerForm.phoneNumber.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.phoneNumber.label"
        :error="errors['phoneNumber']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="whatsAppPhoneNumber"
        placeholder="sale.saleForm.customerForm.whatsAppPhoneNumber.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.whatsAppPhoneNumber.label"
        :error="errors['whatsAppPhoneNumber']"
      />
    </div>

    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('sale.saleForm.customerForm.businessInfoTitle') }}
    </span>

    <div class="col-span-12">
      <TextField
        v-model="businessName"
        placeholder="sale.saleForm.customerForm.businessName.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.businessName.label"
        :error="errors['business.name']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="businessAddress"
        placeholder="sale.saleForm.customerForm.address.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.address.label"
        :error="errors['business.address']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="businessNearestKnownLocation"
        placeholder="sale.saleForm.customerForm.nearestKnownLocation.placeholder"
        required
        :readonly="
          customerFormMode === FormMode.IsView ||
          isExistingCustomer ||
          !canCreateCustomer
        "
        label="sale.saleForm.customerForm.nearestKnownLocation.label"
        :error="errors['business.nearestKnownLocation']"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { computed, watch } from 'vue'
import { object, string } from 'yup'
import type { Emits, Props } from './CustomerDataForm.types'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import type { CustomerForm } from '@/modules/Sale/types/model'
import { REGEX } from '@/modules/Core/constants'
import CalendarField from '@/modules/Core/components/base/Fields/CalendarField/CalendarField.vue'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import useAuthStore from '@/modules/Auth/store'
import CustomerSelectInput from '@/modules/Core/components/shared/CustomerSelectInput/CustomerSelectInput.vue'
import { useCustomerByIdDataQuery } from '@/modules/Customer/requests/queries'
import { usePermission } from '@/modules/Core/composable/usePermission'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const customerId = defineModel<number>('modelValue', { default: -1 })

const { t } = useI18n()

const authStore = useAuthStore()
const branchId = computed(() => authStore.selectedBranch!.id)

const { can } = usePermission()
const canCreateCustomer = can('Customer', 'Create')

const isExistingCustomer = computed(
  () => customerId.value !== undefined && customerId.value !== -1
)

const customerIdForQuery = computed(() => customerId.value || -1)

const { data: customerData, isLoading: isLoadingCustomer } =
  useCustomerByIdDataQuery(customerIdForQuery, {
    enabled: computed(
      () => customerId.value !== undefined && customerId.value !== -1
    ),
  })

const validationSchema = object({
  fullName: string().required(t('sale.saleForm.customerForm.required')),
  motherName: string().required(t('sale.saleForm.customerForm.required')),
  nationalCode: string()
    .required(t('sale.saleForm.customerForm.required'))
    .matches(
      REGEX.ONLY_NUMBERS,
      t('sale.saleForm.customerForm.nationalCode.match')
    ),
  birthDate: string().required(t('sale.saleForm.customerForm.required')),
  phoneNumber: string()
    .required(t('sale.saleForm.customerForm.required'))
    .matches(
      REGEX.PHONE_11_DIGIT_REGEX,
      t('sale.saleForm.customerForm.phoneNumber.match')
    ),
  whatsAppPhoneNumber: string()
    .required(t('sale.saleForm.customerForm.required'))
    .matches(
      REGEX.PHONE_11_DIGIT_REGEX,
      t('sale.saleForm.customerForm.whatsAppPhoneNumber.match')
    ),
  business: object({
    name: string().required(t('sale.saleForm.customerForm.required')),
    address: string().required(t('sale.saleForm.customerForm.required')),
    nearestKnownLocation: string().required(
      t('sale.saleForm.customerForm.required')
    ),
  }).required(),
})

const { defineField, errors, values, setValues } = useForm<CustomerForm>({
  validationSchema,
  initialValues: {
    fullName: props.fullName || '',
    motherName: props.motherName || '',
    nationalCode: props.nationalCode || '',
    birthDate: props.birthDate || '',
    phoneNumber: props.phoneNumber || '',
    whatsAppPhoneNumber: props.whatsAppPhoneNumber || '',
    business: {
      name: props.business?.name || '',
      address: props.business?.address || '',
      nearestKnownLocation: props.business?.nearestKnownLocation || '',
    },
  },
})

const formValues = computed(() => ({
  ...values,
  customerId: customerId.value,
  branchId: branchId.value,
  isExistingCustomer: isExistingCustomer.value,
  business: {
    name: businessName.value,
    address: businessAddress.value,
    nearestKnownLocation: businessNearestKnownLocation.value,
  },
}))

const hasError = computed(() => {
  return (
    Object.keys(errors.value).length > 0 ||
    !fullName.value ||
    !motherName.value ||
    !nationalCode.value ||
    !birthDate.value ||
    !phoneNumber.value ||
    !branchId.value ||
    !whatsAppPhoneNumber.value ||
    !businessName.value ||
    !businessAddress.value ||
    !businessNearestKnownLocation.value
  )
})

const [fullName] = defineField('fullName')
const [motherName] = defineField('motherName')
const [nationalCode] = defineField('nationalCode')
const [birthDate] = defineField('birthDate')
const [phoneNumber] = defineField('phoneNumber')
const [whatsAppPhoneNumber] = defineField('whatsAppPhoneNumber')

const [businessName] = defineField('business.name')
const [businessAddress] = defineField('business.address')
const [businessNearestKnownLocation] = defineField(
  'business.nearestKnownLocation'
)

function handleCustomerSelect(selectedCustomerId: number) {
  customerId.value = selectedCustomerId

  if (!selectedCustomerId || selectedCustomerId === -1) {
    clearForm()
  }
}

function handleCustomerNameUpdate(newName: string) {
  if ((!customerId.value || customerId.value === -1) && newName) {
    fullName.value = newName
  }
}

watch(
  customerData,
  (newCustomerData) => {
    if (newCustomerData && customerId.value && customerId.value !== -1) {
      setValues({
        fullName: newCustomerData.fullName,
        motherName: newCustomerData.motherName,
        nationalCode: newCustomerData.nationalCode,
        birthDate: newCustomerData.birthDate,
        phoneNumber: newCustomerData.phoneNumber,
        whatsAppPhoneNumber: newCustomerData.whatsAppPhoneNumber,
        business: {
          name: newCustomerData.business.name,
          address: newCustomerData.business.address,
          nearestKnownLocation: newCustomerData.business.nearestKnownLocation,
        },
      })

      if (newCustomerData.attachments) {
        emit(
          'update:attachments',
          newCustomerData.attachments,
          customerId.value
        )
      }
    }
  },
  { immediate: true }
)

function clearForm() {
  setValues({
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
  })
}

defineExpose({ formValues, hasError })
</script>
