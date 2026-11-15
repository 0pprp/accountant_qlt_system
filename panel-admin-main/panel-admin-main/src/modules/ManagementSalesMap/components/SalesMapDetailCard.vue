<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import type { SalesMapOrderDetail } from '../domain/types'

const props = defineProps<{
  detail: SalesMapOrderDetail | null
  loading: boolean
  error: boolean
  mobile?: boolean
}>()

const emit = defineEmits<{
  close: []
  retry: []
}>()

const { t } = useI18n()

function money(n: number) {
  return new Intl.NumberFormat('ar-IQ', {
    maximumFractionDigits: 0,
  }).format(n)
}

const lastPaymentText = computed(() => {
  if (!props.detail) return ''
  if (props.detail.lastPaymentDisplay) return props.detail.lastPaymentDisplay
  if (props.detail.lastPaymentDate == null) return t('managementSalesMap.detail.noPayment')
  const amt =
    props.detail.lastPaymentAmount != null
      ? money(props.detail.lastPaymentAmount)
      : '—'
  return `${props.detail.lastPaymentDate} · ${amt}`
})
</script>

<template>
  <aside
    class="msm-detail"
    :class="{ 'msm-detail--mobile': mobile }"
    dir="rtl"
  >
    <header class="msm-detail__head">
      <h3>{{ t('managementSalesMap.detail.title') }}</h3>
      <button type="button" class="msm-detail__close" @click="emit('close')">
        {{ t('managementSalesMap.detail.close') }}
      </button>
    </header>

    <div v-if="loading" class="msm-detail__state">…</div>
    <div v-else-if="error" class="msm-detail__state msm-detail__state--err">
      <p>{{ t('managementSalesMap.detail.loadError') }}</p>
      <button type="button" @click="emit('retry')">
        {{ t('managementSalesMap.map.retry') }}
      </button>
    </div>
    <dl v-else-if="detail" class="msm-detail__grid">
      <div>
        <dt>{{ t('managementSalesMap.detail.customer') }}</dt>
        <dd>{{ detail.customerName }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.orderId') }}</dt>
        <dd>{{ detail.orderId }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.list') }}</dt>
        <dd>{{ detail.listLetter }} — {{ detail.listName }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.status') }}</dt>
        <dd>{{ detail.statusLabel }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.rating') }}</dt>
        <dd>{{ detail.ratingLabel || t('managementSalesMap.detail.unrated') }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.sell') }}</dt>
        <dd>{{ money(detail.sellAmount) }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.paid') }}</dt>
        <dd>{{ money(detail.paidAmount) }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.remaining') }}</dt>
        <dd>{{ money(detail.remainingAmount) }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.saleDate') }}</dt>
        <dd>{{ detail.saleDate || '—' }}</dd>
      </div>
      <div>
        <dt>{{ t('managementSalesMap.detail.lastPayment') }}</dt>
        <dd>{{ lastPaymentText }}</dd>
      </div>
    </dl>
  </aside>
</template>

<style scoped>
.msm-detail {
  background: #fff;
  border-radius: 16px;
  border: 1px solid #d5e2e0;
  padding: 16px;
  box-shadow: 0 8px 24px rgba(0, 32, 30, 0.08);
  max-height: 100%;
  overflow: auto;
}
.msm-detail--mobile {
  border-radius: 20px 20px 0 0;
  padding-bottom: calc(16px + env(safe-area-inset-bottom, 0px));
}
.msm-detail__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}
.msm-detail__head h3 {
  margin: 0;
  font-size: 1.05rem;
  color: #00201e;
}
.msm-detail__close {
  border: 0;
  background: #f4f7f7;
  color: #006a64;
  border-radius: 999px;
  padding: 6px 12px;
  font-weight: 700;
  cursor: pointer;
}
.msm-detail__grid {
  display: grid;
  gap: 10px;
  margin: 0;
}
.msm-detail__grid dt {
  font-size: 0.75rem;
  color: #6f797a;
}
.msm-detail__grid dd {
  margin: 2px 0 0;
  font-weight: 700;
  color: #00201e;
}
.msm-detail__state {
  text-align: center;
  padding: 24px 8px;
  color: #6f797a;
}
.msm-detail__state--err button {
  margin-top: 8px;
  border: 0;
  background: #006a64;
  color: #fff;
  border-radius: 10px;
  padding: 8px 14px;
  cursor: pointer;
}
</style>
