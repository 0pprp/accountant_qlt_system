<template>
  <header class="q-page-header">
    <div class="q-page-header__text">
      <h1 class="q-page-header__title">{{ title }}</h1>
      <p v-if="subtitle" class="q-page-header__sub">{{ subtitle }}</p>
    </div>
    <div v-if="$slots.actions" class="q-page-header__actions">
      <slot name="actions" />
    </div>
  </header>
</template>

<script setup lang="ts">
defineProps<{ title: string; subtitle?: string }>()
</script>

<style scoped>
.q-page-header {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}
.q-page-header__title {
  margin: 0;
  font-size: 26px;
  line-height: 1.4;
  font-weight: 700;
  color: #006a64;
}
.q-page-header__sub {
  margin: 6px 0 0;
  font-size: 16px;
  color: #00201e99;
  max-width: 48rem;
}
.q-page-header__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
</style>
