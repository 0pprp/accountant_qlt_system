<template>
  <Dialog
    class="w-[366px] bg-white rounded-3xl p-8 relative overflow-hidden"
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
    <div class="flex justify-between pb-8">
      <div class="text-[#6F797A] font-medium">
        {{ $t(title) }}
      </div>

      <SvgIcon :name="icon" class="w-7.5 h-7.5" :class="`text-${color}`" />
    </div>

    <div class="relative z-20">
      <slot name="content" />

      <div class="pt-4 flex justify-between gap-6 relative z-20">
        <Button
          class="rounded-2xl font-medium w-2/3 text-white border-0 shadow-2xl"
          :disabled
          :class="`bg-${color}`"
          @click="onSubmit"
        >
          {{ $t(submitText) }}

          <SvgIcon :name="icon" class="w-5.5 h-5.5" />
        </Button>

        <Button
          class="!rounded-2xl font-medium w-1/3 bg-white shadow-2xl"
          :class="`text-${color}`"
          @click="toggleIsOpen"
        >
          {{ $t('words.cancel') }}
        </Button>
      </div>
    </div>

    <Image
      :src="`/backgrounds/${color}-line.svg`"
      alt="Image"
      class="w-full h-full absolute top-0 right-0 z-10"
    />
  </Dialog>
</template>

<script setup lang="ts">
import { Button, Dialog, Image } from 'primevue'
import type { Emits, Props } from './BaseConfirmationDialog.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
defineProps<Props>()
const emit = defineEmits<Emits>()

const isOpen = defineModel<boolean>('modelValue')

function toggleIsOpen() {
  emit('update:isOpen', !isOpen.value)
}

function onSubmit() {
  emit('onSubmit')
  toggleIsOpen()
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
</style>
