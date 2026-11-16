<script setup lang="ts">
import { computed } from 'vue'
import { AppSelect } from '@/modules/Core/components/ui'

const props = defineProps<{
  branches: Array<{ id: number; name: string }>
  modelValue: number | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: number | null]
}>()

const options = computed(() =>
  props.branches.map((b) => ({ label: b.name, value: b.id })),
)

function onUpdate(v: string | number | null | boolean) {
  emit('update:modelValue', typeof v === 'number' ? v : null)
}
</script>

<template>
  <AppSelect
    :model-value="modelValue"
    label="المحافظة"
    placeholder="اختر المحافظة"
    :options="options"
    filterable
    filter-placeholder="ابحث عن المحافظة…"
    empty-message="لا توجد محافظة"
    @update:model-value="onUpdate"
  />
</template>
