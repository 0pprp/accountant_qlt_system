<template>
  <form novalidate @submit="onSubmit" class="mt-10 flex flex-col gap-6">
    <div class="grid grid-cols-12 gap-4">
      <div class="col-span-12 md:col-span-9">
        <TextField
          v-model="name"
          :error="errors['name']"
          placeholder="warehouse.productForm.name.placeholder"
          required
          :readonly="formState === FormsState.Delete"
          label="warehouse.productForm.name.label"
        />
      </div>

      <div class="col-span-12 md:col-span-3">
        <CategorySelectInput
          v-model:modelValue="categoryId"
          :error="errors.categoryId"
          :label="'warehouse.productForm.categoryId.label'"
          required
          :readonly="formState === FormsState.Delete"
          :placeholder="'warehouse.productForm.categoryId.placeholder'"
        />
      </div>

      <div class="col-span-12">
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

      <div class="col-span-12">
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

      <div class="col-span-12">
        <NumberField
          v-model="dailyInstallmentAmount"
          :suffix="$t('words.dinar')"
          required
          :error="errors['dailyInstallmentAmount']"
          placeholder="warehouse.productForm.dailyInstallmentAmount.placeholder"
          label="warehouse.productForm.dailyInstallmentAmount.label"
        />
      </div>
    </div>

    <div class="py-4 w-full flex flex-row gap-4 justify-center">
      <Button
        :disabled="loading || isFetching"
        class="py-4 w-1/4"
        :class="'btn-primary'"
        type="submit"
      >
        {{ $t('words.save') }}
        <i
          v-if="formState === FormsState.Delete"
          class="pi pi-trash"
          style="font-size:"
        ></i>

        <i v-if="loading" class="pi pi-spin pi-spinner"></i>
      </Button>
    </div>
  </form>
</template>

<script lang="ts" setup>
import { computed } from 'vue'
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import type { Emits, Props } from './ProductForm.types'
import type { ProductForm } from '../../types/model'
import CategorySelectInput from '@/modules/Core/components/shared/CategorySelectInput/CategorySelectInput.vue'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import { FormsState } from '@/modules/Core/types/model/forms'
import useAuthStore from '@/modules/Auth/store'
import { useBranchWarehousesDataQuery } from '@/modules/Branch/requests/queries'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch!.id)

const { data, isFetching } = useBranchWarehousesDataQuery(branchId)

const selectedWarehouse = computed(() => data.value?.[0])

const validationSchema = object({
  name: string().required(t('warehouse.productForm.name.validation.required')),
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
})

const { defineField, handleSubmit, errors } = useForm<ProductForm>({
  validationSchema,
  initialValues: {
    name: props.selectedProduct?.name,
    buyAmount: props.selectedProduct?.buyAmount,
    description: null,
    remainingCount: 0,
    sellAmount: props.selectedProduct?.sellAmount,
    dailyInstallmentAmount: props.selectedProduct?.dailyInstallmentAmount,
    categoryId: props.categoryId,
  },
})

const [name] = defineField('name')
const [buyAmount] = defineField('buyAmount')
const [sellAmount] = defineField('sellAmount')
const [dailyInstallmentAmount] = defineField('dailyInstallmentAmount')
const [categoryId] = defineField('categoryId')

const onSubmit = handleSubmit((values: ProductForm) => {
  const submitData = {
    ...values,
    warehouseId: selectedWarehouse.value!.id,
  }

  if (submitData.warehouseId) {
    emit('submit', submitData)
  }
})
</script>
