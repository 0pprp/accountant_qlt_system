<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!running"
    :close-on-escape="!running"
    :dismissable-mask="!running"
    header="ترحيل التسديدات السابقة إلى قاصات القوائم"
    :style="{ width: 'min(640px, 96vw)' }"
    :pt="dialogMaskPt"
    class="bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && !running && emit('close')"
  >
    <div class="backfill">
      <p class="backfill__hint">
        كل تسديد جديد يدخل قاصة قائمته تلقائياً. هذه العملية لمرة واحدة: تُدخل التسديدات التي سُجِّلت قبل التفعيل.
        تسديدات النظام القديم المستوردة موجودة في القاصات أصلاً فلا تتكرر.
      </p>

      <p v-if="preview.isPending.value" class="backfill__state" role="status">جارٍ حساب المعاينة…</p>
      <p v-else-if="previewError" class="backfill__error" role="alert">{{ previewError }}</p>

      <template v-else-if="result">
        <p v-if="done" class="backfill__done" role="status">
          تم ترحيل {{ result.count.toLocaleString('en-US') }} تسديداً بمجموع {{ formattedPrice(result.amount) }} د.ع
        </p>

        <div class="backfill__total">
          <span>{{ done ? 'المتبقي للترحيل' : 'سيُرحَّل' }}</span>
          <strong class="q-num">{{ (done ? 0 : result.count).toLocaleString('en-US') }} تسديد</strong>
          <strong class="q-num">{{ formattedPrice(done ? 0 : result.amount) }} د.ع</strong>
        </div>

        <table v-if="!done && result.lists.length > 0" class="backfill__table">
          <thead>
            <tr>
              <th>القائمة</th>
              <th>عدد التسديدات</th>
              <th>المبلغ</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="line in result.lists" :key="line.orderListId">
              <td>{{ line.orderListName }}</td>
              <td class="q-num">{{ line.count.toLocaleString('en-US') }}</td>
              <td class="q-num">{{ formattedPrice(line.amount) }}</td>
            </tr>
          </tbody>
        </table>

        <ul class="backfill__skipped">
          <li>مُرحَّلة سابقاً: <span class="q-num">{{ result.alreadyPosted.toLocaleString('en-US') }}</span></li>
          <li>مستوردة من النظام القديم (موجودة في القاصة): <span class="q-num">{{ result.skippedImported.toLocaleString('en-US') }}</span></li>
          <li>مقدمات بيع لا تُرحَّل للقائمة: <span class="q-num">{{ result.skippedNotEnteredAsPayment.toLocaleString('en-US') }}</span></li>
        </ul>
      </template>

      <p v-if="runError" class="backfill__error" role="alert">{{ runError }}</p>

      <footer class="backfill__foot">
        <AppButton variant="ghost" :disabled="running" @click="emit('close')">
          {{ done ? 'إغلاق' : 'إلغاء' }}
        </AppButton>
        <AppButton
          v-if="!done"
          :disabled="running || !result || result.count === 0"
          :loading="running"
          @click="run"
        >
          {{ running ? 'جارٍ الترحيل…' : 'ترحيل الآن' }}
        </AppButton>
      </footer>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { AppButton } from '@/modules/Core/components/ui'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { formattedPrice } from '@/modules/Core/utils/price'
import { QUERY_KEY_SAFES } from '../requests/queries/useSafesQuery'
import { QUERY_KEY_SAFE_MOVEMENTS } from '../requests/queries/useSafeMovementsQuery'
import { QUERY_KEY_SAFE_VERIFY } from '../requests/queries/useVerifySafeQuery'
import type { BackfillPaymentCollectionsResult } from '../types/model'

const props = defineProps<{ visible: boolean; branchId: number }>()
const emit = defineEmits<{ close: [] }>()

const queryClient = useQueryClient()
const result = ref<BackfillPaymentCollectionsResult | null>(null)
const done = ref(false)
const runError = ref('')

async function call(dryRun: boolean) {
  const { data } = await axiosInstance.post<BackfillPaymentCollectionsResult>(
    '/admin/Safes/backfill-payment-collections',
    null,
    { params: { branchId: props.branchId, dryRun } },
  )
  return data
}

const preview = useMutation({ mutationFn: () => call(true) })
const execute = useMutation({ mutationFn: () => call(false) })
const running = computed(() => execute.isPending.value)

const previewError = computed(() => (preview.isError.value ? errorText(preview.error.value) : ''))

function errorText(error: unknown) {
  const message = (error as { response?: { data?: { message?: string; detail?: string } } })?.response?.data
  return message?.message ?? message?.detail ?? 'تعذّر تنفيذ العملية'
}

watch(
  () => props.visible,
  (open) => {
    if (!open) return
    result.value = null
    done.value = false
    runError.value = ''
    preview.mutate(undefined, { onSuccess: (data) => (result.value = data) })
  },
  { immediate: true },
)

function run() {
  runError.value = ''
  execute.mutate(undefined, {
    onSuccess: (data) => {
      result.value = data
      done.value = true
      void queryClient.invalidateQueries({ queryKey: [QUERY_KEY_SAFES] })
      void queryClient.invalidateQueries({ queryKey: [QUERY_KEY_SAFE_MOVEMENTS] })
      void queryClient.invalidateQueries({ queryKey: [QUERY_KEY_SAFE_VERIFY] })
    },
    onError: (error) => (runError.value = errorText(error)),
  })
}

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}
</script>

<style scoped>
.backfill { display: grid; gap: var(--q-space-3, 12px); }
.backfill__hint, .backfill__state { color: var(--q-muted); font-size: 13px; line-height: 1.7; }
.backfill__error { color: var(--q-danger); font-size: 13px; }
.backfill__done { color: var(--q-success, #1f7a5c); font-weight: 700; }
.backfill__total {
  align-items: baseline; background: var(--q-neo-bg, #eef3f2); border-radius: 14px;
  display: flex; flex-wrap: wrap; gap: 16px; padding: 12px 16px;
}
.backfill__total span { color: var(--q-muted); font-size: 13px; }
.backfill__table { border-collapse: collapse; font-size: 14px; width: 100%; }
.backfill__table th { color: var(--q-muted); font-weight: 600; text-align: start; }
.backfill__table th, .backfill__table td { border-bottom: 1px solid var(--q-border, #e2e8e6); padding: 8px 6px; }
.backfill__skipped { color: var(--q-muted); display: grid; font-size: 13px; gap: 4px; margin: 0; padding-inline-start: 18px; }
.backfill__foot { display: flex; gap: 8px; justify-content: flex-start; margin-top: 4px; }
</style>
