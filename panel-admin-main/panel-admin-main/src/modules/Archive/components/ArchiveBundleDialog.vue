<template>
  <Dialog
    v-model:visible="isOpen"
    modal
    dir="rtl"
    class="bundle-dialog"
    :closable="false"
    :pt="{ mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' } }"
  >
    <template #header>
      <div class="bd__head">
        <div>
          <h2 class="bd__title">{{ detail?.bundle.periodLabel ?? 'الحزمة' }}</h2>
          <p v-if="detail" class="bd__sub">
            {{ detail.bundle.salesEmployeeName }}
            <span v-if="detail.bundle.branchName"> · {{ detail.bundle.branchName }}</span>
          </p>
        </div>
        <button type="button" class="btn btn--ghost" @click="isOpen = false">إغلاق</button>
      </div>
    </template>

    <p v-if="loading" class="bd__state">جارٍ التحميل…</p>
    <p v-else-if="error" class="bd__state bd__state--error">{{ error }}</p>

    <template v-else-if="detail">
      <p v-if="feedback" class="bd__feedback">{{ feedback }}</p>

      <div class="bd__bar">
        <label class="bd__check">
          <input
            type="checkbox"
            :checked="allSelectableChecked"
            :disabled="selectable.length === 0"
            @change="toggleAll"
          />
          تحديد الكل ({{ selectable.length }})
        </label>
        <span class="bd__picked">محدّد: {{ selected.size }}</span>

        <div class="bd__actions">
          <button
            v-if="detail.canReceive"
            type="button"
            class="btn"
            :disabled="selected.size === 0 || busy"
            @click="doReceive"
          >
            استلمت المعاملات المحددة
          </button>
          <button
            v-if="detail.canUndoSend && undoable.length > 0"
            type="button"
            class="btn btn--ghost"
            :disabled="selectedUndoable.length === 0 || busy"
            @click="doUndo"
          >
            إلغاء الإرسال
          </button>
          <button
            v-if="detail.canReturn"
            type="button"
            class="btn btn--danger"
            :disabled="selectedArchived.length === 0 || busy"
            @click="returnOpen = true"
          >
            استرجاع
          </button>
          <button
            type="button"
            class="btn"
            :disabled="!detail.canSend || busy"
            :title="sendTitle"
            @click="doSend"
          >
            إرسال الحزمة ({{ detail.bundle.pendingSendCount }})
          </button>
        </div>
      </div>

      <p v-if="!detail.bundle.isPeriodClosed" class="bd__note">
        العشرة ما زالت جارية حتى {{ detail.bundle.periodEnd }} — الإرسال مقفل حتى تنتهي.
      </p>

      <div class="bd__scroll">
        <table class="bd__table">
          <thead>
            <tr>
              <th></th>
              <th>الزبون</th>
              <th>المبيع</th>
              <th>التاريخ</th>
              <th>الحالة</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in detail.transactions"
              :key="row.id"
              :class="{
                'is-waiting': row.awaitsMyReceipt,
                'is-void': row.isVoided,
              }"
            >
              <td>
                <input
                  type="checkbox"
                  :checked="selected.has(row.id)"
                  :disabled="row.isVoided"
                  @change="toggle(row.id)"
                />
              </td>
              <td>
                <bdi>{{ row.customerName }}</bdi>
                <span v-if="row.hasOpenReturn" class="tag tag--danger">
                  مسترجَعة — {{ row.lastReturnReason }}
                </span>
                <span v-if="row.isVoided" class="tag tag--muted">
                  مُبطَلة — {{ row.voidReason }}
                </span>
              </td>
              <td>#{{ row.orderId }}</td>
              <td>{{ row.periodSourceDate }}</td>
              <td>{{ row.statusLabel }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <Dialog
      v-model:visible="returnOpen"
      modal
      dir="rtl"
      class="return-dialog"
      :closable="false"
    >
      <template #header><h3 class="bd__title">استرجاع معاملة ناقصة</h3></template>
      <div class="rd">
        <label>
          الوجهة
          <select v-model.number="returnDestination" class="input">
            <option :value="0">موظف المبيعات</option>
            <option :value="1">إدارة الفرع</option>
            <option :value="2">المتابع</option>
          </select>
        </label>
        <label>
          سبب النقص
          <textarea v-model="returnReason" class="input" rows="3" />
        </label>
        <p class="rd__count">{{ selectedArchived.length }} معاملة محدّدة</p>
        <div class="bd__actions">
          <button type="button" class="btn btn--ghost" @click="returnOpen = false">إلغاء</button>
          <button
            type="button"
            class="btn btn--danger"
            :disabled="returnReason.trim().length < 3 || busy"
            @click="doReturn"
          >
            تأكيد الاسترجاع
          </button>
        </div>
      </div>
    </Dialog>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Dialog } from 'primevue'
