<template>
  <div class="sr-neo">
    <div class="sr-match-grid" role="list">
      <button
        v-for="card in displayCards"
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
    <p class="sr-match-note">
      {{ $t('salesRequests.match.disclaimer') }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  MATCH_LABELS,
  type MatchCard,
  type MatchCardState,
  type MatchCriterion,
} from '../types'

const props = defineProps<{
  cards: MatchCard[]
  selectedKey: MatchCriterion | string | null
  disabled?: boolean
  customerName?: string | null
  customerPhone?: string | null
}>()

defineEmits<{
  select: [key: string]
}>()

const { t } = useI18n()

/** دائماً 4 كروت مطابقة بنفس ترتيب SalesHaider */
const displayCards = computed<MatchCard[]>(() => {
  const byKey = new Map(props.cards.map((c) => [c.key, c]))
  return (Object.keys(MATCH_LABELS) as MatchCriterion[]).map((key) => {
    const existing = byKey.get(key)
    return {
      key,
      label: MATCH_LABELS[key],
      state: existing?.state ?? 'loading',
      count: existing?.count ?? null,
    }
  })
})

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
.sr-neo {
  --neo-bg: #e8f0ef;
  --neo-ink: #00201e;
  --neo-brand: #006a64;
  --neo-shadow-dark: rgba(0, 32, 30, 0.14);
  --neo-shadow-light: rgba(255, 255, 255, 0.92);
  color: var(--neo-ink);
}

.sr-match-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.5rem;
}

@media (min-width: 768px) {
  .sr-match-grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}

.sr-match-card {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  align-items: stretch;
  text-align: start;
  min-height: 0;
  padding: 0.65rem 0.7rem;
  border: 0;
  border-radius: 16px;
  background: var(--neo-bg);
  color: var(--neo-ink);
  font: inherit;
  cursor: pointer;
  box-shadow:
    7px 7px 16px var(--neo-shadow-dark),
    -7px -7px 16px var(--neo-shadow-light);
  transition:
    box-shadow 0.15s ease,
    transform 0.15s ease;
}

.sr-match-card.selected {
  box-shadow:
    inset 5px 5px 12px rgba(0, 32, 30, 0.14),
    inset -5px -5px 12px rgba(255, 255, 255, 0.85);
  background: linear-gradient(145deg, #dfecea, #edf5f3);
}

.sr-match-card:focus-visible {
  outline: 3px solid var(--neo-brand);
  outline-offset: 3px;
}

.sr-match-label {
  font-size: 0.8rem;
  line-height: 1.35;
  font-weight: 700;
  min-height: 0;
}

.sr-match-count {
  font-size: 1.35rem;
  font-weight: 800;
  color: var(--neo-brand);
  line-height: 1;
}

.sr-match-count[data-state='failed'],
.sr-match-count[data-state='insufficientData'] {
  color: #8a4b00;
}

.sr-match-state {
  font-size: 0.78rem;
  opacity: 0.8;
}

.sr-match-note {
  margin: 0.65rem 0 0;
  font-size: 0.75rem;
  opacity: 0.72;
}

@media (prefers-reduced-motion: reduce) {
  .sr-match-card {
    transition: none;
  }
}
</style>
