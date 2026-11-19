<template>
  <Dialog
    :visible="open"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    header="تعديل الطلب"
    :style="{ width: 'min(560px, 94vw)' }"
    class="bg-white rounded-3xl p-6"
    dir="rtl"
    :pt="{ mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' } }"
    @update:visible="!$event && !busy && $emit('close')"
  >
    <template #closebutton="{ closeCallback }">
      <button
        type="button"
        class="sr-dlg-close"
        :disabled="busy"
        aria-label="إغلاق"
        @click="closeCallback"
      >
        ×
      </button>
    </template>

    <form
      class="sr-edit-form"
      @submit.prevent="submit"
    >
      <p class="sr-edit-lead">طلب #{{ request.id }}</p>

      <AppInput
        v-model="customerName"
        label="اسم الزبون"
        :disabled="busy"
        required
      />
      <AppInput
        v-model="customerPhone"
        label="رقم الهاتف"
        placeholder="07xxxxxxxxx"
        :disabled="busy"
        required
      />
      <AppInput
        v-model="address"
        label="العنوان"
        :disabled="busy"
      />
      <AppInput
        v-model="requestedProduct"
        label="المنتج المطلوب"
        :disabled="busy"
      />

      <AppSelect
        :model-value="provinceId"
        @update:model-value="onProvinceSelected"
        label="المحافظة"
        placeholder="اختر محافظة"
        :options="provinceOptions"
        :disabled="busy || provincesLoading"
        required
      />
      <AppSelect
        v-model="branchId"
        label="الفرع"
        placeholder="اختر فرعًا"
        :options="branchOptions"
        :disabled="busy || !provinceId || branchesLoading"
        required
      />

      <AppSearchField
        v-model="employeeSearch"
        label="بحث عن موظف"
      />
      <DataState
        v-if="employeesLoading"
        kind="loading"
        title="جاري التحميل"
        message="جارٍ تحميل الموظفين…"
      />
      <ul
        v-else
        class="sr-emp-list"
        role="listbox"
      >
        <li
          v-for="emp in employees"
          :key="emp.id"
        >
          <button
            type="button"
            class="sr-emp"
            :class="{ selected: employeeId === emp.id }"
            role="option"
            :aria-selected="employeeId === emp.id"
            :disabled="busy"
            @click="employeeId = emp.id"
          >
            <span>{{ emp.fullName }}</span>
            <span class="sr-user">{{ emp.username || `#${emp.id}` }}</span>
          </button>
        </li>
        <li v-if="!employees.length">
          <DataState
            kind="empty"
            title="لا نتائج"
            message="لا موظفون مؤهلون في هذا الفرع"
          />
        </li>
      </ul>

      <p
        v-if="branchChanged"
        class="sr-hint"
      >
        تغيير الفرع/المحافظة يتطلب اختيار موظف مؤهل في الفرع الجديد — يُزال من المسند السابق فورًا.
      </p>

      <AppInput
        v-if="employeeChanged || branchChanged"
        v-model="reason"
        label="سبب تغيير الموظف"
        :disabled="busy"
      />

      <p
        v-if="errorMsg"
        class="sr-err"
        role="alert"
      >
        {{ errorMsg }}
      </p>

      <footer class="sr-edit-foot">
        <AppButton
          variant="ghost"
          :disabled="busy"
          @click="$emit('close')"
        >
          إلغاء
        </AppButton>
        <AppButton
          :disabled="!canSubmit || busy"
          :loading="busy"
          @click="submit"
        >
          {{ busy ? 'جارٍ الحفظ…' : 'حفظ التعديل' }}
        </AppButton>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import { computed, ref, watch } from 'vue'
import {
  AppButton,
  AppInput,
  AppSearchField,
  AppSelect,
  DataState,
} from '@/modules/Core/components/ui'
import { useProvinceDataQuery } from '@/modules/Core/requests/queries'
import { useBranchDataQuery } from '@/modules/Branch/requests/queries'
import { useAssignableEmployeesQuery } from '../requests/queries/useAssignableEmployeesQuery'
import type { SalesRequestListItem } from '../types'

const props = defineProps<{
  open: boolean
  request: SalesRequestListItem
  busy?: boolean
  error?: string | null
}>()

const emit = defineEmits<{
  close: []
  confirm: [
    {
      customerName: string
      customerPhone: string
      address?: string
      requestedProduct?: string
      branchId: number
      provinceId?: number | null
      employeeId?: number | null
      reason?: string
    },
  ]
}>()

const customerName = ref('')
const customerPhone = ref('')
const address = ref('')
const requestedProduct = ref('')
const provinceId = ref<number | null>(null)
const branchId = ref<number | null>(null)
const employeeId = ref<number | null>(null)
const employeeSearch = ref('')
const reason = ref('')
const errorMsg = ref<string | null>(null)

const pageIndex = ref(1)
const pageSize = ref(200)
const { data: provinceData, isLoading: provincesLoading } = useProvinceDataQuery(
  pageIndex,
  pageSize,
)
const { data: branchData, isLoading: branchesLoading } = useBranchDataQuery(
  pageIndex,
  pageSize,
)

const provinceOptions = computed(() =>
  (provinceData.value?.items ?? []).map((p) => ({
    label: p.name,
    value: p.id,
  })),
)

const allBranches = computed(() => branchData.value?.data?.items ?? [])

