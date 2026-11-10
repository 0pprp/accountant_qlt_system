<template>
  <div class="sr-match-grid" role="list">
    <button
      v-for="card in cards"
      :key="card.key"
      type="button"
      :aria-pressed="selectedKey === card.key"
      class="sr-match-card"
      :class="{ selected: selectedKey === card.key }"
      :disabled="disabled"
      @click="$emit('select', card.key)"
    >
      <span class="sr-match-label">{{ card.label }}</span>
      <span class="sr-match-count" :data-state="card.state">
        <template v-if="card.state === 'loading'">…</template>
        <template v-else-if="card.state === 'insufficientData'">—</template>
        <template v-else-if="card.state === 'failed'">!</template>
        <template v-else>{{ card.count ?? 0 }}</template>
      </span>
      <span class="sr-match-state">{{ stateLabel(card.state) }}</span>
    </button>
  </div>
  <p class="mt-2 text-xs text-[#00201E]/70">
    {{ $t('salesRequests.match.disclaimer') }}
  </p>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import type { MatchCard, MatchCardState, MatchCriterion } from '../types'

defineProps<{
  cards: MatchCard[]
  selectedKey: MatchCriterion | string | null
  disabled?: boolean
}>()

defineEmits<{
  select: [key: string]
}>()

const { t } = useI18n()

function stateLabel(state: MatchCardState) {
  const map: Record<MatchCardState, string> = {
    loading: t('salesRequests.match.loading'),
    results: t('salesRequests.match.results'),
    noResults: t('salesRequests.match.noResults'),
    insufficientData: t('salesRequests.match.insufficientData'),
    failed: t('salesRequests.match.failed'),
  }
  return map[state] ?? state
}
</script>

<style scoped>
.sr-match-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

@media (min-width: 768px) {
  .sr-match-grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}

.sr-match-card {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  align-items: stretch;
  text-align: start;
  padding: 0.85rem;
  border-radius: 1rem;
  border: 2px solid #d7e2e0;
  background: #fff;
  color: #00201e;
  font-family: inherit;
  transition:
    border-color 0.15s ease,
    box-shadow 0.15s ease;
}

.sr-match-card.selected {
  border-color: #006a64;
  box-shadow: 0 0 0 2px rgba(0, 106, 100, 0.15);
}

.sr-match-label {
  font-size: 0.85rem;
  line-height: 1.35;
  min-height: 2.4em;
}

.sr-match-count {
  font-size: 1.6rem;
  font-weight: 700;
  color: #006a64;
  line-height: 1;
}

.sr-match-count[data-state='failed'],
.sr-match-count[data-state='insufficientData'] {
  color: #8a4b00;
}

.sr-match-state {
  font-size: 0.75rem;
  opacity: 0.8;
}

@media (prefers-reduced-motion: reduce) {
  .sr-match-card {
    transition: none;
  }
}
.sr-match-label { font-size:15px; line-height:1.65; font-weight:600; }
.sr-match-state { font-size:13px; line-height:1.6; }
.sr-match-count { font-size:28px; }
.sr-match-card { min-height:145px; padding:14px 10px; border-width:1px; border-radius:12px; }
.sr-match-card.selected { background:#edf6f2; border-color:#006a64; }
.sr-match-card:focus-visible { outline:3px solid #006a64; outline-offset:3px; }
@media(prefers-reduced-motion:reduce) { .sr-match-card { transition:none; } }
</style>
