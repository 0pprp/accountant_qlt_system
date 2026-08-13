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
          placeholder="purchases.form.factorNumber.placeholder"
          label="purchases.form.factorNumber.label"
        />
      </div>

      <div class="col-span-12 md:col-span-5">
        <SelectField
          v-model="safeType"
          :options="safeTypeOptions"
          placeholder="purchases.form.safeType.placeholder"
          required
          :readonly="
            formState === FormsState.View || formState === FormsState.Update
          "
          label="purchases.form.safeType.label"
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
      <PurchaseItemsForm
        :purchaseItems="purchaseItems"
        :formState
        @update="handlePurchaseItemsUpdate"
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
import { object, string } from 'yup'
import {
  SafeType,
  type PurchaseAttachment,
  type PurchaseItemModel,
  type PurchasesForm,
} from '../../types/model/index.ts'
import type { Emits, Props } from './PurchasesForm.types.ts'
import { useLastFactorNumberDataQuery } from '../../requests/queries/useLastFactorNumberQuery.ts'
import PurchaseItemsForm from './ProductItems/PurchaseItemsForm.vue'
import useAuthStore from '@/modules/Auth/store'
import { FormsState } from '@/modules/Core/types/model/forms.ts'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'
import IconFileUploader from '@/modules/Core/components/shared/IconFileUploader/IconFileUploader.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object().shape({
  safeType: string().required(t('purchases.form.safeType.validation.required')),
})

const localPurchaseItems = ref<PurchaseItemModel[]>([])

const { defineField, handleSubmit, errors, setFieldValue } =
  useForm<PurchasesForm>({
    validationSchema,
  })

const [factorNumber] = defineField('factorNumber')
const [safeType] = defineField('safeType')
const [attachments] = defineField('attachments')
const [purchaseItems] = defineField('purchaseItems')

function handlePurchaseItemsUpdate(items: PurchaseItemModel[]) {
  localPurchaseItems.value = items
}

const safeTypeOptions = computed(() => {
  return [
    { value: SafeType.Branch, label: t('purchases.safeType.branch') },
    { value: SafeType.Main, label: t('purchases.safeType.main') },
  ]
})

const authStore = useAuthStore()
const submit = handleSubmit((values: PurchasesForm) => {
  const updatedValues = {
    ...values,
    branchId: authStore.selectedBranch?.id,
    purchaseItems: localPurchaseItems.value,
  }

  emit('submit', updatedValues)
})
const file = ref<PurchaseAttachment[] | undefined>(undefined)
watch(
  () => props.initialValues,
  () => {
    if (
      props.formState === FormsState.Update ||
      props.formState === FormsState.View
    ) {
      setFieldValue('factorNumber', Number(props.initialValues!.factorNumber))
      setFieldValue('purchaseItems', props.initialValues!.purchaseItems)
      setFieldValue('safeType', props.initialValues!.safeType)
      file.value =
        props.initialValues && props.initialValues?.attachments.length > 0
          ? props.initialValues?.attachments
          : undefined

      if (props.initialValues?.purchaseItems) {
        localPurchaseItems.value = props.initialValues.purchaseItems
      }
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
