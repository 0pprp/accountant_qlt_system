<template>
  <aside class="sr-details" aria-live="polite">
    <header class="sr-details-head">
      <div class="sr-details-head__text">
        <p v-if="request" class="sr-details-eyebrow">طلب #{{ request.id }}</p>
        <h2 class="sr-details-title">
          {{ request?.customerName || $t('salesRequests.details.empty') }}
        </h2>
      </div>
      <button
        type="button"
        class="sr-exit"
        :aria-label="$t('salesRequests.details.exit')"
        @click="$emit('back')"
      >
        <i class="pi pi-times" aria-hidden="true" />
        <span>{{ $t('salesRequests.details.exit') }}</span>
      </button>
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
      <section class="sr-section" aria-label="بيانات الطلب">
        <h3 class="sr-section__title">بيانات الطلب</h3>
        <dl class="sr-dl">
          <div>
            <dt>المنتج المطلوب</dt>
            <dd>{{ request.requestedProduct || 'غير مسجل' }}</dd>
          </div>
          <div>
            <dt>العنوان</dt>
            <dd>{{ request.address || 'غير مسجل' }}</dd>
          </div>
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
            <dd>
              {{ request.branchName || request.branchId
              }}{{ request.provinceName ? ` · ${request.provinceName}` : '' }}
            </dd>
          </div>
          <div>
            <dt>أُرسل بواسطة</dt>
            <dd>{{ request.createdByName || 'غير مسجل' }}</dd>
          </div>
          <div>
            <dt>{{ $t('salesRequests.card.addedAt') }}</dt>
            <dd>{{ formatDate(request.createdAt) }}</dd>
          </div>
          <div>
            <dt>{{ $t('salesRequests.card.lastStatus') }}</dt>
            <dd>{{ formatDate(request.statusChangedAtUtc) }}</dd>
          </div>
          <div
            v-if="request.submissionNote"
            class="sr-full-field"
          >
            <dt>ملاحظة المُرسل</dt>
            <dd>{{ request.submissionNote }}</dd>
          </div>
          <div v-if="request.originalExternalId">
            <dt>مرجع المصدر</dt>
            <dd>{{ request.originalExternalId }}</dd>
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
            class="sr-full-field"
          >
            <dt>{{ $t('salesRequests.sent.caveatTitle') }}</dt>
            <dd>{{ $t('salesRequests.sent.caveatBody') }}</dd>
          </div>
        </dl>
      </section>

      <div
        v-if="(request.existingCustomerId && canProfile) || (request.linkedOrderId && canOrder)"
        class="sr-links"
      >
        <RouterLink
          v-if="request.existingCustomerId && canProfile"
          :to="{
            name: 'CustomerAuditProfileRoute',
            params: { id: request.existingCustomerId },
            query: {
              returnTo: 'SalesRequestsWorkspaceRoute',
              requestId: String(request.id),
              tab: activeTab || 'unassigned',
            },
          }"
          class="sr-chip-link"
        >
          {{ $t('salesRequests.details.profile') }}
        </RouterLink>
        <RouterLink
          v-if="request.linkedOrderId && canOrder"
          :to="{
            name: 'SaleViewRoute',
            params: { saleId: request.linkedOrderId },
          }"
          class="sr-chip-link"
        >
          {{ $t('salesRequests.details.linkedSale') }} #{{ request.linkedOrderId }}
        </RouterLink>
      </div>

      <section class="sr-section">
        <RequestTimeline
          :items="timeline"
          :loading="timelineLoading"
          :error="timelineError"
          :can-read="canTimeline"
        />
      </section>

      <div
        v-if="showAudit"
        class="sr-section sr-audit-slot"
      >
        <h3 class="sr-section__title">المطابقة والتدقيق</h3>
        <slot name="audit" />
      </div>

      <div class="sr-section sr-card-toolbar">
        <RequestLifecycleActions
          :request="request"
          :active-tab="activeTab"
          @done="$emit('action-done')"
        />
      </div>

      <footer class="sr-details-foot">
        <button
          type="button"
          class="sr-exit sr-exit--block"
          @click="$emit('back')"
        >
          <i class="pi pi-times" aria-hidden="true" />
          <span>{{ $t('salesRequests.details.exit') }}</span>
        </button>
      </footer>
    </template>
  </aside>
