<script setup lang="ts">
import type { FollowerCard } from '../domain/types'
import FollowerIdentityHeader from './FollowerIdentityHeader.vue'

const props = defineProps<{
  card: FollowerCard
  selected: boolean
  hasImplementedSection: boolean
  canOpenSales?: boolean
  canOpenDeclining?: boolean
  canOpenNotes?: boolean
  canOpenOldCustomers?: boolean
  canOpenDelegateNotes?: boolean
  canOpenCollection?: boolean
}>()

const emit = defineEmits<{
  select: [followerId: number]
  enter: [followerId: number]
  enterDeclining: [followerId: number]
  enterNotes: [followerId: number]
  enterOldCustomers: [followerId: number]
  enterDelegateNotes: [followerId: number]
  enterCollection: [followerId: number]
}>()

function branchLabel(card: FollowerCard) {
  if (!card.branches.length) return 'بدون فرع تكليف'
  return card.branches.map((b) => b.branchName).join('، ')
}

function trendLabel(t: string | null | undefined) {
  if (t === 'up') return '↑'
  if (t === 'down') return '↓'
  if (t === 'flat') return '→'
  return ''
}
</script>

<template>
  <article
    class="fw-card"
    :class="{ 'fw-card--selected': selected }"
    :aria-selected="selected"
  >
    <FollowerIdentityHeader
      :name="card.followerName"
      :branch-label="branchLabel(card)"
      :list-count="card.assignedListCount"
    />

    <div v-if="card.metrics?.availability === 'available'" class="fw-card__metrics">
      <div class="fw-metric">
        <span class="fw-metric__label">تقييم وهمي</span>
        <span class="fw-metric__value">
          {{ card.metrics.phantomRating?.toFixed(2) ?? '—' }}
          <span class="fw-metric__trend">{{ trendLabel(card.metrics.scoreTrend) }}</span>
        </span>
        <span v-if="card.metrics.phantomRatingLabel" class="fw-metric__sub">
          {{ card.metrics.phantomRatingLabel }}
        </span>
      </div>
      <div class="fw-metric">
        <span class="fw-metric__label">زبائن مقيّمون</span>
        <span class="fw-metric__value">
          {{ card.metrics.ratedCustomerCount ?? '—' }}
          <span class="fw-metric__trend">{{ trendLabel(card.metrics.countTrend) }}</span>
        </span>
      </div>
    </div>
    <p
      v-else-if="card.metrics?.availability === 'unavailable'"
      class="fw-card__metrics-unavailable"
    >
      مؤشرات التقييم غير متاحة حاليًا
    </p>

    <div class="fw-card__actions">
      <button
        type="button"
        class="fw-card__btn"
        :aria-pressed="selected"
        @click="emit('select', card.followerId)"
      >
        {{ selected ? 'محدد' : 'اختيار المتابع' }}
      </button>
      <button
        v-if="canOpenSales"
        type="button"
        class="fw-card__btn fw-card__btn--secondary"
        @click="emit('enter', card.followerId)"
      >
        مصادقة البيع
      </button>
      <button
        v-if="canOpenDeclining"
        type="button"
        class="fw-card__btn fw-card__btn--secondary"
        @click="emit('enterDeclining', card.followerId)"
      >
        المتدنية
      </button>
      <button
        v-if="canOpenNotes"
        type="button"
        class="fw-card__btn fw-card__btn--secondary"
        @click="emit('enterNotes', card.followerId)"
      >
        الملاحظات
      </button>
      <button
        v-if="canOpenOldCustomers"
        type="button"
        class="fw-card__btn fw-card__btn--secondary"
        @click="emit('enterOldCustomers', card.followerId)"
      >
        الزبائن القديمة
      </button>
      <button
        v-if="canOpenDelegateNotes"
        type="button"
        class="fw-card__btn fw-card__btn--secondary"
        @click="emit('enterDelegateNotes', card.followerId)"
      >
        ملاحظات المندوبين
      </button>
      <button
        v-if="canOpenCollection"
        type="button"
        class="fw-card__btn fw-card__btn--secondary"
        @click="emit('enterCollection', card.followerId)"
      >
        الجباية
      </button>
      <span
        v-if="hasImplementedSection === false"
        class="fw-card__note"
      >
        لا يوجد قسم داخلي منفَّذ بعد للدخول
      </span>
    </div>
  </article>
</template>

<style scoped>
.fw-card {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  width: 100%;
  max-width: 100%;
  padding: 1rem;
  border: 1px solid #d5e3e1;
  border-radius: 12px;
  background: #fff;
  box-sizing: border-box;
}
.fw-card--selected {
  border-color: #006a64;
  box-shadow: 0 0 0 1px #006a64;
}
.fw-card__metrics {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(8rem, 1fr));
  gap: 0.75rem;
}
.fw-metric {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}
.fw-metric__label {
  font-size: 14px;
  color: #3d524f;
}
.fw-metric__value {
  font-size: 16px;
  font-weight: 700;
  color: #006a64;
}
.fw-metric__trend {
  margin-inline-start: 0.25rem;
  font-weight: 600;
}
.fw-metric__sub {
  font-size: 14px;
  color: #3d524f;
}
.fw-card__metrics-unavailable {
  margin: 0;
  font-size: 14px;
  color: #3d524f;
}
.fw-card__actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.75rem;
}
.fw-card__btn {
  min-height: 44px;
  min-width: 8rem;
  padding: 0 1rem;
  border: none;
  border-radius: 8px;
  background: #006a64;
  color: #fff;
  font-family: inherit;
  font-size: 16px;
  cursor: pointer;
}
.fw-card__btn--secondary {
  background: #fff;
  color: #006a64;
  border: 1px solid #006a64;
}
.fw-card__btn:focus-visible {
  outline: 2px solid #00201e;
  outline-offset: 2px;
}
.fw-card__note {
  font-size: 14px;
  color: #3d524f;
}
</style>
