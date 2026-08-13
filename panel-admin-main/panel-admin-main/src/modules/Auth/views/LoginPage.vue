<template>
  <div class="flex flex-col h-full">
    <div class="my-auto">
      <LoginForm @submit="onSubmit" :loading="isPending" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { useRouter } from 'vue-router'
import LoginForm from '../components/LoginForm/LoginForm.vue'
import type { AuthLoginForm } from '../types/model'
import { useAuthLoginMutation } from '@/modules/Auth/requests/mutations'
import { useAuthStore } from '@/modules/Auth/store'

const router = useRouter()

const { mutateAsync, isPending } = useAuthLoginMutation()

const authStore = useAuthStore()

async function onSubmit(values: AuthLoginForm) {
  const { data } = await mutateAsync(values)

  authStore.setLoginState(data)
  router.push({ name: 'HomeMainRoute' })
}
</script>
