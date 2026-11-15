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
      <AppInput
        v-model="form.title"
        :label="$t('managementTasks.fields.title')"
        maxlength="200"
        required
        :disabled="busy"
      />

      <AppTextarea
        v-model="form.description"
        :label="$t('managementTasks.fields.description')"
        :rows="4"
        maxlength="2000"
        :disabled="busy"
      />

      <div class="mt-grid">
        <AppSelect
          v-model="form.priority"
          :label="$t('managementTasks.priorityLabel')"
          :options="priorityOptions"
          :disabled="busy"
          required
        />
        <AppDateField
          v-model="form.dueDate"
          :label="$t('managementTasks.fields.dueDate')"
          required
          :min="today"
          :disabled="busy"
        />
      </div>

      <AppSelect
        v-if="mode === 'create'"
        v-model="form.audience"
        :label="$t('managementTasks.fields.audience')"
        :options="audienceOptions"
        :disabled="busy"
        required
      />

      <AppSelect
        v-if="mode === 'resend' || form.audience === ManagementTaskAudience.User"
        v-model="form.assigneeUserId"
        :label="$t('managementTasks.fields.assignee')"
        :options="assigneeMenuOptions"
        :placeholder="$t('managementTasks.fields.assigneePlaceholder')"
        :filter-placeholder="$t('managementTasks.fields.searchUser')"
        :empty-message="$t('managementTasks.noAssignees')"
        :disabled="busy || assigneesLoading"
        required
      />
      <p
        v-if="(mode === 'resend' || form.audience === ManagementTaskAudience.User) && assigneesError"
        class="mt-error"
        role="alert"
      >
        {{ $t('managementTasks.assigneesError') }}
      </p>

      <p
        v-if="mode === 'create' && form.audience !== ManagementTaskAudience.User"
        class="mt-audience-hint"
        aria-live="polite"
      >
        <template v-if="audiencePreviewLoading">
          {{ $t('managementTasks.fields.audiencePreviewLoading') }}
        </template>
        <template v-else-if="audiencePreviewCount === 0">
          {{ $t('managementTasks.fields.audiencePreviewEmpty') }}
        </template>
        <template v-else>
          {{ $t('managementTasks.fields.audiencePreview', { count: audiencePreviewCount }) }}
        </template>
      </p>

      <div class="mt-grid">
        <AppInput
          v-model="form.customerId"
          :label="$t('managementTasks.fields.customerId')"
          type="number"
          :disabled="busy"
        />
        <AppInput
          v-model="form.orderId"
          :label="$t('managementTasks.fields.orderId')"
          type="number"
          :disabled="busy"
        />
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
        <AppButton variant="ghost" :disabled="busy" @click="emit('close')">
          {{ $t('managementTasks.actions.cancel') }}
        </AppButton>
        <AppButton type="submit" :disabled="busy" :loading="busy">
          {{
            busy
              ? $t('managementTasks.actions.submitting')
              : mode === 'resend'
                ? $t('managementTasks.actions.resend')
                : $t('managementTasks.actions.create')
          }}
        </AppButton>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import { useI18n } from 'vue-i18n'
import { AppButton, AppDateField, AppInput, AppSelect, AppTextarea } from '@/modules/Core/components/ui'
import {
  ManagementTaskAudience,
  ManagementTaskPriority,
  type ManagementTaskAudienceValue,
  type ManagementTaskListItem,
  type ManagementTaskPriorityValue,
} from '../domain/types'
import { useManagementTaskAssigneesQuery } from '../requests/queries/useManagementTaskAssigneesQuery'
import { useManagementTaskAudiencePreviewQuery } from '../requests/queries/useManagementTaskAudiencePreviewQuery'

export interface TaskFormSubmit {
  title: string
  description: string
  priority: ManagementTaskPriorityValue
  dueDate: string
  audience: ManagementTaskAudienceValue
  assigneeUserId: number | null
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

const audienceOptions = computed(() => [
  { value: ManagementTaskAudience.User, label: t('managementTasks.audienceOptions.user') },
  { value: ManagementTaskAudience.Delegate, label: t('managementTasks.audienceOptions.delegate') },
  { value: ManagementTaskAudience.Committee, label: t('managementTasks.audienceOptions.committee') },
  {
    value: ManagementTaskAudience.SalesEmployee,
    label: t('managementTasks.audienceOptions.salesEmployee'),
  },
  {
    value: ManagementTaskAudience.AllFieldApps,
    label: t('managementTasks.audienceOptions.allFieldApps'),
  },
])

const form = reactive({
  title: '',
  description: '',
  priority: ManagementTaskPriority.Medium as ManagementTaskPriorityValue,
  dueDate: '',
  audience: ManagementTaskAudience.User as ManagementTaskAudienceValue,
  assigneeUserId: null as number | null,
  customerId: '' as string | number,
  orderId: '' as string | number,
})
const files = ref<File[]>([])
const touched = ref(false)

const today = computed(() => new Date().toISOString().slice(0, 10))

const branchRef = computed(() => props.branchId)
const enabled = computed(() => props.visible)
const audienceRef = computed(() => form.audience)
const assigneesQuery = useManagementTaskAssigneesQuery(branchRef, enabled)
const previewQuery = useManagementTaskAudiencePreviewQuery(branchRef, audienceRef, enabled)
const assigneeMenuOptions = computed(() =>
  (assigneesQuery.data.value ?? []).map((o) => ({
    label: o.displayName,
    value: o.userId,
  })),
)
const assigneesLoading = computed(() => assigneesQuery.isLoading.value)
const assigneesError = computed(() => assigneesQuery.isError.value)
const audiencePreviewLoading = computed(() => previewQuery.isFetching.value)
const audiencePreviewCount = computed(() => previewQuery.data.value?.recipientCount ?? 0)

const valid = computed(() => {
  if (form.title.trim().length === 0 || form.dueDate.length === 0) return false
  if (props.mode === 'resend' || form.audience === ManagementTaskAudience.User) {
    return form.assigneeUserId != null
  }
  return audiencePreviewCount.value > 0
})

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
    form.audience = ManagementTaskAudience.User
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
    audience: props.mode === 'resend' ? ManagementTaskAudience.User : form.audience,
    assigneeUserId: form.assigneeUserId,
    customerId: toId(form.customerId),
    orderId: toId(form.orderId),
    files: files.value,
  })
}
</script>

<style scoped>
.mt-audience-hint {
  margin: 0.25rem 0 0.75rem;
  font-size: 0.875rem;
  color: #004d48;
}
.mt-error {
  color: #b42318;
  font-size: 0.875rem;
}
</style>
