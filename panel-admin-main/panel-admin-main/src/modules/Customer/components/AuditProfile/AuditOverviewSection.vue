<template>
  <section v-if="overview" class="space-y-2 text-sm">
    <h3 class="text-base font-semibold">نظرة عامة</h3>
    <dl class="grid grid-cols-2 gap-2 md:grid-cols-3">
      <div>
        <dt class="opacity-70">اسم الأم</dt>
        <dd>{{ overview.motherName || '—' }}</dd>
      </div>
      <div>
        <dt class="opacity-70">الهوية</dt>
        <dd>{{ overview.nationalCode || '—' }}</dd>
      </div>
      <div>
        <dt class="opacity-70">العنوان</dt>
        <dd>{{ overview.address || '—' }}</dd>
      </div>
      <div>
        <dt class="opacity-70">النشاط</dt>
        <dd>{{ overview.businessType || '—' }}</dd>
      </div>
      <div>
        <dt class="opacity-70">التقييم</dt>
        <dd>{{ overview.ratingLabel || '—' }}</dd>
      </div>
      <div>
        <dt class="opacity-70">إجمالي / مسدد / متبقي</dt>
        <dd>
          {{ fmt(overview.sellTotal) }} /
          {{ fmt(overview.paidTotal) }} /
          {{ fmt(overview.remaining) }}
        </dd>
      </div>
    </dl>
  </section>
</template>

<script setup lang="ts">
import type { CustomerAuditProfile } from '../../requests/queries/useCustomerAuditProfileQuery'

defineProps<{
  overview: NonNullable<CustomerAuditProfile['overview']>
}>()

function fmt(v?: number | null) {
  if (v == null) return 'محجوب'
  return new Intl.NumberFormat('ar-IQ').format(v)
}
</script>
