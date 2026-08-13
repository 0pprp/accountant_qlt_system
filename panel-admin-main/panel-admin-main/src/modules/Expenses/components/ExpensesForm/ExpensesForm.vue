<template>
  <form @submit.prevent="submit">
    <div
      class="grid flex-wrap grid-cols-12 gap-4 align-middle items-center pt-8"
    >
      <div class="col-span-12 md:col-span-4 lg:col-span-5">
        <NumberField
          v-model="factorNumber"
          readonly
          :loading="lastFactorLoading"
          placeholder="expenses.form.factorNumber.placeholder"
          label="expenses.form.factorNumber.label"
        />
      </div>

      <div class="col-span-12 md:col-span-5">
        <SelectField
          v-model="safeType"
          :options="safeTypeOptions"
          placeholder="expenses.form.safeType.placeholder"
          required
          :readonly="
            formState === FormsState.View || formState === FormsState.Update
          "
          label="expenses.form.safeType.label"
          :error="errors['safeType']"
        />
      </div>

      <div class="col-span-12 md:col-span-3 lg:col-span-2 pt-4">
        <IconFileUploader
          :file="file"
          :disabled="props.formState !== FormsState.Create"
          @upload-file="(values) => (attachments = values[0])"
        />
      </div>
    </div>
    <hr class="border-line border my-5" />

    <div>
      <ExpenseItemsForm
        :expenseItems="expenseItems"
        :formState
        @update="handleExpenseItemsUpdate"
      />
    </div>

    <div class="flex justify-center items-center gap-4 mt-8 sticky bottom-0">
      <Button
        class="w-100 !rounded-xl !bg-primary py-3 !border-none text-white"
        type="submit"
        :loading="props.loading"
        :unstyled="false"
        :disabled="formState === FormsState.View"
      >
        {{ $t('words.save') }}
      </Button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Button } from 'primevue'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { object, string, array } from 'yup'
import {
  SafeType,
  type ExpenseAttachment,
  type ExpenseItemModel,
  type ExpensesForm,
} from '../../types/model/index.ts'
import type { Emits, Props } from './ExpensesForm.types.ts'
import { useLastFactorNumberDataQuery } from '../../requests/queries/useLastFactorNumberQuery.ts'
import ExpenseItemsForm from './ProductItems/ExpenseItemsForm.vue'
import useAuthStore from '@/modules/Auth/store'
import { FormsState } from '@/modules/Core/types/model/forms.ts'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'
import IconFileUploader from '@/modules/Core/components/shared/IconFileUploader/IconFileUploader.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object().shape({
  safeType: string().required(t('expenses.form.safeType.validation.required')),
  expenseItems: array().min(
    1,
    t('expenses.form.expenseItems.validation.required')
  ),
})

const { defineField, handleSubmit, errors, setFieldValue } =
  useForm<ExpensesForm>({
    validationSchema,
  })

const [factorNumber] = defineField('factorNumber')
const [safeType] = defineField('safeType')
const [attachments] = defineField('attachments')
const [expenseItems] = defineField('expenseItems')

function handleExpenseItemsUpdate(items: ExpenseItemModel[]) {
  setFieldValue('expenseItems', items)
}

const safeTypeOptions = computed(() => {
  return [
    { value: SafeType.Branch, label: t('expenses.safeType.branch') },
    { value: SafeType.Main, label: t('expenses.safeType.main') },
  ]
})

const authStore = useAuthStore()

const submit = handleSubmit((values: ExpensesForm) => {
  const updatedValues = {
    ...values,
    branchId: authStore.selectedBranch?.id,
  }

  emit('submit', updatedValues)
})

const file = ref<ExpenseAttachment[] | undefined>(undefined)
watch(
  () => props.initialValues,
  () => {
    if (
      props.formState === FormsState.Update ||
      props.formState === FormsState.View
    ) {
      setFieldValue('factorNumber', Number(props.initialValues!.factorNumber))
      setFieldValue('expenseItems', props.initialValues!.expenseItems)
      setFieldValue('safeType', props.initialValues!.safeType)
      file.value =
        props.initialValues && props.initialValues?.attachments.length > 0
          ? props.initialValues?.attachments
          : undefined
    }
  },
  { immediate: true }
)

const newFactor = ref(0)

const { data: lastFactor, isLoading: lastFactorLoading } =
  useLastFactorNumberDataQuery({
    enabled: props.formState === FormsState.Create,
  })

watch(
  [() => props.formState, lastFactor],
  ([formState, lastFactorData]) => {
    if (formState === FormsState.Create && lastFactorData) {
      newFactor.value = lastFactorData.lastFactorNumber + 1
      setFieldValue('factorNumber', newFactor.value)
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
