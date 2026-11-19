<template>
  <Drawer
    :visible="customerId != null"
    position="right"
    :pt="pt"
    :pt-options="{ mergeSections: true, mergeProps: true }"
    :show-close-icon="false"
    @update:visible="!$event && emit('close')"
  >
    <div class="fwb-drawer" dir="rtl">
      <header class="dfb-dialog-head">
        <div>
          <h2 class="dfb-dialog-title">{{ profile?.general.customerName ?? 'بروفايل الزبون' }}</h2>
          <p v-if="profile" class="dfb-dialog-sub">
            {{ [profile.general.listName, profile.general.phoneNumber, profile.general.shopName].filter(Boolean).join(' · ') }}
          </p>
        </div>
        <button type="button" class="dfb-icon-button" aria-label="إغلاق" @click="emit('close')">
          <svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true"><path d="M6 6l12 12M18 6L6 18" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" /></svg>
        </button>
      </header>

      <p v-if="query.isPending.value" class="dfb-state" role="status">جاري تحميل البروفايل…</p>
      <div v-else-if="query.isError.value" class="dfb-error" role="alert">
        تعذر تحميل البروفايل
        <button type="button" class="dfb-button is-small" @click="query.refetch()">إعادة المحاولة</button>
      </div>

      <template v-else-if="profile">
        <section class="fwb-box">
          <h3>عام</h3>
          <dl>
            <div><dt>التقييم</dt><dd :class="`tier-${profile.general.ratingLabel}`">{{ profile.general.ratingLabel ?? '—' }}</dd></div>
            <div><dt>سعر البيع</dt><dd>{{ money(profile.general.sellAmount) }}</dd></div>
            <div><dt>تاريخ البيع</dt><dd><span class="dfb-num">{{ profile.general.saleDate ?? '—' }}</span></dd></div>
            <div><dt>الواصل / الباقي</dt><dd>{{ money(profile.general.paid) }} / {{ money(profile.general.remaining) }}</dd></div>
            <div><dt>القسط اليومي</dt><dd>{{ money(profile.general.dailyInstallment) }}</dd></div>
            <div><dt>آخر تسديد</dt><dd><span class="dfb-num">{{ profile.general.lastPaymentDate ?? '—' }}</span></dd></div>
            <div><dt>البائع</dt><dd>{{ profile.general.sellerName ?? '—' }}</dd></div>
            <div><dt>العنوان</dt><dd>{{ profile.general.address ?? '—' }}</dd></div>
            <div v-if="profile.general.hasLocation">
              <dt>الموقع</dt>
              <dd><a class="dfb-link" :href="`https://www.google.com/maps?q=${profile.general.latitude},${profile.general.longitude}`" target="_blank" rel="noopener noreferrer">خرائط Google</a></dd>
            </div>
          </dl>
        </section>

        <section class="fwb-box" :class="{ 'is-risk': profile.followerAssessment?.isLowRisk }">
          <h3>تقييم المتابع (مصادقة البيع)</h3>
          <p v-if="!profile.followerAssessment" class="dfb-hint">لم يصادق المتابع على هذا البيع بعد</p>
          <dl v-else>
            <div><dt>التقييم</dt><dd>{{ profile.followerAssessment.rating ?? '—' }} / 10</dd></div>
            <div><dt>قيمة البضاعة</dt><dd>{{ money(profile.followerAssessment.shopStockEstimatedValue) }}</dd></div>
            <div><dt>الدخل اليومي</dt><dd>{{ money(profile.followerAssessment.estimatedDailyRevenue) }}</dd></div>
            <div v-if="profile.followerAssessment.note" class="is-wide"><dt>الملاحظة</dt><dd>{{ profile.followerAssessment.note }}</dd></div>
            <div><dt>بواسطة</dt><dd>{{ profile.followerAssessment.ratedByName ?? '—' }}</dd></div>
            <div><dt>الوقت</dt><dd><span class="dfb-num">{{ date(profile.followerAssessment.ratedAtUtc) }}</span></dd></div>
          </dl>
        </section>

        <section class="fwb-box" :class="{ 'is-risk': profile.delegateAssessment?.isLowRisk }">
          <h3>تقييم المندوب</h3>
          <p v-if="!profile.delegateAssessment" class="dfb-hint">لا يوجد تقييم من المندوب</p>
          <dl v-else>
            <div><dt>التقييم</dt><dd>{{ profile.delegateAssessment.rating ?? '—' }} / 10</dd></div>
            <div><dt>قيمة البضاعة</dt><dd>{{ money(profile.delegateAssessment.shopStockEstimatedValue) }}</dd></div>
            <div><dt>الدخل اليومي</dt><dd>{{ money(profile.delegateAssessment.estimatedDailyRevenue) }}</dd></div>
            <div v-if="profile.delegateAssessment.note" class="is-wide"><dt>الملاحظة</dt><dd>{{ profile.delegateAssessment.note }}</dd></div>
            <div><dt>الوقت</dt><dd><span class="dfb-num">{{ date(profile.delegateAssessment.ratedAtUtc) }}</span></dd></div>
          </dl>
        </section>

        <section class="fwb-box">
          <h3>موظف المبيعات</h3>
          <p v-if="!profile.salesEmployee" class="dfb-hint">لا توجد بيانات من موظف المبيعات</p>
          <dl v-else>
            <div><dt>التقييم</dt><dd>{{ profile.salesEmployee.rating != null ? `${profile.salesEmployee.rating} / 10` : '—' }}</dd></div>
            <div><dt>نوع النشاط</dt><dd>{{ profile.salesEmployee.businessType ?? '—' }}</dd></div>
            <div><dt>قيمة البضاعة</dt><dd>{{ money(profile.salesEmployee.stockEstimatedValue) }}</dd></div>
            <div><dt>الدخل اليومي</dt><dd>{{ money(profile.salesEmployee.estimatedDailyRevenue) }}</dd></div>
            <div><dt>مساحة المحل</dt><dd>{{ profile.salesEmployee.shopArea != null ? `${profile.salesEmployee.shopArea} م²` : '—' }}</dd></div>
            <div v-if="profile.salesEmployee.note" class="is-wide"><dt>الملاحظة</dt><dd>{{ profile.salesEmployee.note }}</dd></div>
          </dl>
        </section>

        <button v-if="canOpenFullProfile" type="button" class="dfb-button is-primary" @click="emit('full-profile', profile.general.customerId)">
          فتح البروفايل الكامل
        </button>
      </template>
    </div>
  </Drawer>
</template>

<script setup lang="ts">
import { computed, toRef } from 'vue'
import { Drawer } from 'primevue'
import '@/modules/Core/components/neumorphic/neumorphic.css'
import { useFollowerCustomerProfileQuery } from '../../requests/queries/useFollowerBoardQueries'

const props = defineProps<{
  followerId: number | null
  customerId: number | null
  branchId: number | null
  canOpenFullProfile: boolean
}>()

const emit = defineEmits<{ close: []; 'full-profile': [customerId: number] }>()

const query = useFollowerCustomerProfileQuery(toRef(props, 'followerId'), toRef(props, 'customerId'), toRef(props, 'branchId'))
const profile = computed(() => query.data.value ?? null)

const pt = {
  root: { class: 'dfb-dialog fwb-drawer-root' },
  mask: { class: 'dfb-dialog-mask' },
  content: { class: 'fwb-drawer-content' },
}

function money(n: number | null | undefined) {
  return n == null || Number.isFinite(n) === false || n === 0 ? '—' : new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)
}

function date(v: string | null | undefined) {
  return v ? v.slice(0, 10) : '—'
}
</script>
