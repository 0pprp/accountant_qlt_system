<template>
  <div class="sx-page" dir="rtl">
    <header class="sx-head">
      <h1 class="sx-title">بحث Excel للزبائن</h1>
      <p class="sx-sub">
        ارفع ملفًا بعمود اسم الزبون أو الصق أسماء (سطر لكل اسم). المطابقة ضمن فروع جلستك —
        قراءة فقط.
      </p>
    </header>

    <div v-if="!canRead" class="sx-state sx-state--denied" role="status">
      غير مخول بقراءة الزبائن.
    </div>
    <template v-else>
      <div class="sx-toolbar">
        <label class="sx-file">
          <span>ملف نصي / CSV</span>
          <input
            type="file"
            accept=".csv,.txt,text/csv,text/plain"
            @change="onFile"
          >
        </label>
        <button
          type="button"
          class="sx-btn"
          :disabled="busy || names.length === 0"
          @click="runSearch"
        >
          بحث ({{ names.length }})
        </button>
      </div>

      <label class="sx-paste">
        <span>أو الصق الأسماء</span>
        <textarea
          v-model="paste"
          rows="6"
          placeholder="اسم لكل سطر"
          @change="syncPaste"
        />
      </label>

      <p v-if="formError" class="sx-state sx-state--error" role="alert">{{ formError }}</p>
      <p v-else-if="busy" class="sx-state">جاري البحث…</p>

      <section v-if="result" class="sx-result" aria-live="polite">
        <p class="sx-summary">
          أسماء: {{ result.nameCount }} · وُجد: {{ result.foundCount }} · ناقص:
          {{ result.missingCount }}
        </p>
        <article
          v-for="q in result.queries"
          :key="q.rowNumber"
          class="sx-query"
        >
          <h2 class="sx-query-title">
            #{{ q.rowNumber }} — {{ q.requestedName || '(فارغ)' }}
            <small>{{ q.found ? `مطابقات ${q.matchCount}` : 'غير موجود' }}</small>
          </h2>
          <p v-if="q.warning" class="sx-warn">{{ q.warning }}</p>
          <ul v-if="q.matches.length" class="sx-matches">
            <li
              v-for="m in q.matches"
              :key="m.resultKey"
              class="sx-match"
            >
              <strong>{{ m.fullName }}</strong>
              <span class="sx-meta">{{ m.branchName || `فرع ${m.branchId}` }}</span>
              <span
                v-if="m.phone"
                class="sx-phone"
                dir="ltr"
              >{{ m.phone }}</span>
              <span class="sx-money">
                بيع {{ fmt(m.amountTotalSales) }} · متبقي {{ fmt(m.amountRemaining) }}
              </span>
            </li>
          </ul>
        </article>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Match {
  resultKey: string
  branchId: number
  branchName: string | null
  customerId: number
  fullName: string
  phone: string | null
  amountTotalSales: number | null
  amountRemaining: number | null
}

interface QueryRow {
  rowNumber: number
  requestedName: string
  found: boolean
  matchCount: number
  warning: string | null
  matches: Match[]
}

interface SearchResponse {
  nameCount: number
  foundCount: number
  missingCount: number
  queries: QueryRow[]
}

const auth = useAuthStore()
const canRead = computed(() => auth.hasPermission('Customer', 'Read') === true)
const paste = ref('')
const names = ref<string[]>([])
const busy = ref(false)
const formError = ref<string | null>(null)
const result = ref<SearchResponse | null>(null)

function parseLines(text: string): string[] {
  return text
    .split(/\r?\n/)
    .map((l) => l.replace(/^["']|["']$/g, '').trim())
    .filter((l) => l.length > 0)
}

function syncPaste() {
  names.value = parseLines(paste.value)
}

async function onFile(e: Event) {
  formError.value = null
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  const text = await file.text()
  const lines = parseLines(text)
  // تخطّي صف رأس إن بدا كـ «اسم»
  const body =
    lines.length > 0 && /اسم|name|customer/i.test(lines[0]!)
      ? lines.slice(1)
      : lines
  names.value = body
  paste.value = body.join('\n')
  input.value = ''
}

async function runSearch() {
  formError.value = null
  result.value = null
  if (names.value.length === 0) {
    formError.value = 'أدخل أسماء أو ارفع ملفًا.'
    return
  }
  busy.value = true
  try {
    const { data } = await axiosInstance.post<SearchResponse>(
      '/admin/SalesExcelSearch/customers',
      { names: names.value }
    )
    result.value = data
  } catch {
    formError.value = 'تعذر البحث (صلاحية/نطاق/حد الأسماء).'
  } finally {
    busy.value = false
  }
}

function fmt(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat('ar-IQ').format(n)
}
</script>

<style scoped>
.sx-page {
  --brand: #006a64;
  --ink: #00201e;
  color: var(--ink);
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
}
.sx-title {
  margin: 0;
  font-size: 26px;
  font-weight: 700;
  line-height: 1.5;
}
.sx-sub {
  margin: 4px 0 0;
  font-size: 15px;
  color: #526b67;
  line-height: 1.7;
}
.sx-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  align-items: end;
}
.sx-file,
.sx-paste {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 14px;
}
.sx-paste textarea,
.sx-file input {
  min-height: 44px;
  padding: 10px 12px;
  border: 1px solid #dce7e4;
  border-radius: 10px;
  background: #f8f8f8;
  font: inherit;
}
.sx-paste textarea {
  min-height: 120px;
  width: 100%;
  max-width: 720px;
}
.sx-btn {
  min-height: 44px;
  padding: 10px 16px;
  border: 0;
  border-radius: 10px;
  background: var(--brand);
  color: #fff;
  font: inherit;
  cursor: pointer;
}
.sx-btn:disabled {
  opacity: 0.55;
  cursor: default;
}
.sx-btn:focus-visible,
.sx-file input:focus-visible,
.sx-paste textarea:focus-visible {
  outline: 3px solid #71b4a9;
  outline-offset: 2px;
}
.sx-state {
  margin: 0;
  padding: 12px 14px;
  border-radius: 12px;
  background: #e6f1ef;
  font-size: 15px;
}
.sx-state--error,
.sx-state--denied {
  background: #f8e9e7;
  color: #5c1a14;
}
.sx-summary {
  font-size: 16px;
  font-weight: 600;
}
.sx-query {
  padding: 12px 0;
  border-bottom: 1px solid #dce7e4;
}
.sx-query-title {
  margin: 0;
  font-size: 18px;
  line-height: 1.5;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: baseline;
}
.sx-query-title small {
  font-size: 14px;
  color: #526b67;
  font-weight: 500;
}
.sx-warn {
  color: #7a4a00;
  font-size: 14px;
}
.sx-matches {
  list-style: none;
  margin: 8px 0 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.sx-match {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 16px;
  padding: 12px;
  background: #fff;
  border: 1px solid #dce7e4;
  border-radius: 12px;
  font-size: 16px;
  line-height: 1.6;
}
.sx-match strong {
  font-size: 18px;
}
.sx-meta,
.sx-money {
  font-size: 14px;
  color: #526b67;
}
.sx-phone {
  font-variant-numeric: tabular-nums;
}
</style>
