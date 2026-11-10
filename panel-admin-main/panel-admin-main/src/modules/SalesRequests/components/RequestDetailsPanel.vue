<template>
  <aside class="sr-details" aria-live="polite">
    <header class="sr-details-head">
      <button
        v-if="showBack"
        type="button"
        class="sr-back"
        @click="$emit('back')"
      >
        {{ $t('salesRequests.details.back') }}
      </button>
      <h2 class="sr-details-title">
        {{ request?.customerName || $t('salesRequests.details.empty') }}
      </h2>
    </header>

    <div
      v-if="movedMessage"
      class="sr-moved"
      role="status"
    >
      <span>{{ movedMessage }}</span>
      <button
        v-if="movedTab"
        type="button"
        class="sr-link"
        @click="$emit('open-tab', movedTab)"
      >
        {{ $t('salesRequests.details.openTab') }}
      </button>
    </div>

    <template v-if="request">
      <dl class="sr-dl">
        <div><dt>رقم الطلب</dt><dd>#{{ request.id }}</dd></div>
        <div><dt>أُرسل بواسطة</dt><dd>{{ request.createdByName || 'غير مسجل' }}</dd></div>
        <div><dt>المنتج المطلوب</dt><dd>{{ request.requestedProduct || 'غير مسجل' }}</dd></div>
        <div><dt>العنوان</dt><dd>{{ request.address || 'غير مسجل' }}</dd></div>
        <div v-if="request.submissionNote" class="sr-full-field"><dt>ملاحظة المُرسل</dt><dd>{{ request.submissionNote }}</dd></div>
        <div v-if="request.provinceName"><dt>المحافظة</dt><dd>{{ request.provinceName }}</dd></div>
        <div v-if="request.originalExternalId"><dt>مرجع المصدر</dt><dd>{{ request.originalExternalId }}</dd></div>
        <div>
          <dt>{{ $t('salesRequests.card.status') }}</dt>
          <dd>{{ request.statusLabel }}</dd>
        </div>
        <div>
          <dt>{{ $t('salesRequests.card.phone') }}</dt>
          <dd>{{ request.customerPhone || '—' }}</dd>
        </div>
        <div>
          <dt>{{ $t('salesRequests.card.employee') }}</dt>
          <dd>{{ request.targetEmployeeName || '—' }}</dd>
        </div>
        <div>
          <dt>{{ $t('salesRequests.card.source') }}</dt>
          <dd>{{ request.sourceTypeLabel }}</dd>
        </div>
        <div>
          <dt>{{ $t('salesRequests.card.branch') }}</dt>
          <dd>{{ request.branchName || request.branchId }}</dd>
        </div>
        <div>
          <dt>{{ $t('salesRequests.card.addedAt') }}</dt>
          <dd>{{ formatDate(request.createdAt) }}</dd>
        </div>
        <div>
          <dt>{{ $t('salesRequests.card.lastStatus') }}</dt>
          <dd>{{ formatDate(request.statusChangedAtUtc) }}</dd>
        </div>
        <div v-if="request.lastStatusNote">
          <dt>{{ $t('salesRequests.details.note') }}</dt>
          <dd>{{ request.lastStatusNote }}</dd>
        </div>
        <div v-if="request.rejectReason">
          <dt>{{ $t('salesRequests.details.rejectReason') }}</dt>
          <dd>{{ request.rejectReason }}</dd>
        </div>
        <div v-if="request.exceptionState">
          <dt>{{ $t('salesRequests.lifecycle.exceptionState') }}</dt>
          <dd>{{ exceptionStateLabel(request.exceptionState) }}</dd>
        </div>
        <div v-if="request.deleteReason">
          <dt>{{ $t('salesRequests.lifecycle.deleteReason') }}</dt>
          <dd>{{ request.deleteReason }}</dd>
        </div>
        <div
          v-if="activeTab === 'sent'"
          class="sr-sent-note"
        >
          <dt>{{ $t('salesRequests.sent.caveatTitle') }}</dt>
          <dd>{{ $t('salesRequests.sent.caveatBody') }}</dd>
        </div>
      </dl>

      <div class="sr-links">
        <RouterLink
          v-if="request.existingCustomerId && canProfile"
          :to="{
            name: 'CustomerAuditProfileRoute',
            params: { id: request.existingCustomerId },
          }"
          class="sr-link"
        >
          {{ $t('salesRequests.details.profile') }}
        </RouterLink>
        <RouterLink
          v-if="request.linkedOrderId && canOrder"
          :to="{
            name: 'SaleViewRoute',
            params: { saleId: request.linkedOrderId },
          }"
          class="sr-link"
        >
          {{ $t('salesRequests.details.linkedSale') }} #{{ request.linkedOrderId }}
        </RouterLink>
      </div>

      <RequestTimeline
        :items="timeline"
        :loading="timelineLoading"
        :error="timelineError"
        :can-read="canTimeline"
      />

      <div
        v-if="showAudit"
        class="sr-audit-slot"
      >
        <slot name="audit" />
      </div>

      <RequestLifecycleActions
        :request="request"
        :active-tab="activeTab"
        @done="$emit('action-done')"
      />


    </template>
  </aside>
