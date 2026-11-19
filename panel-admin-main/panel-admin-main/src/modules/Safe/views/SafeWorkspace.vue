<template>
  <AppPage class="q-page--wide" :title="$t('safe.title')" :subtitle="$t('safe.subtitle')">
    <template #actions>
      <AppButton
        v-if="canCreate && branchId > 0"
        variant="secondary"
        icon="pi pi-history"
        @click="backfillOpen = true"
      >
        ترحيل التسديدات السابقة
      </AppButton>
      <AppButton
        v-if="canCreate"
        icon="pi pi-plus"
        @click="createOpen = true"
      >
        {{ $t('safe.operations.create') }}
      </AppButton>
    </template>

    <DataState
      v-if="branchId <= 0"
      kind="empty"
      :title="$t('safe.states.noBranchTitle')"
      :message="$t('safe.states.noBranchMessage')"
    />

    <DataState
      v-else-if="isLoading"
      kind="loading"
      :title="$t('safe.states.loading')"
    />

    <DataState
      v-else-if="isError"
      kind="error"
      :title="$t('safe.states.errorTitle')"
      :message="$t('safe.states.errorMessage')"
    />

    <template v-else>
      <!-- الحاويتان النظاميتان: قاصة الفرع وسجل الصرفيات -->
      <section class="safe-containers">
        <article
          v-for="container in systemSafes"
          :key="container.id"
          class="safe-container"
          :class="`safe-container--${container.kind === SafeKind.Branch ? 'branch' : 'expenses'}`"
        >
          <header class="safe-container__head">
            <span class="safe-container__eyebrow">
              {{ container.kind === SafeKind.Branch ? $t('safe.kinds.branch') : $t('safe.kinds.expenses') }}
            </span>
            <h2 class="safe-container__name">{{ container.name }}</h2>
          </header>

          <p class="safe-container__balance q-num">
            {{ formattedPrice(container.balance) }}
            <small>{{ $t('words.dinar') }}</small>
          </p>

          <footer class="safe-container__foot">
            <AppButton
              size="sm"
              variant="secondary"
              :disabled="mutating"
              @click="openAmount(container, 'deposit')"
            >
              {{ $t('safe.operations.deposit') }}
            </AppButton>
            <AppButton
              size="sm"
              variant="danger"
              :disabled="mutating"
              @click="openAmount(container, 'withdraw')"
            >
              {{ $t('safe.operations.withdraw') }}
            </AppButton>
            <AppButton
              size="sm"
              variant="secondary"
              :disabled="mutating"
              @click="openTransfer(container.id)"
            >
              {{ $t('safe.operations.transfer') }}
            </AppButton>
            <AppButton
              size="sm"
              variant="ghost"
              :to="{ name: 'SafeMovementsRoute', params: { id: container.id } }"
            >
              {{ $t('safe.actions.movements') }}
            </AppButton>
          </footer>
        </article>
      </section>

      <!-- إجمالي الفرع: مجموع كل الحاويات -->
      <p class="safe-total" aria-live="polite">
        {{ $t('safe.totalLabel') }}:
        <strong class="q-num">{{ formattedPrice(grandTotal) }}</strong>
        <small>{{ $t('words.dinar') }}</small>
      </p>

      <AppPanel :title="$t('safe.subSafesTitle')">
        <StatusTabs
          v-model="activeTab"
          :tabs="tabs"
          :aria-label="$t('safe.subSafesTitle')"
        />

        <FilterBar>
          <AppSearchField
            v-model="search"
            :label="$t('safe.fields.search')"
            :placeholder="$t('safe.fields.searchPlaceholder')"
          />
        </FilterBar>

        <DataState
          v-if="visibleSafes.length === 0"
          kind="empty"
          :title="$t('safe.states.emptyTitle')"
          :message="$t('safe.states.emptyMessage')"
        />

        <div v-else class="safe-grid">
          <EntityCard
            v-for="safe in visibleSafes"
            :key="safe.id"
            :name="safe.name"
            :eyebrow="kindLabel(safe.kind)"
            :status-label="safe.isActive ? undefined : $t('safe.inactive')"
            status-tone="warning"
          >
            <template #fields>
              <DataField
                :label="$t('safe.fields.balance')"
                :value="`${formattedPrice(safe.balance)} ${$t('words.dinar')}`"
              />
              <DataField
                :label="$t('safe.fields.binding')"
                :value="safe.orderListName ?? safe.userFullName"
              />
              <DataField
                :label="$t('safe.fields.movementsCount')"
                :value="safe.movementsCount"
              />
            </template>

            <template #actions>
              <div class="safe-card-actions">
                <AppButton
                  size="sm"
                  variant="secondary"
                  :disabled="mutating"
                  @click="openAmount(safe, 'deposit')"
                >
                  {{ $t('safe.operations.deposit') }}
                </AppButton>
                <AppButton
                  size="sm"
                  variant="danger"
                  :disabled="mutating"
                  @click="openAmount(safe, 'withdraw')"
                >
                  {{ $t('safe.operations.withdraw') }}
                </AppButton>
                <AppButton
                  size="sm"
                  variant="secondary"
                  :disabled="mutating"
                  @click="openTransfer(safe.id)"
                >
                  {{ $t('safe.operations.transfer') }}
                </AppButton>
                <AppButton
                  size="sm"
                  variant="ghost"
                  :to="{ name: 'SafeMovementsRoute', params: { id: safe.id } }"
                >
                  {{ $t('safe.actions.movements') }}
                </AppButton>
                <AppButton
                  v-if="canDelete && !safe.isSystem"
                  size="sm"
                  variant="ghost"
                  icon="pi pi-trash"
                  icon-only
                  :disabled="mutating"
                  @click="askDelete(safe)"
                >
                  {{ $t('safe.actions.delete') }}
                </AppButton>
              </div>
            </template>
          </EntityCard>
        </div>
      </AppPanel>
    </template>

    <SafeAmountDialog
      :visible="amountTarget !== null"
      :mode="amountMode"
      :safe-name="amountTarget?.name ?? ''"
      :safe-balance="amountTarget?.balance ?? 0"
      :busy="mutating"
      @close="amountTarget = null"
      @submit="submitAmount"
    />

    <SafeTransferDialog
      :visible="transferOpen"
      :safes="safes"
      :source-id="transferSourceId"
      :busy="mutating"
      @close="transferOpen = false"
      @submit="submitTransfer"
    />

    <SafeCreateDialog
      :visible="createOpen"
      :branch-id="branchId"
      :busy="mutating"
      @close="createOpen = false"
      @submit="submitCreate"
    />

    <SafeBackfillDialog
      v-if="branchId > 0"
      :visible="backfillOpen"
      :branch-id="branchId"
      @close="backfillOpen = false"
    />

    <ConfirmReasonDialog
      v-model="deleteOpen"
      :title="$t('safe.operations.delete')"
      :message="deleteMessage"
      :require-reason="false"
      :busy="mutating"
      @confirm="confirmDelete"
    />
  </AppPage>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  AppButton,
  AppPage,
  AppPanel,
  AppSearchField,
  ConfirmReasonDialog,
  DataField,
  DataState,
  EntityCard,
  FilterBar,
  StatusTabs,
} from '@/modules/Core/components/ui'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { formattedPrice } from '@/modules/Core/utils/price'
import { useSafesQuery } from '../requests/queries/useSafesQuery'
import {
  useCreateSafeMutation,
  useDeleteSafeMutation,
  useSafeDepositMutation,
  useSafeTransferMutation,
  useSafeWithdrawMutation,
} from '../requests/mutations'
import { SafeKind, type CreateSafeForm, type Safe, type SafeAmountForm, type SafeTransferForm } from '../types/model'
import SafeAmountDialog from '../components/SafeAmountDialog.vue'
import SafeBackfillDialog from '../components/SafeBackfillDialog.vue'
import SafeCreateDialog from '../components/SafeCreateDialog.vue'
import SafeTransferDialog from '../components/SafeTransferDialog.vue'

