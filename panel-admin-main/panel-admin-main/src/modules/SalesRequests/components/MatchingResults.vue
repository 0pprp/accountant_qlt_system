<template>
  <div class="space-y-3">
    <p v-if="isLoading" class="text-sm text-[#00201E]/70">
      {{ $t('salesRequests.match.loading') }}
    </p>
    <p v-else-if="isError" class="text-sm text-red-700">
      {{ $t('salesRequests.match.failed') }}
    </p>
    <p v-else-if="insufficient" class="text-sm text-amber-800">
      {{ $t('salesRequests.match.insufficientData') }}
    </p>
    <p v-else-if="!items.length" class="text-sm text-[#00201E]/70">
      {{ $t('salesRequests.match.noHits') }}
    </p>
    <article
      v-for="hit in items"
      :key="hit.customerId"
      class="rounded-xl border border-[#d7e2e0] bg-white p-3"
    >
      <h4 class="text-base font-semibold text-[#00201E]">
        {{ hit.customerName }}
      </h4>
      <p class="mt-1 text-sm">
        <span
          v-for="part in hit.highlightedParts"
          :key="part"
          class="me-1 rounded bg-[#006A64]/15 px-1 text-[#006A64]"
          >{{ part }}</span
        >
        — {{ hit.matchReason }}
      </p>
      <dl class="mt-2 grid grid-cols-2 gap-2 text-sm md:grid-cols-3">
        <div>
          <dt class="opacity-70">الهاتف</dt>
          <dd>{{ hit.phone || '—' }}</dd>
        </div>
        <div>
          <dt class="opacity-70">الفرع</dt>
          <dd>{{ hit.branchName || '—' }}</dd>
        </div>
        <div>
          <dt class="opacity-70">التقييم</dt>
          <dd>{{ hit.ratingLabel || '—' }}</dd>
        </div>
        <div>
          <dt class="opacity-70">إجمالي البيع</dt>
          <dd>
            {{
              hit.sellTotal == null
                ? $t('salesRequests.match.hiddenFinancial')
                : formatMoney(hit.sellTotal)
            }}
          </dd>
        </div>
        <div>
          <dt class="opacity-70">المسدد</dt>
          <dd>
            {{
              hit.paidTotal == null
                ? $t('salesRequests.match.hiddenFinancial')
                : formatMoney(hit.paidTotal)
            }}
          </dd>
        </div>
        <div>
          <dt class="opacity-70">المتبقي</dt>
          <dd>
            {{
              hit.remaining == null
                ? $t('salesRequests.match.hiddenFinancial')
                : formatMoney(hit.remaining)
            }}
          </dd>
        </div>
      </dl>
      <button
        v-if="hit.canOpenProfile"
        type="button"
        class="mt-3 rounded-xl bg-[#006A64] px-3 py-2 text-sm text-white"
        @click="$emit('open-profile', hit.customerId)"
      >
        {{ $t('salesRequests.match.openProfile') }}
      </button>
    </article>
  </div>
</template>

<script setup lang="ts">
import type { MatchHit } from '../types'

defineProps<{
  items: MatchHit[]
  isLoading?: boolean
  isError?: boolean
  insufficient?: boolean
}>()

defineEmits<{
  'open-profile': [customerId: number]
}>()

function formatMoney(value: number) {
  return new Intl.NumberFormat('ar-IQ').format(value)
}
</script>
