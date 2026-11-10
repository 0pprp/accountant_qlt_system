<template>
  <span class="q-status-badge" :data-tone="tone">{{ label }}</span>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    label: string
    tone?: 'neutral' | 'warning' | 'danger' | 'success' | 'info'
  }>(),
  { tone: 'neutral' },
)
</script>

<style scoped>
.q-status-badge {
  display: inline-flex;
  align-items: center;
  min-height: 32px;
  padding: 7px 13px;
  border-radius: 9px;
  background: #e9f3f0;
  color: #145c4c;
  font-size: 14px;
  line-height: 1.5;
  white-space: normal;
}
.q-status-badge[data-tone='warning'] {
  background: #fff3d9;
  color: #79531a;
}
.q-status-badge[data-tone='danger'] {
  background: #fcebea;
  color: #963d35;
}
.q-status-badge[data-tone='success'] {
  background: #e5f6ea;
  color: #1b5e20;
}
.q-status-badge[data-tone='info'] {
  background: #e7f1fb;
  color: #1565c0;
}
</style>
