<template>
  <div class="grid grid-cols-12 gap-6 px-6">
    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('user.userCreateSteps.salaryDetails.stepTitle') }}
    </span>

    <div class="col-span-12">
      <SelectField
        v-model="type"
        :options="salaryOptions"
        placeholder="user.salaryDetailsForm.type.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.salaryDetailsForm.type.label"
        :error="errors['type']"
      />
    </div>

    <div class="col-span-12">
      <NumberField
        v-model="amount"
        :suffix="$t('words.dinar')"
        required
        :readonly="userFormMode === FormMode.IsView"
        :error="errors['amount']"
        placeholder="user.salaryDetailsForm.amount.placeholder"
        label="user.salaryDetailsForm.amount.label"
      />
    </div>

    <div class="col-span-6">
      <NumberField
        v-model="installmentSharePercent"
        isPercent
        :readonly="userFormMode === FormMode.IsView"
        :required="type === SalaryType.CommissionBased"
        :disabled="type === SalaryType.Fixed"
        :error="errors['installmentSharePercent']"
        placeholder="user.salaryDetailsForm.installmentSharePercent.placeholder"
        label="user.salaryDetailsForm.installmentSharePercent.label"
      />
    </div>

    <div class="col-span-6">
      <NumberField
        v-model="saleSharePercent"
        isPercent
        :readonly="userFormMode === FormMode.IsView"
        :required="type === SalaryType.CommissionBased"
        :disabled="type === SalaryType.Fixed"
        :error="errors['saleSharePercent']"
        placeholder="user.salaryDetailsForm.saleSharePercent.placeholder"
        label="user.salaryDetailsForm.saleSharePercent.label"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { computed, watch } from 'vue'
import { number, object, string } from 'yup'
import type { Props } from './SalaryDetailsForm.types'
import { FormMode } from '../../UserTabs/UserTabs.types'
import {
  SalaryType,
  type SalaryDetailsFormTypes,
} from '@/modules/User/types/model'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'
import { REGEX } from '@/modules/Core/constants'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'

const props = defineProps<Props>()
const { t } = useI18n()

const validationSchema = object({
  type: string().required(t('user.salaryDetailsForm.type.validation.required')),
  amount: string()
    .required(t('user.salaryDetailsForm.amount.validation.required'))
    .matches(
      REGEX.ONLY_NUMBERS,
      t('user.salaryDetailsForm.amount.validation.valid')
    ),

  installmentSharePercent: number()
    .nullable()
    .test(
      'is-required',
      t('user.salaryDetailsForm.installmentSharePercent.validation.required'),
      function (value) {
        const { type } = this.parent
        return (
          type === SalaryType.Fixed || (value !== null && value !== undefined)
        )
      }
    ),
  saleSharePercent: number()
    .nullable()
    .test(
      'is-required',
      t('user.salaryDetailsForm.saleSharePercent.validation.required'),
      function (value) {
        const { type } = this.parent
        return (
          type === SalaryType.Fixed || (value !== null && value !== undefined)
        )
      }
    ),
})

const { defineField, errors, values, resetField } =
  useForm<SalaryDetailsFormTypes>({
    validationSchema,
    initialValues: {
      type: props.type,
      amount: props.amount,
      installmentSharePercent: props.installmentSharePercent,
      saleSharePercent: props.saleSharePercent,
    },
  })

const formValues = computed(() => values)
const hasError = computed(
  () =>
    Object.keys(errors.value).length > 0 ||
    type.value === undefined ||
    amount.value === undefined ||
    (installmentSharePercent.value === undefined &&
      type.value === SalaryType.CommissionBased) ||
    (saleSharePercent.value === undefined &&
      type.value === SalaryType.CommissionBased)
)

const [type] = defineField('type')
const [amount] = defineField('amount')
const [installmentSharePercent] = defineField('installmentSharePercent')
const [saleSharePercent] = defineField('saleSharePercent')

const salaryOptions = [
  {
    value: SalaryType.CommissionBased,
    label: t('user.salaryType.commissionBased'),
  },
  {
    value: SalaryType.Fixed,
    label: t('user.salaryType.fixed'),
  },
]

watch(type, () => {
  resetField('installmentSharePercent')
  resetField('saleSharePercent')
})

defineExpose({ formValues, hasError })
</script>
