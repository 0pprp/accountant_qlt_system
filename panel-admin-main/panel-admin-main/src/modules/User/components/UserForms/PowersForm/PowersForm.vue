<template>
  <div
    class="powers-form"
    :class="{ 'powers-form--editable': editable }"
    dir="rtl"
  >
    <header class="powers-form__header">
      <h2 class="powers-form__title">صلاحيات التبويبات</h2>
      <p class="powers-form__identity">
        <span>{{ displayName || '—' }}</span>
        <span class="powers-form__dot" aria-hidden="true">·</span>
        <span>{{ roleDisplayName || '—' }}</span>
      </p>
      <p class="powers-form__hint">
        مفتاح واحد لكل تبويب: مفتوح أو مقفول. التغييرات تُجمع في مسودة حتى الحفظ.
      </p>
      <div class="powers-form__meta">
        <span v-if="dirtyCount > 0">تعديلات بانتظار الحفظ: {{ dirtyCount }}</span>
        <span v-if="exceptionCount > 0">مخصص لهذا الحساب: {{ exceptionCount }}</span>
        <span
          v-if="isProtected"
          class="powers-form__badge powers-form__badge--warn"
        >
          حساب محمي — عرض فقط
        </span>
        <span
          v-else-if="!editable && userFormMode !== FormMode.IsView"
          class="powers-form__badge powers-form__badge--warn"
        >
          لا يمكن تعديل صلاحيات هذا الحساب من جلستك
        </span>
      </div>
      <p
        v-if="createRetryNotice"
        class="powers-form__retry-notice"
        role="alert"
      >
        {{ createRetryNotice }}
      </p>
    </header>

    <div
      v-if="serverLatest || conflictMessage"
      class="powers-form__conflict"
      role="status"
    >
      <p>
        {{
          conflictMessage ||
          'نسخة أحدث على الخادم. احسم التعارضات أو تراجع ثم أعد التحميل.'
        }}
      </p>
      <div v-if="conflicts.length" class="powers-form__conflict-list">
        <div
          v-for="c in conflicts"
          :key="c.tabKey"
          class="powers-form__conflict-row"
        >
          <div class="powers-form__conflict-name">{{ c.displayName }}</div>
          <div class="powers-form__conflict-vals">
            مسودتك: {{ c.draftEnabled ? 'مفتوح' : 'مقفول' }} — الخادم:
            {{ c.serverEnabled ? 'مفتوح' : 'مقفول' }}
          </div>
          <div class="powers-form__conflict-actions">
            <button
              type="button"
              class="powers-form__quiet-btn"
              :class="{
                active: conflictResolutions[c.tabKey] === 'draft',
              }"
              @click="setConflictResolution(c.tabKey, 'draft')"
            >
              إبقاء مسودتي
            </button>
            <button
              type="button"
              class="powers-form__quiet-btn"
              :class="{
                active: conflictResolutions[c.tabKey] === 'server',
              }"
              @click="setConflictResolution(c.tabKey, 'server')"
            >
              اعتماد الخادم
            </button>
          </div>
        </div>
        <p v-if="unresolvedConflicts.length" class="powers-form__warn-text">
          يجب حسم كل التبويبات المتعارضة قبل الحفظ.
        </p>
      </div>
      <div class="powers-form__conflict-bar">
        <button type="button" class="powers-form__quiet-btn" @click="compareWithServer">
          مقارنة مع الخادم
        </button>
        <button
          type="button"
          class="powers-form__quiet-btn"
          :disabled="!serverLatest || unresolvedConflicts.length > 0"
          @click="keepDraftAdoptServerVersion"
        >
          إبقاء المسودة واعتماد إصدار الخادم
        </button>
        <button
          type="button"
          class="powers-form__quiet-btn"
          @click="confirmDiscardAndReload"
        >
          تجاهل المسودة وإعادة التحميل
        </button>
      </div>
    </div>

    <div v-if="isLoading" class="powers-form__loading" aria-busy="true">
      جارٍ تحميل صلاحيات التبويبات…
    </div>

    <section
      v-for="group in groupedRows"
      :key="group.parentKey"
      class="powers-section"
    >
      <h3 class="powers-section__title">{{ group.label }}</h3>
      <ul class="powers-section__list">
        <li
          v-for="row in group.rows"
          :key="row.tabKey"
          class="powers-row"
        >
          <div class="powers-row__main">
            <div class="powers-row__copy">
              <div class="powers-row__name">{{ row.displayName }}</div>
              <p v-if="effectNote(row.tabKey)" class="powers-row__desc">
                {{ effectNote(row.tabKey) }}
              </p>
              <div class="powers-row__source">
                <template v-if="isCustomized(row.tabKey)">
                  <span class="powers-row__custom">مخصص لهذا الحساب</span>
                  <button
                    v-if="editable"
                    type="button"
                    class="powers-form__quiet-btn powers-form__quiet-btn--inline"
                    @click="onResetRole(row.tabKey)"
                  >
                    اعتماد إعداد الدور
                  </button>
                </template>
                <span v-else class="powers-row__role">حسب الدور</span>
              </div>
            </div>
            <PermissionSwitch
              :label="row.displayName"
              :enabled="draftEnabled(row.tabKey)"
              :disabled="!editable"
              @toggle="onToggle(row.tabKey)"
            />
          </div>
        </li>
      </ul>
    </section>

    <footer v-if="editable" class="powers-form__sticky">
      <button
        type="button"
        class="powers-form__btn powers-form__btn--ghost"
        :disabled="dirtyCount === 0 || isSaving"
        @click="discardLocal"
      >
        تراجع عن التغييرات
      </button>
      <button
        type="button"
        class="powers-form__btn powers-form__btn--primary"
        :disabled="!canSaveNow || isSaving"
        :aria-busy="isSaving"
        @click="emitSave"
      >
        {{ isSaving ? 'جارٍ الحفظ…' : 'حفظ التغييرات' }}
      </button>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import PermissionSwitch from './PermissionSwitch.vue'
