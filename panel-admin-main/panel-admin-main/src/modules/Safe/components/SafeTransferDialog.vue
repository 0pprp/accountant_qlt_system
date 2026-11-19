<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    :dismissable-mask="!busy"
    :header="$t('safe.operations.transfer')"
    :style="{ width: 'min(560px, 96vw)' }"
    :pt="dialogMaskPt"
    class="bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && !busy && emit('close')"
  >
    <form
      class="safe-dialog"
      novalidate
      @submit.prevent="submit"
    >
      <div class="safe-dialog__source">
        <span class="safe-dialog__source-label">{{ $t('safe.fields.from') }}</span>
        <strong class="safe-dialog__source-name">{{ sourceSafe?.name ?? '—' }}</strong>
        <span class="safe-dialog__source-balance">
          {{ $t('safe.fields.balance') }}: {{ formattedPrice(sourceSafe?.balance ?? 0) }}
          <small>{{ $t('words.dinar') }}</small>
        </span>
      </div>

      <AppSelect
        v-model="destinationSafeId"
        :label="$t('safe.fields.to')"
        :placeholder="$t('safe.fields.toPlaceholder')"
        :options="destinationOptions"
        required
        :disabled="busy"
      />

      <AppInput
        v-model="amount"
        :label="$t('safe.fields.amount')"
        type="number"
        min="0"
        required
        :disabled="busy"
        dir="ltr"
      />

      <AppTextarea
        v-model="note"
        :label="$t('safe.fields.note')"
        :placeholder="$t('safe.fields.notePlaceholder')"
        :rows="3"
        maxlength="500"
        :disabled="busy"
      />

      <p
        v-if="touched && !valid"
        class="safe-dialog__error"
        role="alert"
      >
        {{ validationMessage }}
      </p>

      <footer class="safe-dialog__foot">
        <AppButton
          variant="ghost"
          :disabled="busy"
          @click="emit('close')"
        >
          {{ $t('safe.actions.cancel') }}
        </AppButton>
        <AppButton
          type="submit"
          :disabled="busy"
          :loading="busy"
        >
          {{ busy ? $t('safe.actions.submitting') : $t('safe.actions.confirm') }}
        </AppButton>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import { useI18n } from 'vue-i18n'
import { AppButton, AppInput, AppSelect, AppTextarea } from '@/modules/Core/components/ui'
import { formattedPrice } from '@/modules/Core/utils/price'
import type { Safe, SafeTransferForm } from '../types/model'

const props = defineProps<{
  visible: boolean
  safes: Safe[]
  /** خزينة المصدر هي الكرت الذي فُتحت منه العملية — ثابتة لا تُختار */
  sourceId: number | null
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: [SafeTransferForm]
}>()

const { t } = useI18n()

const destinationSafeId = ref<number | null>(null)
const amount = ref('')
const note = ref('')
const touched = ref(false)
const operationKey = ref('')

const sourceSafe = computed(
  () => props.safes.find((safe) => safe.id === props.sourceId) ?? null,
)

const destinationOptions = computed(() =>
  props.safes
    .filter((safe) => safe.isActive && safe.id !== props.sourceId)
    .map((safe) => ({ label: safe.name, value: safe.id })),
)

const numericAmount = computed(() => Number(amount.value))

const isValidAmount = computed(
  () =>
    amount.value.trim().length > 0 &&
    Number.isFinite(numericAmount.value) &&
    numericAmount.value > 0 &&
    numericAmount.value % 1000 === 0,
)

const exceedsBalance = computed(
  () => sourceSafe.value !== null && numericAmount.value > sourceSafe.value.balance,
)

const valid = computed(
  () =>
    sourceSafe.value !== null &&
    sourceSafe.value.isActive &&
    destinationSafeId.value !== null &&
    destinationSafeId.value !== props.sourceId &&
    isValidAmount.value &&
    exceedsBalance.value === false,
)

const validationMessage = computed(() => {
  if (sourceSafe.value === null || sourceSafe.value.isActive === false)
    return t('safe.validation.sourceUnavailable')
  if (destinationSafeId.value === null) return t('safe.validation.selectDestination')
  if (destinationSafeId.value === props.sourceId) return t('safe.validation.sameSafe')
  if (!isValidAmount.value) return t('safe.validation.amount')
  if (exceedsBalance.value) return t('safe.validation.exceedsBalance')
  return ''
})

watch(
  () => props.visible,
  (isOpen) => {
    if (!isOpen) return
    destinationSafeId.value = null
    amount.value = ''
    note.value = ''
    touched.value = false
    operationKey.value = crypto.randomUUID()
  },
  { immediate: true },
)

function submit() {
  touched.value = true
  if (props.busy || !valid.value) return

  emit('submit', {
    sourceSafeId: props.sourceId!,
    destinationSafeId: destinationSafeId.value!,
    amount: numericAmount.value,
    note: note.value.trim() || null,
    operationKey: operationKey.value,
  })
}

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}
</script>

<style scoped>
.safe-dialog {
  display: grid;
  gap: var(--q-space-3, 12px);
}

.safe-dialog__source {
  display: grid;
  gap: 2px;
  padding: 10px 12px;
  border-radius: var(--q-radius-md, 12px);
  background: var(--q-surface-sunken, #f1f5f4);
}

.safe-dialog__source-label {
  font-size: 12px;
  color: var(--q-muted);
}

.safe-dialog__source-name {
  font-size: 15px;
}

.safe-dialog__source-balance {
  font-size: 13px;
  color: var(--q-muted);
}

.safe-dialog__error {
  font-size: 13px;
  color: var(--q-danger);
}

.safe-dialog__foot {
  display: flex;
  justify-content: flex-start;
  gap: 8px;
  margin-top: 4px;
}
</style>
