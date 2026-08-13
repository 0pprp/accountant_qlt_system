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
        placeholder="safe.transferForm.amount.placeholder"
        label="safe.transferForm.amount.label"
      />

      <SafeSelectInput
        v-model="sourceSafeId"
        placeholder="safe.transferForm.sourceSafeId.placeholder"
        required
        class="w-full"
        hasForeignSafe
        :hasMainSafe="myBranchId === null ? true : false"
        :items="
          myBranchId === null
            ? [
                {
                  id: safeStore.safeId!,
                  branchId: safeStore.safeData!.branchId!,
                  name: safeStore.safeData!.name,
                },
                { id: -1, name: t('safe.foreignSafe'), branchId: null },
              ]
            : [
                {
                  id: safeStore.safeId!,
                  branchId: safeStore.safeData!.branchId!,
                  name: safeStore.safeData!.name,
                },
              ]
        "
        label="safe.transferForm.sourceSafeId.label"
        :error="errors['sourceSafeId']"
      />

      <SafeSelectInput
        v-model="destinationSafeId"
        placeholder="safe.transferForm.destinationSafeId.placeholder"
        required
        hasMainSafe
        :hasForeignSafe="false"
        :readOnly="myBranchId !== null"
        :items="
          myBranchId !== null
            ? [
                {
                  id: safeStore.safeId!,
                  branchId: safeStore.safeData!.branchId!,
                  name: safeStore.safeData!.name,
                },
              ]
            : undefined
        "
        class="w-full"
        label="safe.transferForm.destinationSafeId.label"
        :error="errors['destinationSafeId']"
      />

      <TextareaField
        v-model="description"
        :suffix="$t('words.dinar')"
        :rows="3"
        class="w-full"
        :error="errors['description']"
        placeholder="safe.transferForm.description.placeholder"
        label="safe.transferForm.description.label"
      />

      <Button
        class="w-1/2 !rounded-xl !bg-primary py-3 !border-none text-white"
        type="submit"
        :loading="props.loading"
        :disabled="!amount || !destinationSafeId"
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
import { onMounted, ref } from 'vue'
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import type { Emits, Props } from './TransferForm.types.ts'
import useSafeStore from '../../store/index.ts'
import type { CreateTransferFormPayload } from '../../types/api/index.ts'
import SafeSelectInput from './SafeSelectInput/SafeSelectInput.vue'
import type { TransferForm } from '@/modules/Safe/types/model'
import { REGEX } from '@/modules/Core/constants'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import TextareaField from '@/modules/Core/components/base/Fields/TextareaField/TextareaField.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()
const safeStore = useSafeStore()

const myBranchId = ref<number | null>(null)
myBranchId.value = safeStore.safeData!.branchId

onMounted(() => {
  sourceSafeId.value = safeStore.safeData!.id

  if (myBranchId.value !== null) {
    destinationSafeId.value = safeStore.safeId!
  }
})
const { t } = useI18n()
const validationSchema = object().shape({
  amount: string()
    .required(t('safe.transferForm.amount.validation.required'))
    .matches(
      REGEX.ONLY_NUMBERS,
      t('safe.transferForm.amount.validation.valid')
    ),
  destinationSafeId: string().required(
    t('safe.transferForm.destinationSafeId.required')
  ),
  sourceSafeId: string().nullable().optional(),
})

const { defineField, handleSubmit, errors } = useForm<TransferForm>({
  validationSchema,
})

const [amount] = defineField('amount')
const [sourceSafeId] = defineField('sourceSafeId')
const [destinationSafeId] = defineField('destinationSafeId')
const [description] = defineField('description')

const submit = handleSubmit((values: TransferForm) => {
  const options: CreateTransferFormPayload = {
    amount: values.amount,
    description: values.description,
    destinationSafeId: values.destinationSafeId,
    sourceSafeId: values.sourceSafeId === -1 ? null : values.sourceSafeId,
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
