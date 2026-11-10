<script setup lang="ts">
defineProps<{
  branches: Array<{ id: number; name: string }>
  modelValue: number | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: number | null]
}>()
</script>

<template>
  <label class="fw-filter">
    <span class="fw-filter__label">المحافظة</span>
    <select
      class="fw-filter__select"
      :value="modelValue ?? ''"
      @change="
        emit(
          'update:modelValue',
          ($event.target as HTMLSelectElement).value
            ? Number(($event.target as HTMLSelectElement).value)
            : null
        )
      "
    >
      <option value="" disabled>اختر المحافظة</option>
      <option v-for="b in branches" :key="b.id" :value="b.id">
        {{ b.name }}
      </option>
    </select>
  </label>
</template>

<style scoped>
.fw-filter {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  min-width: 12rem;
}
.fw-filter__label {
  font-size: 14px;
  color: #00201e;
}
.fw-filter__select {
  min-height: 44px;
  padding: 0 0.75rem;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  background: #f8f8f8;
  font-family: inherit;
  font-size: 16px;
  color: #00201e;
}
</style>
