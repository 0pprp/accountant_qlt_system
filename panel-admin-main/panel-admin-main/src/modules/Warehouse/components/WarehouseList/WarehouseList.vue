<template>
  <div>
    <!-- Category Filter -->
    <div class="my-4 bg-white p-4 rounded-xl border border-line">
      <CategorySelectInput
        v-model:modelValue="categoryId"
        placeholder="warehouse.selectCategory"
        :readonly="false"
        clearable
        class="w-100"
      />
    </div>

    <!-- Products Table -->
    <CustomDataTable
      :data="tableData"
      :columns="columns"
      :totalRecords="totalRecords"
      :pageSize="pageSize"
      :loading="loading"
      :selectable="true"
      :showFlag="true"
      :paginatorEnabled="true"
      tableStyle="min-width: 100%; overflow: auto; table-layout:scroll"
      class="custom-table custom-border mt-4"
      :paginatorLabel="$t('warehouse.products')"
      @selection-change="handleSelectionChange"
      @page-change="handlePageChange"
    >
      <template #paginator>
        <div class="flex items-center justify-between">
          <div class="inline rounded-xl bg-white border-line border py-2 px-4">
            <span class="text-gray-700">
              {{ $t('warehouse.totalRemainingCount') }}:
            </span>
            <span>{{ productData?.totalRemainingCount }}</span>
          </div>

          <div class="inline relative mx-2 py-2">
            <Button
              ref="menuButtonRef"
              @click="toggleMenu"
              :unstyled="false"
              class="!border !border-line !rounded-xl !bg-white transition-colors !p-0"
            >
              <div class="flex items-center justify-between px-4 py-2">
                <div class="flex items-center gap-3">
                  <i
                    :class="isMenuOpen ? 'pi pi-angle-up' : 'pi pi-angle-down'"
                    class="text-gray-700 ml-2"
                  ></i>
                  <span class="rounded-2xl bg-white px-2">
                    <SvgIcon name="cardReceive" class="text-gray-700 w-5 h-5" />
                  </span>
                  <div class="flex items-center gap-2">
                    <span class="text-gray-700 font-medium">
                      {{
                        selectedMenuType === 'buy'
                          ? $t('warehouse.table.buyAmount')
                          : $t('warehouse.table.sellAmount')
                      }}:
                    </span>
                    <span class="text-gray-900">
                      {{
                        formattedPrice(
                          selectedMenuType === 'buy'
                            ? totalBuyAmount
                            : totalSellAmount
                        )
                      }}
                    </span>
                    <small class="text-gray-800">{{ $t('words.dinar') }}</small>
                  </div>
                </div>
              </div>
            </Button>

            <Menu
              ref="menuRef"
              :model="menuItems"
              :popup="true"
              :unstyled="false"
              class="mt-2"
              @show="isMenuOpen = true"
              @hide="isMenuOpen = false"
            />
          </div>
        </div>
      </template>

      <template #body-dailyInstallmentAmount="{ data }">
        <span>
          {{ data.dailyInstallmentAmount }}
        </span>
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-buyAmount="{ data }">
        <span>
          {{ data.buyAmount }}
        </span>
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-sellAmount="{ data }">
        <span>
          {{ data.sellAmount }}
        </span>
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-description="{ data }">
        {{ data.description ? data.description : '-' }}
      </template>

      <template #body-createdAt="{ data }">
        {{ data.createdAt ? formattedDate(data.createdAt) : '-' }}
      </template>

      <template #body-remainingCount="{ data }">
        {{ data.remainingCount }}
      </template>

      <template #body-edit="{ data: rowData }" v-if="canUpdateProduct">
        <SvgIcon
          name="boldPencil"
          class="w-7.5 h-7.5 text-gray-300 cursor-pointer hover:text-primary transition-colors"
          @click="handleUpdateProduct(rowData)"
        />
      </template>

      <template #body-delete="{ data: rowData }" v-if="canDeleteProduct">
        <SvgIcon
          name="boldTrash"
          class="w-7.5 h-7.5 text-gray-300 cursor-pointer hover:text-error transition-colors"
          @click="handleDeleteProduct(rowData)"
        />
      </template>
    </CustomDataTable>
  </div>
</template>

<script setup lang="ts">
import { Button } from 'primevue'
import Menu from 'primevue/menu'
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useProductDataQuery } from '../../requests/queries'
import type { Product } from '../../types/model'
import type { Emits } from './WarehouseList.Types'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type { AllowedTypes } from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import useAuthStore from '@/modules/Auth/store'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { formattedDate } from '@/modules/Core/utils/time'
import { formattedPrice } from '@/modules/Core/utils/price'
import CategorySelectInput from '@/modules/Core/components/shared/CategorySelectInput/CategorySelectInput.vue'

