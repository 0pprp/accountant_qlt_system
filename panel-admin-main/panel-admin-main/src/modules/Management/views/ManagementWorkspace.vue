<template>
  <section class="mgmt-workspace" dir="rtl">
    <header class="mgmt-header">
      <div class="mgmt-heading-icon" aria-hidden="true"><i :class="['pi', appSection === 'branch' ? 'pi-building' : appSection === 'delegate' ? 'pi-briefcase' : 'pi-th-large']" /></div>
      <div>
        <h1 class="mgmt-title">{{ workspaceTitle }}</h1>
        <p class="mgmt-sub">{{ workspaceDescription }}</p>
      </div>
    </header>

    <nav class="mgmt-tabs" aria-label="تبويبات اللوحة">
      <template v-if="appSection">
        <button v-for="item in appItems" :key="item.id" type="button"
          class="mgmt-tab" :class="{ active: item.tab?.id === activeTabId, unavailable: !item.tab, locked: item.tab && isTabLocked(item.tab) }"
          :disabled="!item.tab" :aria-current="item.tab?.id === activeTabId ? 'page' : undefined"
          :aria-disabled="item.tab && isTabLocked(item.tab) ? true : undefined"
          @click="item.tab && goTab(item.tab)">
          <i :class="['pi', item.tab && isTabLocked(item.tab) ? 'pi-lock' : item.icon]" aria-hidden="true" />
          <span>{{ item.label }}</span><small v-if="!item.tab">غير متاح بعد</small>
        </button>
      </template>
      <template v-else>
        <button v-for="tab in primaryTabs" :key="tab.id" type="button" class="mgmt-tab"
          :class="{ active: tab.id === activeTabId || (tab.id === 'followerWork' && inFollowerWork), locked: isTabLocked(tab) }"
          :aria-current="tab.id === activeTabId ? 'page' : undefined"
          :aria-disabled="isTabLocked(tab) ? true : undefined" @click="goTab(tab)">
          <i v-if="isTabLocked(tab)" class="pi pi-lock" aria-hidden="true" />{{ $t(`management.${tab.labelKey}`) }}
        </button>
      </template>
    </nav>

    <nav v-if="!appSection && inFollowerWork" class="mgmt-subtabs" aria-label="أقسام عمل المتابعين">
      <button v-for="tab in followerTabs" :key="tab.id" type="button" class="mgmt-tab"
        :class="{ active: tab.id === activeTabId, locked: isTabLocked(tab) }"
        :aria-current="tab.id === activeTabId ? 'page' : undefined"
        :aria-disabled="isTabLocked(tab) ? true : undefined" @click="goTab(tab)">
        <i v-if="isTabLocked(tab)" class="pi pi-lock" aria-hidden="true" />{{ $t(`management.${tab.labelKey}`) }}
      </button>
    </nav>
    <p
      v-if="isBranchSection && !allowOpsContent"
      class="mgmt-visit-gate"
      role="status"
      data-testid="branch-visit-gate"
    >
      {{ gateMessage }}
    </p>
    <div
      v-if="allowOpsContent"
      :key="`mgmt-${auth.branchGeneration}-${activeSectionId}-${activeTabId}`"
      class="mgmt-body"
      data-testid="mgmt-ops-body"
    >
      <RouterView />
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/modules/Auth/store'
import { useManagementAccess } from '../composables/useManagementAccess'
import { useBranchVisitGate } from '../composables/useBranchVisitGate'
import { getTabByIds, getReadyTabsForSection, type ManagementTabDefinition } from '../domain/sections'
import { APP_TABS } from '../domain/workspaceNavigation'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const { t } = useI18n()
const { canOpenTab, isTabLocked } = useManagementAccess()
const {
  isBranchSection,
  allowOpsContent,
  gateMessage,
} = useBranchVisitGate()
// The router guard owns access and revalidation; the shell never redirects between apps.
const activeSectionId = computed(() => route.meta.managementSectionId ?? 'oversight')
const appSection = computed(() => activeSectionId.value === 'branch' || activeSectionId.value === 'delegate' ? activeSectionId.value : null)
const activeTabId = computed(() => String(route.meta.managementTabId ?? ''))
const workspaceTitle = computed(() => appSection.value ? t(`management.sections.${appSection.value}`) : t('management.title'))
const workspaceDescription = computed(() => appSection.value === 'branch'
  ? 'مهام الفرع والزبائن والتسديدات'
  : appSection.value === 'delegate' ? 'متابعة أعمال الفروع والطلبات والمهام' : t('management.subtitle'))
