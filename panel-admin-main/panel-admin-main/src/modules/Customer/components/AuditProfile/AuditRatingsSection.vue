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
          <p v-if="box.isWrongSale" class="rating-wrong">مبيع خاطئ</p>
          <p v-if="box.occasionLabel" class="rating-occasion">{{ box.occasionLabel }}</p>
          <p v-if="box.ratedByName" class="neo-box__meta">
            بواسطة: {{ box.ratedByName }}
          </p>
          <p v-if="box.ratedAtUtc" class="neo-box__meta">
            {{ formatDate(box.ratedAtUtc) }}
          </p>
          <dl v-if="hasForm(box)" class="rating-form">
            <template v-if="box.shopStockEstimatedValue != null">
              <dt>قيمة المخزون المقدّرة</dt>
              <dd>{{ money(box.shopStockEstimatedValue) }}</dd>
            </template>
            <template v-if="box.estimatedDailyRevenue != null">
              <dt>الإيراد اليومي المقدّر</dt>
              <dd>{{ money(box.estimatedDailyRevenue) }}</dd>
            </template>
            <template v-if="box.contextAmount != null">
              <dt>{{ box.role === 'Delegate' ? 'مبلغ البيع' : 'سقف البيع' }}</dt>
              <dd>{{ money(box.contextAmount) }}</dd>
            </template>
          </dl>
          <p v-if="box.note" class="neo-box__note">{{ box.note }}</p>
        </div>
        <p v-else class="neo-box__empty">لا يوجد تقييم مسجّل بعد</p>
      </article>

      <article v-if="survey" class="neo-box">
        <header class="neo-box__title">{{ survey.title }}</header>
        <div class="neo-box__body">
          <p class="rating-occasion">عند البيع — كشف بلا درجة</p>
          <dl class="rating-form">
            <template v-if="survey.shopName">
              <dt>اسم المحل</dt>
              <dd>{{ survey.shopName }}</dd>
            </template>
            <template v-if="survey.businessType">
              <dt>طبيعة العمل</dt>
              <dd>{{ survey.businessType }}</dd>
            </template>
            <template v-if="survey.stockEstimatedValue != null">
              <dt>قيمة البضاعة المقدّرة</dt>
              <dd>{{ money(survey.stockEstimatedValue) }}</dd>
            </template>
            <template v-if="survey.estimatedDailyRevenue != null">
              <dt>الإيراد اليومي المقدّر</dt>
              <dd>{{ money(survey.estimatedDailyRevenue) }}</dd>
            </template>
            <template v-if="survey.shopLength != null">
              <dt>الطول</dt>
              <dd>{{ survey.shopLength }} م</dd>
            </template>
            <template v-if="survey.shopWidth != null">
              <dt>العرض</dt>
              <dd>{{ survey.shopWidth }} م</dd>
            </template>
            <template v-if="survey.shopArea != null">
              <dt>المساحة</dt>
              <dd>{{ survey.shopArea }} م²</dd>
            </template>
            <template v-if="survey.createdAtUtc">
              <dt>تاريخ الكشف</dt>
              <dd>{{ formatDate(survey.createdAtUtc) }}</dd>
            </template>
          </dl>
          <a
            v-if="survey.latitude != null && survey.longitude != null"
            class="neo-box__map-link"
            :href="mapsLink(survey.latitude, survey.longitude)"
            target="_blank"
            rel="noopener noreferrer"
          >
            موقع الكشف على الخريطة
          </a>
          <button
            v-if="survey.photoId != null"
            type="button"
            class="audit-btn"
            @click="openPhoto(survey.photoId)"
          >
            فتح صورة المحل
          </button>
        </div>
      </article>
    </div>

    <article class="neo-box">
      <header class="neo-box__title">مواقع الزبون — كل مصدر</header>
      <ul v-if="locations.length" class="audit-list">
        <li v-for="loc in locations" :key="loc.source" class="audit-list__row">
          <span>
            {{ loc.sourceLabel }}
            <span class="audit-list__dim">
              {{ loc.latitude.toFixed(6) }}, {{ loc.longitude.toFixed(6) }}
              <template v-if="loc.accuracyMeters != null">
                · دقة {{ Math.round(loc.accuracyMeters) }} م
              </template>
              <template v-if="loc.confidence != null">
                · ثقة {{ Math.round(loc.confidence * 100) }}٪
              </template>
            </span>
          </span>
          <a
            class="audit-btn"
            :href="mapsLink(loc.latitude, loc.longitude)"
            target="_blank"
            rel="noopener noreferrer"
          >
            فتح
          </a>
        </li>
      </ul>
      <p v-else class="neo-box__empty">لا مواقع مسجّلة لهذا الزبون</p>
    </article>

    <article class="neo-box">
      <header class="neo-box__title">صور المحل</header>
      <div v-if="photos.length" class="audit-photos">
        <figure v-for="photo in photos" :key="photo.id" class="audit-photo">
          <img v-if="photoSrc[photo.id]" :src="photoSrc[photo.id]" alt="صورة المحل" />
          <div v-else class="audit-photo__placeholder">جارٍ التحميل…</div>
          <figcaption>
            <button type="button" class="audit-btn" @click="openPhoto(photo.id)">
              فتح
            </button>
          </figcaption>
        </figure>
      </div>
      <p v-else class="neo-box__empty">لا صور محل محفوظة</p>
    </article>

    <article class="neo-box">
      <header class="neo-box__title">لقطات التقييم</header>
      <div v-if="snapshots.length" class="audit-scroll">
        <table class="audit-table">
          <thead>
            <tr>
              <th>المصدر</th>
              <th>التسمية</th>
              <th>الدرجة</th>
              <th>التاريخ</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(snap, i) in snapshots" :key="i">
              <td>{{ snap.origin }}</td>
              <td>{{ snap.ratingLabel ?? '—' }}</td>
              <td>{{ snap.score ?? '—' }}</td>
              <td>{{ snap.captureDate }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <p v-else class="neo-box__empty">لا لقطات تقييم لهذا الزبون</p>
    </article>

    <MediaViewerDialog
      v-model="viewerOpen"
      :path="viewerPath"
      :title="viewerTitle"
    />

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
import { computed, reactive, ref, watch } from 'vue'
import { useAuthedMedia } from '@/modules/Core/composable/useAuthedMedia'
import MediaViewerDialog from '@/modules/Core/components/shared/MediaViewerDialog/MediaViewerDialog.vue'
import { formattedPrice } from '@/modules/Core/utils/price'
import type { CustomerAuditRatingsAndLocation } from '../../requests/queries/useCustomerAuditProfileQuery'

const props = defineProps<{
  ratings: CustomerAuditRatingsAndLocation
}>()

const { fetchBlobUrl } = useAuthedMedia()

const viewerOpen = ref(false)
const viewerPath = ref<string | null>(null)
const viewerTitle = ref('')

const locations = computed(() => props.ratings.locations ?? [])
const photos = computed(() => props.ratings.photos ?? [])
const snapshots = computed(() => props.ratings.snapshots ?? [])
const survey = computed(() => props.ratings.shopSurvey ?? null)

const photoSrc = reactive<Record<number, string>>({})

watch(
  photos,
  async (list) => {
    for (const photo of list) {
      if (photoSrc[photo.id]) continue
      const url = await fetchBlobUrl(`admin/CustomerMedia/photos/${photo.id}`)
      if (url) photoSrc[photo.id] = url
    }
  },
  { immediate: true },
)

type RatingBox = {
  shopStockEstimatedValue?: number | null
  estimatedDailyRevenue?: number | null
  contextAmount?: number | null
}

function hasForm(box: RatingBox) {
  return (
    box.shopStockEstimatedValue != null ||
    box.estimatedDailyRevenue != null ||
    box.contextAmount != null
  )
}

function money(value: number) {
  return formattedPrice(value)
}

function mapsLink(lat: number, lng: number) {
  return `https://www.google.com/maps?q=${lat},${lng}`
}

function openPhoto(id: number) {
  viewerPath.value = `admin/CustomerMedia/photos/${id}`
  viewerTitle.value = 'صورة المحل'
  viewerOpen.value = true
}

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
      shopStockEstimatedValue: r.delegateRating?.shopStockEstimatedValue ?? null,
      estimatedDailyRevenue: r.delegateRating?.estimatedDailyRevenue ?? null,
      contextAmount: r.delegateRating?.contextAmount ?? null,
      occasionLabel: r.delegateRating?.occasionLabel ?? null,
      isWrongSale: r.delegateRating?.isWrongSale ?? false,
    },
    {
      role: 'CommitteeField',
      title:
        r.committeeFieldRating?.title ?? 'تقييم مسؤول اللجنة الميدانية',
      score: r.committeeFieldRating?.score ?? null,
      note: r.committeeFieldRating?.note ?? null,
      ratedByName: r.committeeFieldRating?.ratedByName ?? null,
      ratedAtUtc: r.committeeFieldRating?.ratedAtUtc ?? null,
      shopStockEstimatedValue: r.committeeFieldRating?.shopStockEstimatedValue ?? null,
      estimatedDailyRevenue: r.committeeFieldRating?.estimatedDailyRevenue ?? null,
      contextAmount: r.committeeFieldRating?.contextAmount ?? null,
      occasionLabel: r.committeeFieldRating?.occasionLabel ?? null,
      isWrongSale: r.committeeFieldRating?.isWrongSale ?? false,
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

.audit-list { list-style: none; margin: 0; padding: 0; display: grid; gap: 8px; }
.audit-list__row {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  padding: 10px 12px; border-radius: 10px; background: rgba(0, 106, 100, 0.05);
}
.audit-list__dim { color: #50666a; font-size: 13px; margin-inline-start: 10px; }
.audit-btn {
  display: inline-block; padding: 5px 14px; border-radius: 999px;
  border: 1px solid #006a64; color: #006a64; background: #fff;
  font-size: 13px; font-weight: 600; text-decoration: none; white-space: nowrap;
}
.audit-btn:hover { background: #006a64; color: #fff; }
.audit-photos { display: flex; flex-wrap: wrap; gap: 14px; }
.audit-photo { margin: 0; display: grid; gap: 8px; justify-items: center; }
.audit-photo img {
  width: 190px; height: 190px; object-fit: cover;
  border-radius: 12px; border: 1px solid #cddbd7; background: #f1f5f4;
}
.audit-scroll { overflow-x: auto; max-height: 340px; overflow-y: auto; }
.audit-table { width: 100%; border-collapse: collapse; font-size: 14px; }
.audit-table th, .audit-table td {
  text-align: right; padding: 8px 10px; border-bottom: 1px solid #e3ecea; white-space: nowrap;
}
.audit-table th { position: sticky; top: 0; background: #f1f5f4; font-size: 13px; color: #50666a; }
.audit-photo__placeholder {
  width: 190px; height: 190px; display: flex; align-items: center; justify-content: center;
  border-radius: 12px; border: 1px dashed #cddbd7; color: #50666a; font-size: 13px;
  background: #f1f5f4;
}
button.audit-btn { cursor: pointer; font-family: inherit; }
.rating-occasion {
  display: inline-block; margin: 2px 0 6px; padding: 2px 10px; border-radius: 999px;
  background: rgba(0, 106, 100, 0.1); color: #006a64; font-size: 12px; font-weight: 600;
}
.rating-form {
  display: grid; grid-template-columns: auto 1fr; gap: 3px 12px;
  margin: 8px 0 0; font-size: 13px;
}
.rating-wrong {
  display: inline-block; margin: 2px 0 6px; padding: 2px 10px; border-radius: 999px;
  background: rgba(186, 26, 26, 0.12); color: #ba1a1a; font-size: 12px; font-weight: 700;
}
.rating-form dt { color: #50666a; }
.rating-form dd { margin: 0; font-weight: 600; }
</style>