import type { Props } from './PowersForm.types'
import { FormMode } from '../../UserTabs/UserTabs.types'
import { useUserTabAccessQuery } from '@/modules/User/requests/queries/useUserTabAccessQuery'
import useAuthStore from '@/modules/Auth/store'
import { usePermission } from '@/modules/Core/composable/usePermission'
import {
  TAB_EFFECT_NOTES_AR,
  TAB_PARENT_LABELS_AR,
  TAB_PARENT_ORDER,
} from '@/modules/Core/navigation/panelTabAccess'
import type { TabDraftState } from '@/modules/User/types/tabAccess'
import {
  adoptMergedTabBaseline,
  buildTabAccessChanges,
  collectTabConflicts,
  dirtyTabCount,
  expectedVersionForTabSave,
  mergeTabDraftWithServer,
  resetTabToRole,
  snapshotFromTabAccess,
  toggleTabDraft,
  type TabAccessBaseline,
  type TabAccessConflict,
  type TabAccessServerSnapshot,
  type TabConflictResolution,
} from './tabAccessFormState'

const props = defineProps<Props>()
const emit = defineEmits<{
  save: []
}>()

const auth = useAuthStore()
const { can } = usePermission()

const conflictMessage = ref<string | null>(null)
const draft = reactive<Record<string, TabDraftState>>({})
const baseline = reactive<TabAccessBaseline>({ version: 0, states: {} })
const serverLatest = ref<TabAccessServerSnapshot | null>(null)
const conflictResolutions = reactive<Record<string, TabConflictResolution>>({})
const tabs = ref<TabAccessServerSnapshot['tabs']>([])
const isProtected = ref(false)
const serverCanEdit = ref(false)
const displayName = ref('')
const roleDisplayName = ref('')
const initialized = ref(false)
const isSaving = ref(false)

const hasManagePermission = computed(
  () => auth.isSuperAdmin === true || can('User', 'ManagePermissions')
)

const editable = computed(
  () =>
    props.userFormMode !== FormMode.IsView &&
    hasManagePermission.value &&
    serverCanEdit.value &&
    isProtected.value === false
)

const { data, refetch, isFetching } = useUserTabAccessQuery(
  () => props.userId,
  () => Boolean(props.userId)
)

const isLoading = computed(() => isFetching.value && !initialized.value)

function clearConflictResolutions() {
  for (const key of Object.keys(conflictResolutions)) {
    delete conflictResolutions[key]
  }
}

function clearDraft() {
  for (const key of Object.keys(draft)) {
    delete draft[key]
  }
}

function applyAsBaseline(snapshot: TabAccessServerSnapshot) {
  displayName.value = snapshot.fullName
  roleDisplayName.value = snapshot.roleDisplayName
  isProtected.value = snapshot.isProtectedAccount
  serverCanEdit.value = snapshot.canEdit
  tabs.value = snapshot.tabs
  baseline.version = snapshot.version
  baseline.states = { ...snapshot.states }
  clearDraft()
  Object.assign(draft, snapshot.states)
  serverLatest.value = null
  clearConflictResolutions()
  conflictMessage.value = null
  initialized.value = true
}

