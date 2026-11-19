<template>
  <div class="dfb-panel has-cards">
    <table class="dfb-ctable">
      <thead>
        <tr>
          <th scope="col">الزبون</th>
          <th scope="col">البيع</th>
          <th scope="col">السداد</th>
          <th scope="col">القسط اليومي</th>
          <th scope="col">آخر تسديد</th>
          <th scope="col">المؤشرات</th>
          <th scope="col">التقييم</th>
          <th v-if="showPayments" scope="col"><span class="sr-only">التسديدات</span></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="c in customers" :key="c.customerId">
          <td class="is-wide">
            <button v-if="canOpenProfile" type="button" class="dfb-name-link" @click="emit('profile', c.customerId)">{{ c.customerName }}</button>
            <span v-else class="dfb-cell-main">{{ c.customerName }}</span>
            <span class="dfb-cell-sub">
              {{ [c.shopName, c.address].filter(Boolean).join(' · ') || '—' }}
            </span>
            <span v-if="c.phoneNumber" class="dfb-cell-sub"><span class="dfb-num">{{ c.phoneNumber }}</span></span>
          </td>
          <td class="is-num">
            <span class="dfb-cell-label">البيع</span>
            <span class="dfb-cell-main"><span class="dfb-num">{{ c.dateSaleDevice ?? '—' }}</span></span>
            <span class="dfb-cell-sub">{{ money(c.amountTotalSales) }}</span>
          </td>
          <td class="is-wide">
            <span class="dfb-cell-label">السداد</span>
            <span class="dfb-cell-sub">
              الواصل <strong>{{ money(c.receiptsTotal) }}</strong> · الباقي <strong>{{ money(c.amountRemaining) }}</strong>
            </span>
            <div class="dfb-progress" role="img" :aria-label="`نسبة الواصل ${paidPercent(c)}%`">
              <span :style="{ inlineSize: `${paidPercent(c)}%` }" />
            </div>
          </td>
          <td class="is-num">
            <span class="dfb-cell-label">القسط اليومي</span>
            <span class="dfb-cell-main">{{ money(c.amountDaySales) }}</span>
          </td>
          <td class="is-num">
            <span class="dfb-cell-label">آخر تسديد</span>
            <span class="dfb-cell-main"><span class="dfb-num">{{ c.lastPaymentDate ?? '—' }}</span></span>
            <span class="dfb-cell-sub">{{ c.lastPaymentAmount != null ? money(c.lastPaymentAmount) : '—' }}</span>
          </td>
          <td>
            <span class="dfb-cell-label">المؤشرات</span>
            <span class="dfb-cell-sub"><span class="dfb-num">{{ c.numberOfDayDevice }}</span> يوم · <span class="dfb-num">{{ c.countReceiptDevice }}</span> وصل</span>
            <span class="dfb-cell-sub">نسبة الاستحصال <span class="dfb-num">{{ c.receiptRateDevice }}</span></span>
          </td>
          <td>
            <span class="dfb-cell-label">التقييم</span>
            <span v-if="c.realRatingLabel" class="dfb-badge" :class="`tier-${c.realRatingLabel}`">{{ c.realRatingLabel }}</span>
            <span v-else>—</span>
            <span v-if="c.itemsNames" class="dfb-cell-sub">{{ c.itemsNames }}</span>
          </td>
          <td v-if="showPayments" class="is-wide">
            <button type="button" class="dfb-button is-small" @click="emit('payments', c.customerId)">التسديدات</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { DelegateFollowupCustomer } from '../domain/types'

defineProps<{
  customers: DelegateFollowupCustomer[]
  canOpenProfile: boolean
  showPayments?: boolean
}>()

const emit = defineEmits<{ profile: [customerId: number]; payments: [customerId: number] }>()

function money(n: number | null | undefined) {
  return n == null || Number.isFinite(n) === false ? '—' : new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)
}

function paidPercent(c: DelegateFollowupCustomer) {
  if (!(c.amountTotalSales > 0)) return 0
  return Math.max(0, Math.min(100, Math.round((c.receiptsTotal / c.amountTotalSales) * 100)))
}
</script>

<style scoped>
.sr-only { block-size: 1px; clip: rect(0 0 0 0); inline-size: 1px; overflow: hidden; position: absolute; white-space: nowrap; }
</style>