const appItems = computed(() => appSection.value ? APP_TABS[appSection.value].map(item => ({
  ...item, tab: item.tabId ? getTabByIds(appSection.value!, item.tabId) : undefined,
})) : [])
const oversightTabs = getReadyTabsForSection('oversight')
const primaryTabs = oversightTabs.filter(tab => !tab.id.startsWith('follower') || tab.id === 'followerWork')
const followerTabs = oversightTabs.filter(tab => tab.id.startsWith('follower') && tab.id !== 'followerWork')
const inFollowerWork = computed(() => activeTabId.value.startsWith('follower'))
function goTab(tab: ManagementTabDefinition) {
  if (!canOpenTab(tab)) {
    void router.push({ name: 'AccessDeniedRoute', query: { from: tab.routeName } })
    return
  }
  // Preserve the selected follower/branch only inside that workspace, not arbitrary old page filters.
  const context = tab.id.startsWith('follower') && inFollowerWork.value
    ? Object.fromEntries(Object.entries(route.query).filter(([key]) => ['followerId', 'branchId'].includes(key))) : {}
  void router.push({ name: tab.routeName, query: { ...context, ...tab.defaultQuery } })
}
</script>

<style scoped>
.mgmt-workspace { --mgmt-brand:#006a64; --mgmt-ink:#00201e; --mgmt-line:#dce7e4; color:var(--mgmt-ink); min-width:0; display:flex; flex-direction:column; gap:24px; }
.mgmt-header { display:flex; align-items:center; gap:14px; padding-block:8px; }
.mgmt-heading-icon { display:grid; place-items:center; width:48px; height:48px; flex:none; background:#e6f1ef; color:var(--mgmt-brand); border-radius:14px; font-size:23px; }
.mgmt-title { margin:0; font-size:26px; font-weight:700; line-height:1.5; }
.mgmt-sub { margin:2px 0 0; font-size:15px; line-height:1.8; color:#526b67; }
.mgmt-tabs, .mgmt-subtabs { display:flex; flex-wrap:wrap; gap:8px; padding:12px; border:1px solid var(--mgmt-line); border-radius:16px; background:#fff; }
.mgmt-tab { display:inline-flex; align-items:center; justify-content:center; gap:9px; min-height:46px; padding:10px 14px; border:1px solid transparent; border-radius:10px; color:var(--mgmt-ink); background:transparent; font:inherit; font-size:16px; line-height:1.5; cursor:pointer; }
.mgmt-tab:hover:not(:disabled) { background:#eef5f3; }
.mgmt-tabs > .mgmt-tab.active { color:white; background:var(--mgmt-brand); }
.mgmt-tab:focus-visible { outline:3px solid #71b4a9; outline-offset:3px; }
.mgmt-tab.locked { color:#526b67; }
.mgmt-tab.unavailable { color:#73837f; cursor:default; }
.mgmt-tab small { color:#6c746f; background:#f1f3f2; padding:2px 6px; border-radius:5px; font-size:11px; white-space:nowrap; }
.mgmt-subtabs { margin-top:-14px; border:0; border-bottom:1px solid var(--mgmt-line); border-radius:0; background:transparent; padding:0 0 12px; }
.mgmt-subtabs .mgmt-tab.active { color:var(--mgmt-brand); background:#e6f1ef; border-color:#b8d6ce; }
.mgmt-visit-gate { margin:0; padding:12px 14px; border:1px solid #b8d6ce; border-radius:12px; background:#e6f1ef; color:#00201e; font-size:15px; }
.mgmt-body { min-width:0; min-height:220px; }
.mgmt-body :deep(.mt-page-title), .mgmt-body :deep(.ms-title), .mgmt-body :deep(.mr-title) { font-size:22px; line-height:1.6; color:var(--mgmt-ink); }
@media(max-width:600px) {
  .mgmt-workspace { gap:16px; }
  .mgmt-header { padding-inline-start:42px; align-items:flex-start; }
  .mgmt-heading-icon { display:none; }
  .mgmt-title { font-size:23px; }
  .mgmt-sub { font-size:14px; }
  .mgmt-tabs { display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); padding:8px; gap:5px; }
  .mgmt-tab { font-size:14px; padding:9px 8px; gap:6px; flex-wrap:wrap; }
  .mgmt-tab small { font-size:10px; }
  .mgmt-subtabs .mgmt-tab { flex:1 0 40%; }
}
</style>
