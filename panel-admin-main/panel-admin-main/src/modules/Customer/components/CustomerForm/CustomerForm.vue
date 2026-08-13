<template>
  <form @submit.prevent="submit">
    <div>
      <h2 class="text-[14px] font-medium text-gray-700 mb-4">
        {{ $t('customer.form.personalInfoTitle') }}
      </h2>

      <div class="flex gap-6">
        <div class="w-full">
          <label class="text-gray-300 text-[14px] font-medium">
            {{ $t('customer.form.fullName.label') }}
          </label>

          <InputText v-model="fullName" class="mt-2" type="text" />

          <Message
            v-if="errors['fullName']"
            class="mt-2"
            severity="error"
            size="small"
            variant="simple"
            :unstyled="false"
          >
            {{ errors['fullName'] }}
          </Message>
        </div>

        <div class="w-full">
          <label class="text-gray-300 text-[14px] font-medium">
            {{ $t('customer.form.motherName.label') }}
          </label>

          <InputText v-model="motherName" class="mt-2" type="text" />

          <Message
            v-if="errors['motherName']"
            class="mt-2"
            severity="error"
            size="small"
            variant="simple"
            :unstyled="false"
          >
            {{ errors['motherName'] }}
          </Message>
        </div>
      </div>

      <div class="w-full my-6">
        <label class="text-gray-300 text-[14px] font-medium">
          {{ $t('customer.form.nationalCode.label') }}
        </label>

        <InputText v-model="nationalCode" class="mt-2" :useGrouping="false" />

        <Message
          v-if="errors['nationalCode']"
          class="mt-2"
          severity="error"
          size="small"
          variant="simple"
          :unstyled="false"
        >
          {{ errors['nationalCode'] }}
        </Message>
      </div>

      <div class="w-full flex flex-col gap-1">
        <label class="text-gray-300 text-[14px] font-medium">
          {{ $t('customer.form.birthDate.label') }}
        </label>

        <DatePicker
          v-model="birthDate"
          :unstyled="false"
          size="large"
          view="date"
          dateFormat="yy/mm/dd"
        />

        <Message
          v-if="errors['birthDate']"
          class="mt-2"
          severity="error"
          size="small"
          variant="simple"
          :unstyled="false"
        >
          {{ errors['birthDate'] }}
        </Message>
      </div>

      <div class="flex justify-center my-[32px]">
        <Divider class="!w-2/3" :unstyled="false" />
      </div>

      <h2 class="text-[14px] font-medium text-gray-700 mb-4">
        {{ $t('customer.form.businessInfoTitle') }}
      </h2>

      <div class="w-full mb-4">
        <label class="text-gray-300 text-[14px] font-medium">
          {{ $t('customer.form.businessName.label') }}
        </label>

        <InputText v-model="businessName" class="mt-2" />

        <Message
          v-if="errors['business.name']"
          class="mt-2"
          severity="error"
          size="small"
          variant="simple"
          :unstyled="false"
        >
          {{ errors['business.name'] }}
        </Message>
      </div>

      <div class="flex gap-6 mb-4">
        <div class="w-full">
          <label class="text-gray-300 text-[14px] font-medium">
            {{ $t('customer.form.whatsAppPhoneNumber.label') }}
          </label>

          <InputText v-model="whatsAppPhoneNumber" class="mt-2" type="text" />

          <Message
            v-if="errors['whatsAppPhoneNumber']"
            class="mt-2"
            severity="error"
            size="small"
            variant="simple"
            :unstyled="false"
          >
            {{ errors['whatsAppPhoneNumber'] }}
          </Message>
        </div>

        <div class="w-full">
          <label class="text-gray-300 text-[14px] font-medium">
            {{ $t('customer.form.phoneNumber.label') }}
          </label>

          <InputText class="mt-2" type="text" v-model="phoneNumber" />

          <Message
            v-if="errors['phoneNumber']"
            class="mt-2"
            severity="error"
            size="small"
            variant="simple"
            :unstyled="false"
          >
            {{ errors['phoneNumber'] }}
          </Message>
        </div>
      </div>

      <div class="flex gap-6 mb-4">
        <div class="w-full">
          <label class="text-gray-300 text-[14px] font-medium">
            {{ $t('customer.form.address.label') }}
          </label>

          <InputText v-model="address" class="mt-2" type="text" />

          <Message
            v-if="errors['business.address']"
            class="mt-2"
            severity="error"
            size="small"
            variant="simple"
            :unstyled="false"
          >
            {{ errors['business.address'] }}
          </Message>
        </div>

        <div class="w-full">
          <label class="text-gray-300 text-[14px] font-medium">
            {{ $t('customer.form.nearestKnownLocation.label') }}
          </label>

          <InputText v-model="nearestKnownLocation" class="mt-2" type="text" />

          <Message
            v-if="errors['business.nearestKnownLocation']"
            class="mt-2"
            severity="error"
            size="small"
            variant="simple"
            :unstyled="false"
          >
            {{ errors['business.nearestKnownLocation'] }}
          </Message>
        </div>
      </div>
    </div>

    <div class="flex justify-center items-center gap-4 mt-8 sticky bottom-0">
      <Button
        class="!w-[550px] !rounded-xl !bg-primary py-3 !border-none text-white"
        type="submit"
        :loading="props.loading"
        :unstyled="false"
      >
        {{ $t('words.save') }}
      </Button>

      <Button
        class="w-[80px] !rounded-xl !bg-transparent py-3 !border-none hover:!text-primary !text-primary"
        :unstyled="false"
        :loading="props.loading"
        @click="navigateToCustomerList"
      >
        {{ $t('words.cancel') }}
      </Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { InputText, DatePicker, Divider, Button, Message } from 'primevue'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import { useRouter } from 'vue-router'
