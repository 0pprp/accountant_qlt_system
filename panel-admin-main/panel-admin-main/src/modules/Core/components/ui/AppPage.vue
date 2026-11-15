<template>
  <div class="q-page" dir="rtl">
    <PageHeader v-if="title" :title="title" :subtitle="subtitle">
      <template v-if="$slots.actions" #actions>
        <slot name="actions" />
      </template>
    </PageHeader>
    <slot name="filters" />
    <slot />
  </div>
</template>

<script setup lang="ts">
import PageHeader from './PageHeader.vue'

defineProps<{
  title?: string
  subtitle?: string
}>()
</script>
