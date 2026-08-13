<template>
  <Stepper v-model:value="activeStep" class="pt-5">
    <StepList>
      <Step v-slot="{ value, a11yAttrs }" asChild :value="1">
        <div
          class="flex flex-row flex-auto gap-2 justify-center"
          v-bind="a11yAttrs.root"
        >
          <button
            class="bg-transparent border-0 inline-flex flex-col gap-2"
            v-bind="a11yAttrs.header"
          >
            <span
              :class="[
                'rounded-full border-2 w-12 h-12 inline-flex items-center justify-center',
                {
                  'bg-primary text-primary-contrast border-primary':
                    value <= activeStep,
                  'border-surface-200 dark:border-surface-700':
                    value > activeStep,
                },
              ]"
            >
              <SvgIcon name="documents" class="text-white w-5.5" />
            </span>
          </button>
        </div>
      </Step>
    </StepList>
    <StepPanels>
      <StepPanel :value="1">
        <form
          novalidate
          @submit="onSubmit"
          class="flex-col justify-between mt-10"
        >
          <div class="flex flex-col gap-6 pb-10">
            <div class="flex flex-col gap-1">
              <span class="input__label">{{
                $t('warehouse.productCategoryForm.name.label')
              }}</span>

              <InputText
                v-model="name"
                :placeholder="
                  $t('warehouse.productCategoryForm.name.placeholder')
                "
                name="name"
                type="text"
              />

              <Message
                v-if="errors['name']"
                severity="error"
                class="text-error text-xs"
                variant="simple"
              >
                {{ errors['name'] }}
              </Message>
            </div>
          </div>

          <div class="w-full flex justify-center pt-10">
            <Button
              :disabled="!!errors.name || loading"
              class="btn-primary py-4 w-1/2"
              type="submit"
            >
              {{ $t('words.save') }}
              <i v-if="loading" class="pi pi-spin pi-spinner"></i>
            </Button>
          </div>
        </form>
      </StepPanel>
    </StepPanels>
  </Stepper>
</template>

<script lang="ts" setup>
import { Step, StepList, StepPanel, StepPanels, Stepper } from 'primevue'
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import type { Emits, Props } from './ProductCategoryForm.types'
import type { ProductCategoryForm } from '../../types/model'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const activeStep = ref<number | string>(1)

const validationSchema = object({
  name: string().required(
    t('warehouse.productCategoryForm.name.validation.required')
  ),
})

const { defineField, handleSubmit, errors } = useForm<ProductCategoryForm>({
  validationSchema,
  initialValues: {
    name: props.selectedProductCategory?.name,
  },
})

const [name] = defineField('name')

const onSubmit = handleSubmit((values: ProductCategoryForm) => {
  emit('submit', values)
})
</script>
