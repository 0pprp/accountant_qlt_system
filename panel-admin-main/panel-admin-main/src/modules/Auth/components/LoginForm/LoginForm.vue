<template>
  <form novalidate @submit="onSubmit" class="flex flex-col gap-10">
    <div class="flex flex-col gap-1">
      <span class="input__label required">{{
        $t('auth.form.username.label')
      }}</span>

      <InputText
        v-model="username"
        :placeholder="$t('auth.form.username.placeholder')"
        name="username"
        type="text"
      />

      <Message
        v-if="errors['username']"
        severity="error"
        class="text-error text-xs"
        variant="simple"
      >
        {{ errors['username'] }}
      </Message>
    </div>

    <div class="flex flex-col gap-1">
      <PasswordField
        v-model="password"
        :error="errors['password']"
        required
        placeholder="auth.form.password.placeholder"
        label="auth.form.password.label"
        toggleMask
      />
    </div>

    <Button
      :disabled="loading"
      class="btn-primary w-1/2 mt-10 self-center h-[55px]"
      type="submit"
    >
      {{ $t('words.login') }}
      <i v-if="loading" class="pi pi-spin pi-spinner"></i>
    </Button>
  </form>
</template>

<script lang="ts" setup>
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import type { AuthLoginForm } from '../../types/model'
import type { Emits, Props } from './LoginForm.types'
import PasswordField from '@/modules/Core/components/base/Fields/PasswordField/PasswordField.vue'

defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const validationSchema = object({
  username: string().required(t('auth.form.username.validation.required')),
  password: string().required(t('auth.form.password.validation.required')),
})

const { defineField, handleSubmit, errors } = useForm<AuthLoginForm>({
  validationSchema,
})

const [username] = defineField('username')
const [password] = defineField('password')

const onSubmit = handleSubmit((values: AuthLoginForm) => {
  emit('submit', values)
})
</script>
