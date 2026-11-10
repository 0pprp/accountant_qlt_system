<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { useFollowerDelegateNotesQuery } from '../requests/queries/useFollowerDelegateNotesQuery'
import type { FollowerDelegateNoteRow } from '../domain/delegateNotesTypes'
import '../components/follower-work.css'
import '../components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('FollowerDelegateNotes', 'Read') === true)
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

const notesQuery = useFollowerDelegateNotesQuery({
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
const selected = computed<FollowerDelegateNoteRow | null>(() => {
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
    name: route.name ?? 'ManagementFollowerDelegateNotesRoute',
    query: q,
    params: route.params,
  })
}

function preview(text: string) {
  const t = (text || '').trim()
  if (t.length <= 120) return t
  return `${t.slice(0, 120)}…`
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
      <h1 class="fw-page__title">ملاحظات المندوبين</h1>
      <p class="fw-page__sub">
        {{ followerName || data?.followerName || `متابع ${followerId ?? '—'}` }}
        ·
        {{ branchName || `فرع ${branchId ?? '—'}` }}
      </p>
      <p class="fw-page__sub">
        ملاحظات يكتبها المتابع عن القائمة/المندوب — عرض فقط من اللوحة (ليست ملاحظات الزبون).
      </p>
    </header>

    <div v-if="!canRead" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية قراءة ملاحظات المندوبين.
    </div>
    <div v-else-if="followerId == null || branchId == null" class="fw-state" role="alert">
      يلزم اختيار متابع ومحافظة من دليل عمل المتابعين.
    </div>
    <div v-else-if="notesQuery.isPending.value" class="fw-state fw-state--loading" aria-busy="true">
      جاري تحميل ملاحظات المندوبين…
    </div>
    <div v-else-if="notesQuery.isError.value" class="fw-state fw-state--error" role="alert">
      تعذر تحميل ملاحظات المندوبين.
      <button type="button" class="fw-retry" @click="notesQuery.refetch()">إعادة المحاولة</button>
    </div>

    <template v-else-if="data">
      <div class="fdc-meta">
        <span>العدد: <b>{{ data.totalCount }}</b></span>
      </div>

      <div v-if="data.totalCount === 0" class="fw-state" role="status">لا ملاحظات مندوبين</div>

      <div v-else class="fdc-list" role="list">
        <button
          v-for="r in data.items"
          :key="r.noteId"
          type="button"
          class="fdc-row"
          :class="{ 'fdc-row--active': selectedId === r.noteId }"
          @click="selectedId = r.noteId"
        >
          <span class="fdc-row__title">{{ r.mandobName || `مندوب ${r.mandobUserId}` }}</span>
          <span class="fdc-row__sub">
            {{ r.listName || `قائمة ${r.orderListId}` }} · كاتب: {{ r.authorName || '—' }} ·
            {{ fmtDt(r.createdAtUtc) }}
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

      <section v-if="selected" class="fdc-details" aria-label="تفاصيل ملاحظة المندوب">
        <div class="fdc-details__head">
          <h2>{{ selected.mandobName || `مندوب ${selected.mandobUserId}` }}</h2>
          <button type="button" class="fw-pager__btn" @click="selectedId = null">إغلاق</button>
        </div>
        <div class="fdc-details__grid">
          <div><span>القائمة</span><b>{{ selected.listName || selected.orderListId }}</b></div>
          <div><span>كاتب الملاحظة (المتابع)</span><b>{{ selected.authorName || '—' }}</b></div>
          <div><span>دور الكاتب</span><b>{{ selected.authorRole || '—' }}</b></div>
          <div><span>الوقت</span><b>{{ fmtDt(selected.createdAtUtc) }}</b></div>
          <div style="grid-column: 1 / -1">
            <span>النص</span>
            <b style="white-space: pre-wrap">{{ selected.noteText || '—' }}</b>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>
