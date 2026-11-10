<template>
  <div class="space-y-3">
    <p v-if="isLoading" class="text-sm text-[#00201E]/70">
      {{ $t('salesRequests.match.loading') }}
    </p>
    <p v-else-if="isError" class="text-sm text-red-700">
      {{ $t('salesRequests.match.failed') }}
    </p>
    <p v-else-if="!items.length" class="text-sm text-[#00201E]/70">
      {{ $t('salesRequests.match.noHits') }}
    </p>
    <article
      v-for="hit in items"
      :key="hit.id"
      class="rounded-xl border border-[#d7e2e0] bg-white p-3 text-sm"
    >
      <div class="font-semibold text-[#00201E]">طلب #{{ hit.id }}</div>
      <dl class="mt-2 grid grid-cols-2 gap-2 md:grid-cols-4">
        <div>
          <dt class="opacity-70">المصدر</dt>
          <dd>{{ hit.sourceTypeLabel }}</dd>
        </div>
        <div>
          <dt class="opacity-70">الحالة</dt>
          <dd>{{ hit.statusLabel }}</dd>
        </div>
        <div>
          <dt class="opacity-70">تاريخ الإضافة</dt>
          <dd>{{ formatDate(hit.createdAt) }}</dd>
        </div>
        <div>
          <dt class="opacity-70">الموظف المسند</dt>
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
  return new Intl.DateTimeFormat('ar-IQ', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}
</script>