const { t } = useI18n()
const authStore = useAuthStore()
const toast = useToastStore()
const { can } = usePermission()

const canCreate = can('Safe', 'Create')
const canDelete = can('Safe', 'Delete')

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const { data, isLoading, isError } = useSafesQuery(branchId)

const safes = computed<Safe[]>(() => data.value ?? [])

const systemSafes = computed(() =>
  safes.value
    .filter((safe) => safe.isSystem)
    .sort((a, b) => a.kind - b.kind),
)

const grandTotal = computed(() => safes.value.reduce((sum, safe) => sum + safe.balance, 0))

const activeTab = ref<'orderList' | 'employee'>('orderList')
const search = ref('')

const orderListSafes = computed(() => safes.value.filter((safe) => safe.kind === SafeKind.OrderList))
const employeeSafes = computed(() => safes.value.filter((safe) => safe.kind === SafeKind.Employee))
const miscSafes = computed(() => safes.value.filter((safe) => safe.kind === SafeKind.Misc))

const tabs = computed(() => [
  { id: 'orderList', label: t('safe.kinds.orderList'), count: orderListSafes.value.length },
  { id: 'employee', label: t('safe.kinds.employee'), count: employeeSafes.value.length },
  { id: 'misc', label: t('safe.kinds.misc'), count: miscSafes.value.length },
])