import type { Emits, Props } from './CustomerForm.types.ts'
import type { CustomerForm } from '@/modules/Customer/types/model'
import type { CustomerCreatePayload } from '@/modules/Customer/types/api'
import useAuthStore from '@/modules/Auth/store'
import { REGEX } from '@/modules/Core/constants'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object().shape({
  fullName: string().required(t('customer.form.required')),
  motherName: string().required(t('customer.form.required')),
  nationalCode: string()
    .required(t('customer.form.required'))
    .matches(REGEX.ONLY_NUMBERS, t('customer.form.nationalCode.match')),
  birthDate: string().required(t('customer.form.required')),
  phoneNumber: string()
    .required(t('customer.form.required'))
    .matches(REGEX.PHONE_11_DIGIT_REGEX, t('customer.form.phoneNumber.match')),
  whatsAppPhoneNumber: string()
    .required(t('customer.form.required'))
    .matches(
      REGEX.PHONE_11_DIGIT_REGEX,
      t('customer.form.whatsAppPhoneNumber.match')
    ),
  business: object().shape({
    name: string().required(t('customer.form.required')),
    address: string().required(t('customer.form.required')),
    nearestKnownLocation: string().required(t('customer.form.required')),
  }),
})

const { defineField, handleSubmit, errors, setValues } = useForm<CustomerForm>({
  validationSchema,
})

const [fullName] = defineField('fullName')
const [motherName] = defineField('motherName')
const [nationalCode] = defineField('nationalCode')
const [birthDateField] = defineField('birthDate')
const [phoneNumber] = defineField('phoneNumber')
const [whatsAppPhoneNumber] = defineField('whatsAppPhoneNumber')
const [businessName] = defineField('business.name')
const [address] = defineField('business.address')
const [nearestKnownLocation] = defineField('business.nearestKnownLocation')

const birthDate = computed({
  get: () => {
    if (!birthDateField.value) return null
    return new Date(birthDateField.value)
  },
  set: (val: Date | null) => {
    if (!val) {
      birthDateField.value = null
      return
    }
    const year = val.getFullYear()
    const month = String(val.getMonth() + 1).padStart(2, '0')
    const day = String(val.getDate()).padStart(2, '0')
    birthDateField.value = `${year}-${month}-${day}`
  },
})

const router = useRouter()

function navigateToCustomerList() {
  router.push({ name: 'CustomerMainRoute' })
}

const authStore = useAuthStore()
const submit = handleSubmit((values: CustomerForm) => {
  const options: CustomerCreatePayload = {
    fullName: values.fullName,
    motherName: values.motherName,
    nationalCode: values.nationalCode,
    birthDate: values.birthDate!,
    business: {
      name: values.business.name,
      address: values.business.address,
      nearestKnownLocation: values.business.nearestKnownLocation,
    },
    whatsAppPhoneNumber: values.whatsAppPhoneNumber,
    phoneNumber: values.phoneNumber,
    branchId: authStore.selectedBranch?.id ?? 0,
  }

  emit('submit', options)
})

watch(
  () => props.initialValues,
  () => {
    if (props.isEditMode && props.initialValues) {
      setValues({
        fullName: props.initialValues.fullName,
        motherName: props.initialValues.motherName,
        nationalCode: props.initialValues.nationalCode,
        birthDate: props.initialValues.birthDate,
        phoneNumber: props.initialValues.phoneNumber,
        whatsAppPhoneNumber: props.initialValues.whatsAppPhoneNumber,
        business: {
          name: props.initialValues.business.name,
          address: props.initialValues.business.address,
          nearestKnownLocation:
            props.initialValues.business.nearestKnownLocation,
        },
      })
    }
  },
  { immediate: true }
)
</script>

<style>
.p-datepicker-input {
  font-size: 14px !important;
  padding: 16px !important;
}
</style>
