<template>
  <form
    class="customer-form"
    @submit.prevent="submit"
  >
    <h2 class="customer-form__section">
      {{ $t('customer.form.personalInfoTitle') }}
    </h2>

    <div class="customer-form__grid">
      <AppInput
        v-model="fullName"
        :label="$t('customer.form.fullName.label')"
        :error="errors['fullName']"
        required
      />
      <div
        v-if="isEditMode"
        class="q-field"
      >
        <span class="q-field__label">{{ $t('customer.form.rating.label') }}</span>
        <div class="customer-form__readonly">
          {{ rating || '—' }}
        </div>
      </div>
      <AppInput
        v-model="motherName"
        :label="$t('customer.form.motherName.label')"
        :error="errors['motherName']"
        required
      />
      <AppInput
        v-model="nationalCode"
        :label="$t('customer.form.nationalCode.label')"
        :error="errors['nationalCode']"
        required
      />
      <AppDateField
        v-model="birthDateField"
        :label="$t('customer.form.birthDate.label')"
        :error="errors['birthDate']"
        required
      />
    </div>

    <h2 class="customer-form__section">
      {{ $t('customer.form.businessInfoTitle') }}
    </h2>

    <div class="customer-form__grid">
      <AppInput
        v-model="businessName"
        :label="$t('customer.form.businessName.label')"
        :error="errors['business.name']"
        required
        grow
      />
      <AppInput
        v-model="whatsAppPhoneNumber"
        :label="$t('customer.form.whatsAppPhoneNumber.label')"
        :error="errors['whatsAppPhoneNumber']"
        required
      />
      <AppInput
        v-model="phoneNumber"
        :label="$t('customer.form.phoneNumber.label')"
        :error="errors['phoneNumber']"
        required
      />
      <AppInput
        v-model="address"
        :label="$t('customer.form.address.label')"
        :error="errors['business.address']"
        required
      />
      <AppInput
        v-model="nearestKnownLocation"
        :label="$t('customer.form.nearestKnownLocation.label')"
        :error="errors['business.nearestKnownLocation']"
        required
      />
    </div>

    <div class="customer-form__actions">
      <AppButton
        type="submit"
        variant="primary"
        :loading="props.loading"
      >
        {{ $t('words.save') }}
      </AppButton>
      <AppButton
        type="button"
        variant="ghost"
        :disabled="props.loading"
        @click="navigateToCustomerList"
      >
        {{ $t('words.cancel') }}
      </AppButton>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import { useRouter } from 'vue-router'
import { AppButton, AppDateField, AppInput } from '@/modules/Core/components/ui'
import type { Emits, Props } from './CustomerForm.types.ts'
import type { CustomerForm } from '@/modules/Customer/types/model'
import type { CustomerCreatePayload } from '@/modules/Customer/types/api'
import useAuthStore from '@/modules/Auth/store'
import { REGEX } from '@/modules/Core/constants'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const isEditMode = computed(() => props.isEditMode)
const rating = computed(() => props.rating ?? null)
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
      t('customer.form.whatsAppPhoneNumber.match'),
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

function toDateInput(value: string | null | undefined): string | null {
  if (!value) return value ?? null
  const match = /^(\d{4}-\d{2}-\d{2})/.exec(value)
  return match ? match[1] : value
}

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
        birthDate: toDateInput(props.initialValues.birthDate),
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
  { immediate: true },
)
</script>

<style scoped>
.customer-form {
  direction: rtl;
  display: grid;
  gap: 20px;
}
.customer-form__section {
  margin: 8px 0 0;
  font-size: 16px;
  font-weight: 700;
  color: #006a64;
}
.customer-form__grid {
  display: grid;
  gap: 16px 14px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  align-items: start;
}
.customer-form__readonly {
  min-height: 48px;
  display: flex;
  align-items: center;
  padding: 10px 14px;
  border: 1px solid #cddbd7;
  border-radius: 12px;
  background: #f8f8f8;
  font-weight: 600;
  color: #00201e;
}
.customer-form__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  position: sticky;
  bottom: 0;
  padding: 12px 0;
  background: linear-gradient(180deg, transparent, #fff 30%);
}
</style>
