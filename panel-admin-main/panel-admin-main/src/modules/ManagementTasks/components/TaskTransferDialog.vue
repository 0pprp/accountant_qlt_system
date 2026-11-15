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
      <AppSelect
        v-model="targetUserId"
        :label="$t('managementTasks.fields.transferTarget')"
        :options="menuOptions"
        :placeholder="$t('managementTasks.fields.transferTargetPlaceholder')"
        :filter-placeholder="$t('managementTasks.fields.searchUser')"
        :empty-message="$t('managementTasks.noAssignees')"
        :disabled="busy || loading"
        required
      />
      <p
        v-if="error"
        class="mt-error"
        role="alert"
      >
        {{ $t('managementTasks.assigneesError') }}
      </p>
      <AppTextarea
        v-model="note"
        :label="$t('managementTasks.fields.note')"
        :rows="3"
        maxlength="1000"
        :disabled="busy"
      />
      <footer class="mt-dialog-foot">
        <AppButton variant="ghost" :disabled="busy" @click="emit('close')">
          {{ $t('managementTasks.actions.cancel') }}
        </AppButton>
        <AppButton
          type="submit"
          :disabled="busy || targetUserId == null"
          :loading="busy"
        >
          {{
            busy
              ? $t('managementTasks.actions.submitting')
              : $t('managementTasks.actions.transfer')
          }}
        </AppButton>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import { AppButton, AppSelect, AppTextarea } from '@/modules/Core/components/ui'
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
const menuOptions = computed(() =>
  (query.data.value ?? [])
    .filter((o) => props.excludeUserIds.includes(o.userId) === false)
    .map((o) => ({ label: o.displayName, value: o.userId })),
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
