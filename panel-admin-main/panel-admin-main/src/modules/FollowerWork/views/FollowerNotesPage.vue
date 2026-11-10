<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { useFollowerNotesQuery } from '../requests/queries/useFollowerNotesQuery'
import { buildFollowerReturnTo, sanitizeFollowerReturnTo } from '../composables/useFollowerReturnContext'
import type { FollowerNoteRow } from '../domain/notesTypes'
import '../components/follower-work.css'
import '../components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('FollowerNotes', 'Read') === true)
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

const notesQuery = useFollowerNotesQuery({
  followerId,
  branchId,
  page,
  pageSize,
  enabled: canRead,
})

watch([followerId, branchId], () => {
  selectedId.value = null
})

const data = computed(() => notesQuery.data.value)
const selected = computed<FollowerNoteRow | null>(() => {
  const id = selectedId.value
  if (id == null) return null
  return data.value?.items.find((r) => r.noteId === id) ?? null
})

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'ManagementFollowerNotesRoute',
    query: q,
    params: route.params,
  })
}

function preview(text: string) {
  const t = (text || '').trim()
  if (t.length <= 120) return t
  return `${t.slice(0, 120)}…`
}

function openFullCustomerProfile(customerId: number) {
  const returnTo = buildFollowerReturnTo({
    branchId: String(branchId.value ?? ''),
    followerId: String(followerId.value ?? ''),
    section: 'notes',
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
      <h1 class="fw-page__title">الملاحظات</h1>
      <p class="fw-page__sub">
        {{ followerName || data?.followerName || `متابع ${followerId ?? '—'}` }}
        ·
        {{ branchName || `فرع ${branchId ?? '—'}` }}
      </p>
      <p class="fw-page__sub">ملاحظات الزبون المشتركة — عرض فقط من اللوحة.</p>
    </header>

    <div v-if="!canRead" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية قراءة الملاحظات.
    </div>
    <div v-else-if="followerId == null || branchId == null" class="fw-state" role="alert">
      يلزم اختيار متابع ومحافظة من دليل عمل المتابعين.
    </div>
    <div v-else-if="notesQuery.isPending.value" class="fw-state fw-state--loading" aria-busy="true">
      جاري تحميل الملاحظات…
    </div>
    <div v-else-if="notesQuery.isError.value" class="fw-state fw-state--error" role="alert">
      تعذر تحميل الملاحظات.
      <button type="button" class="fw-retry" @click="notesQuery.refetch()">إعادة المحاولة</button>
    </div>

    <template v-else-if="data">
      <div class="fdc-meta">
        <span>العدد: <b>{{ data.totalCount }}</b></span>
      </div>

      <div v-if="data.totalCount === 0" class="fw-state" role="status">لا ملاحظات تقييم</div>

      <div v-else class="fdc-list" role="list">
        <button
          v-for="r in data.items"
          :key="r.noteId"
          type="button"
          class="fdc-row"
          :class="{ 'fdc-row--active': selectedId === r.noteId }"
          @click="selectedId = r.noteId"
        >
          <span class="fdc-row__title">{{ r.customerName }}</span>
          <span class="fdc-row__sub">
            {{ r.listName || '—' }} · {{ r.authorName || '—' }} · {{ fmtDt(r.createdAtUtc) }}
          </span>
          <span class="fdc-row__sub">{{ preview(r.noteText) }}</span>
        </button>
      </div>

      <div v-if="data.totalPages > 1" class="fdc-pager">
        <button type="button" class="fw-pager__btn" :disabled="page <= 1" @click="patchQuery({ page: String(page - 1) })">
          السابق
        </button>
        <span>صفحة {{ data.page }} / {{ data.totalPages }}</span>
        <button
          type="button"
          class="fw-pager__btn"
          :disabled="page >= data.totalPages"
          @click="patchQuery({ page: String(page + 1) })"
        >
          التالي
        </button>
      </div>

      <section v-if="selected" class="fdc-details" aria-label="تفاصيل الملاحظة">
        <div class="fdc-details__head">
          <h2>{{ selected.customerName }}</h2>
          <button type="button" class="fw-pager__btn" @click="selectedId = null">إغلاق</button>
        </div>
        <div class="fdc-details__grid">
          <div><span>الهاتف</span><b>{{ selected.phone || '—' }}</b></div>
          <div><span>القائمة</span><b>{{ selected.listName || '—' }}</b></div>
          <div><span>الكاتب</span><b>{{ selected.authorName || '—' }}</b></div>
          <div><span>الوقت</span><b>{{ fmtDt(selected.createdAtUtc) }}</b></div>
          <div style="grid-column: 1 / -1"><span>الملاحظة</span><b style="white-space: pre-wrap">{{ selected.noteText || '—' }}</b></div>
        </div>
        <button
          type="button"
          class="fw-card__btn"
          style="margin-top: 1rem; min-height: 44px"
          @click="openFullCustomerProfile(selected.customerId)"
        >
          عرض بروفايل الزبون
        </button>
      </section>
    </template>
  </div>
</template>