function ingestServerPayload(
  payload: NonNullable<typeof data.value>,
  forceBaseline: boolean
) {
  const snapshot = snapshotFromTabAccess(payload)
  const dirty = dirtyTabCount(draft, baseline) > 0

  if (forceBaseline || initialized.value === false || dirty === false) {
    applyAsBaseline(snapshot)
    return
  }

  serverLatest.value = snapshot
  clearConflictResolutions()
  tabs.value = snapshot.tabs
  displayName.value = snapshot.fullName
  roleDisplayName.value = snapshot.roleDisplayName
  isProtected.value = snapshot.isProtectedAccount
  serverCanEdit.value = snapshot.canEdit
}

watch(
  data,
  (payload) => {
    if (!payload) return
    ingestServerPayload(payload, false)
  },
  { immediate: true }
)

watch(
  () => props.roleId,
  async (next, prev) => {
    if (prev == null || next === prev) return
    if (dirtyCount.value > 0) {
      const keep = window.confirm(
        'تغيّر الدور وهناك تعديلات محلية. هل تبقي المسودة وتقارن مع الخادم؟'
      )
      if (!keep) {
        await confirmDiscardAndReload()
      } else {
        await compareWithServer()
      }
    } else {
      const result = await refetch()
      if (result.data) ingestServerPayload(result.data, true)
    }
  }
)

function draftEnabled(tabKey: string): boolean {
  return draft[tabKey]?.enabled ?? false
}

function isCustomized(tabKey: string): boolean {
  return draft[tabKey]?.override != null
}

function effectNote(tabKey: string): string | null {
  return TAB_EFFECT_NOTES_AR[tabKey] ?? null
}

function onToggle(tabKey: string) {
  if (!editable.value) return
  const row = tabs.value.find((t) => t.tabKey === tabKey)
  if (!row) return
  const current = draft[tabKey] ?? {
    enabled: row.effectiveEnabled,
    override: row.override,
  }
  draft[tabKey] = toggleTabDraft(current, row.roleDefaultEnabled)
}

function onResetRole(tabKey: string) {
  if (!editable.value) return
  const row = tabs.value.find((t) => t.tabKey === tabKey)
  if (!row) return
  draft[tabKey] = resetTabToRole(row.roleDefaultEnabled)
}

const dirtyCount = computed(() => dirtyTabCount(draft, baseline))

const exceptionCount = computed(
  () => Object.values(draft).filter((s) => s.override != null).length
)

const conflicts = computed((): TabAccessConflict[] => {
  if (!serverLatest.value) return []
  return collectTabConflicts(draft, baseline, serverLatest.value)
})

const unresolvedConflicts = computed(() =>
  conflicts.value.filter(
    (c) =>
      conflictResolutions[c.tabKey] !== 'draft' &&
      conflictResolutions[c.tabKey] !== 'server'
  )
)

const hasPendingServerAdoption = computed(() => serverLatest.value != null)

const canSaveNow = computed(
  () =>
    dirtyCount.value > 0 &&
    serverLatest.value == null &&
    unresolvedConflicts.value.length === 0
)

function setConflictResolution(
  tabKey: string,
  resolution: TabConflictResolution
) {
  conflictResolutions[tabKey] = resolution
}

const groupedRows = computed(() => {
  const byParent = new Map<
    string,
    { parentKey: string; label: string; rows: typeof tabs.value }
  >()

  for (const row of tabs.value) {
    const parentKey = row.parentKey ?? '_root'
    if (!byParent.has(parentKey)) {
      byParent.set(parentKey, {
        parentKey,
        label: TAB_PARENT_LABELS_AR[parentKey] ?? parentKey,
        rows: [],
      })
    }
    byParent.get(parentKey)!.rows.push(row)
  }

  const ordered: Array<{
    parentKey: string
    label: string
    rows: typeof tabs.value
  }> = []

  for (const key of TAB_PARENT_ORDER) {
    const g = byParent.get(key)
    if (g) {
      ordered.push(g)
      byParent.delete(key)
    }
  }
  for (const g of byParent.values()) ordered.push(g)
  return ordered
})

function getChanges() {
  return buildTabAccessChanges(draft, baseline, tabs.value)
}

const permissionsVersion = computed(() => expectedVersionForTabSave(baseline))

