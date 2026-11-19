<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppDateField, DataState } from '@/modules/Core/components/ui'
import { formatDate, formatMoney, formatNumber } from '@/modules/Core/utils/formatDisplay'
import { useFollowerCollectionQuery } from '../requests/queries/useFollowerCollectionQuery'
import { useSetFollowerCollectionEnabledMutation } from '../requests/mutations/useSetFollowerCollectionEnabledMutation'
import { useFollowersQuery } from '../requests/queries/useFollowersQuery'
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
const branches = computed(() => auth.branches ?? [])
const allowedBranch = computed(() => branches.value.some(b => b.id === branchId.value))
const directoryPage = ref(1)
watch(branchId, () => { directoryPage.value = 1 }, { flush: 'sync' })
const directory = useFollowersQuery({
  branchId, search: ref(''), page: directoryPage, pageSize: ref(30), sort: ref('name'),
  enabled: computed(() => canRead.value && allowedBranch.value && followerId.value == null),
})
function choose(branch: number | null, follower?: { followerId: number; followerName: string }) {
  void router.push({
    name: 'ManagementFollowerCollectionRoute',
    query: {
      ...(branch ? { branchId: String(branch) } : {}),
      ...(follower ? { followerId: String(follower.followerId), followerName: follower.followerName } : {}),
    },
  })
}
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
  enabled: computed(() => canRead.value && allowedBranch.value),
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
      expectedFollowerId: followerId.value,
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
      <nav v-if="canRead && branchId" class="collection-path" aria-label="مسار الجباية">
        <button @click="choose(null)">المحافظات</button>
        <span aria-hidden="true">‹</span>
        <button v-if="followerId" @click="choose(branchId)">{{ branchName }}</button>
        <span v-else>{{ branchName }}</span>
        <template v-if="followerId"><span aria-hidden="true">‹</span>{{ data?.followerName || followerName }}</template>
      </nav>
    </header>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="ليس لديك صلاحية قراءة الجباية."
    />
    <section v-else-if="!allowedBranch" aria-label="المحافظات">
      <h2 class="fw-selected__title">المحافظات المسموحة</h2>
      <div v-if="branches.length" class="collection-grid">
        <button v-for="b in branches" :key="b.id" class="collection-card" @click="choose(b.id)">
          <span>{{ b.name }}</span><span aria-hidden="true">‹</span>
        </button>
      </div>
      <DataState v-else kind="empty" title="لا توجد محافظات متاحة لهذا الحساب" />
    </section>
    <section v-else-if="!followerId" aria-label="متابعو المحافظة">
      <h2 class="fw-selected__title">متابعو {{ branchName }}</h2>
      <DataState v-if="directory.isPending.value" kind="loading" title="جاري تحميل المتابعين" />
      <DataState v-else-if="directory.isError.value" kind="error" title="تعذر تحميل المتابعين">
        <template #actions><AppButton @click="directory.refetch()">إعادة المحاولة</AppButton></template>
      </DataState>
      <template v-else-if="directory.data.value?.items.length">
        <div class="collection-grid">
          <button v-for="f in directory.data.value.items" :key="f.followerId" class="collection-card" @click="choose(branchId, f)">
            <span>{{ f.followerName }}<small>عدد القوائم: {{ formatNumber(f.assignedListCount) }}</small></span>
            <span aria-hidden="true">‹</span>
          </button>
        </div>
        <div v-if="directory.data.value.totalPages > 1" class="fdc-pager">
          <AppButton :disabled="directoryPage === 1" @click="directoryPage--">السابق</AppButton>
          <span>{{ directoryPage }} / {{ directory.data.value.totalPages }}</span>
          <AppButton :disabled="directoryPage >= directory.data.value.totalPages" @click="directoryPage++">التالي</AppButton>
        </div>
      </template>
      <DataState v-else kind="empty" title="لا يوجد متابعون مكلّفون بقوائم في هذه المحافظة" />
    </section>
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
        <h2 class="fw-selected__title">قوائم المتابع</h2>
        <p>عند تفعيل جباية المتابع تتوقف جباية المندوب لهذه القائمة.</p>
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

<style scoped>
.collection-path,.collection-card{display:flex;align-items:center;gap:.75rem}
.collection-path{flex-wrap:wrap;margin-block:.6rem;color:#385550}
.collection-path button{color:#006a64;text-decoration:underline}
.collection-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(min(100%,215px),1fr));gap:.8rem;margin-block:1rem}
.collection-card{justify-content:space-between;text-align:start;border:1px solid #cadbd8;border-radius:14px;background:white;padding:1rem 1.2rem;color:#064c47;font:inherit;font-weight:700;min-height:76px;cursor:pointer}
.collection-card:hover,.collection-card:focus-visible{border-color:#006a64;background:#eff7f5}
.collection-card small{display:block;color:#4c625e;font-weight:400}
</style>
