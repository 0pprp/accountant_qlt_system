<template>
  <section
    class="access-denied flex flex-col items-center justify-center gap-3 rounded-2xl bg-white px-6 py-10 text-center shadow-sm"
    role="status"
    aria-live="polite"
  >
    <span
      class="inline-flex h-12 w-12 items-center justify-center rounded-full bg-surface-2 text-primary"
      aria-hidden="true"
    >
      <i class="pi pi-lock text-xl" />
    </span>
    <h2 class="text-lg font-bold text-[#00201E] m-0">
      {{ titleText }}
    </h2>
    <p class="text-sm text-[#00201E]/90 max-w-md m-0 leading-6">
      {{ messageText }}
    </p>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'

const props = withDefaults(
  defineProps<{
    title?: string
    message?: string
  }>(),
  {
    title: undefined,
    message: undefined,
  }
)

const { t } = useI18n()

const titleText = computed(() => props.title ?? t('access.deniedTitle'))
const messageText = computed(
  () => props.message ?? t('access.deniedMessage')
)
</script>