function discardLocal() {
  clearDraft()
  Object.assign(draft, baseline.states)
}

async function compareWithServer() {
  const result = await refetch()
  if (result.data) {
    const dirty = dirtyCount.value > 0
    ingestServerPayload(result.data, dirty === false)
    if (dirty) {
      conflictMessage.value =
        'هناك نسخة أحدث على الخادم. احسم التبويبات المتعارضة قبل الحفظ.'
    }
  }
}

function keepDraftAdoptServerVersion() {
  if (!serverLatest.value) return
  const merged = mergeTabDraftWithServer(
    draft,
    baseline,
    serverLatest.value,
    conflictResolutions
  )
  if (merged.ok === false) {
    conflictMessage.value = 'يجب حسم كل التبويبات المتعارضة قبل المتابعة.'
    return
  }
  const adopted = adoptMergedTabBaseline(serverLatest.value, merged.states)
  baseline.version = adopted.baseline.version
  baseline.states = { ...adopted.baseline.states }
  clearDraft()
  Object.assign(draft, adopted.draft)
  tabs.value = serverLatest.value.tabs
  serverLatest.value = null
  clearConflictResolutions()
  conflictMessage.value = 'أُبقيَت مسودتك مع اعتماد إصدار الخادم للحفظ التالي.'
}

async function confirmDiscardAndReload() {
  if (dirtyCount.value > 0) {
    const ok = window.confirm('تجاهل التعديلات المحلية وإعادة التحميل من الخادم؟')
    if (!ok) return
  }
  const result = await refetch()
  if (result.data) ingestServerPayload(result.data, true)
}

function markConflict(message?: string) {
  conflictMessage.value =
    message || 'تعارض في الحفظ (409). قارن مع الخادم واحسم التبويبات.'
  void compareWithServer()
}

function emitSave() {
  if (!canSaveNow.value) return
  emit('save')
}

function setSaving(value: boolean) {
  isSaving.value = value
}

defineExpose({
  permissionIds: computed(() => [] as number[]),
  permissionsVersion,
  getChanges,
  dirtyCount,
  exceptionCount,
  reloadFromServer: confirmDiscardAndReload,
  compareWithServer,
  markConflict,
  hasConflictDraft: computed(() => dirtyCount.value > 0),
  hasPendingServerAdoption,
  unresolvedConflictCount: computed(() => unresolvedConflicts.value.length),
  canSaveOverrides: computed(
    () =>
      serverLatest.value == null && unresolvedConflicts.value.length === 0
  ),
  baselineVersion: computed(() => baseline.version),
  serverLatestVersion: computed(() => serverLatest.value?.version ?? null),
  setSaving,
})
</script>

<style scoped>
.powers-form {
  --powers-bg: #f8f8f8;
  --powers-primary: #006a64;
  --powers-text: #00201e;
  --powers-muted: #526b67;
  --powers-line: rgba(0, 32, 30, 0.08);
  background: var(--powers-bg);
  color: var(--powers-text);
  font-family: 'El Messiri', 'Segoe UI', Tahoma, sans-serif;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 0.25rem 0 1rem;
}

.powers-form--editable {
  padding-bottom: calc(5.5rem + env(safe-area-inset-bottom, 0px));
}

.powers-form__header {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  padding-inline: 0.25rem;
}

.powers-form__title {
  margin: 0;
  font-size: 1.375rem;
  font-weight: 700;
  color: var(--powers-primary);
  line-height: 1.3;
}

.powers-form__identity {
  margin: 0;
  font-size: 0.9375rem;
  color: var(--powers-text);
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  align-items: center;
}

.powers-form__dot {
  opacity: 0.45;
}

.powers-form__hint,
.powers-form__meta {
  margin: 0;
  font-size: 0.875rem;
  color: var(--powers-muted);
}

.powers-form__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
}

.powers-form__badge {
  border-radius: 8px;
  padding: 0.2rem 0.55rem;
  font-size: 0.75rem;
  font-weight: 600;
}

.powers-form__badge--warn {
  background: #fff4e5;
  color: #7a4a00;
}

.powers-form__retry-notice {
  margin: 0.25rem 0 0;
  padding: 0.65rem 0.85rem;
  border-radius: 12px;
  background: #fff4e5;
  color: #7a4a00;
  font-size: 0.875rem;
}

.powers-form__conflict {
  border-radius: 16px;
  padding: 0.85rem 1rem;
  background: #fff8ef;
  box-shadow: 6px 6px 14px rgba(0, 32, 30, 0.06),
    -4px -4px 10px rgba(255, 255, 255, 0.8);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  font-size: 0.875rem;
}

