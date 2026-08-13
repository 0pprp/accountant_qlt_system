<template>
  <div class="grid grid-cols-12 gap-6 px-6">
    <span class="col-span-12 text-gray-700 font-medium">
      {{ $t('user.userCreateSteps.accountDetails.title') }}
    </span>

    <div class="col-span-12">
      <TextField
        v-model="userName"
        placeholder="user.userForm.userName.placeholder"
        required
        :readonly="userFormMode === FormMode.IsView"
        label="user.userForm.userName.label"
        :error="errors['userName']"
      />
    </div>

    <div v-if="userFormMode === FormMode.IsCreate" class="col-span-12">
      <PasswordField
        v-model="password"
        :error="errors['password']"
        required
        placeholder="user.userForm.password.placeholder"
        label="user.userForm.password.label"
        toggleMask
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { computed } from 'vue'
import { object, string } from 'yup'
import type { Props } from './AccountDetailsForm.types'
import { FormMode } from '../../UserTabs/UserTabs.types'
import TextField from '@/modules/Core/components/base/Fields/TextField/TextField.vue'
import type { AccountDetailsFormTypes } from '@/modules/User/types/model'
import PasswordField from '@/modules/Core/components/base/Fields/PasswordField/PasswordField.vue'

const props = defineProps<Props>()
const { t } = useI18n()

const validationSchema = object({
  userName: string().required(t('user.userForm.userName.validation.required')),
  password: string()
    .nullable()
    .required(t('user.userForm.password.validation.required'))
    .min(8, t('user.userForm.password.validation.minLength')),
})

const { defineField, errors, values } = useForm<AccountDetailsFormTypes>({
  validationSchema,
  initialValues: {
    userName: props.userName,
  },
})

const formValues = computed(() => values)

const hasError = computed(
  () =>
    Object.keys(errors.value).length > 0 ||
    userName.value === undefined ||
    (password.value === undefined && props.userFormMode === FormMode.IsCreate)
)

const [userName] = defineField('userName')
const [password] = defineField('password')

defineExpose({ formValues, hasError })
</script>