const branchOptions = computed(() =>
  allBranches.value
    .filter((b) => !provinceId.value || b.province?.id === provinceId.value)
    .map((b) => ({
      label: b.province?.name ? `${b.name} · ${b.province.name}` : b.name,
      value: b.id,
    })),
)

const employeesEnabled = computed(
  () => props.open && branchId.value != null && branchId.value > 0,
)
const branchRef = computed(() => branchId.value)
const employeesQuery = useAssignableEmployeesQuery(
  branchRef,
  employeeSearch,
  employeesEnabled,
)
const employees = computed(() => employeesQuery.data.value ?? [])
const employeesLoading = computed(() => employeesQuery.isLoading.value)

const employeeChanged = computed(
  () =>
    employeeId.value != null &&
    employeeId.value !== (props.request.targetEmployeeId ?? null),
)

const branchChanged = computed(
  () => branchId.value != null && branchId.value !== props.request.branchId,
)

const normalizedPhone = computed(() => customerPhone.value
  .replace(/[٠-٩]/g, c => String(c.charCodeAt(0) - 0x660))
  .replace(/[۰-۹]/g, c => String(c.charCodeAt(0) - 0x6f0))
  .replace(/[\s()-]/g, ''))

const canSubmit = computed(() => {
  if (!customerName.value.trim()) return false
  if (!/^07\d{9}$/.test(normalizedPhone.value)) return false
  if (!branchId.value) return false
  if (branchChanged.value && !employeeId.value) return false
  if ((employeeChanged.value || branchChanged.value) && !reason.value.trim())
    return false
  return true
})

function hydrateFromRequest() {
  customerName.value = props.request.customerName || ''
  customerPhone.value = props.request.customerPhone || ''
  address.value = props.request.address || ''
  requestedProduct.value = props.request.requestedProduct || ''
  branchId.value = props.request.branchId
  employeeId.value = props.request.targetEmployeeId ?? null
  employeeSearch.value = ''
  reason.value = ''
  errorMsg.value = null

  const fromRequest = props.request.provinceId ?? null
  if (fromRequest) {
    provinceId.value = fromRequest
    return
  }
  const matched = allBranches.value.find((b) => b.id === props.request.branchId)
  provinceId.value = matched?.province?.id ?? null
}

watch(
  () => props.open,
  (v) => {
    if (!v) return
    hydrateFromRequest()
  },
)

watch(allBranches, () => {
  if (!props.open) return
  if (provinceId.value == null && props.request.branchId) {
    const matched = allBranches.value.find((b) => b.id === props.request.branchId)
    if (matched?.province?.id) provinceId.value = matched.province.id
  }
})

watch(
  () => props.error,
  (v) => {
    errorMsg.value = v ?? null
  },
)

function onProvinceSelected(value: string | number | boolean | null) {
  const next = typeof value === 'number' ? value : null
  if (next === provinceId.value) return
  provinceId.value = next
  const stillValid = allBranches.value.some(
    (b) => b.id === branchId.value && b.province?.id === next,
  )
  if (!stillValid) {
    branchId.value = null
    employeeId.value = null
  }
}

watch(branchId, (next, prev) => {
  if (!props.open || next === prev) return
  if (next !== props.request.branchId) {
    employeeId.value = null
  }
})

function submit() {
  if (props.busy || !canSubmit.value || !branchId.value) return
  errorMsg.value = null
  emit('confirm', {
    customerName: customerName.value.trim(),
    customerPhone: normalizedPhone.value,
    address: address.value.trim() || undefined,
    requestedProduct: requestedProduct.value.trim() || undefined,
    branchId: branchId.value,
    provinceId: provinceId.value,
    employeeId: employeeChanged.value || branchChanged.value ? employeeId.value : undefined,
    reason: reason.value.trim() || undefined,
  })
}
</script>

<style scoped>
.sr-edit-form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.sr-edit-lead {
  margin: 0;
  font-weight: 700;
}
.sr-hint {
  margin: 0;
  font-size: 0.82rem;
  color: #8a4b00;
  line-height: 1.5;
}
.sr-emp-list {
  list-style: none;
  margin: 0;
  padding: 0;
  max-height: 10rem;
  overflow: auto;
  border: 1px solid #d7e2e0;
  border-radius: 0.75rem;
}
.sr-emp {
  width: 100%;
  min-height: 44px;
  display: flex;
  justify-content: space-between;
  gap: 0.5rem;
  padding: 0.55rem 0.75rem;
  border: none;
  border-bottom: 1px solid #eef3f2;
  background: #fff;
  text-align: start;
  font: inherit;
  cursor: pointer;
}
.sr-emp.selected {
  background: #e6f2f1;
}
.sr-user {
  opacity: 0.65;
  font-size: 0.8rem;
}
.sr-edit-foot {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.35rem;
}
.sr-err {
  color: #9b1c1c;
  font-size: 0.85rem;
  margin: 0;
}
.sr-dlg-close {
  width: 2.35rem;
  height: 2.35rem;
  min-width: 2.35rem;
  min-height: 2.35rem;
  padding: 0;
  border: 1px solid #e8a39a;
  border-radius: 10px;
  background: #fff5f4;
  color: #c62828;
  font-size: 1.55rem;
  font-weight: 700;
  line-height: 1;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.sr-dlg-close:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
</style>
