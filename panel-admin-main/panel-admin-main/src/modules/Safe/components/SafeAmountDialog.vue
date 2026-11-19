<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    :dismissable-mask="!busy"
    :header="mode === 'withdraw' ? $t('safe.operations.withdraw') : $t('safe.operations.deposit')"
    :style="{ width: 'min(520px, 96vw)' }"
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
      <p class="safe-dialog__target">
        {{ safeName }}
        <span class="safe-dialog__balance">
          {{ $t('safe.fields.balance') }}: {{ formattedBalance }}
          <small>{{ $t('words.dinar') }}</small>
        </span>
      </p>

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
        :required="mode === 'withdraw'"
        :disabled="busy"
      />

      <p
        v-if="mode === 'withdraw'"
        class="safe-dialog__hint"
      >
        {{ $t('safe.operations.withdrawHint') }}
      </p>

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
          :variant="mode === 'withdraw' ? 'danger' : 'primary'"
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
import { AppButton, AppInput, AppTextarea } from '@/modules/Core/components/ui'
import { formattedPrice } from '@/modules/Core/utils/price'
import type { SafeAmountForm } from '../types/model'

const props = defineProps<{
  visible: boolean
  mode: 'deposit' | 'withdraw'
  safeName: string
  safeBalance: number
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: [SafeAmountForm]
}>()

const { t } = useI18n()

const amount = ref('')
const note = ref('')
const touched = ref(false)

/**
 * يُولَّد مرة واحدة عند فتح النافذة لا عند الإرسال — فإعادة المحاولة بعد فشل شبكة
 * تحمل نفس المفتاح فلا تُنتج عملية ثانية.
 */
const operationKey = ref('')

const numericAmount = computed(() => Number(amount.value))

const isValidAmount = computed(
  () =>
    amount.value.trim().length > 0 &&
    Number.isFinite(numericAmount.value) &&
    numericAmount.value > 0 &&
    numericAmount.value % 1000 === 0,
)

const exceedsBalance = computed(
  () => props.mode === 'withdraw' && numericAmount.value > props.safeBalance,
)

const noteProvided = computed(() => props.mode !== 'withdraw' || note.value.trim().length > 0)

const valid = computed(() => isValidAmount.value && noteProvided.value && !exceedsBalance.value)

const validationMessage = computed(() => {
  if (!isValidAmount.value) return t('safe.validation.amount')
  if (exceedsBalance.value) return t('safe.validation.exceedsBalance')
  if (!noteProvided.value) return t('safe.validation.noteRequired')
  return ''
})

const formattedBalance = computed(() => formattedPrice(props.safeBalance))

watch(
  () => props.visible,
  (isOpen) => {
    if (!isOpen) return
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

.safe-dialog__target {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 8px;
  font-weight: 600;
  color: var(--q-fg);
}

.safe-dialog__balance {
  font-weight: 500;
  color: var(--q-muted);
}

.safe-dialog__hint {
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