.powers-form__conflict-row {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  padding: 0.55rem 0;
  border-top: 1px solid var(--powers-line);
}

.powers-form__conflict-name {
  font-weight: 700;
}

.powers-form__conflict-vals {
  color: var(--powers-muted);
  font-size: 0.8125rem;
}

.powers-form__conflict-actions,
.powers-form__conflict-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.powers-form__warn-text {
  margin: 0;
  color: #9f1239;
  font-size: 0.8125rem;
}

.powers-form__quiet-btn {
  min-height: 40px;
  padding: 0.35rem 0.7rem;
  border-radius: 10px;
  border: 1px solid transparent;
  background: transparent;
  color: var(--powers-muted);
  font: inherit;
  font-size: 0.8125rem;
  text-decoration: underline;
  text-underline-offset: 3px;
  cursor: pointer;
}

.powers-form__quiet-btn:disabled {
  opacity: 0.45;
  cursor: default;
}

.powers-form__quiet-btn.active {
  color: var(--powers-primary);
  font-weight: 700;
  text-decoration: none;
  border-color: rgba(0, 106, 100, 0.25);
  background: rgba(0, 106, 100, 0.06);
}

.powers-form__quiet-btn--inline {
  min-height: 32px;
  padding-inline: 0.25rem;
}

.powers-form__quiet-btn:focus-visible {
  outline: 2px solid var(--powers-primary);
  outline-offset: 2px;
}

.powers-form__loading {
  padding: 1.5rem;
  text-align: center;
  color: var(--powers-muted);
}

.powers-section {
  border-radius: 18px;
  background: #f3f5f4;
  box-shadow: 8px 8px 18px rgba(0, 32, 30, 0.07),
    -6px -6px 14px rgba(255, 255, 255, 0.9);
  padding: 0.85rem 0.85rem 0.35rem;
}

.powers-section__title {
  margin: 0 0 0.35rem;
  padding-inline: 0.35rem;
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--powers-primary);
}

.powers-section__list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.powers-row {
  border-top: 1px solid var(--powers-line);
}

.powers-row__main {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
  min-height: 52px;
  padding: 0.7rem 0.35rem;
}

.powers-row__copy {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.powers-row__name {
  font-size: 1.0625rem;
  font-weight: 700;
  line-height: 1.35;
}

.powers-row__desc {
  margin: 0;
  font-size: 0.875rem;
  color: var(--powers-muted);
  line-height: 1.45;
}

.powers-row__source {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.8125rem;
  color: var(--powers-muted);
}

.powers-row__custom {
  color: var(--powers-primary);
  font-weight: 600;
}

.powers-form__sticky {
  position: sticky;
  bottom: 0;
  z-index: 12;
  display: flex;
  flex-wrap: wrap;
  gap: 0.65rem;
  justify-content: center;
  padding: 0.75rem 0.5rem calc(0.75rem + env(safe-area-inset-bottom, 0px));
  margin-inline: -0.25rem;
  background: rgba(248, 248, 248, 0.96);
  border-top: 1px solid var(--powers-line);
  backdrop-filter: blur(6px);
}

.powers-form__btn {
  min-height: 44px;
  min-width: 9rem;
  padding: 0.55rem 1.1rem;
  border-radius: 12px;
  border: 1px solid transparent;
  font: inherit;
  font-weight: 700;
  cursor: pointer;
}

.powers-form__btn:disabled {
  opacity: 0.45;
  cursor: default;
}

.powers-form__btn:focus-visible {
  outline: 2px solid var(--powers-primary);
  outline-offset: 2px;
}

.powers-form__btn--primary {
  background: var(--powers-primary);
  color: #fff;
  box-shadow: 4px 4px 10px rgba(0, 106, 100, 0.28);
}

.powers-form__btn--ghost {
  background: #f3f5f4;
  color: var(--powers-text);
  border-color: rgba(0, 32, 30, 0.12);
}

@media (max-width: 767px) {
  .powers-row__main {
    flex-direction: row;
    align-items: flex-start;
  }

  .powers-row__name {
    font-size: 1rem;
  }

  .powers-section__title {
    font-size: 1.125rem;
  }
}

@media (min-width: 1440px) {
  .powers-form__title {
    font-size: 1.5rem;
  }

  .powers-row__name {
    font-size: 1.125rem;
  }
}
</style>
