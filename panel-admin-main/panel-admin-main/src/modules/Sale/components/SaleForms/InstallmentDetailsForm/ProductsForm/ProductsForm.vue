<template>
  <Form @submit="onSubmit">
    <div
      class="max-h-[400px] overflow-scroll overflow-x-hidden custom-scrollbar"
    >
      <div class="pe-2">
        <div class="flex justify-between mb-4">
          <span class="col-span-12 text-gray-700 font-medium px-3">
            {{ $t('sale.saleCreateSteps.installmentDetails.soldOutTitle') }}
          </span>

          <SvgIcon
            v-if="formState !== FormMode.IsView"
            name="addCircle"
            class="text-primary cursor-pointer"
            @click="addNewItem"
          />
        </div>

        <div>
          <div
            v-for="(item, index) in items"
            :key="item.key"
            class="grid grid-cols-12 align-start items-start border border-line rounded-2xl pe-3 pb-3 mx-3 mb-4"
          >
            <div
              class="col-span-12 md:col-span-1 text-center pt-14 text-gray-700"
            >
              {{ index + 1 }}
            </div>

            <div
              class="col-span-12 md:col-span-11 pt-4 grid grid-cols-12 gap-4 align-top items-center"
            >
              <div class="col-span-12 md:col-span-3">
                <SelectField
                  :modelValue="items[index].productType"
                  @update:model-value="
                    (value) => updateField(index, 'productType', value)
                  "
                  :options="productTypeOptions"
                  placeholder="sale.saleForm.productType.placeholder"
                  required
                  :readonly="formState === FormMode.IsView"
                  label="sale.saleForm.productType.label"
                  :error="getError(`items[${index}].productType`)"
                />
              </div>

              <div class="col-span-12 md:col-span-6">
                <AllProductSelectInput
                  v-if="items[index].productType === ProductType.Warehouse"
                  :modelValue="items[index].productId"
                  @update:model-value="
                    (value) => updateField(index, 'productId', value)
                  "
                  :canCreate="true"
                  :readonly="formState === FormMode.IsView"
                  label="purchases.form.productName.label"
                  placeholder="purchases.form.productName.placeholder"
                  :error="getError(`items[${index}].productId`)"
                  @update:amount="(amount) => handleAmountUpdate(index, amount)"
                  @update:daily-installment-amount="
                    (amount) =>
                      handleDailyInstallmentAmountUpdate(index, amount)
                  "
                  @update:sell-amount="
                    (amount) => handleSellAmountUpdate(index, amount)
                  "
                />

                <TextField
                  v-else
                  :modelValue="items[index].productName ?? undefined"
                  @update:model-value="
                    (value) => updateField(index, 'productName', value)
                  "
                  :readonly="formState === FormMode.IsView"
                  label="purchases.form.productName.label"
                  placeholder="purchases.form.productName.placeholder"
                  :error="getError(`items[${index}].productName`)"
                />
              </div>

              <div class="col-span-12 md:col-span-3">
                <NumberField
                  :modelValue="items[index].quantity"
                  @update:model-value="
                    (value) => updateField(index, 'quantity', value)
                  "
                  :readonly="formState === FormMode.IsView"
                  placeholder="purchases.form.quantity.placeholder"
                  label="purchases.form.quantity.label"
                  :error="getError(`items[${index}].quantity`)"
                />
              </div>

              <div class="col-span-6 md:col-span-3">
                <NumberField
                  :modelValue="items[index].buyAmount"
                  @update:model-value="
                    (value) => updateField(index, 'buyAmount', value)
                  "
                  :suffix="$t('words.dinar')"
                  required
                  :readonly="formState === FormMode.IsView"
                  :error="getError(`items[${index}].buyAmount`)"
                  placeholder="sale.saleForm.buyAmount.placeholder"
                  label="sale.saleForm.buyAmount.label"
                />
              </div>

              <div class="col-span-6 md:col-span-3">
                <NumberField
                  :modelValue="items[index].sellAmount"
                  @update:model-value="
                    (value) => updateField(index, 'sellAmount', value)
                  "
                  :suffix="$t('words.dinar')"
                  required
                  :readonly="formState === FormMode.IsView"
                  :error="getError(`items[${index}].sellAmount`)"
                  placeholder="sale.saleForm.sellAmount.placeholder"
                  label="sale.saleForm.sellAmount.label"
                />
              </div>

              <div class="col-span-6 md:col-span-3">
                <NumberField
                  :modelValue="items[index].prepaymentAmount"
                  @update:model-value="
                    (value) => updateField(index, 'prepaymentAmount', value)
                  "
                  :suffix="$t('words.dinar')"
                  required
                  :readonly="formState === FormMode.IsView"
                  :error="getError(`items[${index}].prepaymentAmount`)"
                  placeholder="sale.saleForm.prepaymentAmount.placeholder"
                  label="sale.saleForm.prepaymentAmount.label"
                />
              </div>

              <div class="col-span-6 md:col-span-3">
                <NumberField
                  :modelValue="items[index].dailyInstallmentAmount"
                  @update:model-value="
                    (value) =>
                      updateField(index, 'dailyInstallmentAmount', value)
                  "
                  :suffix="$t('words.dinar')"
                  required
                  :readonly="formState === FormMode.IsView"
                  :error="getError(`items[${index}].dailyInstallmentAmount`)"
                  placeholder="sale.saleForm.dailyInstallmentAmount.placeholder"
                  label="sale.saleForm.dailyInstallmentAmount.label"
                />
              </div>

              <div class="col-span-12 flex justify-end">
                <SvgIcon
                  v-if="formState !== FormMode.IsView && items.length > 1"
                  name="delete"
                  class="text-red-500 cursor-pointer"
                  @click="removeItem(index)"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Form>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useForm, Form } from 'vee-validate'
