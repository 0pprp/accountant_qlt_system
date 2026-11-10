<template>
  <div class="q-data-state" :data-kind="kind" role="status">
    <p class="q-data-state__title">{{ title }}</p>
    <p v-if="message" class="q-data-state__msg">{{ message }}</p>
    <div v-if="$slots.actions" class="q-data-state__actions">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    kind: 'loading' | 'empty' | 'error' | 'forbidden'
    title: string
    message?: string
  }>(),
  {},
)
</script>

<style scoped>
.q-data-state {
  padding: 24px 16px;
  border-radius: 16px;
  background: #f8f8f8;
  text-align: center;
  color: #00201e;
}
.q-data-state[data-kind='error'],
.q-data-state[data-kind='forbidden'] {
  background: #fcebea;
  color: #963d35;
  text-align: start;
}
.q-data-state__title {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
}
.q-data-state__msg {
  margin: 8px 0 0;
  font-size: 16px;
  line-height: 1.7;
  white-space: pre-wrap;
}
.q-data-state__actions {
  margin-top: 16px;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: center;
}
.q-data-state[data-kind='error'] .q-data-state__actions,
.q-data-state[data-kind='forbidden'] .q-data-state__actions {
  justify-content: flex-start;
}
</style>
