<template>
  <div class="sr-neo">
    <div
      v-if="codes.length"
      class="sr-code-bar"
      aria-label="أكواد المقارنة من النظام"
    >
      <div class="sr-code-bar__head">
        <strong>أكواد المقارنة</strong>
        <span>انسخ من النظام والصق عند الحاجة للمراجعة</span>
      </div>
      <div class="sr-code-chips">
        <div
          v-for="item in codes"
          :key="item.criterion"
          class="sr-code-chip"
          :data-ready="item.ready ? '1' : '0'"
        >
          <div class="sr-code-chip__meta">
            <span class="sr-code-chip__label">{{ item.label }}</span>
            <span class="sr-code-chip__hint">{{ item.hint }}</span>
          </div>
          <code class="sr-code-chip__value" :title="item.code || '—'">
            {{ item.code || '—' }}
          </code>
          <button
            type="button"
            class="sr-copy"
            :disabled="!item.ready || !item.code"
            :aria-label="`نسخ كود ${item.label}`"
            @click.stop="onCopy(item.code, item.criterion)"
          >
            {{ copiedKey === item.criterion ? 'تم النسخ' : 'نسخ' }}
          </button>
        </div>
      </div>
    </div>

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
        <span
          v-if="codeFor(card.key)"
          class="sr-match-code"
          :title="codeFor(card.key)!.code || undefined"
        >
          كود: {{ codeFor(card.key)!.code || 'غير جاهز' }}
        </span>
      </button>
    </div>
    <p class="sr-match-note">
      {{ $t('salesRequests.match.disclaimer') }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  MATCH_LABELS,
  type MatchCard,
  type MatchCardState,
  type MatchCriterion,
} from '../types'
import {
  buildComparisonCodes,
  copyText,
  type ComparisonCode,
} from '../domain/salesMatchCodes'

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
const copiedKey = ref<string | null>(null)
let copiedTimer: ReturnType<typeof setTimeout> | null = null

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

const codes = computed(() =>
  buildComparisonCodes(props.customerName, props.customerPhone)
)

const codeMap = computed(() => {
  const map = new Map<string, ComparisonCode>()
  for (const c of codes.value) map.set(c.criterion, c)
  return map
})

function codeFor(key: string) {
  return codeMap.value.get(key) ?? null
}

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

async function onCopy(code: string, key: string) {
  const ok = await copyText(code)
  if (!ok) return
  copiedKey.value = key
  if (copiedTimer) clearTimeout(copiedTimer)
  copiedTimer = setTimeout(() => {
    copiedKey.value = null
  }, 1600)
}
</script>

<style scoped>
.sr-neo {
  --neo-bg: #e8f0ef;
  --neo-ink: #00201e;
  --neo-brand: #006a64;
  --neo-soft: #f4f8f7;
  --neo-shadow-dark: rgba(0, 32, 30, 0.14);
  --neo-shadow-light: rgba(255, 255, 255, 0.92);
  color: var(--neo-ink);
}

.sr-code-bar {
  margin-bottom: 0.85rem;
  padding: 0.9rem 1rem;
  border-radius: 16px;
  background: var(--neo-bg);
  box-shadow:
    6px 6px 14px var(--neo-shadow-dark),
    -6px -6px 14px var(--neo-shadow-light);
}

.sr-code-bar__head {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem 0.75rem;
  align-items: baseline;
  margin-bottom: 0.65rem;
}

.sr-code-bar__head strong {
  font-size: 0.95rem;
  color: var(--neo-brand);
}

.sr-code-bar__head span {
  font-size: 0.78rem;
  opacity: 0.75;
}

.sr-code-chips {
  display: grid;
  gap: 0.55rem;
}

@media (min-width: 720px) {
  .sr-code-chips {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

.sr-code-chip {
  display: grid;
  grid-template-columns: 1fr auto;
  grid-template-rows: auto auto;
  gap: 0.25rem 0.55rem;
  align-items: center;
  padding: 0.65rem 0.75rem;
  border-radius: 14px;
  background: var(--neo-soft);
  box-shadow:
    inset 3px 3px 7px rgba(0, 32, 30, 0.08),
    inset -3px -3px 7px rgba(255, 255, 255, 0.85);
}

.sr-code-chip[data-ready='0'] {
  opacity: 0.72;
}

.sr-code-chip__meta {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  min-width: 0;
}

.sr-code-chip__label {
  font-size: 0.82rem;
  font-weight: 700;
}

.sr-code-chip__hint {
  font-size: 0.7rem;
  opacity: 0.65;
}

.sr-code-chip__value {
  grid-column: 1 / 2;
  font-family: ui-monospace, 'Cascadia Code', Consolas, monospace;
  font-size: 0.86rem;
  direction: ltr;
  text-align: start;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--neo-brand);
  font-weight: 700;
}

.sr-copy {
  grid-row: 1 / span 2;
  grid-column: 2;
  align-self: center;
  min-height: 40px;
  min-width: 64px;
  padding: 0.35rem 0.7rem;
  border: 0;
  border-radius: 12px;
  background: var(--neo-bg);
  color: var(--neo-brand);
  font: inherit;
  font-weight: 700;
  font-size: 0.8rem;
  cursor: pointer;
  box-shadow:
    4px 4px 10px var(--neo-shadow-dark),
    -4px -4px 10px var(--neo-shadow-light);
}

.sr-copy:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.sr-copy:not(:disabled):active {
  box-shadow:
    inset 3px 3px 7px rgba(0, 32, 30, 0.12),
    inset -3px -3px 7px rgba(255, 255, 255, 0.8);
}

.sr-match-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.85rem;
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
  min-height: 150px;
  padding: 0.95rem 0.85rem;
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
  font-size: 0.92rem;
  line-height: 1.55;
  font-weight: 700;
  min-height: 2.6em;
}

.sr-match-count {
  font-size: 1.75rem;
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

.sr-match-code {
  margin-top: auto;
  font-size: 0.72rem;
  font-family: ui-monospace, Consolas, monospace;
  direction: ltr;
  text-align: start;
  opacity: 0.85;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.sr-match-note {
  margin: 0.65rem 0 0;
  font-size: 0.75rem;
  opacity: 0.72;
}

@media (prefers-reduced-motion: reduce) {
  .sr-match-card,
  .sr-copy {
    transition: none;
  }
}
</style>