import { object, string, array, number } from 'yup'
import type { Props } from './ProductsForm.types'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import AllProductSelectInput from '@/modules/Core/components/shared/AllProductSelectInput/AllProductSelectInput.vue'
import NumberField from '@/modules/Core/components/base/Fields/NumberField/NumberField.vue'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'
import { ProductType } from '@/modules/Sale/types/model'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'

const props = defineProps<Props>()

const { t } = useI18n()

interface LocalProductItem {
  key: string
  productId: number | null
  productName: string | null
  quantity: number
  productType: ProductType
  buyAmount: number
  sellAmount: number
  prepaymentAmount: number
  dailyInstallmentAmount: number
}

type ProductItemField = keyof Omit<LocalProductItem, 'key'>

// Product type options
const productTypeOptions = computed(() => {
  return [
    { value: ProductType.Foreign, label: t('sale.productType.foreign') },
    { value: ProductType.Warehouse, label: t('sale.productType.warehouse') },
  ]
})

// Validation schema for dynamic items array
const validationSchema = object({
  items: array()
    .of(
      object({
        key: string(),
        productId: number()
          .nullable()
          .transform((value, originalValue) => {
            return originalValue === '' ||
              originalValue === null ||
              originalValue === undefined
              ? null
              : value
          })
          .when('productType', {
            is: ProductType.Warehouse,
            then: (schema) =>
              schema.required(
                t('purchases.form.productName.validation.required')
              ),
            otherwise: (schema) => schema.notRequired(),
          }),
        productName: string()
          .nullable()
          .when('productType', {
            is: ProductType.Foreign,
            then: (schema) =>
              schema.required(
                t('purchases.form.productName.validation.required')
              ),
            otherwise: (schema) => schema.notRequired(),
          }),
        quantity: number()
          .required(t('purchases.form.quantity.validation.required'))
          .min(1, t('purchases.form.quantity.validation.required')),
        productType: string().required(
          t('sale.saleForm.productType.validation.required')
        ),
        buyAmount: number()
          .required(t('sale.saleForm.buyAmount.validation.required'))
          .min(0, t('sale.saleForm.buyAmount.validation.required')),
        sellAmount: number()
          .required(t('sale.saleForm.sellAmount.validation.required'))
          .min(0, t('sale.saleForm.sellAmount.validation.required')),
        prepaymentAmount: number()
          .required(t('sale.saleForm.prepaymentAmount.validation.required'))
          .min(0, t('sale.saleForm.prepaymentAmount.validation.required')),
        dailyInstallmentAmount: number()
          .required(
            t('sale.saleForm.dailyInstallmentAmount.validation.required')
          )
          .min(
            0,
            t('sale.saleForm.dailyInstallmentAmount.validation.required')
          ),
      })
    )
    .min(1, t('purchases.form.productName.validation.required')),
})

function getInitialItems(): LocalProductItem[] {
  if (props.orderItems && props.orderItems.length > 0) {
    return props.orderItems.map((item, index) => ({
      key: `item-${Date.now()}-${index}`,
      productId: item.productId,
      productName: item.productName,
      quantity: item.quantity || 1,
      productType: (item.productType as ProductType) || ProductType.Warehouse,
      buyAmount: item.buyAmount || 0,
      sellAmount: item.sellAmount || 0,
      prepaymentAmount: item.prepaymentAmount || 0,
      dailyInstallmentAmount: item.dailyInstallmentAmount || 0,
    }))
  }

  return [
    {
      key: `item-${Date.now()}`,
      productId: null,
      productName: null,
      quantity: 1,
      productType: ProductType.Warehouse,
      buyAmount: 0,
      sellAmount: 0,
      prepaymentAmount: 0,
      dailyInstallmentAmount: 0,
    },
  ]
}

const { errors, values, validate, setFieldValue, resetForm } = useForm<{
  items: LocalProductItem[]
}>({
  validationSchema,
  initialValues: {
    items: getInitialItems(),
  },
})

