<template>
  <form
    id="login-form"
    novalidate
    class="login-form"
    @submit="onSubmit"
  >
    <AppInput
      v-model="username"
      label="أسم المستخدم"
      name="username"
      autocomplete="username"
      placeholder="يرجى أدخال اسم المستخدم"
      :error="errors['username']"
      required
    />

    <AppInput
      v-model="password"
      label="الرمز السري"
      name="password"
      type="password"
      autocomplete="current-password"
      placeholder="يرجى أدخال الرمز السري للمستخدم"
      :error="errors['password']"
      required
    />
  </form>
</template>

<script lang="ts" setup>
import { useForm } from 'vee-validate'
import { object, string } from 'yup'
import type { AuthLoginForm } from '../../types/model'
import type { Emits, Props } from './LoginForm.types'
import { AppInput } from '@/modules/Core/components/ui'

defineProps<Props>()
const emit = defineEmits<Emits>()

const validationSchema = object({
  username: string().required('يرجى إدخال اسم المستخدم'),
  password: string().required('يرجى إدخال الرمز السري'),
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

<style scoped>
.login-form {
  display: grid;
  gap: 24px;
}
</style>
