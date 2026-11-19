<template>
  <section class="flow" dir="rtl">
    <div class="flow__bar">
      <p class="flow__hint">
        أين كل حزمة، وكم استُلم منها، وبأي سرعة تمشي. الساعات محسوبة من سجل الأحداث
        لا من آخر ختم، لأن الاسترجاع يُمرّ المعاملة بالمرحلة أكثر من مرة.
      </p>
      <button type="button" class="btn btn--ghost" :disabled="loading" @click="load">تحديث</button>
    </div>

    <p v-if="loading" class="flow__state">جارٍ التحميل…</p>
    <p v-else-if="error" class="flow__state flow__state--error">{{ error }}</p>
    <p v-else-if="rows.length === 0" class="flow__state">لا حزم بعد</p>

    <div v-else class="flow__scroll">
      <table class="flow__table">
        <thead>
          <tr>
            <th>الحزمة</th>
            <th>موظف المبيعات</th>
            <th>الفرع</th>
            <th>أين</th>
            <th>عالقة عند</th>
            <th>مؤرشف</th>
            <th>مسترجَع</th>
            <th>منذ أول إرسال</th>
            <th>مبيعات ← فرع</th>
            <th>فرع ← متابع</th>
            <th>متابع ← أرشيف</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in rows" :key="row.bundleId">
            <td>{{ row.periodLabel }}</td>
            <td>{{ row.salesEmployeeName }}</td>
            <td>{{ row.branchName ?? '—' }}</td>
            <td>{{ row.whereLabel }}</td>
            <td>
              <span v-if="row.stuckAtStage != null" class="tag">
                {{ ARCHIVE_STAGE_LABEL[row.stuckAtStage] }}
              </span>
              <span v-else>—</span>
            </td>
            <td>{{ row.archivedCount }} / {{ row.itemCount }}</td>
            <td :class="{ 'is-danger': row.openReturnCount > 0 }">{{ row.openReturnCount }}</td>
            <td>{{ row.daysSinceFirstSend != null ? `${row.daysSinceFirstSend} يوم` : '—' }}</td>
            <td>{{ hours(row.averageHoursSalesToBranch) }}</td>
            <td>{{ hours(row.averageHoursBranchToFollower) }}</td>
            <td>{{ hours(row.averageHoursFollowerToArchive) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { fetchFlowBoard } from '../requests'
import { ARCHIVE_STAGE_LABEL, type ArchiveFlowRow } from '../types'

const rows = ref<ArchiveFlowRow[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

function hours(value?: number | null) {
  if (value == null) return '—'
  if (value < 24) return `${value} ساعة`
  return `${Math.round(value / 24)} يوم`
}

async function load() {
  loading.value = true
  error.value = null
  try {
    rows.value = await fetchFlowBoard()
  } catch {
    error.value = 'تعذّر تحميل لوحة التدفّق'
    rows.value = []
  } finally {
    loading.value = false
  }
}

onMounted(() => void load())
</script>

<style scoped>
.flow {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.flow__bar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.flow__hint {
  margin: 0;
  max-width: 70ch;
  font-size: 13px;
  color: #50666a;
  line-height: 1.6;
}

.flow__state {
  margin: 20px 0;
  color: #50666a;
}

.flow__state--error {
  color: #ba1a1a;
}

.flow__scroll {
  overflow: auto;
  max-height: 65vh;
  border-radius: 14px;
  border: 1px solid #e3ecea;
  background: #fff;
}

.flow__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
  white-space: nowrap;
}

.flow__table th,
.flow__table td {
  text-align: right;
  padding: 9px 12px;
  border-bottom: 1px solid #eef4f2;
}

.flow__table th {
  position: sticky;
  top: 0;
  background: #f1f5f4;
  color: #50666a;
  font-weight: 700;
}

.is-danger {
  color: #ba1a1a;
  font-weight: 700;
}

.tag {
  padding: 2px 10px;
  border-radius: 999px;
  background: rgba(0, 106, 100, 0.1);
  color: #006a64;
  font-size: 12px;
  font-weight: 700;
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
}

.btn--ghost {
  background: #fff;
  color: #006a64;
}

.btn:disabled {
  opacity: 0.5;
  cursor: default;
}
</style>