</template>

<script setup lang="ts">
import type { SalesRequestListItem, SalesRequestTimelineItem } from '../types'
import RequestTimeline from './RequestTimeline.vue'
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
  return new Intl.DateTimeFormat('en-US', {
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
  --neo-bg: #e8f0ef;
  --neo-ink: #00201e;
  --neo-brand: #006a64;
  --neo-soft: #f4f8f7;
  --neo-dark: rgba(0, 32, 30, 0.12);
  --neo-light: rgba(255, 255, 255, 0.92);
  color: var(--neo-ink);
  background: var(--neo-bg);
  border-radius: 18px;
  padding: 1.1rem 1.15rem 1.25rem;
  min-height: 12rem;
  box-shadow:
    8px 8px 18px var(--neo-dark),
    -8px -8px 18px var(--neo-light);
}

.sr-details-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.sr-details-head__text {
  min-width: 0;
  flex: 1;
}

.sr-details-eyebrow {
  margin: 0 0 0.2rem;
  font-size: 0.85rem;
  color: #5f736f;
  font-weight: 650;
}

.sr-details-title {
  margin: 0;
  font-size: 1.35rem;
  line-height: 1.55;
  font-weight: 800;
  overflow-wrap: anywhere;
}

.sr-exit {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  flex: none;
  min-height: 44px;
  min-width: 44px;
  padding: 0.45rem 0.95rem;
  border: 0;
  border-radius: 14px;
  background: var(--neo-soft);
  color: var(--neo-brand);
  font: inherit;
  font-weight: 800;
  cursor: pointer;
  box-shadow:
    5px 5px 12px var(--neo-dark),
    -5px -5px 12px var(--neo-light);
}

.sr-exit:active,
.sr-exit--block:active {
  box-shadow:
    inset 3px 3px 8px rgba(0, 32, 30, 0.12),
    inset -3px -3px 8px rgba(255, 255, 255, 0.85);
}

.sr-exit--block {
  width: 100%;
  min-height: 48px;
}

.sr-moved {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 0.85rem;
  padding: 0.7rem 0.85rem;
  border-radius: 14px;
  background: #fff8e6;
  font-size: 0.9rem;
  box-shadow:
    inset 2px 2px 6px rgba(138, 75, 0, 0.08),
    inset -2px -2px 6px rgba(255, 255, 255, 0.7);
}

.sr-section {
  margin-bottom: 0.95rem;
  padding: 0.95rem 1rem;
  border-radius: 16px;
  background: var(--neo-soft);
  box-shadow:
    inset 3px 3px 8px rgba(0, 32, 30, 0.07),
    inset -3px -3px 8px rgba(255, 255, 255, 0.88);
}

.sr-section__title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
  font-weight: 800;
  color: var(--neo-brand);
}

.sr-dl {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.85rem 1.1rem;
  margin: 0;
}

.sr-dl dt {
  margin: 0;
  font-size: 0.78rem;
  color: #61756f;
}

.sr-dl dd {
  margin: 0.15rem 0 0;
  font-size: 0.98rem;
  line-height: 1.65;
  font-weight: 650;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}

.sr-full-field {
  grid-column: 1 / -1;
}

.sr-links {
  display: flex;
  flex-wrap: wrap;
  gap: 0.55rem;
  margin-bottom: 0.95rem;
}

.sr-chip-link,
.sr-link {
  display: inline-flex;
  align-items: center;
  min-height: 42px;
  padding: 0.35rem 0.85rem;
  border-radius: 999px;
  background: var(--neo-soft);
  color: var(--neo-brand);
  font-weight: 700;
  text-decoration: none;
  box-shadow:
    4px 4px 10px var(--neo-dark),
    -4px -4px 10px var(--neo-light);
}

.sr-link {
  border: 0;
  font: inherit;
  cursor: pointer;
}

.sr-audit-slot {
  margin-top: 0;
}

.sr-details-foot {
  margin-top: 0.35rem;
}

@media (max-width: 560px) {
  .sr-details {
    padding: 0.95rem 0.85rem 1.1rem;
  }
  .sr-dl {
    grid-template-columns: 1fr;
  }
  .sr-details-title {
    font-size: 1.15rem;
  }
}
</style>
