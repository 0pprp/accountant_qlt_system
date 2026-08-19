<template>
  <div class="p-4">
    <SafeCards
      v-if="canReadSafe"
      :safeData="safeData"
      :isLoading="safeDataLoading"
    />

    <CustomDataTable
      :data="tableData"
      :columns="columns"
      :totalRecords="totalRecords"
      :pageSize="pageSize"
      :loading="loading || isFetching"
      showRowNumbers
      :selectable="true"
      :showFlag="false"
      :paginatorEnabled="true"
      class="custom-table custom-border mt-4"
      :paginatorLabel="$t('safe.paginatorLabel')"
      @selection-change="handleSelectionChange"
      @page-change="handlePageChange"
    >
      <template #body-fullName="{ data }">
        {{ data.fullName || '-' }}
      </template>

      <template #body-roleName="{ data }">
        {{ data.roleName || '-' }}
      </template>

      <template #body-orderListName="{ data }">
        {{ data.orderListName || '-' }}
      </template>

      <template #body-deliveredCashAmount="{ data }">
        {{ formattedPrice(data.deliveredCashAmount) }}

        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-undeliveredCashAmount="{ data }">
        {{ formattedPrice(data.undeliveredCashAmount) }}
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-remaining="{ data }">
        {{ formattedPrice(data.undeliveredCashAmount) }}
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-ordersCount="{ data }">
        {{ data.ordersCount || '-' }}
      </template>

      <template #body-report="{ data }" v-if="canReadUser">
        <RouterLink
          :to="{ name: 'UserDailyReportRoute', params: { userId: data.id } }"
        >
          <SvgIcon
            name="boldReport"
            class="text-gray-300 hover:text-info transition-colors"
          />
        </RouterLink>
      </template>

      <template
        #body-details="{ data: rowData }"
        v-if="canCollectCashTransaction"
      >
        <SvgIcon
          name="addCircle"
          class="transition-colors"
          :class="
            selectedItems.length > 1
              ? 'text-gray-700 cursor-not-allowed'
              : 'text-primary cursor-pointe'
          "
          :title="$t('words.edit')"
          @click="openDialog(rowData.id)"
        />
      </template>
    </CustomDataTable>

    <CustomDialog
      v-model="isOpen"
      :class="selectedItems.length > 1 ? 'w-[420px]' : ''"
      :title="
        selectedItems.length > 1
          ? 'safe.someSellerCashDeliveriesBtn'
          : 'safe.createSellerCashDeliveries'
      "
      @update:is-open="toggleIsOpen"
    >
      <template #content>
        <div
          v-if="selectedItems.length > 1"
          class="pt-5 text-center"
          :loading="isCreateSomeSellersCashDeliveries"
        >
          <div class="text-gray-700">
            {{ $t('safe.someSellerCashDeliveriesDescription') }}:
          </div>

          <span v-for="(item, index) in selectedItems" :key="index">
            {{ item.fullName }}
            <span v-if="selectedItems.length > index + 1">-</span>
          </span>

          <div class="py-4 text-start">
            <span class="text-gray-700"> {{ $t('safe.allAmount') }} : </span>
            <strong>{{ formattedPrice(getAllAmount()) }} </strong>
          </div>

          <div class="pt-4 w-full flex flex-row gap-4 justify-center">
            <Button
              :disabled="isCreateSomeSellersCashDeliveries"
              :loading="isCreateSomeSellersCashDeliveries"
              class="py-4 w-full"
              :class="'btn-primary'"
              @click.prevent="onSubmitSomeSellers"
            >
              {{ $t('words.save') }}
              <i
                v-if="isCreateSomeSellersCashDeliveries"
                class="pi pi-spin pi-spinner"
              ></i>
            </Button>

            <Button class="btn-text-primary py-4 w-1/2">
              {{ $t('words.cancel') }}
            </Button>
          </div>
        </div>

        <SafeForm
          v-else
          :loading="isCreateSellerCashDeliveries"
          @submit="onSubmit"
        />
      </template>
    </CustomDialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, watchEffect } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import { Button } from 'primevue'
import SafeForm from '../SafeForm/SafeForm.vue'
import {
  useSafeCurrentDataQuery,
  useSafeSellersDataQuery,
} from '../../requests/queries'
import SafeCards from '../SafeCards/SafeCards.vue'
import type { Emits } from './SafeList.types'
import {
  useCreateSellerCashDeliveriesMutation,
  useCreateSomeSellersCashDeliveriesMutation,
} from '../../requests/mutations'
import useSafeStore from '../../store/index'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type {
  SafeSellers,
  SellerCashDeliveriesForm,
} from '@/modules/Safe/types/model'
import type { AllowedTypes } from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import useAuthStore from '@/modules/Auth/store'
import { formattedPrice } from '@/modules/Core/utils'
import CustomDialog from '@/modules/Core/components/shared/CustomDialog/CustomDialog.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { useToastStore } from '@/modules/Core/store'
import { usePermission } from '@/modules/Core/composable/usePermission'

const emit = defineEmits<Emits>()
const route = useRoute()
const authStore = useAuthStore()
const safeStore = useSafeStore()
const pageIndex = ref(1)
const pageSize = ref(10)

