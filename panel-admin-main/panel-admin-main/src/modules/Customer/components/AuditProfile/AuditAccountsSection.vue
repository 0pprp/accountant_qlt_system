<template>
  <section class="space-y-3">
    <h3 class="text-base font-semibold">الحسابات والتسديدات</h3>
    <p v-if="!rows.length" class="text-sm opacity-70">لا حسابات ضمن النطاق</p>
    <div v-else class="flex flex-wrap gap-2">
      <button
        v-for="row in rows"
        :key="row.orderId"
        type="button"
        class="rounded-full border px-3 py-1 text-sm"
        :class="
          selectedOrderId === row.orderId
            ? 'border-[#006A64] bg-[#006A64] text-white'
            : 'border-[#d7e2e0] bg-white'
        "
        @click="selectedOrderId = row.orderId"
      >
        حساب #{{ row.orderId }}
      </button>
    </div>
    <article v-if="selected" class="rounded-xl border border-[#d7e2e0] p-3 text-sm">
      <p>
        إجمالي:
        {{ fmt(selected.sellAmount) }} — مسدد:
        {{ fmt(selected.paidAmount) }} — متبقي:
        {{ fmt(selected.remaining) }}
      </p>
      <table class="mt-3 w-full text-start">
        <thead>
          <tr class="border-b">
            <th class="py-1">التاريخ</th>
            <th>المبلغ</th>
            <th>الوصل</th>
            <th>ملاحظة</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in selected.payments" :key="p.id" class="border-b">
            <td class="py-1">{{ p.date }}</td>
            <td>{{ fmt(p.amount) }}</td>
            <td>{{ p.receiptNumber || '—' }}</td>
            <td>{{ p.note || '—' }}</td>
          </tr>
        </tbody>
      </table>
    </article>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { CustomerAuditProfile } from '../../requests/queries/useCustomerAuditProfileQuery'

const props = defineProps<{
  accounts: NonNullable<CustomerAuditProfile['accounts']>
}>()

const rows = computed(() => props.accounts.rows)
const selectedOrderId = ref<number | null>(rows.value[0]?.orderId ?? null)

watch(
  rows,
  (r) => {
    if (!r.some((x) => x.orderId === selectedOrderId.value)) {
      selectedOrderId.value = r[0]?.orderId ?? null
    }
  },
  { immediate: true }
)

const selected = computed(() =>
  rows.value.find((r) => r.orderId === selectedOrderId.value)
)

function fmt(v?: number | null) {
  if (v == null) return 'محجوب'
  return new Intl.NumberFormat('ar-IQ').format(v)
}
</script>