import {
  fetchBundle,
  receiveTransactions,
  returnTransactions,
  sendBundle,
  undoSend,
} from '../requests'
import { ArchiveTransactionStatus, type ArchiveBundleDetail } from '../types'

const props = defineProps<{
  bundleId: number | null
  stage?: number | null
}>()

const emit = defineEmits<{ changed: [] }>()
const isOpen = defineModel<boolean>({ default: false })

const detail = ref<ArchiveBundleDetail | null>(null)
const loading = ref(false)
const busy = ref(false)
const error = ref<string | null>(null)
const feedback = ref<string | null>(null)
const selected = ref<Set<number>>(new Set())

const returnOpen = ref(false)
const returnDestination = ref(0)
const returnReason = ref('')

const selectable = computed(
  () => detail.value?.transactions.filter((t) => !t.isVoided) ?? [],
)

const allSelectableChecked = computed(
  () => selectable.value.length > 0 && selectable.value.every((t) => selected.value.has(t.id)),
)

const undoable = computed(() => detail.value?.transactions.filter((t) => t.isInTransit) ?? [])

const selectedUndoable = computed(() =>
  undoable.value.filter((t) => selected.value.has(t.id)).map((t) => t.id),
)

const selectedArchived = computed(() =>
  (detail.value?.transactions ?? [])
    .filter((t) => selected.value.has(t.id) && t.status === ArchiveTransactionStatus.Archived)
    .map((t) => t.id),
)

const sendTitle = computed(() =>
  detail.value?.bundle.isPeriodClosed === false
    ? 'لا يمكن الإرسال قبل انتهاء العشرة'
    : 'إرسال كل ما بعهدتك إلى المرحلة التالية',
)

function toggle(id: number) {
  const next = new Set(selected.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  selected.value = next
}

function toggleAll() {
  selected.value = allSelectableChecked.value
    ? new Set()
    : new Set(selectable.value.map((t) => t.id))
}

async function load() {
  if (props.bundleId == null) return
  loading.value = true
  error.value = null
  feedback.value = null
  selected.value = new Set()

  try {
    detail.value = await fetchBundle(props.bundleId, props.stage ?? undefined)
  } catch {
    error.value = 'تعذّر تحميل الحزمة'
    detail.value = null
  } finally {
    loading.value = false
  }
}

async function run(action: () => Promise<{ message: string }>) {
  busy.value = true
  error.value = null
  try {
    const result = await action()
    feedback.value = result.message
    await load()
    emit('changed')
  } catch (e) {
    const payload = (e as { response?: { data?: { errors?: { message?: string }[] } } })
      .response?.data?.errors?.[0]?.message
    error.value = payload ?? 'تعذّر تنفيذ الإجراء'
  } finally {
    busy.value = false
  }
}

function doReceive() {
  const stage = detail.value?.myStage
  if (stage == null) return
  void run(() => receiveTransactions([...selected.value], stage))
}

function doSend() {
  const stage = detail.value?.myStage
  const id = props.bundleId
  if (stage == null || id == null) return
  void run(() => sendBundle(id, stage))
}

function doUndo() {
  const stage = detail.value?.myStage
  if (stage == null) return
  void run(() => undoSend(selectedUndoable.value, stage))
}

function doReturn() {
  const ids = selectedArchived.value
  if (ids.length === 0) return
  void run(async () => {
    const result = await returnTransactions(ids, returnDestination.value, returnReason.value)
    returnOpen.value = false
    returnReason.value = ''
    return result
  })
}

watch(
  () => [isOpen.value, props.bundleId] as const,
  ([open]) => {
    if (open) void load()
    else detail.value = null
  },
  { immediate: true },
)
</script>

<style scoped>
.bd__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  width: 100%;
}

