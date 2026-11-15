<template>
  <section class="q-panel" :class="flush ? 'q-panel--flush' : undefined">
    <header v-if="title || $slots.header" class="q-panel__head">
      <h2 v-if="title" class="q-panel__title">{{ title }}</h2>
      <slot name="header" />
    </header>
    <div class="q-panel__body">
      <slot />
    </div>
  </section>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    title?: string
    flush?: boolean
  }>(),
  { flush: false },
)
</script>

<style scoped>
.q-panel__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;
}
.q-panel__title {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #006a64;
}
.q-panel--flush {
  padding: 0;
  overflow: hidden;
}
.q-panel--flush .q-panel__head {
  padding: 16px 20px 0;
}
.q-panel--flush .q-panel__body {
  padding: 0;
}
</style>
