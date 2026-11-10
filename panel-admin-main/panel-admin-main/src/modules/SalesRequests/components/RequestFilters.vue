<template>
  <section class="sr-filters">
    <div class="grid gap-3 md:grid-cols-[240px_1fr]">
      <label class="flex flex-col gap-1 text-sm">
        <span class="sr-filter-label">{{ $t('salesRequests.filters.province') }}</span>
        <Select
          v-model="local.branchId"
          :options="branchOptions"
          option-label="name"
          option-value="id"
          :unstyled="false"
          placeholder="كل المحافظات المسموحة"
          filter
          filter-placeholder="ابحث عن المحافظة…"
          empty-filter-message="لا توجد محافظة بهذا الاسم"
          empty-message="لا توجد محافظات متاحة"
          aria-label="المحافظة"
          class="w-full"
          :reset-filter-on-hide="true"
          @change="emitChange"
        />
      </label>
      <label class="flex flex-col gap-1 text-sm">
        <span class="sr-filter-label">{{ $t('salesRequests.filters.search') }}</span>
        <span class="sr-search-wrap">
          <input
            v-model="local.search"
            type="search"
            class="sr-input"
            :placeholder="$t('salesRequests.filters.search')"
            aria-label="بحث"
            @keyup.enter="emitChange"
            @change="emitChange"
          >
          <i
            class="pi pi-search sr-search-icon"
            aria-hidden="true"
          />
        </span>
      </label>
    </div>

    <div
      v-if="activeChips.length"
      class="mt-3 flex flex-wrap items-center gap-2"
    >
      <span class="text-sm text-[#00201E]/70">
        {{ $t('salesRequests.filters.active') }}:
      </span>
      <span
        v-for="chip in activeChips"
        :key="chip"
        class="rounded-full bg-[#006A64]/10 px-3 py-1 text-xs text-[#006A64]"
      >
        {{ chip }}
      </span>
      <button
        type="button"
        class="ms-auto text-sm text-[#006A64] underline min-h-11"
        @click="clearAll"
      >
        {{ $t('salesRequests.filters.clear') }}
      </button>
    </div>
  </section>
</template>

<script setup lang="ts">
import Select from 'primevue/select'
import { computed, reactive, watch, onMounted } from 'vue'
import { useAuthStore } from '@/modules/Auth/store'
import type { UnassignedFilters } from '@/modules/SalesRequests/requests/queries/useUnassignedSalesRequestsQuery'

const props = defineProps<{
  modelValue: UnassignedFilters
  tab?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [UnassignedFilters]
}>()

const local = reactive({ ...emptyFilters(), ...props.modelValue })

watch(
  () => props.modelValue,
  (v) => Object.assign(local, emptyFilters(), v),
  { deep: true }
)

const auth = useAuthStore()
const branches = computed(() => auth.branches ?? [])
const branchOptions = computed(() => [
  { id: null, name: 'كل المحافظات المسموحة' },
  ...branches.value,
])
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
      `محافظة: ${branches.value.find((o) => o.id === local.branchId)?.name ?? local.branchId}`
    )
  if (local.search) chips.push(`بحث: ${local.search}`)
  return chips
})
function emitChange() {
  emit('update:modelValue', {
    ...emptyFilters(),
    branchId: local.branchId || null,
    search: local.search || null,
  })
}

function clearAll() {
  Object.assign(local, emptyFilters())
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
  }
}
</script>

<style scoped>
.sr-filter-label {
  font-size: 14px;
  color: #526b67;
  margin-bottom: 4px;
}
.sr-input {
  border: 1px solid #d7e2e0;
  border-radius: 0.75rem;
  padding: 0.55rem 0.75rem;
  padding-inline-end: 46px;
  background: #f8f8f8;
  color: #00201e;
  font-family: inherit;
  min-height: 46px;
  font-size: 16px;
  width: 100%;
  min-width: 0;
}
.sr-input:focus-visible {
  outline: 2px solid #006a64;
  outline-offset: 2px;
}
.sr-search-wrap {
  position: relative;
  display: block;
  width: 100%;
}
.sr-search-icon {
  position: absolute;
  inset-inline-end: 16px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 18px;
  line-height: 1;
  color: #647b77;
  pointer-events: none;
}
</style>
