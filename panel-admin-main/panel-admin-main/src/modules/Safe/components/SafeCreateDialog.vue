<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    :dismissable-mask="!busy"
    :header="$t('safe.operations.create')"
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
      <p class="safe-dialog__hint">
        {{ $t('safe.operations.createHint') }}
      </p>

      <AppSelect
        v-model="binding"
        :label="$t('safe.fields.bindingType')"
        :options="bindingOptions"
        required
        :disabled="busy"
      />

      <OrderListSelectInput
        v-if="binding === 'orderList'"
        v-model="orderListId"
        :label="$t('safe.fields.orderList')"
        :placeholder="$t('safe.fields.orderListPlaceholder')"
        required
        :disabled="busy"
      />

      <UsersSelectInput
        v-if="binding === 'user'"
        v-model="userId"
        :label="$t('safe.fields.employee')"
        :placeholder="$t('safe.fields.employeePlaceholder')"
        :branch-id="branchId"
        required
        :disabled="busy"
      />

      <p
        v-if="touched && !valid"
        class="safe-dialog__error"
        role="alert"
      >
        {{ $t('safe.validation.bindingRequired') }}
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
import { AppButton, AppSelect } from '@/modules/Core/components/ui'
import OrderListSelectInput from '@/modules/Core/components/shared/OrderListSelectInput/OrderListSelectInput.vue'
import UsersSelectInput from '@/modules/Core/components/shared/UsersSelectInput/UsersSelectInput.vue'
import type { CreateSafeForm } from '../types/model'

const props = defineProps<{
  visible: boolean
  branchId: number
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: [CreateSafeForm]
}>()

const { t } = useI18n()

const binding = ref<'orderList' | 'user'>('orderList')
const orderListId = ref<number | null>(null)
const userId = ref<number | null>(null)
const touched = ref(false)

const bindingOptions = computed(() => [
  { label: t('safe.fields.bindingOrderList'), value: 'orderList' },
  { label: t('safe.fields.bindingUser'), value: 'user' },
])

const valid = computed(() =>
  binding.value === 'orderList' ? orderListId.value !== null : userId.value !== null,
)

watch(
  () => props.visible,
  (isOpen) => {
    if (!isOpen) return
    binding.value = 'orderList'
    orderListId.value = null
    userId.value = null
    touched.value = false
  },
  { immediate: true },
)

// الربط واحد فقط — تغيير النوع يُفرّغ الآخر
watch(binding, () => {
  orderListId.value = null
  userId.value = null
})

function submit() {
  touched.value = true
  if (props.busy || !valid.value) return

  emit('submit', {
    branchId: props.branchId,
    orderListId: binding.value === 'orderList' ? orderListId.value : null,
    userId: binding.value === 'user' ? userId.value : null,
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
