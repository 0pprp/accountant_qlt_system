<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { useFollowerOldCustomersQuery } from '../requests/queries/useFollowerOldCustomersQuery'
import { useCancelLegalFollowupMutation } from '../requests/mutations/useCancelLegalFollowupMutation'
import { buildFollowerReturnTo, sanitizeFollowerReturnTo } from '../composables/useFollowerReturnContext'
import type { FollowerOldCustomerRow } from '../domain/oldCustomersTypes'
import '../components/follower-work.css'
import '../components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('FollowerOldCustomers', 'Read') === true)
const canCancel = computed(() => auth.hasPermission('LegalFollowup', 'Cancel') === true)

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
const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const pageSize = ref(50)
const selectedId = ref<number | null>(null)

const cancelOpen = ref(false)
const cancelReason = ref('')
const cancelError = ref<string | null>(null)

const query = useFollowerOldCustomersQuery({
  followerId,
  branchId,
  page,
  pageSize,
  enabled: canRead,
})
const cancelMutation = useCancelLegalFollowupMutation()

watch([followerId, branchId], () => {
  selectedId.value = null
  cancelOpen.value = false
})

const data = computed(() => query.data.value)
const selected = computed<FollowerOldCustomerRow | null>(() => {
  const id = selectedId.value
  if (id == null) return null
  return data.value?.items.find((r) => r.id === id) ?? null
})

function showCancelButton(row: FollowerOldCustomerRow) {
  return canCancel.value && row.canCancel && row.isDone === false
}

function openCancel() {
  if (!selected.value || !showCancelButton(selected.value)) return
  cancelReason.value = ''
  cancelError.value = null
  cancelOpen.value = true
}

async function submitCancel() {
  const row = selected.value
  if (!row) return
  const reason = cancelReason.value.trim()
  if (!reason) {
    cancelError.value = 'سبب الإلغاء مطلوب'
    return
  }
  if (!canCancel.value) {
    cancelError.value = 'ليست لديك صلاحية الإلغاء'
    return
  }
  cancelError.value = null
  try {
    await cancelMutation.mutateAsync({
      caseId: row.id,
      reason,
      expectedVersion: row.rowVersion,
    })
    cancelOpen.value = false
    selectedId.value = null
    await query.refetch()
  } catch (e: unknown) {
    const msg =
      (e as { response?: { data?: { message?: string } }; message?: string })?.response?.data
        ?.message ||
      (e as { message?: string })?.message ||
      'تعذر إلغاء الإرسال'
    cancelError.value = msg
  }
}

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'ManagementFollowerOldCustomersRoute',
    query: q,
    params: route.params,
  })
}

function openFullCustomerProfile(customerId: number) {
  const returnTo = buildFollowerReturnTo({
    branchId: String(branchId.value ?? ''),
    followerId: String(followerId.value ?? ''),
    section: 'old-customers',
    page: String(page.value),
  })
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: String(customerId) },
    query: { returnTo: sanitizeFollowerReturnTo(returnTo) ?? undefined },
  })
}

function fmtDt(v: string | null | undefined) {
  if (!v) return '—'
  try {
    return new Intl.DateTimeFormat('ar-IQ', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(v))
  } catch {
    return v
  }
}
</script>