const emit = defineEmits<Emits>()

const { t } = useI18n()
const route = useRoute()

const { can } = usePermission()
const canUpdateProduct = can('Product', 'Update')
const canDeleteProduct = can('Product', 'Delete')

const selectedProducts = ref<Product[]>([])

const menuButtonRef = ref()
const menuRef = ref()
const isMenuOpen = ref(false)
const selectedMenuType = ref<'buy' | 'sell'>('buy')

const pageSize = ref(10)
const pageIndex = ref(1)

const searchTerm = computed(() => {
  const s = route.query.s
  if (!s) return null
  return Array.isArray(s) ? s.map(String) : [String(s)]
})

watch(
  searchTerm,
  () => {
    pageIndex.value = 1
  },
  { deep: true }
)

const authStore = useAuthStore()
const branchId = computed(() => authStore.selectedBranch!.id)

// Category filter
const categoryId = ref<number | null>(null)

// Convert null to -1 for the query (useProductDataQuery expects Ref<number>)
const categoryIdForQuery = computed(() => categoryId.value ?? -1)

watch(categoryId, () => {
  pageIndex.value = 1
})

const { data: productData, isLoading } = useProductDataQuery(
  categoryIdForQuery,
  branchId,
  pageIndex,
  pageSize,
  searchTerm
)

const tableData = computed(
  () => productData.value?.paginatedProducts.items || []
)
const totalRecords = computed(
  () => productData.value?.paginatedProducts.totalCount || 0
)
const loading = computed(() => isLoading.value)

const totalBuyAmount = computed(() => productData.value?.totalBuyAmount ?? 0)
const totalSellAmount = computed(() => productData.value?.totalSellAmount ?? 0)

const menuItems = computed(() => [
  {
    label: `${t('warehouse.table.buyAmount')}: ${formattedPrice(totalBuyAmount.value)} ${t('words.dinar')}`,
    command: () => {
      selectedMenuType.value = 'buy'
    },
  },
  {
    label: `${t('warehouse.table.sellAmount')}: ${formattedPrice(totalSellAmount.value)} ${t('words.dinar')}`,
    command: () => {
      selectedMenuType.value = 'sell'
    },
  },
])

function toggleMenu(event: Event) {
  if (menuRef.value) {
    menuRef.value.toggle(event)
    isMenuOpen.value = !isMenuOpen.value
  }
}

const columns = computed(() => {
  const baseColumns = [
    {
      field: 'name',
      header: t('warehouse.table.name'),
      sortable: true,
    },
    {
      field: 'categoryName',
      header: t('warehouse.table.categoryName'),
      sortable: true,
    },
    {
      field: 'warehouseName',
      header: t('warehouse.table.warehouseName'),
      sortable: true,
    },
    {
      field: 'remainingCount',
      header: t('warehouse.table.remainingCount'),
      sortable: true,
    },
    {
      field: 'dailyInstallmentAmount',
      header: t('warehouse.table.dailyInstallmentAmount'),
      sortable: true,
    },
    {
      field: 'buyAmount',
      header: t('warehouse.table.buyAmount'),
      sortable: true,
    },
    {
      field: 'sellAmount',
      header: t('warehouse.table.sellAmount'),
      sortable: true,
    },
    {
      field: 'creatorName',
      header: t('warehouse.table.creatorName'),
      sortable: true,
    },
    {
      field: 'description',
      header: t('warehouse.table.description'),
      sortable: false,
    },
    {
      field: 'createdAt',
      header: t('warehouse.table.createdAt'),
      sortable: true,
    },
  ]

  if (canUpdateProduct) {
    baseColumns.push({
      field: 'edit',
      header: t('words.edit'),
      sortable: false,
    })
  }

  if (canDeleteProduct) {
    baseColumns.push({
      field: 'delete',
      header: t('words.delete'),
      sortable: false,
    })
  }

  return baseColumns
})

function handleUpdateProduct(product: Product) {
  emit('updateProduct', product)
}

function handleDeleteProduct(product: Product) {
  emit('deleteProduct', product)
}

function handleSelectionChange(selection: AllowedTypes[]) {
  selectedProducts.value = selection as Product[]
}

function handlePageChange(event: {
  first: number
  page: number
  rows: number
}) {
  pageIndex.value = event.page + 1
}
</script>