.bd__title {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 800;
  color: #00201e;
}

.bd__sub {
  margin: 4px 0 0;
  font-size: 0.85rem;
  color: #50666a;
}

.bd__state {
  margin: 20px 0;
  color: #50666a;
}

.bd__state--error {
  color: #ba1a1a;
}

.bd__feedback {
  margin: 0 0 12px;
  padding: 8px 12px;
  border-radius: 10px;
  background: rgba(0, 106, 100, 0.1);
  color: #006a64;
  font-size: 13px;
  font-weight: 600;
}

.bd__bar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 14px;
  padding-bottom: 12px;
  border-bottom: 1px solid #eef4f2;
}

.bd__check {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  cursor: pointer;
}

.bd__picked {
  font-size: 13px;
  font-weight: 700;
  color: #006a64;
}

.bd__actions {
  display: flex;
  gap: 8px;
  margin-inline-start: auto;
  flex-wrap: wrap;
}

.bd__note {
  margin: 12px 0 0;
  font-size: 13px;
  color: #8a6d00;
}

.bd__scroll {
  margin-top: 12px;
  max-height: 55vh;
  overflow: auto;
}

.bd__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.bd__table th,
.bd__table td {
  text-align: right;
  padding: 8px 10px;
  border-bottom: 1px solid #eef4f2;
}

.bd__table th {
  position: sticky;
  top: 0;
  background: #f1f5f4;
  font-size: 13px;
  color: #50666a;
}

.is-waiting {
  background: rgba(0, 106, 100, 0.06);
}

.is-void {
  opacity: 0.55;
  text-decoration: line-through;
}

.tag {
  display: inline-block;
  margin-inline-start: 8px;
  padding: 1px 8px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
}

.tag--danger {
  background: rgba(186, 26, 26, 0.12);
  color: #ba1a1a;
}

.tag--muted {
  background: #eceff0;
  color: #50666a;
}

.rd {
  display: grid;
  gap: 12px;
  font-size: 13px;
  color: #50666a;
}

.rd__count {
  margin: 0;
  font-weight: 700;
  color: #00201e;
}

.input {
  display: block;
  width: 100%;
  margin-top: 4px;
  padding: 6px 10px;
  border-radius: 10px;
  border: 1px solid #cddbd7;
  font-family: inherit;
  background: #fff;
}

.btn {
  padding: 6px 16px;
  border-radius: 999px;
  border: 1px solid #006a64;
  background: #006a64;
  color: #fff;
  font-family: inherit;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
}

.btn--ghost {
  background: #fff;
  color: #006a64;
}

.btn--danger {
  background: #ba1a1a;
  border-color: #ba1a1a;
}

.btn:disabled {
  opacity: 0.45;
  cursor: default;
}
</style>

<style>
.bundle-dialog {
  width: min(94vw, 1100px);
  background: #fff;
  border-radius: 20px;
  padding: 18px 20px 20px;
}

.return-dialog {
  width: min(92vw, 460px);
  background: #fff;
  border-radius: 18px;
  padding: 16px 18px 18px;
}
</style>
