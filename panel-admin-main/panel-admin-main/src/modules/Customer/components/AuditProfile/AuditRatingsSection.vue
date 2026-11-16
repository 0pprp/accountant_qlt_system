<template>
  <section class="audit-ratings" dir="rtl">
    <div class="audit-ratings__grid">
      <article
        v-for="box in boxes"
        :key="box.role"
        class="neo-box"
      >
        <header class="neo-box__title">{{ box.title }}</header>
        <div v-if="box.score != null" class="neo-box__body">
          <p class="neo-box__score">{{ box.score }} / 10</p>
          <p v-if="box.ratedByName" class="neo-box__meta">
            بواسطة: {{ box.ratedByName }}
          </p>
          <p v-if="box.ratedAtUtc" class="neo-box__meta">
            {{ formatDate(box.ratedAtUtc) }}
          </p>
          <p v-if="box.note" class="neo-box__note">{{ box.note }}</p>
        </div>
        <p v-else class="neo-box__empty">لا يوجد تقييم مسجّل بعد</p>
      </article>
    </div>

    <article class="neo-box neo-box--location">
      <header class="neo-box__title">الموقع</header>
      <div v-if="hasDisplayGps" class="neo-box__body">
        <p class="neo-box__meta">
          خط العرض: {{ displayLat?.toFixed(6) }}
        </p>
        <p class="neo-box__meta">
          خط الطول: {{ displayLng?.toFixed(6) }}
        </p>
        <a
          class="neo-box__map-link"
          :href="mapsUrl"
          target="_blank"
          rel="noopener noreferrer"
        >
          فتح في الخريطة
        </a>
        <p v-if="ratings.hasGps" class="neo-box__meta">مصدر: بروفايل الزبون</p>
        <p
          v-else-if="ratings.latestOrderLatitude != null"
          class="neo-box__meta"
        >
          مصدر: آخر طلب بموقع
        </p>
      </div>
      <p v-else class="neo-box__empty">لم يُحدَّد موقع GPS بعد</p>
    </article>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { CustomerAuditRatingsAndLocation } from '../../requests/queries/useCustomerAuditProfileQuery'

const props = defineProps<{
  ratings: CustomerAuditRatingsAndLocation
}>()

const boxes = computed(() => {
  const r = props.ratings
  return [
    {
      role: 'Delegate',
      title: r.delegateRating?.title ?? 'تقييم المندوب',
      score: r.delegateRating?.score ?? null,
      note: r.delegateRating?.note ?? null,
      ratedByName: r.delegateRating?.ratedByName ?? null,
      ratedAtUtc: r.delegateRating?.ratedAtUtc ?? null,
    },
    {
      role: 'CommitteeField',
      title:
        r.committeeFieldRating?.title ?? 'تقييم مسؤول اللجنة الميدانية',
      score: r.committeeFieldRating?.score ?? null,
      note: r.committeeFieldRating?.note ?? null,
      ratedByName: r.committeeFieldRating?.ratedByName ?? null,
      ratedAtUtc: r.committeeFieldRating?.ratedAtUtc ?? null,
    },
    {
      role: 'SalesEmployee',
      title: r.salesEmployeeRating?.title ?? 'تقييم موظف المبيعات',
      score: r.salesEmployeeRating?.score ?? null,
      note: r.salesEmployeeRating?.note ?? null,
      ratedByName: r.salesEmployeeRating?.ratedByName ?? null,
      ratedAtUtc: r.salesEmployeeRating?.ratedAtUtc ?? null,
    },
  ]
})

const displayLat = computed(() => {
  if (props.ratings.hasGps && props.ratings.gpsLatitude != null) {
    return props.ratings.gpsLatitude
  }
  return props.ratings.latestOrderLatitude ?? null
})

const displayLng = computed(() => {
  if (props.ratings.hasGps && props.ratings.gpsLongitude != null) {
    return props.ratings.gpsLongitude
  }
  return props.ratings.latestOrderLongitude ?? null
})

const hasDisplayGps = computed(
  () => displayLat.value != null && displayLng.value != null
)

const mapsUrl = computed(() => {
  if (!hasDisplayGps.value) return '#'
  return `https://www.google.com/maps?q=${displayLat.value},${displayLng.value}`
})

function formatDate(value: string) {
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  return d.toLocaleString('ar-IQ')
}
</script>

<style scoped>
.audit-ratings {
  --neo-bg: #e8f0ef;
  --neo-ink: #00201e;
  --neo-accent: #006a64;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.audit-ratings__grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.neo-box {
  border: 0;
  border-radius: 18px;
  background: var(--neo-bg);
  color: var(--neo-ink);
  padding: 1rem 1.1rem 1.15rem;
  box-shadow:
    6px 6px 14px rgba(0, 32, 30, 0.12),
    -6px -6px 14px rgba(255, 255, 255, 0.92);
}

.neo-box--location {
  max-width: 420px;
}

.neo-box__title {
  display: inline-block;
  margin: 0 0 0.85rem;
  padding: 0.45rem 0.9rem;
  border-radius: 12px;
  background: var(--neo-bg);
  color: var(--neo-accent);
  font-size: 0.98rem;
  font-weight: 800;
  box-shadow:
    4px 4px 10px rgba(0, 32, 30, 0.12),
    -4px -4px 10px rgba(255, 255, 255, 0.9);
}

.neo-box__score {
  margin: 0 0 0.4rem;
  font-size: 1.55rem;
  font-weight: 800;
  color: var(--neo-accent);
}

.neo-box__meta,
.neo-box__note,
.neo-box__empty {
  margin: 0.25rem 0;
  font-size: 0.9rem;
  line-height: 1.45;
}

.neo-box__empty {
  opacity: 0.7;
}

.neo-box__note {
  margin-top: 0.55rem;
  padding: 0.65rem 0.75rem;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.45);
  box-shadow: inset 2px 2px 5px rgba(0, 32, 30, 0.08);
}

.neo-box__map-link {
  display: inline-flex;
  margin-top: 0.65rem;
  color: var(--neo-accent);
  font-weight: 700;
  text-decoration: none;
}

.neo-box__map-link:hover {
  text-decoration: underline;
}
</style>
