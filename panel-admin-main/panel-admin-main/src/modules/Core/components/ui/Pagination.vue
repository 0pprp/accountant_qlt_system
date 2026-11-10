<template>
  <nav class="q-pagination" aria-label="ترقيم الصفحات">
    <button type="button" :disabled="!hasPrev || busy" @click="$emit('prev')">السابق</button>
    <span class="q-pagination__meta">صفحة {{ page }}</span>
    <button type="button" :disabled="!hasNext || busy" @click="$emit('next')">التالي</button>
  </nav>
</template>

<script setup lang="ts">
defineProps<{
  page: number
  hasPrev?: boolean
  hasNext?: boolean
  busy?: boolean
}>()

defineEmits<{ prev: []; next: [] }>()
</script>

<style scoped>
.q-pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-top: 16px;
}
.q-pagination button {
  min-height: 44px;
  min-width: 88px;
  padding: 8px 16px;
  border-radius: 10px;
  border: 1px solid #cfdfda;
  background: #fff;
  color: #006a64;
  font: inherit;
  font-weight: 600;
  cursor: pointer;
}
.q-pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.q-pagination__meta {
  font-size: 14px;
  color: #526b67;
}
.q-pagination button:focus-visible {
  outline: 3px solid #006a64;
  outline-offset: 2px;
}
</style>
