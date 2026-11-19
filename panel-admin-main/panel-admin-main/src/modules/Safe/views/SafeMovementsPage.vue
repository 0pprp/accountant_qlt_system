<template>
  <AppPage class="q-page--wide" :title="$t('safe.movements.title')" :subtitle="safeName">
    <template #actions>
      <AppButton
        variant="ghost"
        icon="pi pi-arrow-left"
        :to="{ name: 'SafeMainRoute' }"
      >
        {{ $t('safe.actions.back') }}
      </AppButton>
    </template>

    <!-- شريط التحقق الحسابي: الرصيد مقابل مجموع السجل -->
    <div
      v-if="verification"
      class="safe-verify"
      :class="verification.isBalanced ? 'safe-verify--ok' : 'safe-verify--off'"
      role="status"
    >
      <span>
        {{ verification.isBalanced ? $t('safe.movements.balanced') : $t('safe.movements.unbalanced') }}
      </span>
      <span class="q-num">
        {{ $t('safe.movements.storedBalance') }}: {{ formattedPrice(verification.storedBalance) }}
      </span>
      <span class="q-num">
        {{ $t('safe.movements.ledgerBalance') }}: {{ formattedPrice(verification.ledgerBalance) }}
      </span>
      <span v-if="!verification.isBalanced" class="q-num">
        {{ $t('safe.movements.difference') }}: {{ formattedPrice(verification.difference) }}
      </span>
    </div>

    <AppPanel>
      <FilterBar>
        <AppSelect
          v-model="kind"
          :label="$t('safe.fields.movementKind')"
          :placeholder="$t('safe.fields.allKinds')"
          :options="kindOptions"
        />
        <AppDateField
          v-model="startDate"
          :label="$t('safe.fields.startDate')"
        />
        <AppDateField
          v-model="endDate"
          :label="$t('safe.fields.endDate')"
        />
      </FilterBar>

      <CustomDataTable
        :data="rows"
        :columns="columns"
        :rows="pageSize"
        :page-size="pageSize"
        paginator-enabled
        :total-records="totalCount"
        :paginator-label="$t('safe.movements.paginatorLabel')"
        table-style="min-width: 100%"
        class="custom-table custom-border mt-4"
        :loading="isLoading"
        @page-change="onPageChange"
      >
        <template #body-kind="{ data }">
          {{ kindLabel(data.kind) }}
          <span v-if="data.customerFullName" class="safe-movement__customer">— {{ data.customerFullName }}</span>
        </template>

        <template #body-amount="{ data }">
          <span
            class="q-num"
            :class="data.amount < 0 ? 'safe-amount--out' : 'safe-amount--in'"
          >
            {{ data.amount < 0 ? '−' : '+' }}{{ formattedPrice(Math.abs(data.amount)) }}
          </span>
          <small class="text-gray-700 px-1">{{ $t('words.dinar') }}</small>
        </template>

        <template #body-balanceAfter="{ data }">
          <span class="q-num">{{ formattedPrice(data.balanceAfter) }}</span>
        </template>

        <template #body-counterpartySafeName="{ data }">
          {{ data.counterpartySafeName || '—' }}
        </template>

        <template #body-note="{ data }">
          {{ data.note || '—' }}
        </template>

        <template #body-performedByFullName="{ data }">
          {{ data.performedByFullName || '—' }}
        </template>

        <template #body-createdAt="{ data }">
          {{ data.createdAt ? formattedDate(data.createdAt) : '—' }}
        </template>
      </CustomDataTable>
    </AppPanel>
  </AppPage>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import {
  AppButton,
  AppDateField,
  AppPage,
  AppPanel,
  AppSelect,
  FilterBar,
} from '@/modules/Core/components/ui'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type { Column } from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import { formattedPrice } from '@/modules/Core/utils/price'
import { formattedDate } from '@/modules/Core/utils/time'
import { useAuthStore } from '@/modules/Auth/store'
import { useSafeMovementsQuery } from '../requests/queries/useSafeMovementsQuery'
import { useVerifySafeQuery } from '../requests/queries/useVerifySafeQuery'
import { useSafesQuery } from '../requests/queries/useSafesQuery'
import { SafeMovementKind, type SafeMovement } from '../types/model'

