<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useLegalFollowupsQuery } from '../requests/queries/useLegalFollowupsQuery'
import { useCancelLegalFollowupMutation } from '../requests/mutations/useCancelLegalFollowupMutation'
import {
  downloadLegalFollowupDocument,
  previewLegalFollowupDocument,
} from '../requests/mutations/downloadLegalFollowupDocument'
import { SalesLegalEndpoints } from '../domain/endpoints'
import type { LegalFollowupCaseRow, LegalFollowupDocument } from '../domain/types'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('LegalFollowup', 'Read') === true)
const canCancel = computed(() => auth.hasPermission('LegalFollowup', 'Cancel') === true)

const branchId = computed(() => {
  const n = Number(route.query.branchId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const status = computed(() => {
  const s = String(route.query.status ?? 'InProgress')
  return s === 'Completed' ? 'Completed' : 'InProgress'
})
const search = computed(() => String(route.query.search ?? ''))
const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({ name: 'SaleLegalFollowupRoute', query: q })
}

const searchModel = ref(search.value)
watch(search, (v) => {
  searchModel.value = v
})

const listQuery = useLegalFollowupsQuery({
  branchId,
  status,
  search,
  page,
  enabled: canRead,
})

const detail = ref<LegalFollowupCaseRow | null>(null)
const detailLoading = ref(false)
const detailError = ref('')
const cancelReason = ref('')
const cancelError = ref('')
const previewUrl = ref<string | null>(null)
const previewType = ref('')
const docBusyId = ref<number | null>(null)
const cancelMut = useCancelLegalFollowupMutation()

async function openDetail(row: LegalFollowupCaseRow) {
  detail.value = row
  cancelReason.value = ''
  cancelError.value = ''
  detailError.value = ''
  detailLoading.value = true
  clearPreview()
  try {
    const { data } = await axiosInstance.get<LegalFollowupCaseRow>(
      SalesLegalEndpoints.byId(row.id)
    )
    detail.value = data
  } catch {
    detailError.value = 'تعذر تحميل تفاصيل المستمسكات.'
  } finally {
    detailLoading.value = false
  }
}

function clearPreview() {
  if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
  previewUrl.value = null
  previewType.value = ''
}

async function onPreview(doc: LegalFollowupDocument) {
  if (!detail.value || docBusyId.value != null) return
  docBusyId.value = doc.id
  try {
    clearPreview()
    const res = await previewLegalFollowupDocument(detail.value.id, doc.id)
    previewUrl.value = res.url
    previewType.value = res.contentType
  } catch {
    detailError.value = 'تعذر معاينة المستمسك.'
  } finally {
    docBusyId.value = null
  }
}

async function onDownload(doc: LegalFollowupDocument) {
  if (!detail.value || docBusyId.value != null) return
  docBusyId.value = doc.id
  try {
    await downloadLegalFollowupDocument(detail.value.id, doc.id, doc.fileName)
  } catch {
    detailError.value = 'تعذر تنزيل المستمسك.'
  } finally {
    docBusyId.value = null
  }
}

function closeDetail() {
  clearPreview()
  detail.value = null
}

async function submitCancel() {
  if (!detail.value || !canCancel.value) return
  cancelError.value = ''
  try {
    await cancelMut.mutateAsync({
      caseId: detail.value.id,
      reason: cancelReason.value.trim(),
      expectedVersion: detail.value.rowVersion,
    })
    detail.value = null
  } catch (e: unknown) {
    cancelError.value = 'تعذر الإلغاء. تحقق من السبب والصلاحية.'
  }
}

function fmtDt(v: string | null | undefined) {
  if (!v) return '—'
  try {
    return new Intl.DateTimeFormat('ar-IQ', {
      dateStyle: 'medium',
      timeStyle: 'short',
      timeZone: 'Asia/Baghdad',
    }).format(new Date(v))
  } catch {
    return v
  }
}
</script>

<template>
  <div class="sl-page" dir="rtl">
    <header class="sl-head">
      <h1 class="sl-title">المتابعة القانونية</h1>
      <p class="sl-sub">
        دورة متابعة قانونية لطلبات البيع — منفصلة عن أسماء الفرع القانونية وعن سجل قرارات المفوض.
      </p>
    </header>

    <div v-if="!canRead" class="sl-state sl-state--denied" role="alert">
      غير مخول بقراءة المتابعة القانونية.
    </div>

    <template v-else>
      <div class="sl-filters">
        <label class="sl-field sl-field--grow">
          <span>بحث</span>
          <div class="sl-search">
            <span aria-hidden="true">⌕</span>
            <input
              v-model="searchModel"
              type="search"
              class="sl-input"
              placeholder="زبون / هاتف / متابع"
              @keyup.enter="patchQuery({ search: searchModel.trim() || null, page: '1' })"
            />
          </div>
        </label>
        <label class="sl-field">
          <span>الفرع</span>
          <select
            class="sl-select"
            :value="branchId ?? ''"
            @change="
              patchQuery({
                branchId: ($event.target as HTMLSelectElement).value || null,
                page: '1',
              })
            "
          >
            <option value="">كل الفروع المسموحة</option>
            <option v-for="b in auth.branches ?? []" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </label>
        <div class="sl-tabs" role="tablist">
          <button
            type="button"
            class="sl-tab"
            :class="{ active: status === 'InProgress' }"
            @click="patchQuery({ status: 'InProgress', page: '1' })"
          >
            قيد العمل
          </button>
          <button
            type="button"
            class="sl-tab"
            :class="{ active: status === 'Completed' }"
            @click="patchQuery({ status: 'Completed', page: '1' })"
          >
            تمت المتابعة
          </button>
        </div>
      </div>

      <div v-if="listQuery.isPending.value" class="sl-state">جاري التحميل…</div>
      <div v-else-if="listQuery.isError.value" class="sl-state sl-state--error" role="alert">
        تعذر تحميل القائمة.
        <button type="button" class="sl-btn" @click="listQuery.refetch()">إعادة المحاولة</button>
      </div>
      <template v-else-if="listQuery.data.value">
        <p class="sl-count">{{ listQuery.data.value.totalCount }} متابعة</p>
        <div class="sl-table-wrap">
          <table class="sl-table">
            <thead>
              <tr>
                <th>الزبون</th>
                <th>الهاتف</th>
                <th>المتابع</th>
                <th>الحالة</th>
                <th>تاريخ الإرسال</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in listQuery.data.value.items" :key="row.id">
                <td>
                  <strong>{{ row.customerName }}</strong>
                  <div v-if="row.salesRequestId" class="sl-muted">طلب #{{ row.salesRequestId }}</div>
                </td>
                <td>{{ row.customerPhone || '—' }}</td>
                <td>{{ row.followerName || `#${row.followerUserId}` }}</td>
                <td>
                  <span class="sl-badge">{{ row.statusLabel }}</span>
                </td>
                <td>{{ fmtDt(row.createdAtUtc) }}</td>
                <td>
                  <button type="button" class="sl-btn sl-btn--sm" @click="openDetail(row)">
                    تفاصيل
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <p v-if="!listQuery.data.value.items.length" class="sl-state">لا متابعات مطابقة.</p>
        </div>
        <div class="sl-pager">
          <button
            type="button"
            class="sl-btn"
            :disabled="page <= 1"
            @click="patchQuery({ page: String(page - 1) })"
          >
            السابق
          </button>
          <span>صفحة {{ listQuery.data.value.page }}</span>
          <button
            type="button"
            class="sl-btn"
            :disabled="page * 20 >= listQuery.data.value.totalCount"
            @click="patchQuery({ page: String(page + 1) })"
          >
            التالي
          </button>
        </div>
      </template>
    </template>

    <div v-if="detail" class="sl-modal-bg" @click.self="closeDetail">
      <div class="sl-modal" role="dialog" aria-modal="true">
        <header>
          <h2>متابعة #{{ detail.id }}</h2>
          <button type="button" class="sl-btn sl-btn--ghost" @click="closeDetail">إغلاق</button>
        </header>
        <dl class="sl-dl">
          <div><dt>الزبون</dt><dd>{{ detail.customerName }}</dd></div>
          <div><dt>الهاتف</dt><dd>{{ detail.customerPhone || '—' }}</dd></div>
          <div><dt>العنوان</dt><dd>{{ detail.customerAddress || '—' }}</dd></div>
          <div><dt>المتابع</dt><dd>{{ detail.followerName || '—' }}</dd></div>
          <div><dt>الحالة</dt><dd>{{ detail.statusLabel }}</dd></div>
          <div>
            <dt>موقع المحل</dt>
            <dd>
              <template v-if="detail.shopLatitude != null && detail.shopLongitude != null">
                {{ detail.shopLatitude }}, {{ detail.shopLongitude }}
              </template>
              <template v-else>—</template>
            </dd>
          </div>
          <div v-if="detail.note"><dt>ملاحظة</dt><dd>{{ detail.note }}</dd></div>
          <div v-if="detail.completionNote">
            <dt>ملاحظة إكمال/إلغاء</dt>
            <dd>{{ detail.completionNote }}</dd>
          </div>
        </dl>

        <section class="sl-docs">
          <h3>المستمسكات</h3>
          <p v-if="detailLoading" class="sl-muted">جاري تحميل المستمسكات…</p>
          <p v-else-if="!detail.documents?.length" class="sl-muted">لا مستمسكات مرفوعة بعد.</p>
          <ul v-else class="sl-doc-list">
            <li v-for="doc in detail.documents" :key="doc.id">
              <div>
                <strong>{{ doc.typeLabel }}</strong>
                <span class="sl-muted"> — {{ doc.fileName }}</span>
              </div>
              <div class="sl-doc-actions">
                <button
                  type="button"
                  class="sl-btn"
                  :disabled="docBusyId === doc.id"
                  @click="onPreview(doc)"
                >
                  معاينة
                </button>
                <button
                  type="button"
                  class="sl-btn sl-btn--ghost"
                  :disabled="docBusyId === doc.id"
                  @click="onDownload(doc)"
                >
                  تنزيل
                </button>
              </div>
            </li>
          </ul>
          <div v-if="previewUrl" class="sl-preview">
            <img
              v-if="previewType.startsWith('image/')"
              :src="previewUrl"
              alt="معاينة مستمسك"
            />
            <iframe v-else :src="previewUrl" title="معاينة مستمسك" />
          </div>
          <p v-if="detailError" class="sl-state sl-state--error">{{ detailError }}</p>
        </section>

        <div v-if="canCancel && detail.canCancel" class="sl-cancel">
          <label>
            سبب الإلغاء
            <input v-model="cancelReason" class="sl-input" type="text" />
          </label>
          <button
            type="button"
            class="sl-btn sl-btn--danger"
            :disabled="!cancelReason.trim() || cancelMut.isPending.value"
            @click="submitCancel"
          >
            إلغاء المتابعة
          </button>
          <p v-if="cancelError" class="sl-state sl-state--error">{{ cancelError }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.sl-page {
  font-family: 'El Messiri', 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
  font-size: 16px;
  padding: 1rem 1.25rem 2rem;
  background: linear-gradient(180deg, #f8f8f8 0%, #eef5f4 100%);
  min-height: 100%;
}
.sl-title {
  margin: 0;
  color: #006a64;
  font-size: 1.75rem;
}
.sl-sub,
.sl-muted,
.sl-count {
  color: #3d524f;
  font-size: 14px;
}
.sl-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin: 1rem 0;
  align-items: flex-end;
}
.sl-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}
.sl-field--grow {
  flex: 1 1 14rem;
}
.sl-search {
  display: flex;
  align-items: stretch;
  min-height: 44px;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  overflow: hidden;
  background: #fff;
}
.sl-search span {
  display: grid;
  place-items: center;
  width: 2.5rem;
  background: #f8f8f8;
  color: #006a64;
}
.sl-search .sl-input {
  border: 0;
  flex: 1;
}
.sl-input,
.sl-select {
  min-height: 44px;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  padding: 0 0.75rem;
  font: inherit;
  background: #f8f8f8;
}
.sl-tabs {
  display: flex;
  gap: 0.35rem;
}
.sl-tab {
  min-height: 44px;
  padding: 0 1rem;
  border-radius: 8px;
  border: 1px solid #c5c5c5;
  background: #fff;
  font: inherit;
  cursor: pointer;
}
.sl-tab.active {
  background: #006a64;
  color: #fff;
  border-color: #006a64;
}
.sl-table-wrap {
  overflow-x: auto;
  background: #fff;
  border: 1px solid #d7e3e1;
  border-radius: 8px;
}
.sl-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 40rem;
}
.sl-table th,
.sl-table td {
  padding: 0.65rem 0.75rem;
  border-bottom: 1px solid #e6eeee;
  text-align: right;
}
.sl-table th {
  background: #f0f7f6;
  color: #006a64;
}
.sl-badge {
  display: inline-block;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  background: #d8f3e8;
  color: #0b6b4a;
  font-size: 13px;
}
.sl-btn {
  min-height: 44px;
  padding: 0 1rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #006a64;
  color: #fff;
  font: inherit;
  cursor: pointer;
}
.sl-btn--sm {
  min-height: 36px;
}
.sl-btn--ghost {
  background: #fff;
  color: #006a64;
}
.sl-btn--danger {
  background: #8b1e1e;
  border-color: #8b1e1e;
}
.sl-btn:disabled {
  opacity: 0.45;
}
.sl-pager {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-top: 1rem;
}
.sl-state {
  padding: 1rem;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #d7e3e1;
}
.sl-state--denied,
.sl-state--error {
  border-color: #c45c5c;
  color: #7a1f1f;
}
.sl-modal-bg {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  z-index: 40;
  padding: 1rem;
}
.sl-modal {
  background: #fff;
  border-radius: 12px;
  width: min(40rem, 100%);
  max-height: 85vh;
  overflow: auto;
  padding: 1rem 1.25rem;
}
.sl-modal header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.sl-dl {
  margin: 0.75rem 0;
}
.sl-dl > div {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.35rem 0;
  border-bottom: 1px solid #eef3f2;
}
.sl-dl dt {
  color: #5a6f6c;
}
.sl-dl dd {
  margin: 0;
  font-weight: 600;
}
.sl-docs {
  margin-top: 1rem;
  border-top: 1px solid #e6eeee;
  padding-top: 0.75rem;
}
.sl-docs h3 {
  margin: 0 0 0.5rem;
  color: #006a64;
  font-size: 1.05rem;
}
.sl-doc-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.sl-doc-list li {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 0.5rem;
  align-items: center;
  padding: 0.5rem 0.65rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.sl-doc-actions {
  display: flex;
  gap: 0.35rem;
}
.sl-preview {
  margin-top: 0.75rem;
  border: 1px solid #d7e3e1;
  border-radius: 8px;
  overflow: hidden;
  background: #f8f8f8;
  min-height: 12rem;
}
.sl-preview img,
.sl-preview iframe {
  display: block;
  width: 100%;
  max-height: 22rem;
  border: 0;
  object-fit: contain;
}
.sl-cancel {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-top: 1rem;
}
</style>