const visibleSafes = computed(() => {
  const pool =
    activeTab.value === 'orderList'
      ? orderListSafes.value
      : activeTab.value === 'employee'
        ? employeeSafes.value
        : miscSafes.value
  const term = search.value.trim()
  if (term.length === 0) return pool

  return pool.filter(
    (safe) =>
      safe.name.includes(term) ||
      (safe.orderListName ?? '').includes(term) ||
      (safe.userFullName ?? '').includes(term),
  )
})

const depositMutation = useSafeDepositMutation()
const withdrawMutation = useSafeWithdrawMutation()
const transferMutation = useSafeTransferMutation()
const createMutation = useCreateSafeMutation()
const deleteMutation = useDeleteSafeMutation()

const mutating = computed(
  () =>
    depositMutation.isPending.value ||
    withdrawMutation.isPending.value ||
    transferMutation.isPending.value ||
    createMutation.isPending.value ||
    deleteMutation.isPending.value,
)

const amountTarget = ref<Safe | null>(null)
const amountMode = ref<'deposit' | 'withdraw'>('deposit')
const transferOpen = ref(false)
const transferSourceId = ref<number | null>(null)
const createOpen = ref(false)
const backfillOpen = ref(false)
const deleteOpen = ref(false)
const deleteTarget = ref<Safe | null>(null)

const deleteMessage = computed(() =>
  deleteTarget.value ? t('safe.operations.deleteConfirm', { name: deleteTarget.value.name }) : '',
)

function kindLabel(kind: SafeKind) {
  if (kind === SafeKind.Branch) return t('safe.kinds.branch')
  if (kind === SafeKind.Expenses) return t('safe.kinds.expenses')
  if (kind === SafeKind.OrderList) return t('safe.kinds.orderList')
  if (kind === SafeKind.Misc) return t('safe.kinds.misc')
  return t('safe.kinds.employee')
}

function openAmount(safe: Safe, mode: 'deposit' | 'withdraw') {
  amountMode.value = mode
  amountTarget.value = safe
}

function openTransfer(sourceId: number) {
  transferSourceId.value = sourceId
  transferOpen.value = true
}

function askDelete(safe: Safe) {
  deleteTarget.value = safe
  deleteOpen.value = true
}

async function submitAmount(form: SafeAmountForm) {
  const safe = amountTarget.value
  if (safe === null) return

  const mutation = amountMode.value === 'withdraw' ? withdrawMutation : depositMutation

  try {
    await mutation.mutateAsync({
      safeId: safe.id,
      payload: { amount: form.amount, note: form.note, operationKey: form.operationKey },
    })
    amountTarget.value = null
  } catch {
    // الخطأ يظهر عبر معترض axios العام
  }
}

async function submitTransfer(form: SafeTransferForm) {
  try {
    await transferMutation.mutateAsync(form)
    transferOpen.value = false
  } catch {
    // الخطأ يظهر عبر معترض axios العام
  }
}

async function submitCreate(form: CreateSafeForm) {
  try {
    await createMutation.mutateAsync(form)
    createOpen.value = false
  } catch {
    // الخطأ يظهر عبر معترض axios العام
  }
}

async function confirmDelete() {
  const safe = deleteTarget.value
  if (safe === null) return

  try {
    await deleteMutation.mutateAsync(safe.id)
    deleteOpen.value = false
    deleteTarget.value = null
  } catch {
    toast.setMassage({
      title: t('safe.operations.deleteFailed'),
      dialogState: DialogState.Error,
      isOpen: true,
    })
    deleteOpen.value = false
  }
}
</script>

<style scoped>
.safe-containers {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: var(--q-space-3, 12px);
  margin-bottom: var(--q-space-3, 12px);
}

.safe-container {
  border-radius: var(--q-radius-lg, 18px);
  padding: 18px 20px;
  background: var(--q-neo-bg, #eef3f2);
  box-shadow: var(--q-neo-raised);
  display: grid;
  gap: 10px;
}

.safe-container--expenses .safe-container__balance {
  color: var(--q-danger);
}

.safe-container__eyebrow {
  font-size: 13px;
  color: var(--q-muted);
}

.safe-container__name {
  font-size: var(--q-value, 16px);
  font-weight: 700;
  color: var(--q-fg);
  margin: 0;
}

.safe-container__balance {
  font-size: var(--q-title, 26px);
  font-weight: 700;
  color: var(--q-primary);
  margin: 0;
}

.safe-container__balance small {
  font-size: 14px;
  font-weight: 500;
  color: var(--q-muted);
  padding-inline-start: 6px;
}

.safe-container__foot,
.safe-card-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.safe-total {
  text-align: start;
  color: var(--q-muted);
  margin-bottom: var(--q-space-3, 12px);
}

.safe-total strong {
  color: var(--q-fg);
  padding-inline: 6px;
}

.safe-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--q-space-3, 12px);
  margin-top: var(--q-space-3, 12px);
}
</style>