const { t } = useI18n()
const route = useRoute()
const authStore = useAuthStore()

const safeId = computed(() => Number(route.params.id ?? 0))
const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const pageSize = ref(25)
const pageIndex = ref(1)
const kind = ref<SafeMovementKind | null>(null)
const startDate = ref('')
const endDate = ref('')

const params = computed(() => ({
  pageIndex: pageIndex.value,
  pageSize: pageSize.value,
  kind: kind.value,
  startDate: startDate.value || null,
  endDate: endDate.value || null,
}))

// أي تغيير في الفلاتر يرجع للصفحة الأولى
watch([kind, startDate, endDate], () => {
  pageIndex.value = 1
})

const { data, isLoading } = useSafeMovementsQuery(safeId, params)
const { data: verifyData } = useVerifySafeQuery(safeId)
const { data: safesData } = useSafesQuery(branchId)

const rows = computed<SafeMovement[]>(() => data.value?.items ?? [])
const totalCount = computed(() => data.value?.totalCount ?? 0)
const verification = computed(() => verifyData.value ?? null)

const safeName = computed(
  () => safesData.value?.find((safe) => safe.id === safeId.value)?.name ?? '',
)

const kindOptions = computed(() => [
  { label: t('safe.fields.allKinds'), value: null },
  ...Object.values(SafeMovementKind)
    .filter((value): value is SafeMovementKind => typeof value === 'number')
    .map((value) => ({ label: kindLabel(value), value })),
])

const columns = computed<Column<SafeMovement>[]>(() => [
  { field: 'kind', header: t('safe.movements.columns.kind') },
  { field: 'amount', header: t('safe.movements.columns.amount') },
  { field: 'balanceAfter', header: t('safe.movements.columns.balanceAfter') },
  { field: 'counterpartySafeName', header: t('safe.movements.columns.counterparty') },
  { field: 'note', header: t('safe.movements.columns.note') },
  { field: 'performedByFullName', header: t('safe.movements.columns.performedBy') },
  { field: 'createdAt', header: t('safe.movements.columns.createdAt') },
])

function kindLabel(value: SafeMovementKind) {
  const map: Record<SafeMovementKind, string> = {
    [SafeMovementKind.Opening]: t('safe.movementKinds.opening'),
    [SafeMovementKind.Deposit]: t('safe.movementKinds.deposit'),
    [SafeMovementKind.Withdrawal]: t('safe.movementKinds.withdrawal'),
    [SafeMovementKind.TransferIn]: t('safe.movementKinds.transferIn'),
    [SafeMovementKind.TransferOut]: t('safe.movementKinds.transferOut'),
    [SafeMovementKind.Collection]: t('safe.movementKinds.collection'),
    [SafeMovementKind.Prepayment]: t('safe.movementKinds.prepayment'),
    [SafeMovementKind.ExpenseOut]: t('safe.movementKinds.expenseOut'),
    [SafeMovementKind.PurchaseOut]: t('safe.movementKinds.purchaseOut'),
    [SafeMovementKind.Refund]: t('safe.movementKinds.refund'),
  }
  return map[value] ?? '—'
}

function onPageChange(event: { page: number; rows: number }) {
  pageIndex.value = event.page + 1
  pageSize.value = event.rows
}
</script>

<style scoped>
.safe-movement__customer {
  color: var(--q-muted);
  font-size: 13px;
}

.safe-verify {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  align-items: center;
  border-radius: var(--q-radius-md, 14px);
  padding: 12px 16px;
  margin-bottom: var(--q-space-3, 12px);
  font-size: 14px;
  box-shadow: var(--q-neo-inset);
}

.safe-verify--ok {
  color: var(--q-success);
}

.safe-verify--off {
  color: var(--q-danger);
  font-weight: 600;
}

.safe-amount--in {
  color: var(--q-success);
}

.safe-amount--out {
  color: var(--q-danger);
}
</style>
