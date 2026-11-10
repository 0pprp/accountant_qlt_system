<template>
  <div class="q-status-tabs" role="tablist" :aria-label="ariaLabel">
    <button
      v-for="tab in tabs"
      :key="tab.id"
      type="button"
      role="tab"
      class="q-status-tabs__tab"
      :class="{ active: tab.id === modelValue }"
      :aria-selected="tab.id === modelValue"
      :disabled="tab.disabled"
      @click="$emit('update:modelValue', tab.id)"
    >
      <span>{{ tab.label }}</span>
      <span v-if="tab.count != null" class="q-status-tabs__count">{{ tab.count }}</span>
    </button>
  </div>
</template>

<script setup lang="ts">
export type StatusTab = { id: string; label: string; count?: number | null; disabled?: boolean }

defineProps<{
  tabs: StatusTab[]
  modelValue: string
  ariaLabel?: string
}>()

defineEmits<{ 'update:modelValue': [id: string] }>()
</script>

<style scoped>
.q-status-tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}
.q-status-tabs__tab {
  min-height: 44px;
  padding: 8px 14px;
  border-radius: 12px;
  border: 1px solid #d7e2e0;
  background: #fff;
  color: #00201e;
  font: inherit;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}
.q-status-tabs__tab.active {
  background: #006a64;
  border-color: #006a64;
  color: #fff;
}
.q-status-tabs__count {
  font-size: 13px;
  opacity: 0.9;
}
.q-status-tabs__tab:focus-visible {
  outline: 3px solid #006a64;
  outline-offset: 2px;
}
.q-status-tabs__tab:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
