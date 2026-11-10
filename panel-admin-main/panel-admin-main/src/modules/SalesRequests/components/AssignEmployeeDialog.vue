<template>
  <Dialog :visible="open" modal :closable="!busy" :close-on-escape="!busy" :header="mode === 'reassign' ? 'إعادة إسناد الطلب' : 'إسناد الطلب'" :style="{width:'min(520px, 92vw)'}" class="bg-white rounded-3xl p-6" dir="rtl" :pt="{mask:{class:'bg-[#C6C6C680] backdrop-blur-[2px]'}}" @update:visible="!$event && !busy && $emit('close')">
      <label class="sr-field">
        {{ $t('salesRequests.actions.searchEmployee') }}
        <input
          v-model="search"
          type="search"
          class="sr-input"
          autocomplete="off"
        />
      </label>

      <p v-if="loading" class="sr-hint">
        {{ $t('salesRequests.actions.loadingEmployees') }}
      </p>
      <p v-else-if="error" class="sr-hint error">
        {{ $t('salesRequests.actions.employeesError') }}
      </p>
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
            :class="{ selected: selectedId === emp.id }"
            role="option"
            :aria-selected="selectedId === emp.id"
            @click="selectedId = emp.id"
          >
            <span>{{ emp.fullName }}</span>
            <span class="sr-user">{{ emp.username || `#${emp.id}` }}</span>
          </button>
        </li>
        <li
          v-if="!employees.length"
          class="sr-hint"
        >
          {{ $t('salesRequests.actions.noEmployees') }}
        </li>
      </ul>

      <label
        v-if="mode === 'reassign'"
        class="sr-field"
      >
        {{ $t('salesRequests.actions.reason') }}
        <input
          v-model="reason"
          type="text"
          class="sr-input"
        />
      </label>

      <footer class="sr-dlg-foot">
        <button
          type="button"
          class="sr-btn ghost"
          @click="$emit('close')"
        >
          {{ $t('salesRequests.actions.cancel') }}
        </button>
        <button
          type="button"
          class="sr-btn primary"
          :disabled="!selectedId || busy || (mode === 'reassign' && !reason.trim())"
          @click="confirm"
        >
          {{
            busy
              ? $t('salesRequests.actions.submitting')
              : $t('salesRequests.actions.confirm')
          }}
        </button>
      </footer>
  </Dialog>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import { computed, ref, watch } from 'vue'
import { useAssignableEmployeesQuery } from '../requests/queries/useAssignableEmployeesQuery'

const props = defineProps<{
  open: boolean
  branchId: number | null
  mode: 'assign' | 'reassign'
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  confirm: [{ employeeId: number; reason?: string }]
}>()

const search = ref('')
const selectedId = ref<number | null>(null)
const reason = ref('')

const enabled = computed(() => props.open && props.branchId != null && props.branchId > 0)
const branchRef = computed(() => props.branchId)
const query = useAssignableEmployeesQuery(branchRef, search, enabled)

const employees = computed(() => query.data.value ?? [])
const loading = computed(() => query.isLoading.value)
const error = computed(() => query.isError.value)

watch(
  () => props.open,
  (v) => {
    if (v) {
      selectedId.value = null
      reason.value = ''
      search.value = ''
    }
  }
)

function confirm() {
  if (!selectedId.value) return
  emit('confirm', {
    employeeId: selectedId.value,
    reason: reason.value.trim() || undefined,
  })
}
</script>

<style scoped>
.sr-dlg-backdrop {
  position: fixed;
  inset: 0;
  z-index: 80;
  background: rgba(0, 32, 30, 0.35);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  padding: max(0.75rem, env(safe-area-inset-bottom));
}
@media (min-width: 768px) {
  .sr-dlg-backdrop {
    align-items: center;
  }
}
.sr-dlg {
  width: min(28rem, 100%);
  max-height: min(85vh, 36rem);
  overflow: auto;
  background: #fff;
  border-radius: 1rem;
  padding: 1rem;
  color: #00201e;
  font-family: 'ElMessiri', 'El Messiri', sans-serif;
}
.sr-dlg-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}
.sr-dlg-head h3 {
  margin: 0;
  font-size: 1.1rem;
}
.sr-icon-btn {
  min-width: 44px;
  min-height: 44px;
  border: none;
  background: transparent;
  font-size: 1.4rem;
  cursor: pointer;
}
.sr-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  font-size: 0.85rem;
  margin-bottom: 0.75rem;
}
.sr-input {
  border: 1px solid #d7e2e0;
  border-radius: 0.75rem;
  padding: 0.55rem 0.75rem;
  min-height: 44px;
  background: #f8f8f8;
}
.sr-emp-list {
  list-style: none;
  margin: 0 0 0.75rem;
  padding: 0;
  max-height: 12rem;
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
  font-family: inherit;
  cursor: pointer;
}
.sr-emp.selected {
  background: #e6f2f1;
}
.sr-user {
  opacity: 0.65;
  font-size: 0.8rem;
}
.sr-hint {
  font-size: 0.85rem;
  opacity: 0.75;
  padding: 0.5rem;
}
.sr-hint.error {
  color: #9b1c1c;
  opacity: 1;
}
.sr-dlg-foot {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}
.sr-btn {
  min-height: 44px;
  border-radius: 0.85rem;
  padding: 0.45rem 1rem;
  border: none;
  font-family: inherit;
  cursor: pointer;
}
.sr-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.sr-btn.primary {
  background: #006a64;
  color: #fff;
}
.sr-btn.ghost {
  background: #f0f4f3;
  color: #00201e;
}
</style>
