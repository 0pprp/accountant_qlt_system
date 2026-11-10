<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    :dismissable-mask="!busy"
    :header="mode === 'resend' ? $t('managementTasks.actions.resend') : $t('managementTasks.actions.create')"
    :style="{ width: 'min(640px, 96vw)' }"
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
        <span>{{ $t('managementTasks.fields.title') }} *</span>
        <input
          v-model="form.title"
          type="text"
          class="mt-input"
          maxlength="200"
          required
          :disabled="busy"
          :aria-invalid="touched && !form.title.trim()"
        >
      </label>

      <label class="mt-field">
        <span>{{ $t('managementTasks.fields.description') }}</span>
        <textarea
          v-model="form.description"
          class="mt-textarea"
          rows="4"
          maxlength="2000"
          :disabled="busy"
        />
      </label>

      <div class="mt-grid">
        <label class="mt-field">
          <span>{{ $t('managementTasks.priorityLabel') }} *</span>
          <Select
            v-model="form.priority"
            :options="priorityOptions"
            option-label="label"
            option-value="value"
            :unstyled="false"
            :disabled="busy"
            :aria-label="$t('managementTasks.priorityLabel')"
            class="w-full"
          />
        </label>
        <label class="mt-field">
          <span>{{ $t('managementTasks.fields.dueDate') }} *</span>
          <input
            v-model="form.dueDate"
            type="date"
            class="mt-input"
            required
            :min="today"
            :disabled="busy"
            :aria-invalid="touched && !form.dueDate"
          >
        </label>
      </div>

      <label class="mt-field">
        <span>{{ $t('managementTasks.fields.assignee') }} *</span>
        <Select
          v-model="form.assigneeUserId"
          :options="assigneeOptions"
          option-label="displayName"
          option-value="userId"
          :unstyled="false"
          :placeholder="$t('managementTasks.fields.assigneePlaceholder')"
          filter
          :filter-placeholder="$t('managementTasks.fields.searchUser')"
          :empty-filter-message="$t('managementTasks.noAssignees')"
          :empty-message="$t('managementTasks.noAssignees')"
          :loading="assigneesLoading"
          :disabled="busy"
          :aria-label="$t('managementTasks.fields.assignee')"
          class="w-full"
          :reset-filter-on-hide="true"
        />
      </label>
      <p
        v-if="assigneesError"
        class="mt-error"
        role="alert"
      >
        {{ $t('managementTasks.assigneesError') }}
      </p>

      <div class="mt-grid">
        <label class="mt-field">
          <span>{{ $t('managementTasks.fields.customerId') }}</span>
          <input
            v-model="form.customerId"
            type="number"
            min="1"
            inputmode="numeric"
            class="mt-input"
            :disabled="busy"
          >
        </label>
        <label class="mt-field">
          <span>{{ $t('managementTasks.fields.orderId') }}</span>
          <input
            v-model="form.orderId"
            type="number"
            min="1"
            inputmode="numeric"
            class="mt-input"
            :disabled="busy"
          >
        </label>
      </div>

      <div
        v-if="mode === 'create'"
        class="mt-field"
      >
        <label
          class="mt-label"
          for="mt-create-files"
        >
          {{ $t('managementTasks.fields.attachments') }}
        </label>
        <input
          id="mt-create-files"
          type="file"
          multiple
          class="mt-file-input"
          :disabled="busy"
          @change="onFiles"
        >
        <small v-if="files.length">
          {{ $t('managementTasks.fields.filesSelected', { count: files.length }) }}
        </small>
      </div>

      <p
        v-if="touched && !valid"
        class="mt-error"
        role="alert"
      >
        {{ $t('managementTasks.validation.formIncomplete') }}
      </p>

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
          :disabled="busy"
        >
          {{
            busy
              ? $t('managementTasks.actions.submitting')
              : mode === 'resend'
                ? $t('managementTasks.actions.resend')
                : $t('managementTasks.actions.create')
          }}
        </button>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import Select from 'primevue/select'
import { useI18n } from 'vue-i18n'
import {
  ManagementTaskPriority,
  type ManagementTaskListItem,
  type ManagementTaskPriorityValue,
} from '../domain/types'
import { useManagementTaskAssigneesQuery } from '../requests/queries/useManagementTaskAssigneesQuery'

export interface TaskFormSubmit {
  title: string
  description: string
  priority: ManagementTaskPriorityValue
  dueDate: string
  assigneeUserId: number
  customerId: number | null
  orderId: number | null
  files: File[]
}

const props = defineProps<{
  visible: boolean
  mode: 'create' | 'resend'
  branchId: number | null
  /** مهمة معادة لإعادة الإرسال */
  task?: ManagementTaskListItem | null
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: [TaskFormSubmit]
}>()

const { t } = useI18n()

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}

const priorityOptions = computed(() => [
  { value: ManagementTaskPriority.High, label: t('managementTasks.priority.high') },
  { value: ManagementTaskPriority.Medium, label: t('managementTasks.priority.medium') },
  { value: ManagementTaskPriority.Low, label: t('managementTasks.priority.low') },
])

const form = reactive({
  title: '',
  description: '',
  priority: ManagementTaskPriority.Medium as ManagementTaskPriorityValue,
  dueDate: '',
  assigneeUserId: null as number | null,
  customerId: '' as string | number,
  orderId: '' as string | number,
})
const files = ref<File[]>([])
const touched = ref(false)

const today = computed(() => new Date().toISOString().slice(0, 10))

const branchRef = computed(() => props.branchId)
const enabled = computed(() => props.visible)
const assigneesQuery = useManagementTaskAssigneesQuery(branchRef, enabled)
const assigneeOptions = computed(() => assigneesQuery.data.value ?? [])
const assigneesLoading = computed(() => assigneesQuery.isLoading.value)
const assigneesError = computed(() => assigneesQuery.isError.value)

const valid = computed(
  () =>
    form.title.trim().length > 0 &&
    form.dueDate.length > 0 &&
    form.assigneeUserId != null
)

function toId(value: string | number): number | null {
  const n = Number(value)
  return value !== '' && Number.isInteger(n) && n > 0 ? n : null
}

watch(
  () => props.visible,
  (v) => {
    if (!v) return
    touched.value = false
    files.value = []
    const task = props.mode === 'resend' ? props.task : null
    form.title = task?.title ?? ''
    form.description = task?.description ?? ''
    form.priority = task?.priority ?? ManagementTaskPriority.Medium
    form.dueDate = task?.dueDate?.slice(0, 10) ?? ''
    form.assigneeUserId = task?.assigneeUserId ?? null
    form.customerId = task?.customerId ?? ''
    form.orderId = task?.orderId ?? ''
  },
  { immediate: true }
)

function onFiles(e: Event) {
  const input = e.target as HTMLInputElement
  files.value = Array.from(input.files ?? [])
}

function submit() {
  touched.value = true
  if (props.busy || !valid.value) return
  emit('submit', {
    title: form.title.trim(),
    description: form.description.trim(),
    priority: form.priority,
    dueDate: form.dueDate,
    assigneeUserId: form.assigneeUserId!,
    customerId: toId(form.customerId),
    orderId: toId(form.orderId),
    files: files.value,
  })
}
</script>
