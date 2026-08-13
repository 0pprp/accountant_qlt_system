<template>
  <div class="flex flex-row border-b border-line items-center py-2 px-3 gap-2">
    <span class="text-sm text-gray-700 shrink-0">{{ label }}</span>
    <span class="flex-1 text-gray-900 break-words text-end">{{
      value || '-'
    }}</span>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string
  value?: string | null
}>()
</script>
