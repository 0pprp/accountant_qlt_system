<template>
  <section class="archive" dir="rtl">
    <header class="archive__head">
      <div>
        <h1 class="archive__title">الأرشيف</h1>
        <p class="archive__sub">حزم المعاملات وسلسلة العهدة الورقية</p>
      </div>
      <nav class="archive__tabs">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          type="button"
          class="tab"
          :class="{ 'tab--on': activeTab === tab.key }"
          @click="activeTab = tab.key"
        >
          {{ tab.label }}
        </button>
      </nav>
    </header>

    <template v-if="activeTab === 'bundles'">
      <div class="archive__filters">
        <label>
          المرحلة
          <select v-model.number="stage" class="input">
            <option v-for="s in stageOptions" :key="s.value" :value="s.value">
              {{ s.label }}
            </option>
          </select>
        </label>
        <label class="archive__check">
          <input v-model="pendingOnly" type="checkbox" />
          الوارد الذي ينتظرني فقط
        </label>
        <button type="button" class="btn btn--ghost" :disabled="isRefreshing" @click="reload">
          تحديث
        </button>
      </div>

      <p v-if="isLoading" class="archive__state">جارٍ التحميل…</p>
      <p v-else-if="error" class="archive__state archive__state--error">{{ error }}</p>
      <p v-else-if="bundles.length === 0" class="archive__state">
        لا حزم في هذه الشريحة
      </p>
      <div v-else class="archive__grid">
        <ArchiveBundleCard
          v-for="bundle in bundles"
          :key="bundle.id"
          :bundle="bundle"
          :stage="stage"
          @open="openBundle"
        />
      </div>
    </template>

    <ArchiveFlowBoard v-else />

    <ArchiveBundleDialog
      v-model="dialogOpen"
      :bundle-id="selectedBundleId"
      :stage="stage"
      @changed="reload"
    />
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import ArchiveBundleCard from '../components/ArchiveBundleCard.vue'
import ArchiveBundleDialog from '../components/ArchiveBundleDialog.vue'
import ArchiveFlowBoard from '../components/ArchiveFlowBoard.vue'
import { fetchBundles } from '../requests'
import { ArchiveStage, type ArchiveBundleListItem } from '../types'

const tabs = [
  { key: 'bundles' as const, label: 'الحزم' },
  { key: 'flow' as const, label: 'لوحة التدفّق' },
]

const stageOptions = [
  { value: ArchiveStage.Archive, label: 'وارد الأرشيف' },
  { value: ArchiveStage.Follower, label: 'عند المتابع' },
  { value: ArchiveStage.BranchManagement, label: 'عند إدارة الفرع' },
  { value: ArchiveStage.SalesEmployee, label: 'عند موظفي المبيعات' },
]

const activeTab = ref<'bundles' | 'flow'>('bundles')
const stage = ref<number>(ArchiveStage.Archive)
const pendingOnly = ref(false)
const bundles = ref<ArchiveBundleListItem[]>([])
const isLoading = ref(false)
const isRefreshing = ref(false)
const error = ref<string | null>(null)

const dialogOpen = ref(false)
const selectedBundleId = ref<number | null>(null)

async function load(initial = false) {
  if (initial) isLoading.value = true
  else isRefreshing.value = true
  error.value = null

  try {
    const data = await fetchBundles({
      pageIndex: 1,
      pageSize: 50,
      stage: stage.value,
      pendingOnly: pendingOnly.value,
    })
    bundles.value = data.items ?? []
  } catch {
    error.value = 'تعذّر تحميل الحزم'
    bundles.value = []
  } finally {
    isLoading.value = false
    isRefreshing.value = false
  }
}

function reload() {
  void load()
}

function openBundle(id: number) {
  selectedBundleId.value = id
  dialogOpen.value = true
}

watch([stage, pendingOnly], () => void load())
onMounted(() => void load(true))
</script>

<style scoped>
.archive {
  display: flex;
  flex-direction: column;
  gap: 18px;
  padding: 20px;
}

.archive__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
}

.archive__title {
  margin: 0;
  font-size: 1.4rem;
  font-weight: 800;
  color: #00201e;
}

.archive__sub {
  margin: 4px 0 0;
  color: #50666a;
  font-size: 0.9rem;
}

.archive__tabs {
  display: flex;
  gap: 8px;
}

.tab {
  padding: 6px 18px;
  border-radius: 999px;
  border: 1px solid #cddbd7;
  background: #fff;
  color: #50666a;
  font-family: inherit;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
}

.tab--on {
  background: #006a64;
  border-color: #006a64;
  color: #fff;
}

.archive__filters {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  font-size: 13px;
  color: #50666a;
}

.archive__check {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}

.input {
  margin-inline-start: 8px;
  padding: 5px 10px;
  border-radius: 10px;
  border: 1px solid #cddbd7;
  font-family: inherit;
  background: #fff;
}

.archive__grid {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(auto-fill, minmax(290px, 1fr));
}

.archive__state {
  margin: 24px 0;
  color: #50666a;
}

.archive__state--error {
  color: #ba1a1a;
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
