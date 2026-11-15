<template>
  <div class="sr-hits">
    <p v-if="isLoading" class="sr-hits__state">
      {{ $t('salesRequests.match.loading') }}
    </p>
    <p v-else-if="isError" class="sr-hits__state sr-hits__state--err">
      {{ $t('salesRequests.match.failed') }}
    </p>
    <p v-else-if="insufficient" class="sr-hits__state sr-hits__state--warn">
      {{ $t('salesRequests.match.insufficientData') }}
    </p>
    <p v-else-if="!items.length" class="sr-hits__state">
      {{ $t('salesRequests.match.noHits') }}
    </p>

    <article
      v-for="hit in items"
      :key="hit.customerId"
      class="sr-hit-card"
      :data-band="hit.paymentShareBand || undefined"
      data-testid="match-hit-card"
    >
      <div
        v-if="paymentBanner(hit)"
        class="sr-hit-banner"
        :data-band="hit.paymentShareBand || undefined"
      >
        {{ paymentBanner(hit) }}
      </div>
      <header class="sr-hit-card__head">
        <div class="sr-hit-card__title">
          <h4>{{ hit.customerName }}</h4>
          <p class="sr-hit-card__reason">
            <span
              v-for="part in hit.highlightedParts"
              :key="part"
              class="sr-hit-chip"
              >{{ part }}</span
            >
            <span v-if="hit.matchReason">{{ hit.matchReason }}</span>
          </p>
        </div>
        <span
          v-if="hit.ratingLabel"
          class="sr-hit-rating"
          :data-tone="ratingTone(hit.ratingLabel)"
        >
          {{ hit.ratingLabel }}
          <template v-if="hit.score != null">({{ hit.score }})</template>
        </span>
      </header>

      <dl class="sr-hit-grid">
        <div>
          <dt>الهاتف</dt>
          <dd class="sr-hit-phone">
            <span>{{ hit.phone || '—' }}</span>
            <button
              v-if="hit.phone"
              type="button"
              class="sr-hit-copy"
              @click="onCopy(hit.phone, `p-${hit.customerId}`)"
            >
              {{ copiedKey === `p-${hit.customerId}` ? 'تم' : 'نسخ' }}
            </button>
          </dd>
        </div>
        <div>
          <dt>الفرع</dt>
          <dd>{{ hit.branchName || '—' }}</dd>
        </div>
        <div>
          <dt>عدد التسديدات</dt>
          <dd>{{ hit.paymentCount ?? '—' }}</dd>
        </div>
        <div>
          <dt>أيام التسديد</dt>
          <dd>{{ hit.paymentDays != null ? `${hit.paymentDays} يوم` : '—' }}</dd>
        </div>
        <div>
          <dt>تاريخ المبيع</dt>
          <dd>{{ formatDate(hit.saleDate) }}</dd>
        </div>
        <div>
          <dt>الحالة القانونية</dt>
          <dd>{{ hit.isLegal == null ? '—' : hit.isLegal ? 'قانونية' : 'غير قانونية' }}</dd>
        </div>
        <div>
          <dt>إجمالي البيع</dt>
          <dd>
            {{
              hit.sellTotal == null
                ? $t('salesRequests.match.hiddenFinancial')
                : formatMoney(hit.sellTotal)
            }}
          </dd>
        </div>
        <div>
          <dt>المسدد</dt>
          <dd>
            {{
              hit.paidTotal == null
                ? $t('salesRequests.match.hiddenFinancial')
                : formatMoney(hit.paidTotal)
            }}
          </dd>
        </div>
        <div>
          <dt>المتبقي</dt>
          <dd>
            {{
              hit.remaining == null
                ? $t('salesRequests.match.hiddenFinancial')
                : formatMoney(hit.remaining)
            }}
          </dd>
        </div>
      </dl>

      <footer class="sr-hit-card__foot">
        <button
          v-if="hit.canOpenProfile"
          type="button"
          class="sr-hit-btn"
          @click="$emit('open-profile', hit.customerId)"
        >
          {{ $t('salesRequests.match.openProfile') }}
        </button>
        <button
          type="button"
          class="sr-hit-btn sr-hit-btn--ghost"
          @click="onCopy(hit.customerName, `n-${hit.customerId}`)"
        >
          {{
            copiedKey === `n-${hit.customerId}`
              ? 'تم نسخ الاسم'
              : 'نسخ الاسم'
          }}
        </button>
      </footer>
    </article>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { MatchHit } from '../types'
import { copyText } from '../domain/salesMatchCodes'

defineProps<{
  items: MatchHit[]
  isLoading?: boolean
  isError?: boolean
  insufficient?: boolean
}>()

defineEmits<{
  'open-profile': [customerId: number]
}>()

const copiedKey = ref<string | null>(null)
let copiedTimer: ReturnType<typeof setTimeout> | null = null

function formatMoney(value: number) {
  return new Intl.NumberFormat('en-US').format(value)
}

function formatDate(value?: string | null) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return String(value)
  return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium' }).format(d)
}

function formatShortDay(value?: string | null) {
  if (!value) return ''
  // DateOnly "yyyy-MM-dd" أو ISO
  const m = String(value).match(/^(\d{4})-(\d{2})-(\d{2})/)
  if (m) return `${Number(m[3])}/${Number(m[2])}`
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return String(value)
  return `${d.getDate()}/${d.getMonth() + 1}`
}

function paymentBanner(hit: MatchHit) {
  if (!hit.significantPaymentDate || hit.significantPaymentAmount == null)
    return null
  const day = formatShortDay(hit.significantPaymentDate)
  const amount = formatMoney(hit.significantPaymentAmount)
  return `في يوم ${day} دفع ${amount} دينار`
}

