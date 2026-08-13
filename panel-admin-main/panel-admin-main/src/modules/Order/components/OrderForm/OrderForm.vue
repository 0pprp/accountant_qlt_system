<template>
  <form novalidate @submit="onSubmit" class="flex-col justify-between mt-10">
    <div class="grid grid-cols-12 gap-4">
      <div class="col-span-12">
        <TextField
          v-model="name"
          placeholder="order.form.name.placeholder"
          required
          label="order.form.name.label"
          :error="errors['name']"
        />
      </div>

      <div class="col-span-12 md:col-span-4">
        <UsersSelectInput
          v-model="motabaId"
          :branchId
          required
          :roleId="motabaRoleId"
          label="order.form.motabaId.label"
          placeholder="order.form.motabaId.placeholder"
          :error="errors['motabaId']"
          :loading="isLoading"
        />
      </div>

      <div class="col-span-12 md:col-span-4">
        <UsersSelectInput
          v-model="mandobId"
          :branchId
          required
          :roleId="mandobRoleId"
          label="order.form.mandobId.label"
          placeholder="order.form.mandobId.placeholder"
          :error="errors['mandobId']"
          :loading="isLoading"
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
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import type { Emits, Props } from './OrderForm.types'
import type { OrderForm } from '../../types/model'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import { useRoleDataQuery } from '@/modules/Role/requests/queries'
import UsersSelectInput from '@/modules/Core/components/shared/UsersSelectInput/UsersSelectInput.vue'
import useAuthStore from '@/modules/Auth/store'

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object({
  name: string().required(t('order.form.name.validation.required')),
  mandobId: string().required(t('order.form.mandobId.validation.required')),
  motabaId: string().required(t('order.form.motabaId.validation.required')),
})

const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const { defineField, handleSubmit, errors } = useForm<OrderForm>({
  validationSchema,
  initialValues: {
    name: props.selectedItem?.name,
    mandobId: props.selectedItem?.mandob.id,
    motabaId: props.selectedItem?.motaba.id,
  },
})

const [name] = defineField('name')
const [mandobId] = defineField('mandobId')
const [motabaId] = defineField('motabaId')

const onSubmit = handleSubmit((values: OrderForm) => {
  emit('submit', { ...values, branchId: branchId.value })
})

const mandobRoleId = ref<Array<number>>([])
const motabaRoleId = ref<Array<number>>([])

const { data: roleData, isLoading } = useRoleDataQuery()
watch(
  () => roleData.value,
  () => {
    const mandobId = roleData.value?.find((role) => role.name === 'Mandob')?.id
    if (mandobId !== undefined) {
      mandobRoleId.value.push(mandobId)
    }

    const motabaId = roleData.value?.find((role) => role.name === 'Motaba')?.id
    if (motabaId !== undefined) {
      motabaRoleId.value.push(motabaId)
    }
  },
  { immediate: true }
)
</script>
