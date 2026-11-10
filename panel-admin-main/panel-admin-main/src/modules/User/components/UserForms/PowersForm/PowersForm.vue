<template>
  <div
    class="powers-form px-4 md:px-6 flex flex-col gap-4"
    :class="{ 'powers-form--editable': editable }"
    dir="rtl"
  >
    <header class="flex flex-col gap-2">
      <h2 class="text-xl font-bold text-[var(--powers-primary)] powers-title">
        {{ headerTitle }}
      </h2>
      <p class="text-sm text-gray-600">
        شغّل الصلاحية للسماح بها، أو أوقفها لمنعها حتى لو كانت موروثة من الدور. اضغط «حسب الدور» لإلغاء التخصيص. التغييرات تُطبّق عند الحفظ.
      </p>
      <div class="flex flex-wrap gap-3 text-sm items-center">
        <span class="text-gray-700">
          {{ $t('user.permissionOverrides.exceptionsCount') }}:
          <strong>{{ exceptionCount }}</strong>
        </span>
        <span class="text-gray-700">
          {{ $t('user.permissionOverrides.dirtyCount') }}:
          <strong>{{ dirtyCount }}</strong>
        </span>
        <span
          v-if="serverLatest"
          class="text-amber-800 text-xs font-medium"
        >
          {{ $t('user.permissionOverrides.syncStatusServerNewer') }}
        </span>
        <span
          v-else-if="initialized"
          class="text-gray-500 text-xs"
        >
          {{ $t('user.permissionOverrides.syncStatusCurrent') }}
        </span>
        <span
          v-if="isProtected"
          class="rounded-md bg-amber-100 text-amber-900 px-2 py-1 text-xs font-medium"
        >
          {{ $t('user.permissionOverrides.protectedBadge') }}
        </span>
      </div>
    </header>

    <div
      v-if="!editable && userFormMode !== FormMode.IsView"
      class="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900"
    >
      {{ $t('user.permissionOverrides.readOnlyNotice') }}
    </div>

    <div class="flex flex-col md:flex-row gap-3">
      <InputText
        v-model="search"
        class="w-full md:flex-1"
        :placeholder="$t('user.permissionOverrides.searchPlaceholder')"
      />
      <Select
        v-model="filter"
        :options="filterOptions"
        optionLabel="label"
        optionValue="value"
        class="w-full md:w-56"
      />
    </div>

    <div
      v-if="serverLatest || conflictMessage"
      class="rounded-lg border border-amber-300 bg-amber-50 p-3 text-sm flex flex-col gap-3"
    >
      <p>{{ conflictMessage || $t('user.permissionOverrides.serverNewer') }}</p>
      <p v-if="serverLatest" class="text-xs text-amber-900">
        {{ $t('user.permissionOverrides.saveUsesBaselineOnly') }}
      </p>
      <div v-if="conflicts.length" class="overflow-x-auto">
        <table class="w-full text-xs border-collapse">
          <thead>
            <tr class="text-right border-b">
              <th class="py-2">{{ $t('user.permissionOverrides.colOperation') }}</th>
              <th class="py-2">{{ $t('user.permissionOverrides.draftLabel') }}</th>
              <th class="py-2">{{ $t('user.permissionOverrides.serverLabel') }}</th>
              <th class="py-2">{{ $t('user.permissionOverrides.resolveLabel') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in conflicts" :key="c.permissionId" class="border-b">
              <td class="py-2">{{ c.displayName }}</td>
              <td class="py-2">{{ modeLabel(c.draft) }}</td>
              <td class="py-2">{{ modeLabel(c.server) }}</td>
              <td class="py-2">
                <SelectButton
                  :modelValue="conflictResolutions[c.permissionId]"
                  :options="resolutionOptions"
                  optionLabel="label"
                  optionValue="value"
                  :allowEmpty="true"
                  class="powers-mode-select"
                  @update:modelValue="
                    (v: ConflictResolution | null) => {
                      if (v === 'draft' || v === 'server') {
                        setConflictResolution(c.permissionId, v)
                      } else {
                        delete conflictResolutions[c.permissionId]
                      }
                    }
                  "
                />
              </td>
            </tr>
          </tbody>
        </table>
        <p
          v-if="unresolvedConflicts.length"
          class="text-xs text-rose-700 mt-2"
        >
          {{ $t('user.permissionOverrides.unresolvedBlocksSave') }}
        </p>
      </div>
      <div class="flex flex-wrap gap-2">
        <Button
          type="button"
          class="btn-secondary"
          :label="$t('user.permissionOverrides.compareOnly')"
          @click="compareWithServer"
        />
        <Button
          type="button"
          class="btn-secondary"
          :label="$t('user.permissionOverrides.keepDraftUseServerVersion')"
          :disabled="!serverLatest || unresolvedConflicts.length > 0"
          @click="keepDraftAdoptServerVersion"
        />
        <Button
          type="button"
          class="btn-primary"
          :label="$t('user.permissionOverrides.discardDraftReload')"
          @click="confirmDiscardAndReload"
        />
      </div>
    </div>

    <div class="hidden md:block overflow-x-auto">
      <table class="w-full text-sm border-collapse">
        <thead>
          <tr class="border-b border-line text-gray-600 text-right">
            <th class="py-3 px-2 font-medium">
              {{ $t('user.permissionOverrides.colOperation') }}
            </th>
            <th class="py-3 px-2 font-medium">
              {{ $t('user.permissionOverrides.colRole') }}
            </th>
            <th class="py-3 px-2 font-medium">
              {{ $t('user.permissionOverrides.colMode') }}
            </th>
            <th class="py-3 px-2 font-medium">
              {{ $t('user.permissionOverrides.colEffective') }}
            </th>
          </tr>
        </thead>
        <tbody>
          <template v-for="group in groupedRows" :key="group.scope">
            <tr>
              <td
                colspan="4"
                class="pt-4 pb-2 px-2 font-semibold text-[var(--powers-primary)]"
              >
                {{ group.scopeDisplayName }}
              </td>
            </tr>
            <tr
              v-for="row in group.rows"
              :key="row.permissionId"
              class="border-b border-line/60"
            >
              <td class="py-3 px-2 align-middle">
                <div>{{ row.displayName }}</div>
                <p
                  v-if="dependencyNote(row)"
                  class="text-xs text-[#526b67] mt-1 m-0 leading-5"
                >
                  {{ dependencyNote(row) }}
                </p>
              </td>
              <td class="py-3 px-2 align-middle">
                {{
                  row.roleAllows
                    ? $t('user.permissionOverrides.roleAllowed')
                    : $t('user.permissionOverrides.roleDenied')
                }}
              </td>
              <td class="py-3 px-2 align-middle">
                <PermissionSwitch :label="row.displayName" :enabled="effectiveFor(row)" :inherited="(draftModes[row.permissionId] ?? row.mode) === 'inherit'" :disabled="!editable" @toggle="setMode(row.permissionId, $event ? 'allow' : 'deny')" @inherit="setMode(row.permissionId, 'inherit')" />
              </td>
              <td class="py-3 px-2 align-middle">
                <span
                  :class="
                    effectiveFor(row) ? 'text-emerald-700' : 'text-rose-700'
                  "
                >
                  {{
                    effectiveFor(row)
                      ? $t('user.permissionOverrides.effectiveAllowed')
                      : $t('user.permissionOverrides.effectiveDenied')
                  }}
                </span>
              </td>
            </tr>
          </template>
        </tbody>
      </table>
    </div>

    <div class="md:hidden flex flex-col gap-4">
      <section
        v-for="group in groupedRows"
        :key="group.scope"
        class="border border-line rounded-xl p-3 flex flex-col gap-3"
      >
        <h3 class="font-semibold text-[var(--powers-primary)]">
          {{ group.scopeDisplayName }}
        </h3>
        <article
          v-for="row in group.rows"
          :key="row.permissionId"
          class="border-t border-line/50 pt-3 flex flex-col gap-2 min-h-11"
        >
          <div class="font-medium">{{ row.displayName }}</div>
          <p
            v-if="dependencyNote(row)"
            class="text-xs text-[#526b67] m-0 leading-5"
          >
            {{ dependencyNote(row) }}
          </p>
          <PermissionSwitch :label="row.displayName" :enabled="effectiveFor(row)" :inherited="(draftModes[row.permissionId] ?? row.mode) === 'inherit'" :disabled="!editable" @toggle="setMode(row.permissionId, $event ? 'allow' : 'deny')" @inherit="setMode(row.permissionId, 'inherit')" />
        </article>
      </section>
    </div>

    <footer
      v-if="editable"
      class="powers-form__sticky-actions sticky bottom-0 z-10 bg-surface-0/95 border-t border-line py-3 flex flex-wrap gap-2 justify-center"
    >
      <Button
        type="button"
        class="btn-secondary"
        :label="$t('user.permissionOverrides.discardLocal')"
        :disabled="dirtyCount === 0"
        @click="discardLocal"
      />
      <Button
        type="button"
        class="btn-secondary"
        :label="$t('user.permissionOverrides.restoreRoleDefaults')"
        @click="confirmRestoreDefaults"
      />
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import SelectButton from 'primevue/selectbutton'
import Button from 'primevue/button'
import PermissionSwitch from './PermissionSwitch.vue'
import type { Props } from './PowersForm.types'
import { FormMode } from '../../UserTabs/UserTabs.types'
import { useUserPermissionOverridesQuery } from '@/modules/User/requests/queries/useUserPermissionOverridesQuery'
import type {
  PermissionOverrideMode,
  PermissionOverrideRow,
} from '@/modules/User/types/permissionOverrides'
import { usePermission } from '@/modules/Core/composable/usePermission'
import useAuthStore from '@/modules/Auth/store'
import {
  adoptMergedBaseline,
  collectConflicts,
  type ConflictResolution,
  expectedVersionForSave,
  mergeDraftWithServer,
  type PowersBaseline,
  type PowersServerSnapshot,
  snapshotFromResponse,
  unresolvedConflictIds,
} from './powersFormState'
import { dependenciesFor } from '@/modules/Core/navigation/permissionDependencies'

const props = defineProps<Props>()
const { t } = useI18n()
const auth = useAuthStore()
const { can } = usePermission()

const search = ref('')
const filter = ref<'all' | 'exceptions' | 'dirty'>('all')
const conflictMessage = ref<string | null>(null)
const draftModes = reactive<Record<number, PermissionOverrideMode>>({})
const baseline = reactive<PowersBaseline>({ version: 0, modes: {} })
const serverLatest = ref<PowersServerSnapshot | null>(null)
const conflictResolutions = reactive<Record<number, ConflictResolution>>({})
const rows = ref<PermissionOverrideRow[]>([])
const isProtected = ref(false)
const serverCanEdit = ref(false)
const displayName = ref('')
const roleDisplayName = ref('')
const initialized = ref(false)

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

const { data, refetch } = useUserPermissionOverridesQuery(
  () => props.userId,
  () => Boolean(props.userId)
)

function clearConflictResolutions() {
  for (const key of Object.keys(conflictResolutions)) {
    delete conflictResolutions[Number(key)]
  }
}

function applyAsBaseline(snapshot: PowersServerSnapshot) {
  displayName.value = snapshot.fullName
  roleDisplayName.value = snapshot.roleDisplayName
  isProtected.value = snapshot.isProtectedAccount
  serverCanEdit.value = snapshot.canEditOverrides
  rows.value = snapshot.rows
  baseline.version = snapshot.version
  baseline.modes = { ...snapshot.modes }
  for (const key of Object.keys(draftModes)) {
    delete draftModes[Number(key)]
  }
  Object.assign(draftModes, snapshot.modes)
  serverLatest.value = null
  clearConflictResolutions()
  conflictMessage.value = null
  initialized.value = true
}

function ingestServerPayload(
  payload: NonNullable<typeof data.value>,
  forceBaseline: boolean
) {
  const snapshot = snapshotFromResponse(payload)
  const dirty = Object.keys(draftModes).some(
    (id) => draftModes[Number(id)] !== baseline.modes[Number(id)]
  )

  if (forceBaseline || initialized.value === false || dirty === false) {
    applyAsBaseline(snapshot)
    return
  }

  // تعديلات محلية: لا تستبدل baseline ولا إصدار الحفظ — احتفظ بنسخة الخادم منفصلة
  serverLatest.value = snapshot
  clearConflictResolutions()
  rows.value = snapshot.rows
  displayName.value = snapshot.fullName
  roleDisplayName.value = snapshot.roleDisplayName
  isProtected.value = snapshot.isProtectedAccount
  serverCanEdit.value = snapshot.canEditOverrides
  logVersionDiagnostics('server-newer-retained-draft')
}

function logVersionDiagnostics(reason: string) {
  // تفاصيل الإصدار للتشخيص فقط — لا تُعرض في واجهة الموظف
  console.debug('[PowersForm]', reason, {
    baselineVersion: baseline.version,
    serverLatestVersion: serverLatest.value?.version ?? null,
  })
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
        t('user.permissionOverrides.roleChangedKeepLocal')
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

const headerTitle = computed(() => {
  if (displayName.value) {
    return `${displayName.value} — ${roleDisplayName.value}`
  }
  return t('user.userCreateSteps.powers.stepTitle')
})

const modeOptions = computed(() => [
  { label: t('user.permissionOverrides.modeInherit'), value: 'inherit' },
  { label: t('user.permissionOverrides.modeAllow'), value: 'allow' },
  { label: t('user.permissionOverrides.modeDeny'), value: 'deny' },
])

const resolutionOptions = computed(() => [
  { label: t('user.permissionOverrides.chooseDraft'), value: 'draft' as const },
  { label: t('user.permissionOverrides.chooseServer'), value: 'server' as const },
])

const filterOptions = computed(() => [
  { label: t('user.permissionOverrides.filterAll'), value: 'all' },
  {
    label: t('user.permissionOverrides.filterExceptions'),
    value: 'exceptions',
  },
  { label: t('user.permissionOverrides.filterDirty'), value: 'dirty' },
])

function modeLabel(mode: PermissionOverrideMode) {
  if (mode === 'allow') return t('user.permissionOverrides.modeAllow')
  if (mode === 'deny') return t('user.permissionOverrides.modeDeny')
  return t('user.permissionOverrides.modeInherit')
}

function setMode(permissionId: number, mode: PermissionOverrideMode) {
  if (!editable.value) return
  draftModes[permissionId] = mode
}

function effectiveFor(row: PermissionOverrideRow): boolean {
  const mode = draftModes[row.permissionId] ?? row.mode
  if (mode === 'deny') return false
  if (mode === 'allow') return true
  return row.roleAllows
}

const dirtyCount = computed(
  () =>
    rows.value.filter(
      (r) => draftModes[r.permissionId] !== baseline.modes[r.permissionId]
    ).length
)

const exceptionCount = computed(
  () =>
    rows.value.filter((r) => (draftModes[r.permissionId] ?? r.mode) !== 'inherit')
      .length
)

const conflicts = computed(() => {
  if (!serverLatest.value) return []
  return collectConflicts(draftModes, baseline, serverLatest.value)
})

const unresolvedConflicts = computed(() =>
  unresolvedConflictIds(conflicts.value, conflictResolutions)
)

const hasPendingServerAdoption = computed(() => serverLatest.value != null)

function setConflictResolution(
  permissionId: number,
  resolution: ConflictResolution
) {
  conflictResolutions[permissionId] = resolution
}

const filteredRows = computed(() => {
  const q = search.value.trim()
  return rows.value.filter((row) => {
    const mode = draftModes[row.permissionId] ?? row.mode
    if (filter.value === 'exceptions' && mode === 'inherit') return false
    if (
      filter.value === 'dirty' &&
      draftModes[row.permissionId] === baseline.modes[row.permissionId]
    ) {
      return false
    }
    if (!q) return true
    return (
      row.displayName.includes(q) ||
      row.scopeDisplayName.includes(q) ||
      row.name.includes(q)
    )
  })
})

const groupedRows = computed(() => {
  const map = new Map<
    string,
    { scope: string; scopeDisplayName: string; rows: PermissionOverrideRow[] }
  >()
  for (const row of filteredRows.value) {
    if (!map.has(row.scope)) {
      map.set(row.scope, {
        scope: row.scope,
        scopeDisplayName: row.scopeDisplayName,
        rows: [],
      })
    }
    map.get(row.scope)!.rows.push(row)
  }
  return Array.from(map.values())
})

/** توضيح تبعية فقط — لا يوسّع الصلاحيات خفيًا */
function dependencyNote(row: PermissionOverrideRow): string | null {
  const [module, action] = row.name.split('.')
  if (!module || !action) return null
  return dependenciesFor(module, action)?.noteAr ?? null
}

function getChanges() {
  return rows.value
    .filter(
      (r) => draftModes[r.permissionId] !== baseline.modes[r.permissionId]
    )
    .map((r) => ({
      permissionId: r.permissionId,
      mode: draftModes[r.permissionId],
    }))
}

const permissionsVersion = computed(() => expectedVersionForSave(baseline))

const permissionIds = computed(() =>
  rows.value.filter((r) => effectiveFor(r)).map((r) => r.permissionId)
)

function discardLocal() {
  for (const row of rows.value) {
    draftModes[row.permissionId] = baseline.modes[row.permissionId] ?? 'inherit'
  }
}

function confirmRestoreDefaults() {
  const count = exceptionCount.value
  if (count === 0) return
  if (!window.confirm(t('user.permissionOverrides.restoreConfirm', { count })))
    return
  for (const row of rows.value) {
    draftModes[row.permissionId] = 'inherit'
  }
}

async function compareWithServer() {
  const result = await refetch()
  if (result.data) {
    // لا يمسح المسودة ولا يعتمد الإصدار — يحدّث serverLatest فقط إن وُجدت تعديلات
    const dirty = dirtyCount.value > 0
    ingestServerPayload(result.data, dirty === false)
    if (dirty) {
      conflictMessage.value = t('user.permissionOverrides.conflictMessage')
    }
  }
}

function keepDraftAdoptServerVersion() {
  if (!serverLatest.value) return

  const merged = mergeDraftWithServer(
    draftModes,
    baseline,
    serverLatest.value,
    conflictResolutions
  )
  if (merged.ok === false) {
    conflictMessage.value = t('user.permissionOverrides.unresolvedBlocksSave')
    return
  }

  const adopted = adoptMergedBaseline(serverLatest.value, merged.modes)
  baseline.version = adopted.baseline.version
  baseline.modes = { ...adopted.baseline.modes }
  for (const key of Object.keys(draftModes)) {
    delete draftModes[Number(key)]
  }
  Object.assign(draftModes, adopted.draft)
  rows.value = serverLatest.value.rows
  serverLatest.value = null
  clearConflictResolutions()
  conflictMessage.value = t('user.permissionOverrides.draftKeptNewVersion')
}

async function confirmDiscardAndReload() {
  if (dirtyCount.value > 0) {
    const ok = window.confirm(
      t('user.permissionOverrides.discardDraftConfirm')
    )
    if (!ok) return
  }
  const result = await refetch()
  if (result.data) ingestServerPayload(result.data, true)
}

function markConflict(message?: string) {
  conflictMessage.value =
    message || t('user.permissionOverrides.conflictMessage')
  void compareWithServer()
}

defineExpose({
  permissionIds,
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
})
</script>

<style scoped>
.powers-form {
  --powers-primary: #006a64;
}
.powers-form--editable {
  /* مساحة تمرير كافية حتى لا يحجب الشريط الثابت آخر خيارات الهاتف */
  padding-bottom: 7.5rem;
}
.powers-title {
  font-family: 'El Messiri', 'Segoe UI', Tahoma, sans-serif;
}
.powers-mode-select :deep(.p-togglebutton) {
  min-height: 44px;
}
.powers-form__sticky-actions {
  margin-inline: -1rem;
  padding-inline: 1rem;
}
@media (min-width: 768px) {
  .powers-form--editable {
    padding-bottom: 5.5rem;
  }
  .powers-form__sticky-actions {
    margin-inline: -1.5rem;
    padding-inline: 1.5rem;
  }
}
</style>
