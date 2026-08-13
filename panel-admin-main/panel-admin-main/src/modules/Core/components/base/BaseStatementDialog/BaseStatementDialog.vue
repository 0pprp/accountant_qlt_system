<template>
  <Dialog
    v-if="toastStore.messages"
    class="w-[250px] flex flex-col justify-between bg-white rounded-3xl p-8"
    unstyled
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
    <div
      class="w-16 h-16 rounded-full flex items-center justify-center mx-auto"
      :class="{
        'bg-primary':
          toastStore.messages.dialogState === DialogState.Success ||
          toastStore.messages.dialogState === DialogState.Loading,
        'bg-error': toastStore.messages.dialogState === DialogState.Error,
      }"
    >
      <SvgIcon :name="iconName" class="text-white" />
    </div>

    <div class="text-center mt-12 font-bold">
      <h2 class="text-gray-900">
        {{
          toastStore.messages.dialogState === DialogState.Error
            ? toastStore.messages.title
            : $t(toastStore.messages.title)
        }}
      </h2>

      <p
        v-if="toastStore.messages.description"
        class="text-[10px] text-gray-700 mt-1 font-medium"
      >
        {{ $t(toastStore.messages.description) }}
      </p>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import { computed, watch, ref, onUnmounted } from 'vue'
import { DialogState } from '@/modules/Core/types/model/dialog'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { useToastStore } from '@/modules/Core/store'

const toastStore = useToastStore()
const timeoutId = ref<number | null>(null)

const isOpen = computed(() => {
  return toastStore.messages ? toastStore.messages.isOpen : false
})

function clearExistingTimeout() {
  if (timeoutId.value) {
    clearTimeout(timeoutId.value)
    timeoutId.value = null
  }
}

watch(
  () => toastStore.messages,
  (newMessages) => {
    clearExistingTimeout()

    if (newMessages) {
      if (
        newMessages.dialogState === DialogState.Error ||
        newMessages.dialogState === DialogState.Success
      ) {
        timeoutId.value = setTimeout(() => {
          toastStore.clearToast()
          timeoutId.value = null
        }, 3000)
      }
    }
  },
  { immediate: true }
)

onUnmounted(() => {
  clearExistingTimeout()
})

const iconName = computed(() => {
  return toastStore.messages?.dialogState === DialogState.Error
    ? 'close'
    : toastStore.messages?.dialogState === DialogState.Success
      ? 'check'
      : 'loading'
})
</script>
