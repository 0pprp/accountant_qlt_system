<template>
  <div class="border-t border-white">
    <Button
      class="flex items-center font-medium text-white gap-2"
      @click="toggleIsOpen"
    >
      <SvgIcon name="logout" class="w-7.5 h-7.5 text-white" />
      <span v-if="rail" class="font-semibold">{{ $t('words.logout') }}</span>
    </Button>

    <BaseConfirmationDialog
      v-model="isOpen"
      icon="logout"
      title="auth.exitWord"
      color="error"
      submitText="auth.exitWord"
      @update:is-open="toggleIsOpen"
      @on-submit="logout"
    >
      <template #content>
        <div class="text-gray-700 text-center">
          {{ $t('auth.exitDescription') }}
        </div>
      </template>
    </BaseConfirmationDialog>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Button } from 'primevue'
import type { Props } from './TheSideBarFooter.types'
import BaseConfirmationDialog from '@/modules/Core/components/base/BaseConfirmationDialog/BaseConfirmationDialog.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import useAuthStore from '@/modules/Auth/store'
import router from '@/router'

defineProps<Props>()

const isOpen = ref(false)

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const authStore = useAuthStore()
async function logout() {
  authStore.logout()
  toggleIsOpen()
  router.push('/auth')
}
</script>
