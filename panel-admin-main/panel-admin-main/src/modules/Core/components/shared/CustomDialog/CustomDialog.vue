<template>
  <Dialog
    class="w-[70%] !blok bg-white rounded-3xl p-8 overflow-auto dialog"
    v-model:visible="isOpen"
    modal
    :closable="false"
    :pt="{
      mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
      transition: {
        appear: true,
        name: 'fade-scale',
        appearActiveClass: 'fade-scale-enter-active fade-scale-leave-active',
        appearFromClass: 'fade-scale-enter-from fade-scale-leave-to',
        appearToClass: 'fade-scale-enter-to fade-scale-enter-to',
      },
    }"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <div class="text-gray-700">
          {{ $t(title) }}
        </div>

        <SvgIcon
          name="closeSquareIcon"
          class="text-error w-5.5 h-5.5 cursor-pointer"
          @click="toggleIsOpen"
        />
      </div>
    </template>

    <template #default>
      <slot name="content" class="h-full" />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import type { Emits, Props } from './CustomDialog.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
defineProps<Props>()
const emit = defineEmits<Emits>()

const isOpen = defineModel<boolean>('modelValue')

function toggleIsOpen() {
  emit('update:isOpen', !isOpen.value)
}
</script>

<style>
.fade-scale-enter-active,
.fade-scale-leave-active {
  transition: all 0.3s ease;
}
.fade-scale-enter-from,
.fade-scale-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
.fade-scale-enter-to,
.fade-scale-enter-to {
  opacity: 1;
  transform: scale(1);
}

.dialog {
  background-image: url(/src/assets/svg/backgrounds/background-primary-line.svg);
  background-size: cover;
}
</style>