function ratingTone(label: string) {
  if (label.includes('ممتاز') || label.includes('جيد')) return 'ok'
  if (label.includes('ضعيف') || label.includes('سي')) return 'bad'
  return 'mid'
}

async function onCopy(value: string, key: string) {
  const ok = await copyText(value)
  if (!ok) return
  copiedKey.value = key
  if (copiedTimer) clearTimeout(copiedTimer)
  copiedTimer = setTimeout(() => {
    copiedKey.value = null
  }, 1400)
}
</script>

<style scoped>
.sr-hits {
  --neo-bg: #e8f0ef;
  --neo-ink: #00201e;
  --neo-brand: #006a64;
  --neo-shadow-dark: rgba(0, 32, 30, 0.14);
  --neo-shadow-light: rgba(255, 255, 255, 0.92);
  display: grid;
  gap: 0.85rem;
  color: var(--neo-ink);
}

.sr-hits__state {
  margin: 0;
  padding: 0.85rem 1rem;
  border-radius: 14px;
  background: var(--neo-bg);
  box-shadow:
    inset 3px 3px 8px rgba(0, 32, 30, 0.08),
    inset -3px -3px 8px rgba(255, 255, 255, 0.85);
  font-size: 0.9rem;
}

.sr-hits__state--err {
  color: #b91c1c;
}

.sr-hits__state--warn {
  color: #8a4b00;
}

.sr-hit-card {
  padding: 1rem 1.05rem;
  border-radius: 16px;
  background: var(--neo-bg);
  box-shadow:
    7px 7px 16px var(--neo-shadow-dark),
    -7px -7px 16px var(--neo-shadow-light);
  overflow: hidden;
}

.sr-hit-card[data-band='green'] {
  box-shadow:
    7px 7px 16px rgba(15, 118, 110, 0.18),
    -7px -7px 16px var(--neo-shadow-light),
    inset 0 0 0 2px rgba(15, 118, 110, 0.35);
}

.sr-hit-card[data-band='orange'] {
  box-shadow:
    7px 7px 16px rgba(194, 100, 12, 0.16),
    -7px -7px 16px var(--neo-shadow-light),
    inset 0 0 0 2px rgba(194, 100, 12, 0.4);
}

.sr-hit-banner {
  margin: -1rem -1.05rem 0.85rem;
  padding: 0.65rem 1.05rem;
  font-size: 0.92rem;
  font-weight: 800;
  line-height: 1.45;
  text-align: center;
}

.sr-hit-banner[data-band='green'] {
  background: linear-gradient(180deg, #d8f3e8, #c5ebd9);
  color: #0f5c4c;
}

.sr-hit-banner[data-band='orange'] {
  background: linear-gradient(180deg, #ffe4c4, #ffd4a1);
  color: #8a4b00;
}

.sr-hit-card__head {
  display: flex;
  flex-wrap: wrap;
  gap: 0.65rem;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.sr-hit-card__title h4 {
  margin: 0;
  font-size: 1.05rem;
  line-height: 1.55;
  font-weight: 800;
}

.sr-hit-card__reason {
  margin: 0.35rem 0 0;
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  align-items: center;
  font-size: 0.82rem;
  opacity: 0.85;
}

.sr-hit-chip {
  display: inline-flex;
  padding: 0.12rem 0.45rem;
  border-radius: 999px;
  background: rgba(0, 106, 100, 0.12);
  color: var(--neo-brand);
  font-weight: 700;
  font-size: 0.78rem;
}

.sr-hit-rating {
  display: inline-flex;
  align-items: center;
  min-height: 32px;
  padding: 0.2rem 0.65rem;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 700;
  background: #fff;
  box-shadow:
    inset 2px 2px 5px rgba(0, 32, 30, 0.08),
    inset -2px -2px 5px rgba(255, 255, 255, 0.9);
}

.sr-hit-rating[data-tone='ok'] {
  color: #0f766e;
}

.sr-hit-rating[data-tone='bad'] {
  color: #b45309;
}

.sr-hit-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.65rem 0.85rem;
  margin: 0;
}

@media (min-width: 720px) {
  .sr-hit-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

.sr-hit-grid dt {
  font-size: 0.72rem;
  color: #5f736f;
  margin: 0;
}

.sr-hit-grid dd {
  margin: 0.15rem 0 0;
  font-size: 0.95rem;
  font-weight: 650;
  line-height: 1.45;
  overflow-wrap: anywhere;
}

.sr-hit-phone {
  display: inline-flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  align-items: center;
  direction: ltr;
}

.sr-hit-copy,
.sr-hit-btn {
  border: 0;
  border-radius: 12px;
  font: inherit;
  font-weight: 700;
  cursor: pointer;
  min-height: 40px;
  padding: 0.35rem 0.8rem;
  background: #edf5f3;
  color: var(--neo-brand);
  box-shadow:
    4px 4px 10px var(--neo-shadow-dark),
    -4px -4px 10px var(--neo-shadow-light);
}

.sr-hit-copy {
  min-height: 32px;
  font-size: 0.75rem;
  padding: 0.2rem 0.55rem;
}

.sr-hit-btn--ghost {
  background: transparent;
  box-shadow:
    inset 2px 2px 5px rgba(0, 32, 30, 0.08),
    inset -2px -2px 5px rgba(255, 255, 255, 0.85);
}

.sr-hit-card__foot {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.85rem;
}

.sr-hit-btn:active,
.sr-hit-copy:active {
  box-shadow:
    inset 3px 3px 7px rgba(0, 32, 30, 0.12),
    inset -3px -3px 7px rgba(255, 255, 255, 0.8);
}
</style>
