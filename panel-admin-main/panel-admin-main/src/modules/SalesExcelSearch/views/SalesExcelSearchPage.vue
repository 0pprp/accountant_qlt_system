<template>
  <AppPage
    title="بحث Excel للزبائن"
    subtitle="ارفع ملفًا بعمود اسم الزبون أو الصق أسماء (سطر لكل اسم). المطابقة ضمن فروع جلستك — قراءة فقط."
  >
    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="غير مخول بقراءة الزبائن."
    />
    <template v-else>
      <div class="q-filters sx-toolbar">
        <AppField label="ملف نصي / CSV">
          <input
            class="q-input"
            type="file"
            accept=".csv,.txt,text/csv,text/plain"
            @change="onFile"
          />
        </AppField>
        <AppButton
          variant="primary"
          :disabled="busy || names.length === 0"
          :loading="busy"
          @click="runSearch"
        >
          بحث ({{ names.length }})
        </AppButton>
      </div>

      <AppTextarea
        v-model="paste"
        label="أو الصق الأسماء"
        placeholder="اسم لكل سطر"
        :rows="6"
        @update:model-value="syncPaste"
      />

      <p v-if="formError" class="q-state q-state--error" role="alert">{{ formError }}</p>
      <DataState
        v-else-if="busy"
        kind="loading"
        title="جاري البحث"
        message="جاري مطابقة الأسماء ضمن نطاق الفروع."
      />

      <AppPanel v-if="result" title="النتائج">
        <p class="q-count">
          أسماء: {{ result.nameCount }} · وُجد: {{ result.foundCount }} · ناقص:
          {{ result.missingCount }}
        </p>
        <article v-for="q in result.queries" :key="q.rowNumber" class="sx-query">
          <h2 class="sx-query-title">
            #{{ q.rowNumber }} — {{ q.requestedName || '(فارغ)' }}
            <small>{{ q.found ? `مطابقات ${q.matchCount}` : 'غير موجود' }}</small>
          </h2>
          <p v-if="q.warning" class="sx-warn">{{ q.warning }}</p>
          <ul v-if="q.matches.length" class="sx-matches">
            <li v-for="m in q.matches" :key="m.resultKey" class="sx-match">
              <strong>{{ m.fullName }}</strong>
              <span class="sx-meta">{{ m.branchName || `فرع ${m.branchId}` }}</span>
              <span v-if="m.phone" class="sx-phone" dir="ltr">{{ m.phone }}</span>
              <span class="sx-money">
                بيع {{ fmt(m.amountTotalSales) }} · متبقي {{ fmt(m.amountRemaining) }}
              </span>
            </li>
          </ul>
        </article>
      </AppPanel>
    </template>
  </AppPage>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import {
  AppButton,
  AppField,
  AppPage,
  AppPanel,
  AppTextarea,
  DataState,
} from '@/modules/Core/components/ui'

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
      { names: names.value },
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
  return new Intl.NumberFormat('en-US').format(n)
}
</script>

<style scoped>
.sx-toolbar {
  align-items: end;
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
  color: #00201e !important;
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
.sx-meta,
.sx-money {
  font-size: 14px;
  color: #526b67;
}
.sx-phone {
  font-variant-numeric: tabular-nums;
}
</style>
