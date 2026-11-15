<template>
  <div class="sr-dups">
    <p v-if="isLoading" class="sr-dups__state">
      {{ $t('salesRequests.match.loading') }}
    </p>
    <p v-else-if="isError" class="sr-dups__state sr-dups__state--err">
      {{ $t('salesRequests.match.failed') }}
    </p>
    <p v-else-if="!items.length" class="sr-dups__state">
      {{ $t('salesRequests.match.noHits') }}
    </p>
    <article
      v-for="hit in items"
      :key="hit.id"
      class="sr-dup-card"
    >
      <div class="sr-dup-card__title">طلب #{{ hit.id }}</div>
      <dl class="sr-dup-grid">
        <div>
          <dt>المصدر</dt>
          <dd>{{ hit.sourceTypeLabel }}</dd>
        </div>
        <div>
          <dt>الحالة</dt>
          <dd>{{ hit.statusLabel }}</dd>
        </div>
        <div>
          <dt>تاريخ الإضافة</dt>
          <dd>{{ formatDate(hit.createdAt) }}</dd>
        </div>
        <div>
          <dt>الموظف المسند</dt>
          <dd>{{ hit.assignedEmployeeName || '—' }}</dd>
        </div>
      </dl>
    </article>
  </div>
</template>

<script setup lang="ts">
import type { DuplicateRequestHit } from '../types'

defineProps<{
  items: DuplicateRequestHit[]
  isLoading?: boolean
  isError?: boolean
}>()

function formatDate(value?: string | null) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}
</script>

<style scoped>
.sr-dups {
  display: grid;
  gap: 0.75rem;
  color: #00201e;
}
.sr-dups__state {
  margin: 0;
  padding: 0.85rem 1rem;
  border-radius: 14px;
  background: #e8f0ef;
  box-shadow:
    inset 3px 3px 8px rgba(0, 32, 30, 0.08),
    inset -3px -3px 8px rgba(255, 255, 255, 0.85);
}
.sr-dups__state--err {
  color: #b91c1c;
}
.sr-dup-card {
  padding: 0.9rem 1rem;
  border-radius: 16px;
  background: #e8f0ef;
  box-shadow:
    7px 7px 16px rgba(0, 32, 30, 0.12),
    -7px -7px 16px rgba(255, 255, 255, 0.9);
}
.sr-dup-card__title {
  font-weight: 800;
  margin-bottom: 0.55rem;
}
.sr-dup-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.55rem 0.75rem;
  margin: 0;
}
@media (min-width: 720px) {
  .sr-dup-grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}
.sr-dup-grid dt {
  font-size: 0.72rem;
  color: #5f736f;
}
.sr-dup-grid dd {
  margin: 0.12rem 0 0;
  font-weight: 650;
  font-size: 0.92rem;
}
</style>
