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
      <ActionRow
        :primary="{
          label: selected
            ? 'تفاصيل الطلب مفتوحة'
            : request.tabKey === 'unassigned'
              ? 'تدقيق الطلب ونتائج المطابقة'
              : 'تفاصيل الطلب وسجل الحركة',
        }"
        @primary="$emit('select'); $emit('audit')"
      />
      <slot v-if="expanded" />
    </template>
  </EntityCard>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import EntityCard from '@/modules/Core/components/ui/EntityCard.vue'
import DataField from '@/modules/Core/components/ui/DataField.vue'
import ActionRow from '@/modules/Core/components/ui/ActionRow.vue'
import { formatDurationInStatus, type SalesRequestListItem } from '../types'

const props = defineProps<{
  request: SalesRequestListItem
  selected?: boolean
  expanded?: boolean
}>()
defineEmits<{ select: []; audit: [] }>()

const statusTone = computed(() => {
  const tab = props.request.tabKey
  if (tab === 'unassigned' || tab === 'pending') return 'warning' as const
  if (tab === 'rejected' || tab === 'deleted') return 'danger' as const
  if (tab === 'completed' || tab === 'sold') return 'success' as const
  return 'neutral' as const
})

function formatDate(value?: string | null) {
  if (!value || Number.isNaN(new Date(value).getTime())) return 'غير مسجل'
  return new Intl.DateTimeFormat('ar-IQ', {
    dateStyle: 'medium',
    timeStyle: 'short',
    timeZone: 'Asia/Baghdad',
  }).format(new Date(value))
}
</script>

<style scoped>
.sr-note {
  border-inline-start: 3px solid #b5cec8;
  padding: 10px 14px;
  margin-top: 12px;
  background: #fafcfb;
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