const searchTerm = ref<string | null>(
  route.query.s ? String(route.query.s) : null
)

const branchId = computed(() => {
  if (route.name === 'SafeMainMainRoute') {
    return null
  }

  return authStore.selectedBranch?.id || 0
})

watch(
  () => route.query,
  (newQuery) => {
    searchTerm.value = newQuery.s ? String(newQuery.s) : null
  },
  { immediate: true }
)

const selectedItems = ref<SafeSellers[] | []>([])

watch(selectedItems, () => {
  if (selectedItems.value.length > 1) {
    emit('updateHeader', true)
  } else {
    emit('updateHeader', false)
  }
})

const {
  data: safeData,
  isLoading: safeDataLoading,
  isFetched,
} = useSafeCurrentDataQuery(branchId)

const safeId = ref()

watchEffect(() => {
  if (isFetched.value && safeData.value?.id) {
    safeStore.setLoading(safeDataLoading.value)
    safeStore.setSafeData(safeData.value)
    safeId.value = safeStore.safeId
  }
})

const {
  data: safeSellerListData,
  isLoading,
  isFetching,
} = useSafeSellersDataQuery(pageIndex, pageSize, safeId, searchTerm, {
  enabled: computed(() => safeId.value > 0),
})

const tableData = computed(() => safeSellerListData.value?.items || [])
const totalRecords = computed(() => safeSellerListData.value?.totalCount || 0)
const loading = computed(() => isLoading.value)

const { t } = useI18n()

const { can } = usePermission()
const canReadSafe = can('Safe', 'Read')
const canCollectCashTransaction = can('Transaction', 'CollectCash')
const canReadUser = can('User', 'Read')

const columns = computed(() => {
  const baseColumns = [
    {
      field: 'fullName',
      header: t('safe.tableColumns.fullName'),
      sortable: false,
    },
    {
      field: 'roleName',
      header: t('safe.tableColumns.roleName'),
      sortable: false,
    },
    {
      field: 'orderListName',
      header: t('safe.tableColumns.orderListName'),
      sortable: false,
    },
    {
      field: 'deliveredCashAmount',
      header: t('safe.tableColumns.deliveredCashAmount'),
      sortable: false,
    },
    {
      field: 'undeliveredCashAmount',
      header: t('safe.tableColumns.undeliveredCashAmount'),
      sortable: false,
    },
    {
      field: 'remaining',
      header: t('safe.tableColumns.remaining'),
      sortable: false,
    },
    {
      field: 'description',
      header: t('safe.tableColumns.description'),
      sortable: false,
      style: 'min-width: 180px; max-width: 180px',
    },
  ]

  if (canReadUser) {
    baseColumns.push({
      field: 'report',
      header: t('safe.tableColumns.report'),
      sortable: false,
    })
  }

  if (canCollectCashTransaction) {
    baseColumns.push({
      field: 'details',
      header: t('customer.tableColumns.details'),
      sortable: false,
    })
  }

  return baseColumns
})

function handleSelectionChange(selection: AllowedTypes[]) {
  selectedItems.value = selection as SafeSellers[]
}

function handlePageChange(event: {
  first: number
  page: number
  rows: number
}) {
  pageIndex.value = event.page + 1
}
const isOpen = ref(false)

function toggleIsOpen() {
  isOpen.value = !isOpen.value
}

const selectedSafe = ref(-1)

function openDialog(id: number) {
  if (selectedItems.value.length > 1) {
    return
  } else {
    selectedSafe.value = id
    toggleIsOpen()
  }
}

defineExpose({
  openMultipleDialog: () => {
    if (selectedItems.value.length > 1) {
      toggleIsOpen()
    }
  },
})

function getAllAmount() {
  return selectedItems.value.reduce((total, item) => {
    return total + (item.deliveredCashAmount - item.undeliveredCashAmount)
  }, 0)
}

const toastStore = useToastStore()

const {
  isPending: isCreateSomeSellersCashDeliveries,
  mutateAsync: createSomeSellersCashDeliveries,
} = useCreateSomeSellersCashDeliveriesMutation()

const {
  isPending: isCreateSellerCashDeliveries,
  mutateAsync: createSellerCashDeliveries,
} = useCreateSellerCashDeliveriesMutation()

watch([isCreateSomeSellersCashDeliveries, isCreateSellerCashDeliveries], () => {
  if (isCreateSomeSellersCashDeliveries.value) {
    toastStore.setMassage({
      title: 'branch.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  } else if (isCreateSellerCashDeliveries.value) {
    toastStore.setMassage({
      title: 'branch.createPending',
      description: undefined,
      dialogState: DialogState.Loading,
      isOpen: true,
    })
  }
})

async function onSubmit(values: SellerCashDeliveriesForm) {
  await createSellerCashDeliveries({
    id: safeId.value,
    sellerId: selectedSafe.value,
    payload: values,
  })

  toggleIsOpen()
}

async function onSubmitSomeSellers() {
  await createSomeSellersCashDeliveries({
    id: safeId.value,
    payload: {
      sellerIds: selectedItems.value.map((item) => item.id),
    },
  })

  toggleIsOpen()
}
</script>
