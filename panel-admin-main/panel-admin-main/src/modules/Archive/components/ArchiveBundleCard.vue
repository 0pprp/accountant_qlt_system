<template>
  <article class="bundle" :class="{ 'bundle--open': !bundle.isPeriodClosed }">
    <header class="bundle__head">
      <div>
        <h3 class="bundle__title">{{ bundle.periodLabel }}</h3>
        <p class="bundle__sub">
          {{ bundle.salesEmployeeName }}
          <span v-if="bundle.branchName"> · {{ bundle.branchName }}</span>
        </p>
      </div>
      <span v-if="!bundle.isPeriodClosed" class="chip chip--open">العشرة جارية</span>
      <span v-else-if="bundle.openReturnCount > 0" class="chip chip--danger">
        {{ bundle.openReturnCount }} مسترجَعة
      </span>
    </header>

    <p class="bundle__count">
      <template v-if="showReceivedByMe">
        استلمتُ <b>{{ bundle.receivedByMeCount }}</b> من <b>{{ bundle.itemCount }}</b>
      </template>
      <template v-else>
        تم استلام <b>{{ bundle.receivedOnwardCount }}</b> من <b>{{ bundle.itemCount }}</b>
      </template>
    </p>

    <ul class="bundle__stages">
      <li v-for="row in stageRows" :key="row.label">
        <span>{{ row.label }}</span>
        <b>{{ row.value }}</b>
      </li>
    </ul>

    <footer class="bundle__foot">
      <span v-if="bundle.pendingSendCount > 0" class="bundle__pending">
        {{ bundle.pendingSendCount }} بعهدتك
      </span>
      <span v-else class="bundle__pending bundle__pending--muted">لا شيء بعهدتك</span>
      <button type="button" class="btn" @click="$emit('open', bundle.id)">فتح الحزمة</button>
    </footer>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ArchiveStage, type ArchiveBundleListItem } from '../types'

const props = defineProps<{
  bundle: ArchiveBundleListItem
  stage?: number | null
}>()

defineEmits<{ open: [id: number] }>()

const showReceivedByMe = computed(
  () => props.stage != null && props.stage !== ArchiveStage.SalesEmployee,
)

const stageRows = computed(() => {
  const b = props.bundle
  return [
    { label: 'عند موظف المبيعات', value: b.withSalesEmployeeCount },
    { label: 'بانتظار الفرع', value: b.sentToBranchCount },
    { label: 'بعهدة الفرع', value: b.atBranchCount },
    { label: 'بانتظار المتابع', value: b.sentToFollowerCount },
    { label: 'بعهدة المتابع', value: b.atFollowerCount },
    { label: 'بانتظار الأرشيف', value: b.sentToArchiveCount },
    { label: 'مؤرشفة', value: b.archivedCount },
  ].filter((row) => row.value > 0)
})
</script>

<style scoped>
.bundle {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px 18px;
  border-radius: 16px;
  background: #fff;
  border: 1px solid #e3ecea;
  box-shadow: 0 2px 10px rgba(0, 32, 30, 0.05);
}

.bundle--open {
  border-style: dashed;
  background: #f8fbfa;
}

.bundle__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.bundle__title {
  margin: 0;
  font-size: 1rem;
  font-weight: 800;
  color: #00201e;
}

.bundle__sub {
  margin: 4px 0 0;
  font-size: 0.85rem;
  color: #50666a;
}

.chip {
  padding: 2px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
}

.chip--open {
  background: rgba(0, 106, 100, 0.1);
  color: #006a64;
}

.chip--danger {
  background: rgba(186, 26, 26, 0.12);
  color: #ba1a1a;
}

.bundle__count {
  margin: 0;
  font-size: 1.05rem;
  color: #00201e;
}

.bundle__count b {
  color: #006a64;
  font-size: 1.2rem;
}

.bundle__stages {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 4px;
  font-size: 13px;
}

.bundle__stages li {
  display: flex;
  justify-content: space-between;
  color: #50666a;
}

.bundle__stages b {
  color: #00201e;
}

.bundle__foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding-top: 8px;
  border-top: 1px solid #eef4f2;
}

.bundle__pending {
  font-size: 13px;
  font-weight: 700;
  color: #006a64;
}

.bundle__pending--muted {
  color: #93a5a8;
  font-weight: 500;
}

.btn {
  padding: 6px 16px;
  border-radius: 999px;
  border: 1px solid #006a64;
  background: #fff;
  color: #006a64;
  font-family: inherit;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
}

.btn:hover {
  background: #006a64;
  color: #fff;
}
</style>
