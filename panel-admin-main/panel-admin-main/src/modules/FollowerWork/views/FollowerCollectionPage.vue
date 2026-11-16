<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppDateField, DataState } from '@/modules/Core/components/ui'
import { formatDate, formatMoney, formatNumber } from '@/modules/Core/utils/formatDisplay'
import { useFollowerCollectionQuery } from '../requests/queries/useFollowerCollectionQuery'
import { useSetFollowerCollectionEnabledMutation } from '../requests/mutations/useSetFollowerCollectionEnabledMutation'
import type { FollowerCollectionListRow } from '../domain/collectionTypes'
import '../components/follower-work.css'
import '../components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('FollowerCollection', 'Read') === true)
const canManage = computed(
  () => auth.hasPermission('FollowerCollection', 'ManageEnabled') === true
)
const followerId = computed(() => {
  const n = Number(route.params.followerId ?? route.query.followerId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const branchId = computed(() => {
  const n = Number(route.query.branchId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const followerName = computed(() => String(route.query.followerName ?? '').trim())
const branchName = computed(() => {
  const b = (auth.branches ?? []).find((x) => x.id === branchId.value)
  return b?.name ?? ''
})
const paymentsPage = computed(() => {
  const n = Number(route.query.paymentsPage)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const fromDate = computed(() => {
  const v = String(route.query.fromDate ?? '').trim()
  return v || null
})
const toDate = computed(() => {
  const v = String(route.query.toDate ?? '').trim()
  return v || null
})
const paymentsPageSize = ref(50)
const savingListId = ref<number | null>(null)
const toggleError = ref<string | null>(null)
const conflictHint = ref(false)

const collectionQuery = useFollowerCollectionQuery({
  followerId,
  branchId,
  fromDate,
  toDate,
  paymentsPage,
  paymentsPageSize,
  enabled: canRead,
})
const toggleMutation = useSetFollowerCollectionEnabledMutation()

watch([followerId, branchId], () => {
  toggleError.value = null
  conflictHint.value = false
})

const data = computed(() => collectionQuery.data.value)

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'ManagementFollowerCollectionRoute',
    query: q,
    params: route.params,
  })
}

function fmtMoney(n: number, currency = 'IQD') {
  return formatMoney(n, currency)
}

function fmtDate(v: string | null | undefined) {
  return formatDate(v)
}

function sourceLabel(s: string) {
  if (s === 'follower') return 'متابع'
  if (s === 'mandob') return 'مندوب'
  return 'أخرى'
}

async function onToggle(list: FollowerCollectionListRow, next: boolean) {
  if (canManage.value !== true) return
  toggleError.value = null
  conflictHint.value = false
  savingListId.value = list.listId
  try {
    await toggleMutation.mutateAsync({
      listId: list.listId,
      enabled: next,
      expectedVersion: list.collectionToggleVersion,
    })
    await collectionQuery.refetch()
  } catch (e: unknown) {
    const status = (e as { response?: { status?: number; data?: { message?: string } } })?.response
      ?.status
    const msg =
      (e as { response?: { data?: { message?: string; error?: { message?: string } } } })?.response
        ?.data?.message ||
      (e as { response?: { data?: { error?: { message?: string } } } })?.response?.data?.error
        ?.message ||
      'تعذر تحديث حالة الجباية'
    toggleError.value = msg
    if (status === 409) {
      conflictHint.value = true
      await collectionQuery.refetch()
    }
  } finally {
    savingListId.value = null
  }
}
</script>

<template>
  <div class="fw-page" dir="rtl">
    <header class="fw-page__header">
      <h1 class="fw-page__title">الجباية</h1>
      <p class="fw-page__sub">
        {{ followerName || data?.followerName || `متابع ${followerId ?? '—'}` }}
        ·
        {{ branchName || `فرع ${branchId ?? '—'}` }}
      </p>
      <p class="fw-page__sub">
        تمكين جباية المتابع على القائمة يمنع جباية المندوب. لا إنشاء دفعات من هذه اللوحة.
      </p>
    </header>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="ليس لديك صلاحية قراءة الجباية."
    />
    <DataState
      v-else-if="followerId == null || branchId == null"
      kind="empty"
      title="يلزم اختيار"
      message="يلزم اختيار متابع ومحافظة من دليل عمل المتابعين."
    />
    <DataState
      v-else-if="collectionQuery.isPending.value"
      kind="loading"
      title="جاري التحميل"
      message="جاري تحميل الجباية…"
    />
    <DataState
      v-else-if="collectionQuery.isError.value"
      kind="error"
      title="تعذر التحميل"
      message="تعذر تحميل الجباية."
    >
      <template #actions>
        <AppButton variant="secondary" @click="collectionQuery.refetch()">إعادة المحاولة</AppButton>
      </template>
    </DataState>

    <template v-else-if="data">
      <div v-if="toggleError" class="fw-state fw-state--error" role="alert">
        {{ toggleError }}
        <span v-if="conflictHint"> — تم تحديث الحالة الأحدث.</span>
      </div>

      <section class="fdc-details" aria-label="قوائم الجباية" style="margin-bottom: 1rem">
        <h2 class="fw-selected__title">القوائم</h2>
        <DataState v-if="!data.lists.length" kind="empty" title="لا قوائم" message="لا قوائم مكلّفة" />
        <div v-else class="fdc-list" role="list">
          <div v-for="l in data.lists" :key="l.listId" class="fdc-row" role="listitem">
            <span class="fdc-row__title">{{ l.listName }}</span>
            <span class="fdc-row__sub">
              مندوب: {{ l.mandobName || l.mandobUserId }} ·
              {{ l.followerCollectionEnabled ? 'مفعّلة' : 'متوقفة' }}
              <template v-if="l.assignedAtUtc"> · تفعيل: {{ fmtDate(l.assignedAtUtc) }}</template>
            </span>
            <span class="fdc-row__sub">
              جباية مندوب أمس: {{ fmtMoney(l.lastDelegatePaymentAmount, data.totals.currency) }}
              · يوم التفعيل (متابع):
              {{ fmtMoney(l.lastFollowerPaymentAmount, data.totals.currency) }}
            </span>
            <label
              v-if="canManage"
              class="fw-card__btn fw-card__btn--secondary"
              style="display: inline-flex; align-items: center; gap: 0.5rem; margin-top: 0.5rem"
            >
              <input
                type="checkbox"
                :checked="l.followerCollectionEnabled"
                :disabled="savingListId === l.listId || toggleMutation.isPending.value"
                :aria-label="`تمكين جباية المتابع للقائمة ${l.listName}`"
                @change="
                  onToggle(l, ($event.target as HTMLInputElement).checked)
                "
              />
              تمكين جباية المتابع — {{ l.listName }}
            </label>
          </div>
        </div>
      </section>

      <div class="fdc-meta">
        <span>
          إجمالي الفترة (كل النتائج):
          <b>{{ fmtMoney(data.totals.paymentsFilteredAmount, data.totals.currency) }}</b>
          · {{ formatNumber(data.totals.paymentsFilteredCount) }} دفعة
        </span>
        <span>
          إجمالي الصفحة:
          <b>{{ fmtMoney(data.totals.paymentsPageAmount, data.totals.currency) }}</b>
        </span>
      </div>

      <div class="q-filters">
        <AppDateField
          :model-value="fromDate ?? ''"
          label="من"
          @update:model-value="(v) => patchQuery({ fromDate: v || null, paymentsPage: '1' })"
        />
        <AppDateField
          :model-value="toDate ?? ''"
          label="إلى"
          @update:model-value="(v) => patchQuery({ toDate: v || null, paymentsPage: '1' })"
        />
      </div>

      <h2 class="fw-selected__title">المدفوعات</h2>
      <DataState
        v-if="data.paymentsTotalCount === 0"
        kind="empty"
        title="لا مدفوعات"
        message="لا مدفوعات في النطاق"
      />
      <div v-else class="fdc-list" role="list">
        <div v-for="p in data.payments" :key="p.paymentId" class="fdc-row" role="listitem">
          <span class="fdc-row__title">{{ p.customerName }}</span>
          <span class="fdc-row__sub">
            {{ p.listName || `قائمة ${p.listId}` }} · {{ fmtMoney(p.amount, p.currency) }} ·
            {{ fmtDate(p.paymentDate) }}
          </span>
          <span class="fdc-row__sub">
            منفّذ: {{ p.collectedByName || '—' }} ({{ sourceLabel(p.source) }})
          </span>
        </div>
      </div>

      <div v-if="data.paymentsTotalPages > 1" class="fdc-pager">
        <AppButton
          variant="secondary"
          :disabled="paymentsPage <= 1"
          @click="patchQuery({ paymentsPage: String(paymentsPage - 1) })"
        >
          السابق
        </AppButton>
        <span>
          صفحة {{ formatNumber(data.paymentsPage) }} / {{ formatNumber(data.paymentsTotalPages) }}
        </span>
        <AppButton
          variant="secondary"
          :disabled="paymentsPage >= data.paymentsTotalPages"
          @click="patchQuery({ paymentsPage: String(paymentsPage + 1) })"
        >
          التالي
        </AppButton>
      </div>
    </template>
  </div>
</template>
