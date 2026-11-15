<template>
  <section class="q-filters" dir="rtl">
    <AppSelect
      v-model="local.branchId"
      :label="$t('salesRequests.filters.province')"
      placeholder="كل المحافظات المسموحة"
      :options="branchMenuOptions"
      filterable
      filter-placeholder="ابحث عن المحافظة…"
      empty-message="لا توجد محافظة بهذا الاسم"
      @update:model-value="emitChange"
    />
    <AppSearchField
      v-model="searchText"
      :label="$t('salesRequests.filters.search')"
      :placeholder="$t('salesRequests.filters.search')"
      grow
      @update:model-value="onSearch"
    />
    <AppSelect
      v-if="tab === 'exceptions'"
      v-model="local.exceptionStatus"
      label="حالة الاستثناء"
      placeholder="كل الحالات"
      :options="exceptionStatusOptions"
      @update:model-value="emitChange"
    />

    <div v-if="activeChips.length" class="sr-chips">
      <span class="sr-chips__label">{{ $t('salesRequests.filters.active') }}:</span>
      <span v-for="chip in activeChips" :key="chip" class="sr-chip">{{ chip }}</span>
      <AppButton variant="ghost" size="sm" class="sr-chips__clear" @click="clearAll">
        {{ $t('salesRequests.filters.clear') }}
      </AppButton>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, watch, onMounted, ref } from 'vue'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppSearchField, AppSelect } from '@/modules/Core/components/ui'
import type { UnassignedFilters } from '@/modules/SalesRequests/requests/queries/useUnassignedSalesRequestsQuery'

const props = defineProps<{
  modelValue: UnassignedFilters
  tab?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [UnassignedFilters]
}>()

const local = reactive({ ...emptyFilters(), ...props.modelValue })
const searchText = ref(String(props.modelValue.search ?? ''))
const tab = computed(() => props.tab ?? '')

watch(
  () => props.modelValue,
  (v) => {
    Object.assign(local, emptyFilters(), v)
    searchText.value = String(v.search ?? '')
  },
  { deep: true },
)

const auth = useAuthStore()
const branches = computed(() => auth.branches ?? [])
const branchMenuOptions = computed(() => [
  { value: null as number | null, label: 'كل المحافظات المسموحة' },
  ...branches.value.map((b) => ({ value: b.id, label: b.name })),
])
const exceptionStatusOptions = [
  { value: null as string | null, label: 'كل الحالات' },
  { value: 'Pending', label: 'معلّق' },
  { value: 'Approved', label: 'موافق' },
  { value: 'Rejected', label: 'مرفوض' },
]

onMounted(() => {
  Object.assign(local, {
    provinceId: null,
    employeeId: null,
    sourceType: '',
    fromDate: null,
    toDate: null,
  })
  emitChange()
})

const activeChips = computed(() => {
  const chips: string[] = []
  if (local.branchId)
    chips.push(
      `محافظة: ${branches.value.find((o) => o.id === local.branchId)?.name ?? local.branchId}`,
    )
  if (local.search) chips.push(`بحث: ${local.search}`)
  if (local.exceptionStatus) {
    const label =
      exceptionStatusOptions.find((o) => o.value === local.exceptionStatus)
        ?.label ?? local.exceptionStatus
    chips.push(`استثناء: ${label}`)
  }
  return chips
})

function emitChange() {
  emit('update:modelValue', {
    ...emptyFilters(),
    branchId: local.branchId || null,
    search: local.search || null,
    exceptionStatus: local.exceptionStatus || null,
  })
}

function onSearch(v: string) {
  local.search = v
  emitChange()
}

function clearAll() {
  Object.assign(local, emptyFilters())
  searchText.value = ''
  emitChange()
}

function emptyFilters(): UnassignedFilters {
  return {
    branchId: null,
    provinceId: null,
    employeeId: null,
    sourceType: '',
    fromDate: null,
    toDate: null,
    search: null,
    exceptionStatus: null,
  }
}
</script>

<style scoped>
.sr-chips {
  grid-column: 1 / -1;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}
.sr-chips__label {
  font-size: 14px;
  color: #5a6b6a;
}
.sr-chip {
  border-radius: 999px;
  background: #e7f3f1;
  color: #006a64;
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 600;
}
.sr-chips__clear {
  margin-inline-start: auto;
}
</style>
