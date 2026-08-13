<template>
  <form novalidate @submit="onSubmit" class="flex-col justify-between mt-10">
    <div class="grid grid-cols-12 gap-4">
      <div class="col-span-12">
        <TextField
          v-model="name"
          placeholder="branch.form.name.placeholder"
          required
          label="branch.form.name.label"
          :error="errors['name']"
        />
      </div>

      <div class="col-span-12">
        <ProvinceSelectInput
          v-model="provinceId"
          required
          label="branch.form.provinceId.label"
          placeholder="branch.form.provinceId.placeholder"
          :error="errors['provinceId']"
        />
      </div>
    </div>

    <div class="pt-10 w-full flex flex-row gap-4 justify-center">
      <Button
        :disabled="Object.keys(errors).length > 0 || loading"
        class="btn-primary py-4 w-1/2"
        type="submit"
      >
        {{ $t('words.save') }}

        <i v-if="loading" class="pi pi-spin pi-spinner"></i>
      </Button>
    </div>
  </form>
</template>

<script lang="ts" setup>
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import type { Emits, Props } from './BranchForm.types'
import type { BranchForm } from '../../types/model'
import ProvinceSelectInput from '@/modules/Core/components/shared/ProvinceSelectInput/ProvinceSelectInput.vue'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object({
  name: string().required(t('branch.form.name.validation.required')),
  provinceId: string().required(
    t('branch.form.provinceId.validation.required')
  ),
})

const { defineField, handleSubmit, errors } = useForm<BranchForm>({
  validationSchema,
  initialValues: {
    name: props.selectedItem?.name,
    provinceId: props.selectedItem?.province.id,
  },
})

const [name] = defineField('name')
const [provinceId] = defineField('provinceId')

const onSubmit = handleSubmit((values: BranchForm) => {
  emit('submit', values)
})
</script>
