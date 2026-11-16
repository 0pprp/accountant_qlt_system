<template>
  <div
    class="hdr-dates"
    dir="rtl"
  >
    <AppDateField
      :model-value="startDate"
      label="من"
      @update:model-value="onStart"
    />
    <AppDateField
      :model-value="endDate"
      label="إلى"
      @update:model-value="onEnd"
    />
    <AppButton
      variant="ghost"
      size="sm"
      type="button"
      @click="onClear"
    >
      مسح
    </AppButton>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { AppButton, AppDateField } from '@/modules/Core/components/ui'

const route = useRoute()
const router = useRouter()

function asDate(value: unknown): string {
  const raw = Array.isArray(value) ? value[0] : value
  if (typeof raw !== 'string') return ''
  const match = /^(\d{4}-\d{2}-\d{2})/.exec(raw)
  return match ? match[1] : ''
}

const startDate = computed(() => asDate(route.query.startDate))
const endDate = computed(() => asDate(route.query.endDate))

function patch(next: { startDate?: string | null; endDate?: string | null }) {
  const query = { ...route.query }
  if (next.startDate === null) delete query.startDate
  else if (next.startDate !== undefined) query.startDate = next.startDate
  if (next.endDate === null) delete query.endDate
  else if (next.endDate !== undefined) query.endDate = next.endDate

  const sameStart = (query.startDate ?? null) === (route.query.startDate ?? null)
  const sameEnd = (query.endDate ?? null) === (route.query.endDate ?? null)
  if (sameStart && sameEnd) return

  void router.push({ query })
}

function onStart(v: string) {
  patch({ startDate: v || null })
}

function onEnd(v: string) {
  patch({ endDate: v || null })
}

function onClear() {
  patch({ startDate: null, endDate: null })
}
</script>

<style scoped>
.hdr-dates {
  display: flex;
  flex-wrap: wrap;
  align-items: end;
  gap: 12px;
  margin: 0;
}
.hdr-dates :deep(.q-field) {
  min-width: 160px;
}
</style>
