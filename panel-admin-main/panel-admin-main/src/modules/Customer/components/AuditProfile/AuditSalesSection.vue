<template>
  <section class="space-y-3">
    <h3 class="text-base font-semibold">المبيعات</h3>
    <p v-if="!sales.length" class="text-sm opacity-70">لا مبيعات ضمن النطاق</p>
    <article
      v-for="sale in sales"
      :key="sale.id"
      class="rounded-xl border border-[#d7e2e0] p-3 text-sm"
    >
      <div class="font-semibold">بيع #{{ sale.id }}</div>
      <p class="opacity-70">{{ formatDate(sale.createdAt) }}</p>
      <p>الموظف: {{ sale.sellerName || '—' }}</p>
      <p>القائمة: {{ sale.orderListName || '—' }}</p>
      <p>
        المبلغ: {{ fmt(sale.sellAmount) }} — مقدمة:
        {{ fmt(sale.prepaymentAmount) }}
      </p>
      <p>المنتجات: {{ sale.products.join('، ') || '—' }}</p>
    </article>
  </section>
</template>

<script setup lang="ts">
import type { CustomerAuditProfile } from '../../requests/queries/useCustomerAuditProfileQuery'

defineProps<{
  sales: NonNullable<CustomerAuditProfile['sales']>
}>()

function fmt(v?: number | null) {
  if (v == null) return 'محجوب'
  return new Intl.NumberFormat('ar-IQ').format(v)
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ar-IQ', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}
</script>
