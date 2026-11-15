<template>
  <EntityCard
    class="sr-card-host"
    :eyebrow="`طلب #${request.id}`"
    :name="request.customerName"
    :status-label="request.statusLabel"
    :status-tone="statusTone"
    :selected="selected"
  >
    <template #identity-extra>
      <DataField
        label="الهاتف"
        :value="request.customerPhone"
        is-phone
        empty-label="رقم الهاتف غير مسجل"
      />
    </template>

    <template #fields>
      <DataField label="المنتج المطلوب" :value="request.requestedProduct" />
      <DataField label="العنوان" :value="request.address" />
      <DataField
        label="موظف المبيعات"
        :value="
          request.targetEmployeeName ||
          (request.targetEmployeeId ? `موظف #${request.targetEmployeeId}` : 'لم يُسند بعد')
        "
      />
      <DataField label="أُرسل بواسطة" :value="request.createdByName" />
      <DataField
        v-if="request.exceptionState"
        label="حالة الاستثناء"
        :value="exceptionStateLabel(request.exceptionState)"
      />
      <DataField label="مصدر الطلب" :value="request.sourceTypeLabel" />
      <DataField
        label="الفرع والمحافظة"
        :value="
          `${request.branchName || `فرع #${request.branchId}`}${request.provinceName ? ` · ${request.provinceName}` : ''}`
        "
      />
      <DataField
        v-if="request.originalExternalId"
        label="مرجع المصدر"
        :value="request.originalExternalId"
      />
      <DataField
        v-if="request.linkedOrderId"
        label="رقم البيع المرتبط"
        :value="`#${request.linkedOrderId}`"
      />
      <DataField label="تاريخ الإضافة" :value="formatDate(request.createdAt)" />
      <DataField
        label="آخر تغيير للحالة"
        :value="`${formatDate(request.statusChangedAtUtc)} — ${formatDurationInStatus(request.statusChangedAtUtc, request.createdAt)}`"
      />
    </template>

    <template #notes>
      <div v-if="request.submissionNote" class="sr-note">
        <strong>ملاحظة المُرسل</strong>
        <p>{{ request.submissionNote }}</p>
      </div>
      <div v-if="request.lastStatusNote" class="sr-note">
        <strong>آخر ملاحظة على الطلب</strong>
        <p>{{ request.lastStatusNote }}</p>
      </div>
      <div v-if="request.rejectReason" class="sr-note sr-reason">
        <strong>سبب الرفض</strong>
        <p>{{ request.rejectReason }}</p>
      </div>
      <div v-if="request.deleteReason" class="sr-note sr-reason">
        <strong>سبب الحذف</strong>
        <p>{{ request.deleteReason }}</p>
      </div>
    </template>

    <template #actions>
      <slot name="matching" />
      <slot name="actions" />
      <slot v-if="expanded" />
    </template>
  </EntityCard>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import EntityCard from '@/modules/Core/components/ui/EntityCard.vue'
import DataField from '@/modules/Core/components/ui/DataField.vue'
import { formatDurationInStatus, type SalesRequestListItem } from '../types'

const props = defineProps<{
  request: SalesRequestListItem
  selected?: boolean
  expanded?: boolean
}>()
defineEmits<{ select: [] }>()

const statusTone = computed(() => {
  const tab = props.request.tabKey
  if (tab === 'unassigned' || tab === 'pending') return 'warning' as const
  if (tab === 'rejected' || tab === 'deleted') return 'danger' as const
  if (tab === 'completed' || tab === 'sold') return 'success' as const
  return 'neutral' as const
})

function formatDate(value?: string | null) {
  if (!value || Number.isNaN(new Date(value).getTime())) return 'غير مسجل'
  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Baghdad',
  }).format(new Date(value))
}

function exceptionStateLabel(state: string) {
  switch (state) {
    case 'pending':
      return 'بانتظار قرار المفوض'
    case 'approved':
      return 'مقبول — بانتظار الاستهلاك'
    case 'approved_consumed':
      return 'مقبول ومُستهلك'
    case 'rejected':
      return 'مرفوض'
    default:
      return state
  }
}
</script>

<style scoped>
.sr-card-host :deep(.q-entity-card) {
  border: 0;
  border-inline-start: 0;
  background: #e8f0ef;
  border-radius: 18px;
  box-shadow:
    8px 8px 18px rgba(0, 32, 30, 0.12),
    -8px -8px 18px rgba(255, 255, 255, 0.9);
}
.sr-card-host :deep(.q-entity-card.selected) {
  background: linear-gradient(145deg, #dfecea, #edf5f3);
  box-shadow:
    inset 5px 5px 12px rgba(0, 32, 30, 0.12),
    inset -5px -5px 12px rgba(255, 255, 255, 0.85);
}
.sr-note {
  border-inline-start: 3px solid #b5cec8;
  padding: 10px 14px;
  margin-top: 12px;
  background: #f4f8f7;
  border-radius: 12px;
  box-shadow:
    inset 2px 2px 5px rgba(0, 32, 30, 0.06),
    inset -2px -2px 5px rgba(255, 255, 255, 0.85);
  font-size: 15px;
  line-height: 1.8;
}
.sr-note strong {
  font-size: 14px;
  color: #526b67;
}
.sr-note p {
  margin: 4px 0 0;
  white-space: pre-wrap;
}
.sr-reason {
  border-color: #cc8177;
  background: #fff8f6;
}
</style>
