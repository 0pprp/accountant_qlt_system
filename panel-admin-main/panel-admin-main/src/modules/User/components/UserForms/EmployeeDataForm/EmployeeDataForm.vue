<template>
  <div class="grid grid-cols-12 gap-6 px-6">
    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('user.userCreateSteps.employeeData.jobInfoTitle') }}
    </span>

    <div class="col-span-12">
      <SelectField
        v-model="roleId"
        :options="roleOptions"
        placeholder="user.userForm.roleId.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.roleId.label"
        :loading="isFetching"
        :error="errors['roleId']"
      />
    </div>

    <div class="col-span-12">
      <BranchSelectInput
        v-model="branchIds"
        required
        :readonly="userFormMode === FormMode.IsView"
        multiple
        label="user.userForm.branchIds.label"
        placeholder="user.userForm.branchIds.placeholder"
        :error="errors['branchIds']"
      />
    </div>

    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('user.userCreateSteps.employeeData.personnelInfoTitle') }}
    </span>

    <div class="col-span-6">
      <TextField
        v-model="fullName"
        placeholder="user.userForm.fullName.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.fullName.label"
        :error="errors['fullName']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="motherName"
        placeholder="user.userForm.motherName.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.motherName.label"
        :error="errors['motherName']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="nationalCode"
        placeholder="user.userForm.nationalCode.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.nationalCode.label"
        :error="errors['nationalCode']"
      />
    </div>

    <div class="col-span-6">
      <TextField
        v-model="phoneNumber"
        placeholder="user.userForm.phoneNumber.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.phoneNumber.label"
        :error="errors['phoneNumber']"
      />
    </div>

    <div class="col-span-6">
      <CalendarField
        v-model="birthDate"
        placeholder="user.userForm.birthDate.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.birthDate.label"
        :error="errors['birthDate']"
      />
    </div>

    <div class="col-span-12">
      <TextareaField
        v-model="address"
        placeholder="user.userForm.address.placeholder"
        required
        :rows="3"
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.address.label"
        :error="errors['address']"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { computed } from 'vue'
import { array, number, object, string } from 'yup'
import type { Props } from './EmployeeDataForm.types'
import { FormMode } from '../../UserTabs/UserTabs.types'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import type { EmployeeDataFormTypes } from '@/modules/User/types/model'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'
import { useRoleDataQuery } from '@/modules/Role/requests/queries'
import { REGEX } from '@/modules/Core/constants'
import CalendarField from '@/modules/Core/components/base/Fields/CalendarField/CalendarField.vue'
import BranchSelectInput from '@/modules/Core/components/shared/BranchSelectInput/BranchSelectInput.vue'
import TextareaField from '@/modules/Core/components/base/Fields/TextareaField/TextareaField.vue'

const props = defineProps<Props>()
const { t } = useI18n()

const validationSchema = object({
  fullName: string().required(t('user.userForm.fullName.validation.required')),
  motherName: string().required(
    t('user.userForm.motherName.validation.required')
  ),
  nationalCode: string()
    .required(t('user.userForm.nationalCode.validation.required'))
    .matches(
      REGEX.ONLY_NUMBERS,
      t('user.userForm.nationalCode.validation.valid')
    ),
  birthDate: string().required(
    t('user.userForm.birthDate.validation.required')
  ),
  roleId: string().required(t('user.userForm.roleId.validation.required')),
  branchIds: array()
    .of(number().required(t('user.userForm.branchIds.validation.required')))
    .required(t('user.userForm.branchIds.validation.required')),

  address: string().required(t('user.userForm.address.validation.required')),
  phoneNumber: string()
    .required(t('user.userForm.phoneNumber.validation.required'))
    .matches(
      REGEX.PHONE_REGEX,
      t('user.userForm.phoneNumber.validation.valid')
    ),
})

const { defineField, errors, values } = useForm<EmployeeDataFormTypes>({
  validationSchema,
  initialValues: {
    fullName: props.fullName,
    motherName: props.motherName,
    nationalCode: props.nationalCode,
    birthDate: props.birthDate,
    roleId: props.roleId,
    branchIds: props.branchIds,
    address: props.address ?? '',
    phoneNumber: props.phoneNumber,
  },
})
const formValues = computed(() => values)
const hasError = computed(
  () =>
    Object.keys(errors.value).length > 0 ||
    fullName.value === undefined ||
    motherName.value === undefined ||
    nationalCode.value === undefined ||
    birthDate.value === undefined ||
    roleId.value === undefined ||
    branchIds.value === undefined ||
    address.value === undefined ||
    !address.value?.trim() ||
    phoneNumber.value === undefined
)

const [fullName] = defineField('fullName')
const [motherName] = defineField('motherName')
const [nationalCode] = defineField('nationalCode')
const [birthDate] = defineField('birthDate')
const [roleId] = defineField('roleId')
const [branchIds] = defineField('branchIds')
const [address] = defineField('address')
const [phoneNumber] = defineField('phoneNumber')

const { data: roleData, isFetching } = useRoleDataQuery()

const roleOptions = computed(() => {
  return (
    roleData.value?.map((role) => ({
      value: role.id,
      label: role.displayName,
    })) || []
  )
})

defineExpose({ formValues, hasError })
</script>
