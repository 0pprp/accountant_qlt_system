<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    :dismissable-mask="!busy"
    :header="$t('managementTasks.actions.transfer')"
    :style="{ width: 'min(520px, 94vw)' }"
    :pt="dialogMaskPt"
    class="bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && !busy && emit('close')"
  >
    <form
      class="mt-dialog-body"
      novalidate
      @submit.prevent="submit"
    >
      <label class="mt-field">
        <span>{{ $t('managementTasks.fields.transferTarget') }} *</span>
        <Select
          v-model="targetUserId"
          :options="options"
          option-label="displayName"
          option-value="userId"
          :unstyled="false"
          :placeholder="$t('managementTasks.fields.transferTargetPlaceholder')"
          filter
          :filter-placeholder="$t('managementTasks.fields.searchUser')"
          :empty-filter-message="$t('managementTasks.noAssignees')"
          :empty-message="$t('managementTasks.noAssignees')"
          :loading="loading"
          :disabled="busy"
          :aria-label="$t('managementTasks.fields.transferTarget')"
          class="w-full"
          :reset-filter-on-hide="true"
          append-to="body"
        />
      </label>
      <p
        v-if="error"
        class="mt-error"
        role="alert"
      >
        {{ $t('managementTasks.assigneesError') }}
      </p>
      <label class="mt-field">
        <span>{{ $t('managementTasks.fields.note') }}</span>
        <textarea
          v-model="note"
          class="mt-textarea"
          rows="3"
          maxlength="1000"
          :disabled="busy"
        />
      </label>
      <footer class="mt-dialog-foot">
        <button
          type="button"
          class="mt-btn ghost"
          :disabled="busy"
          @click="emit('close')"
        >
          {{ $t('managementTasks.actions.cancel') }}
        </button>
        <button
          type="submit"
          class="mt-btn primary"
          :disabled="busy || targetUserId == null"
        >
          {{
            busy
              ? $t('managementTasks.actions.submitting')
              : $t('managementTasks.actions.transfer')
          }}
        </button>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import Select from 'primevue/select'
import type { ManagementTaskAssigneeOption } from '../domain/types'
import { useManagementTaskAssigneesQuery } from '../requests/queries/useManagementTaskAssigneesQuery'

const props = defineProps<{
  visible: boolean
  branchId: number | null
  /** مستخدمون يُستبعدون من الخيارات (الحامل الحالي / المستخدم الحالي) */
  excludeUserIds: number[]
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: { targetUserId: number; note?: string }]
}>()

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}

const targetUserId = ref<number | null>(null)
const note = ref('')

const branchRef = computed(() => props.branchId)
const enabled = computed(() => props.visible)
const query = useManagementTaskAssigneesQuery(branchRef, enabled)

const loading = computed(() => query.isLoading.value)
const error = computed(() => query.isError.value)
const options = computed<ManagementTaskAssigneeOption[]>(() =>
  (query.data.value ?? []).filter(
    (o) => props.excludeUserIds.includes(o.userId) === false
  )
)

watch(
  () => props.visible,
  (v) => {
    if (v) {
      targetUserId.value = null
      note.value = ''
    }
  }
)

function submit() {
  if (props.busy || targetUserId.value == null) return
  emit('submit', {
    targetUserId: targetUserId.value,
    note: note.value.trim() || undefined,
  })
}
</script>
