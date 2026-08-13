<template>
  <DataTable
    v-if="products"
    :value="products"
    v-model:selection="selectedProducts"
    responsiveLayout="scroll"
    :showHeaders="true"
    scrollable
    :rows="products.length"
    dataKey="id"
    :unstyled="false"
    tableStyle="min-width: 100%; overflow: auto; table-layout:scroll "
    class="custom-table"
    :lazy="true"
  >
    <template #loading>
      <TableSkeletonLoading />
    </template>

    <Column selectionMode="multiple" headerStyle="width: 1rem"></Column>

    <Column field="name" :header="headers.name"> </Column>

    <Column field="warehouseName" :header="headers.warehouseName"> </Column>

    <Column field="remainingCount" :header="headers.remainingCount"></Column>

    <Column
      field="dailyInstallmentAmount"
      :header="headers.dailyInstallmentAmount"
    >
      <template #body="{ data }">
        <span>
          {{ data.dailyInstallmentAmount }}
        </span>
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>
    </Column>

    <Column field="buyAmount" :header="headers.buyAmount">
      <template #body="{ data }">
        <span>
          {{ data.buyAmount }}
        </span>
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>
    </Column>

    <Column field="sellAmount" :header="headers.sellAmount">
      <template #body="{ data }">
        <span>
          {{ data.sellAmount }}
        </span>
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>
    </Column>

    <Column field="creatorName" :header="headers.creatorName"></Column>

    <Column field="description" :header="headers.description">
      <template #body="{ data }">
        {{ data.description ? data.description : '-' }}
      </template>
    </Column>

    <Column field="createdAt" :header="headers.createdAt">
      <template #body="{ data }">
        {{ data.createdAt ? formattedDate(data.createdAt) : '-' }}
      </template>
    </Column>

    <Column :header="headers.edit" v-if="canUpdateProduct">
      <template #body="{ data }">
        <SvgIcon
          name="boldPencil"
          class="w-7.5 h-7.5 text-gray-300 cursor-pointer hover:text-primary transition-colors"
          @click="handleUpdateProduct(data)"
        />
      </template>
    </Column>

    <Column :header="headers.delete" v-if="canDeleteProduct">
      <template #body="{ data }">
        <SvgIcon
          name="boldTrash"
          class="w-7.5 h-7.5 text-gray-300 cursor-pointer hover:text-error transition-colors"
          @click="handleDeleteProduct(data)"
        />
      </template>
    </Column>

    <template #empty>
      <NoData class="mx-auto" />
    </template>
  </DataTable>
</template>

<script setup lang="ts">
import { DataTable, Column } from 'primevue'
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import type { Product } from '../../../types/model'
import type { Emits, Props } from './ProductList.Types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import TableSkeletonLoading from '@/modules/Core/components/shared/TableSkeletonLoading/TableSkeletonLoading.vue'
import NoData from '@/modules/Core/components/shared/NoData/NoData.vue'
import { formattedDate } from '@/modules/Core/utils/time'
import { usePermission } from '@/modules/Core/composable/usePermission'

defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const { can } = usePermission()
const canUpdateProduct = can('Product', 'Update')
const canDeleteProduct = can('Product', 'Delete')

const headers = {
  name: t('warehouse.table.name'),
  warehouseName: t('warehouse.table.warehouseName'),
  remainingCount: t('warehouse.table.remainingCount'),
  dailyInstallmentAmount: t('warehouse.table.dailyInstallmentAmount'),
  buyAmount: t('warehouse.table.buyAmount'),
  sellAmount: t('warehouse.table.sellAmount'),
  creatorName: t('warehouse.table.creatorName'),
  description: t('warehouse.table.description'),
  createdAt: t('warehouse.table.createdAt'),
  edit: t('words.edit'),
  delete: t('words.delete'),
}

const selectedProducts = ref<Product[]>([])

function handleUpdateProduct(product: Product) {
  emit('updateProduct', product)
}

function handleDeleteProduct(product: Product) {
  emit('deleteProduct', product)
}
</script>
