<template>
  <section class="sr-tl" aria-label="سجل الحركة">
    <h4 class="sr-tl-title">{{ $t('salesRequests.timeline.title') }}</h4>
    <p v-if="!canRead" class="sr-tl-msg">
      {{ $t('salesRequests.timeline.noPermission') }}
    </p>
    <p v-else-if="loading" class="sr-tl-msg">
      {{ $t('salesRequests.timeline.loading') }}
    </p>
    <p v-else-if="error" class="sr-tl-msg error">
      {{ $t('salesRequests.timeline.error') }}
    </p>
    <p v-else-if="!items.length" class="sr-tl-msg">
      {{ $t('salesRequests.timeline.empty') }}
    </p>
    <ol v-else class="sr-tl-list">
      <li
        v-for="ev in items"
        :key="ev.id"
        class="sr-tl-item"
      >
        <div class="sr-tl-head">
          <strong>{{ ev.eventLabel }}</strong>
          <time>{{ formatDate(ev.occurredAtUtc) }}</time>
        </div>
        <p class="sr-tl-actor">
          {{ ev.actorName || `#${ev.actorUserId}` }}
        </p>
        <p
          v-if="ev.previousStatusLabel || ev.newStatusLabel"
          class="sr-tl-status"
        >
          {{ ev.previousStatusLabel || '—' }} → {{ ev.newStatusLabel || '—' }}
        </p>
        <p
          v-if="ev.reason"
          class="sr-tl-reason"
        >
          {{ ev.reason }}
        </p>
      </li>
    </ol>
  </section>
</template>

<script setup lang="ts">
import type { SalesRequestTimelineItem } from '../types'

defineProps<{
  items: SalesRequestTimelineItem[]
  loading?: boolean
  error?: boolean
  canRead?: boolean
}>()

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}
</script>

<style scoped>
.sr-tl-title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}
.sr-tl-msg {
  font-size: 0.85rem;
  opacity: 0.75;
}
.sr-tl-msg.error {
  color: #9b1c1c;
  opacity: 1;
}
.sr-tl-list {
  list-style: none;
  margin: 0;
  padding: 0;
  border-inline-start: 2px solid #d7e2e0;
}
.sr-tl-item {
  position: relative;
  padding: 0.55rem 0 0.55rem 0.85rem;
}
.sr-tl-item::before {
  content: '';
  position: absolute;
  inset-inline-start: -5px;
  top: 0.85rem;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #006a64;
}
.sr-tl-head {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 0.35rem;
  font-size: 0.9rem;
}
.sr-tl-head time {
  font-size: 0.75rem;
  opacity: 0.7;
}
.sr-tl-actor,
.sr-tl-status,
.sr-tl-reason {
  margin: 0.2rem 0 0;
  font-size: 0.8rem;
  opacity: 0.85;
}
</style>