<template>
  <div class="fw-page" dir="rtl">
    <header class="fw-page__header">
      <h1 class="fw-page__title">الزبائن القديمة</h1>
      <p class="fw-page__sub">
        {{ followerName || data?.followerName || `متابع ${followerId ?? '—'}` }}
        ·
        {{ branchName || `فرع ${branchId ?? '—'}` }}
      </p>
    </header>

    <div v-if="!canRead" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية قراءة الزبائن القديمة.
    </div>
    <div v-else-if="followerId == null || branchId == null" class="fw-state" role="alert">
      يلزم اختيار متابع ومحافظة من دليل عمل المتابعين.
    </div>
    <div v-else-if="query.isPending.value" class="fw-state fw-state--loading" aria-busy="true">
      جاري التحميل…
    </div>
    <div v-else-if="query.isError.value" class="fw-state fw-state--error" role="alert">
      تعذر تحميل القسم.
      <button type="button" class="fw-retry" @click="query.refetch()">إعادة المحاولة</button>
    </div>

    <template v-else-if="data">
      <p class="fdc-hint">{{ data.definitionHint }}</p>
      <div class="fdc-meta"><span>العدد: <b>{{ data.totalCount }}</b></span></div>

      <div v-if="data.totalCount === 0" class="fw-state" role="status">لا زبائن قديمة لهذا المتابع</div>

      <div v-else class="fdc-list" role="list">
        <button
          v-for="r in data.items"
          :key="r.id"
          type="button"
          class="fdc-row"
          :class="{ 'fdc-row--active': selectedId === r.id }"
          @click="selectedId = r.id"
        >
          <span class="fdc-row__title">
            {{ r.customerName }}
            <span class="fsr-badge" :class="r.isDone ? 'fsr-badge--ok' : 'fsr-badge--warn'">
              {{ r.statusLabel }}
            </span>
          </span>
          <span class="fdc-row__sub">
            {{ r.phone || r.address || '—' }} · أُرسل {{ fmtDt(r.createdAtUtc) }}
          </span>
        </button>
      </div>

      <div v-if="data.totalPages > 1" class="fdc-pager">
        <button type="button" class="fw-pager__btn" :disabled="page <= 1" @click="patchQuery({ page: String(page - 1) })">السابق</button>
        <span>صفحة {{ data.page }} / {{ data.totalPages }}</span>
        <button type="button" class="fw-pager__btn" :disabled="page >= data.totalPages" @click="patchQuery({ page: String(page + 1) })">التالي</button>
      </div>

      <section v-if="selected" class="fdc-details" aria-label="تفاصيل المتابعة">
        <div class="fdc-details__head">
          <h2>زبون قديم — {{ selected.customerName }}</h2>
          <div style="display: flex; gap: 0.5rem; flex-wrap: wrap">
            <button
              v-if="showCancelButton(selected)"
              type="button"
              class="fw-pager__btn"
              style="color: #a33; min-height: 44px"
              @click="openCancel"
            >
              إلغاء
            </button>
            <button type="button" class="fw-pager__btn" @click="selectedId = null">إغلاق</button>
          </div>
        </div>
        <div class="fdc-details__grid">
          <div><span>الهاتف</span><b>{{ selected.phone || '—' }}</b></div>
          <div><span>العنوان</span><b>{{ selected.address || '—' }}</b></div>
          <div><span>الحالة</span><b>{{ selected.statusLabel }}</b></div>
          <div><span>أُرسل</span><b>{{ fmtDt(selected.createdAtUtc) }}</b></div>
          <div><span>اكتمل</span><b>{{ fmtDt(selected.completedAtUtc) }}</b></div>
          <div><span>طلب البيع</span><b>{{ selected.salesRequestId ?? '—' }}</b></div>
          <div style="grid-column: 1 / -1">
            <span>ملاحظة</span>
            <b style="white-space: pre-wrap">{{ selected.note || selected.completionNote || '—' }}</b>
          </div>
        </div>
        <button
          v-if="selected.customerId"
          type="button"
          class="fw-card__btn"
          style="margin-top: 1rem; min-height: 44px"
          @click="openFullCustomerProfile(selected.customerId!)"
        >
          عرض بروفايل الزبون
        </button>
      </section>

      <div
        v-if="cancelOpen && selected"
        class="fdc-details"
        role="dialog"
        aria-modal="true"
        aria-label="إلغاء المتابعة القانونية"
        style="position: fixed; inset: 0; z-index: 40; background: rgba(0, 32, 30, 0.45); display: flex; align-items: center; justify-content: center; padding: 1rem; margin: 0"
      >
        <div style="background: #fff; border-radius: 12px; padding: 1.25rem; width: min(28rem, 100%); max-height: 90vh; overflow: auto">
          <h2 style="margin: 0 0 0.75rem; font-size: 18px; color: #00201e">إلغاء المتابعة</h2>
          <p style="font-size: 14px; color: #334; margin: 0 0 0.75rem">
            {{ selected.customerName }} — سيُعاد الطلب إلى غير مسند إن كان في حالة متابعة قانونية. لا يُحذف الزبون ولا الدين.
          </p>
          <label style="display: flex; flex-direction: column; gap: 0.35rem; margin-bottom: 0.75rem">
            <span style="font-size: 14px">سبب الإلغاء (إلزامي)</span>
            <textarea
              v-model="cancelReason"
              rows="3"
              style="font: inherit; font-size: 16px; padding: 0.65rem; background: #f8f8f8; border: 1px solid #d5e3e1; border-radius: 8px; min-height: 88px"
            />
          </label>
          <p v-if="cancelError" role="alert" style="color: #a33; font-size: 14px">{{ cancelError }}</p>
          <div style="display: flex; gap: 0.5rem; flex-wrap: wrap; margin-top: 0.75rem">
            <button
              type="button"
              class="fw-card__btn"
              style="min-height: 44px; background: #a33; color: #fff"
              :disabled="cancelMutation.isPending.value"
              @click="submitCancel"
            >
              {{ cancelMutation.isPending.value ? 'جاري الإلغاء…' : 'تأكيد الإلغاء' }}
            </button>
            <button
              type="button"
              class="fw-pager__btn"
              style="min-height: 44px"
              :disabled="cancelMutation.isPending.value"
              @click="cancelOpen = false"
            >
              إغلاق
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
