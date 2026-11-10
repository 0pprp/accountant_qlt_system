<template>
  <div class="sr-list" role="list">
    <p v-if="error" class="sr-msg error">
      {{ $t('salesRequests.loadError') }}
    </p>
    <p v-else-if="!items.length && !loading" class="sr-msg">
      {{
        emptyHintKey
          ? $t(emptyHintKey)
          : $t('salesRequests.empty')
      }}
    </p>
    <RequestCard
      v-for="item in items"
      :key="item.id"
      role="listitem"
      :request="item"
      :selected="selectedId === item.id"
      @select="$emit('select', item)"
    >
      <template #matching><slot name="matching" :item="item" /></template>
      <template #actions><slot name="actions" :item="item" /></template>
    </RequestCard>
  </div>
</template>

<script setup lang="ts">
import type { SalesRequestListItem } from '../types'
import RequestCard from './RequestCard.vue'

defineProps<{
  items: SalesRequestListItem[]
  selectedId: number | null
  loading?: boolean
  error?: boolean
  /** مفتاح i18n اختياري (مثل المرسلة بلا ingestion) */
  emptyHintKey?: string
}>()

defineEmits<{
  select: [SalesRequestListItem]
}>()
</script>

<style scoped>
.sr-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.sr-msg {
  opacity: 0.75;
  font-size: 0.95rem;
}
.sr-msg.error {
  color: #9b1c1c;
  opacity: 1;
}
</style>
