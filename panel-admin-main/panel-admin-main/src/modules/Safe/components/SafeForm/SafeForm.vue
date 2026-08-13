<template>
  <form @submit.prevent="submit">
    <div
      class="flex flex-col justify-center items-center gap-4 mt-8 sticky bottom-0"
    >
      <NumberField
        v-model="amount"
        :suffix="$t('words.dinar')"
        required
        class="w-full"
        :error="errors['amount']"
        placeholder="safe.sellerCashDeliveriesForm.amount.placeholder"
        label="safe.sellerCashDeliveriesForm.amount.label"
      />

      <CalendarField
        v-model="date"
        placeholder="safe.sellerCashDeliveriesForm.date.placeholder"
        required
        class="w-full"
        label="safe.sellerCashDeliveriesForm.date.label"
        :error="errors['date']"
      />

      <TextareaField
        v-model="description"
        :suffix="$t('words.dinar')"
        :rows="3"
        class="w-full"
        :error="errors['description']"
        placeholder="safe.sellerCashDeliveriesForm.description.placeholder"
        label="safe.sellerCashDeliveriesForm.description.label"
      />

      <Button
        class="w-1/2 !rounded-xl !bg-primary py-3 !border-none text-white"
        type="submit"
        :loading="props.loading"
        :disabled="!amount || !date"
        :unstyled="false"
      >
        {{ $t('words.save') }}
      </Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { Button } from 'primevue'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import type { Emits, Props } from './SafeForm.types.ts'
import type { CreateSellerCashDeliveriesPayload } from '../../types/api/index.ts'
import type { SellerCashDeliveriesForm } from '@/modules/Safe/types/model'
import { REGEX } from '@/modules/Core/constants'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import CalendarField from '@/modules/Core/components/base/Fields/CalendarField/CalendarField.vue'
import TextareaField from '@/modules/Core/components/base/Fields/TextareaField/TextareaField.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object().shape({
  amount: string()
    .required(t('safe.sellerCashDeliveriesForm.amount.validation.required'))
    .matches(
      REGEX.ONLY_NUMBERS,
      t('safe.sellerCashDeliveriesForm.amount.validation.valid')
    ),
  date: string().required(t('safe.sellerCashDeliveriesForm.date.required')),
})

const { defineField, handleSubmit, errors } = useForm<SellerCashDeliveriesForm>(
  {
    validationSchema,
  }
)

const [amount] = defineField('amount')
const [date] = defineField('date')
const [description] = defineField('description')

const submit = handleSubmit((values: SellerCashDeliveriesForm) => {
  const options: CreateSellerCashDeliveriesPayload = {
    amount: values.amount,
    description: values.description,
    date: values.date,
  }

  emit('submit', options)
})
</script>

<style>
.p-datepicker-input {
  font-size: 14px !important;
  padding: 16px !important;
}
</style>
