<template>
  <CustomStepper :steps v-model:modelValue="activeStep">
    <template #content>
      <form novalidate @submit="onSubmit" class="mt-10 flex flex-col gap-6">
        <div v-show="activeStep === 'category'" class="flex flex-col gap-4">
          <WarehouseSelectInput
            v-model:modelValue="warehouseId"
            :error="errors.warehouseId"
            :label="'warehouse.productForm.warehouseId.label'"
            required
            :readonly="formState === FormsState.Delete"
            :placeholder="'warehouse.productForm.warehouseId.placeholder'"
          />

          <CategorySelectInput
            v-model:modelValue="categoryId"
            canCreate
            :error="errors.categoryId"
            :label="'warehouse.productForm.categoryId.label'"
            required
            :readonly="formState === FormsState.Delete"
            :placeholder="'warehouse.productForm.categoryId.placeholder'"
          />
        </div>

        <div v-show="activeStep === 'product'" class="grid grid-cols-12 gap-4">
          <div class="col-span-12 md:col-span-6">
            <TextField
              v-model="name"
              :error="errors['name']"
              placeholder="warehouse.productForm.name.placeholder"
              required
              :readonly="formState === FormsState.Delete"
              label="warehouse.productForm.name.label"
            />
          </div>

          <div class="col-span-12 md:col-span-6">
            <NumberField
              v-model="remainingCount"
              :error="errors['remainingCount']"
              required
              :readonly="formState === FormsState.Delete"
              placeholder="warehouse.productForm.remainingCount.placeholder"
              label="warehouse.productForm.remainingCount.label"
            />
          </div>

          <div class="col-span-12 md:col-span-4">
            <NumberField
              v-model="buyAmount"
              :suffix="$t('words.dinar')"
              required
              :readonly="formState === FormsState.Delete"
              :error="errors['buyAmount']"
              placeholder="warehouse.productForm.buyAmount.placeholder"
              label="warehouse.productForm.buyAmount.label"
            />
          </div>

          <div class="col-span-12 md:col-span-4">
            <NumberField
              v-model="sellAmount"
              :suffix="$t('words.dinar')"
              required
              :readonly="formState === FormsState.Delete"
              :error="errors['sellAmount']"
              placeholder="warehouse.productForm.sellAmount.placeholder"
              label="warehouse.productForm.sellAmount.label"
            />
          </div>

          <div class="col-span-12 md:col-span-4">
            <NumberField
              v-model="dailyInstallmentAmount"
              :suffix="$t('words.dinar')"
              required
              :error="errors['dailyInstallmentAmount']"
              placeholder="warehouse.productForm.dailyInstallmentAmount.placeholder"
              label="warehouse.productForm.dailyInstallmentAmount.label"
            />
          </div>

          <div class="col-span-12">
            <TextareaField
              v-model="description"
              :suffix="$t('words.dinar')"
              :rows="3"
              :readonly="formState === FormsState.Delete"
              :error="errors['description']"
              placeholder="warehouse.productForm.description.placeholder"
              label="warehouse.productForm.description.label"
            />
          </div>
        </div>

        <div class="py-4 w-full flex flex-row gap-4 justify-center">
          <Button
            v-if="activeStep === 'product'"
            :disabled="isNextButtonDisabled || loading"
            class="py-4 w-1/4"
            :class="
              formState === FormsState.Delete ? 'btn-error' : 'btn-primary'
            "
            type="submit"
          >
            {{
              formState === FormsState.Delete
                ? $t('words.delete')
                : $t('words.save')
            }}
            <i
              v-if="formState === FormsState.Delete"
              class="pi pi-trash"
              style="font-size:"
            ></i>

            <i v-if="loading" class="pi pi-spin pi-spinner"></i>
          </Button>

          <Button
            v-if="activeStep === 'category'"
            :disabled="isNextButtonDisabled"
            class="btn-text-primary py-4 w-1/6"
            @click="nextStep()"
          >
            {{ $t('words.next') }}
            <i class="pi pi-angle-left" style="font-size:"></i>
          </Button>
        </div>
      </form>
    </template>
  </CustomStepper>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue'
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import type { Emits, Props } from './ProductForm.types'
import type { ProductForm } from '../../types/model'
import CustomStepper from '@/modules/Core/components/shared/CustomStepper/CustomStepper.vue'
import CategorySelectInput from '@/modules/Core/components/shared/CategorySelectInput/CategorySelectInput.vue'
import WarehouseSelectInput from '@/modules/Core/components/shared/WarehouseSelectInput/WarehouseSelectInput.vue'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import TextareaField from '@/modules/Core/components/base/Fields/TextareaField/TextareaField.vue'
import { FormsState } from '@/modules/Core/types/model/forms'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const activeStep = ref<number | string>('category')

const steps = ref([
  {
    title: 'الفئة و الخزينة',
    icon: 'documents',
    value: 'category',
    isLastStep: false,
  },
  {
    title: 'العنصر',
    icon: 'shopping',
    value: 'product',
    isLastStep: true,
  },
])

const validationSchema = object({
  name: string().required(t('warehouse.productForm.name.validation.required')),
  remainingCount: string().required(
    t('warehouse.productForm.remainingCount.validation.required')
  ),
  buyAmount: string().required(
    t('warehouse.productForm.buyAmount.validation.required')
  ),
  sellAmount: string().required(
    t('warehouse.productForm.sellAmount.validation.required')
  ),
  dailyInstallmentAmount: string().required(
    t('warehouse.productForm.dailyInstallmentAmount.validation.required')
  ),
  categoryId: string().required(
    t('warehouse.productForm.categoryId.validation.required')
  ),
  warehouseId: string().required(
    t('warehouse.productForm.warehouseId.validation.required')
  ),
})

const { defineField, handleSubmit, errors } = useForm<ProductForm>({
  validationSchema,
  initialValues: {
    name: props.selectedProduct?.name,
    buyAmount: props.selectedProduct?.buyAmount,
    description: props.selectedProduct?.description,
    remainingCount: props.selectedProduct?.remainingCount,
    sellAmount: props.selectedProduct?.sellAmount,
    dailyInstallmentAmount: props.selectedProduct?.dailyInstallmentAmount,
    warehouseId: props.warehouseId,
    categoryId: props.categoryId,
  },
})

const [name] = defineField('name')
const [remainingCount] = defineField('remainingCount')
const [buyAmount] = defineField('buyAmount')
const [sellAmount] = defineField('sellAmount')
const [dailyInstallmentAmount] = defineField('dailyInstallmentAmount')
const [description] = defineField('description')
const [categoryId] = defineField('categoryId')
const [warehouseId] = defineField('warehouseId')

const isNextButtonDisabled = computed(() => {
  if (activeStep.value === 'product') {
    return !(
      !errors.value.name &&
      !errors.value.remainingCount &&
      !errors.value.buyAmount &&
      !errors.value.sellAmount &&
      !errors.value.dailyInstallmentAmount
    )
  }

  if (activeStep.value === 'category') {
    const hasRequiredFields = warehouseId.value && categoryId.value
    const hasErrors = errors.value.warehouseId || errors.value.categoryId

    return !(hasRequiredFields && !hasErrors)
  }

  return false
})

const onSubmit = handleSubmit((values: ProductForm) => {
  emit('submit', values)
  nextStep()
})

function nextStep() {
  const currentIndex = steps.value.findIndex(
    (step) => step.value === activeStep.value
  )
  if (currentIndex !== -1 && currentIndex < steps.value.length - 1) {
    activeStep.value = steps.value[currentIndex + 1].value
  }
}
</script>
