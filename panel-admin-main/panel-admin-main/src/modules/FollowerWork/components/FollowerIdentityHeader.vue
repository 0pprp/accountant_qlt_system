<script setup lang="ts">
defineProps<{
  name: string
  branchLabel: string
  listCount: number
}>()
</script>

<template>
  <header class="fw-id">
    <h2 class="fw-id__name">{{ name }}</h2>
    <p class="fw-id__meta">
      <span>{{ branchLabel }}</span>
      <span aria-hidden="true">·</span>
      <span>قوائم مسندة: {{ listCount }}</span>
    </p>
  </header>
</template>

<style scoped>
.fw-id__name {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 700;
  color: #00201e;
  line-height: 1.3;
}
.fw-id__meta {
  margin: 0.35rem 0 0;
  font-size: 14px;
  color: #3d524f;
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}
</style>
