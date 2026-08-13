<template>
  <section class="rounded-2xl border border-line bg-white overflow-hidden">
    <div
      class="flex items-center gap-2 !bg-surface-3 border-b border-line text-gray-900 px-4 py-3"
    >
      <SvgIcon :name="icon" class="w-5 h-5 text-primary" />
      <span class="font-medium">{{ title }}</span>
    </div>

    <div class="p-4 flex flex-col gap-2">
      <slot />
    </div>
  </section>
</template>

<script setup lang="ts">
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

defineProps<{
  title: string
  icon: string
}>()
</script>