</template>

<script setup lang="ts">
import type { SalesRequestListItem, SalesRequestTimelineItem } from '../types'
import RequestTimeline from './RequestTimeline.vue'
import RequestActions from './RequestActions.vue'
import RequestLifecycleActions from './RequestLifecycleActions.vue'

withDefaults(
  defineProps<{
    request: SalesRequestListItem | null
    timeline: SalesRequestTimelineItem[]
    timelineLoading?: boolean
    timelineError?: boolean
    canTimeline?: boolean
    canProfile?: boolean
    canOrder?: boolean
    showBack?: boolean
    showAudit?: boolean
    movedMessage?: string | null
    movedTab?: string | null
    activeTab?: string
    draft?: { reject?: string; exception?: string; assignReason?: string }
  }>(),
  { activeTab: 'unassigned' }
)

defineEmits<{
  back: []
  'open-tab': [string]
  'action-done': []
  'draft-change': [{ reject?: string; exception?: string; assignReason?: string }]
}>()

function formatDate(value?: string | null) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('ar-IQ', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

function exceptionStateLabel(state: string) {
  switch (state) {
    case 'pending':
      return 'بانتظار القرار'
    case 'approved':
      return 'موافق (غير مستهلك)'
    case 'approved_consumed':
      return 'موافق ومُستهلك'
    case 'rejected':
      return 'مرفوض'
    default:
      return state
  }
}
</script>

<style scoped>
.sr-details {
  border-radius: 1rem;
  border: 1px solid #d7e2e0;
  background: #fff;
  padding: 1rem;
  color: #00201e;
  font-family: 'ElMessiri', 'El Messiri', sans-serif;
  min-height: 12rem;
}
.sr-details-head {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}
.sr-details-title {
  margin: 0;
  font-size: 1.15rem;
}
.sr-back {
  min-height: 44px;
  min-width: 44px;
  border-radius: 0.75rem;
  border: 1px solid #d7e2e0;
  background: #f8f8f8;
  padding: 0.35rem 0.75rem;
  font-family: inherit;
}
.sr-moved {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 0.75rem;
  padding: 0.55rem 0.75rem;
  border-radius: 0.75rem;
  background: #fff8e6;
  font-size: 0.85rem;
}
.sr-dl {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.55rem;
  margin: 0 0 0.85rem;
  font-size: 0.85rem;
}
.sr-dl dt {
  opacity: 0.65;
  font-size: 0.75rem;
}
.sr-dl dd {
  margin: 0.1rem 0 0;
}
.sr-links {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1rem;
}
.sr-link {
  color: #006a64;
  text-decoration: underline;
  background: none;
  border: none;
  font-family: inherit;
  cursor: pointer;
  min-height: 44px;
  display: inline-flex;
  align-items: center;
}
.sr-audit-slot {
  margin: 1rem 0;
  padding-top: 0.75rem;
  border-top: 1px solid #d7e2e0;
}
.sr-details { padding:24px; border-radius:18px; }
.sr-details-title { font-size:24px; line-height:1.7; overflow-wrap:anywhere; }
.sr-dl { gap:20px; }
.sr-dl dt { font-size:14px; color:#61756f; }
.sr-dl dd { font-size:16px; line-height:1.8; white-space:pre-wrap; overflow-wrap:anywhere; }
.sr-full-field { grid-column:1 / -1; }
@media(max-width:480px) { .sr-details { padding:18px 15px; } }
</style>