// Watch for changes in orderItems prop
watch(
  () => props.orderItems,
  (newOrderItems) => {
    if (newOrderItems && newOrderItems.length > 0) {
      resetForm({
        values: {
          items: getInitialItems(),
        },
      })
    }
  },
  { deep: true, immediate: true }
)

function getError(fieldPath: string): string | undefined {
  return (errors.value as Record<string, string | undefined>)[fieldPath]
}

const items = computed(() => values.items || [])

function updateField(
  index: number,
  field: ProductItemField,
  value: string | number | ProductType | null
) {
  // Capture previous values before mutating
  const previousQuantity = values.items?.[index]?.quantity || 1
  const previousBuyAmount = values.items?.[index]?.buyAmount || 0
  const previousSellAmount = values.items?.[index]?.sellAmount || 0
  const previousDailyInstallmentAmount =
    values.items?.[index]?.dailyInstallmentAmount || 0
  const currentProductType = values.items?.[index]?.productType

  setFieldValue(`items.${index}.${field}`, value)

  if (field === 'productType') {
    if (value === ProductType.Warehouse)
      setFieldValue(`items.${index}.productName`, null)
    else setFieldValue(`items.${index}.productId`, null)

    // reset amounts only when productType changes
    setFieldValue(`items.${index}.buyAmount`, 0)
    setFieldValue(`items.${index}.sellAmount`, 0)
    setFieldValue(`items.${index}.prepaymentAmount`, 0)
    setFieldValue(`items.${index}.dailyInstallmentAmount`, 0)
  }

  if (field === 'productId') {
    // if selecting a different warehouse product, also reset amounts
    setFieldValue(`items.${index}.buyAmount`, 0)
    setFieldValue(`items.${index}.sellAmount`, 0)
    setFieldValue(`items.${index}.prepaymentAmount`, 0)
    setFieldValue(`items.${index}.dailyInstallmentAmount`, 0)
  }

  // When quantity changes for warehouse product, scale amounts by quantity
  if (field === 'quantity' && currentProductType === ProductType.Warehouse) {
    const newQuantity = Number(value) || 0
    const unitBuyAmount = previousQuantity
      ? previousBuyAmount / previousQuantity
      : previousBuyAmount
    const unitSellAmount = previousQuantity
      ? previousSellAmount / previousQuantity
      : previousSellAmount
    const unitDailyInstallmentAmount = previousQuantity
      ? previousDailyInstallmentAmount / previousQuantity
      : previousDailyInstallmentAmount

    setFieldValue(`items.${index}.buyAmount`, unitBuyAmount * newQuantity)
    setFieldValue(`items.${index}.sellAmount`, unitSellAmount * newQuantity)
    setFieldValue(
      `items.${index}.dailyInstallmentAmount`,
      unitDailyInstallmentAmount * newQuantity
    )
  }
}

// Add new item
function addNewItem() {
  const newItem: LocalProductItem = {
    key: `item-${Date.now()}`,
    productId: null,
    productName: null,
    quantity: 1,
    productType: ProductType.Warehouse,
    buyAmount: 0,
    sellAmount: 0,
    prepaymentAmount: 0,
    dailyInstallmentAmount: 0,
  }

  const currentItems = [...(values.items || [])]
  currentItems.push(newItem)
  setFieldValue('items', currentItems)
}

function removeItem(index: number) {
  const currentItems = [...(values.items || [])]
  currentItems.splice(index, 1)
  setFieldValue('items', currentItems)
}

function handleAmountUpdate(index: number, amount: number) {
  setFieldValue(`items.${index}.buyAmount`, amount)
}

function handleSellAmountUpdate(index: number, amount: number) {
  setFieldValue(`items.${index}.sellAmount`, amount)
}
function handleDailyInstallmentAmountUpdate(index: number, amount: number) {
  setFieldValue(`items.${index}.dailyInstallmentAmount`, amount)
}
const hasError = computed(() => {
  return Object.keys(errors.value).length > 0
})

async function onSubmit() {
  const result = await validate()
  if (result.valid) {
    // console.log('Form is valid', values.items)
  }
}

defineExpose({
  items: computed(() => values.items),
  hasError,
  validate,
  errors,
  values,
})
</script>

<style scoped>
.custom-scrollbar {
  direction: ltr;
  border-radius: 10px;
  overflow-y: auto;
  height: 100%;
}

.custom-scrollbar > div {
  direction: rtl;
  border-radius: 10px;
}

.custom-scrollbar {
  --sb-track-color: #cdcdcd;
  --sb-thumb-color: #6f797a;
  --sb-size: 5px;
}

.custom-scrollbar::-webkit-scrollbar {
  width: var(--sb-size);
  height: 50px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: var(--sb-track-color);
  border-radius: 10px;
  margin-top: 120px;
  margin-bottom: 120px;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: var(--sb-thumb-color);
  border-radius: 10px;
}
</style>
