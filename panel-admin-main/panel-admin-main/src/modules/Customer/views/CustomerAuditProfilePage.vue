<template>
  <div class="audit-page min-h-screen bg-[#F8F8F8] p-4 text-[#00201E] pb-20">
    <button
      type="button"
      class="mb-4 text-sm text-[#006A64] underline"
      @click="goBack"
    >
      العودة للتدقيق
    </button>

    <header
      class="sticky top-0 z-10 mb-4 rounded-2xl border border-[#d7e2e0] bg-white/95 p-4 backdrop-blur"
    >
      <h1 class="text-xl font-semibold">
        {{ profile?.header.fullName || 'بروفايل التدقيق' }}
      </h1>
      <p class="text-sm opacity-80">
        #{{ customerId }} — {{ profile?.header.phone || '—' }} —
        {{ profile?.header.branchName || '—' }}
        <span v-if="profile?.header.statusLabel">
          — {{ profile.header.statusLabel }}
        </span>
      </p>
    </header>

    <p v-if="profileError" class="text-red-700">
      تعذر تحميل البروفايل
    </p>

    <nav class="mb-4 flex flex-wrap gap-2">
      <button
        v-for="tab in visibleTabs"
        :key="tab"
        type="button"
        class="rounded-full border px-3 py-1 text-sm"
        :class="
          activeTab === tab
            ? 'border-[#006A64] bg-[#006A64] text-white'
            : 'border-[#d7e2e0] bg-white'
        "
        @click="activeTab = tab"
      >
        {{ tabLabel(tab) }}
      </button>
    </nav>

    <AuditOverviewSection
      v-if="activeTab === 'overview' && profile?.overview"
      :overview="profile.overview"
    />
    <AuditAccountsSection
      v-else-if="activeTab === 'accounts' && profile?.accounts"
      :accounts="profile.accounts"
    />
    <AuditSalesSection
      v-else-if="activeTab === 'sales' && profile?.sales"
      :sales="profile.sales"
    />
    <AuditDocumentsSection
      v-else-if="activeTab === 'documents'"
      :documents="documents"
      :loading="docsLoading"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePermission } from '@/modules/Core/composable/usePermission'
import {
  isAllowedReturnRoute,
  parseAuditReturnQuery,
} from '@/modules/SalesRequests/composables/useAuditReturnContext'
import { useCustomerAuditProfileQuery } from '../requests/queries/useCustomerAuditProfileQuery'
import AuditOverviewSection from '../components/AuditProfile/AuditOverviewSection.vue'
import AuditAccountsSection from '../components/AuditProfile/AuditAccountsSection.vue'
import AuditSalesSection from '../components/AuditProfile/AuditSalesSection.vue'
import AuditDocumentsSection from '../components/AuditProfile/AuditDocumentsSection.vue'

const route = useRoute()
const router = useRouter()
const { can } = usePermission()

const customerId = computed(() => Number(route.params.id))
const canOpen =
  can('SalesRequest', 'ReadAuditProfile') ||
  can('SalesRequest', 'MatchRead') ||
  can('Customer', 'Read')
const canDocs =
  can('SalesRequest', 'ReadAuditDocuments') || can('Attachment', 'Read')

const activeTab = ref('overview')
const baseSections = computed(() => {
  const parts = ['overview', 'accounts', 'sales']
  return parts.join(',')
})

const enabled = computed(() => canOpen && Number.isFinite(customerId.value))
const baseQuery = useCustomerAuditProfileQuery(
  customerId,
  baseSections,
  enabled
)

const docsSections = ref('documents')
const docsEnabled = computed(
  () =>
    canDocs &&
    activeTab.value === 'documents' &&
    Number.isFinite(customerId.value)
)
const docsQuery = useCustomerAuditProfileQuery(
  customerId,
  docsSections,
  docsEnabled
)

const profile = computed(() => baseQuery.data.value)
const profileError = computed(() => baseQuery.isError.value)
const documents = computed(() => docsQuery.data.value?.documents ?? null)
const docsLoading = computed(() => docsQuery.isLoading.value)

const visibleTabs = computed(() => {
  const available = profile.value?.availableSections ?? [
    'overview',
    'accounts',
    'sales',
  ]
  return available.filter((t) => t !== 'documents' || canDocs)
})

watch(visibleTabs, (tabs) => {
  if (!tabs.includes(activeTab.value)) activeTab.value = tabs[0] ?? 'overview'
})

function tabLabel(tab: string) {
  const map: Record<string, string> = {
    overview: 'نظرة عامة',
    accounts: 'الحسابات والتسديدات',
    sales: 'المبيعات',
    documents: 'المستندات',
  }
  return map[tab] ?? tab
}

function goBack() {
  const returnTo = route.query.returnTo
  if (!isAllowedReturnRoute(returnTo)) {
    void router.push({ name: 'UnassignedSalesRequestsRoute' })
    return
  }
  const target = String(returnTo)
  // مسار نسبي (عمل المتابعين / زبائن الفرع) — يحفظ query الموجود في الرابط.
  if (target.startsWith('/')) {
    void router.push(target)
    return
  }
  const ctx = parseAuditReturnQuery(route.query as Record<string, unknown>)
  void router.push({
    name: target,
    query: { ...ctx },
  })
}
</script>

<style scoped>
.audit-page {
  font-family: 'ElMessiri', 'El Messiri', sans-serif;
}
</style>
