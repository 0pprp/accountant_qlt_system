<template>
  <div>
    <Tabs :value="activeIndex" :unstyled="false">
      <TabList class="bg-background rounded-t-2xl mb-10" :unstyled="false">
        <Tab
          class="w-1/3 p-3 !text-gray-700"
          :value="CustomerUpsertTabSteps.CustomerForm"
        >
          {{ $t('customer.tabs.form') }}
        </Tab>

        <Tab
          class="w-1/3 p-3 !text-gray-700 !font-regular"
          :unstyled="false"
          :disabled="!customerId"
          :value="CustomerUpsertTabSteps.CustomerDocuments"
        >
          {{ $t('customer.tabs.document') }}
        </Tab>

        <Tab
          class="w-1/3 p-3 !text-gray-700 !font-regular"
          :unstyled="false"
          :disabled="!customerId"
          :value="CustomerUpsertTabSteps.CustomerOrders"
        >
          {{ $t('customer.tabs.orders') }}
        </Tab>
      </TabList>

      <TabPanels>
        <TabPanel :value="CustomerUpsertTabSteps.CustomerForm">
          <div
            v-if="!!customerId && isCustomerByIdDataPending"
            class="flex justify-center w-full py-20"
          >
            <ProgressSpinner class="!w-10 !h-10" :unstyled="false" />
          </div>

          <CustomerForm
            v-else
            :isEditMode="!!customerId"
            :loading="isCreatePending || isUpdatePending"
            :initialValues="initialValues"
            @submit="handleSubmit"
          />
        </TabPanel>

        <TabPanel :value="CustomerUpsertTabSteps.CustomerDocuments">
          <CustomerDocument
            ref="customerDocumentsFormRef"
            :formMode="customerId ? FormMode.IsUpdate : FormMode.IsCreate"
            :customerId="customerId"
            :attachments="customerByIdData?.attachments"
          />
        </TabPanel>

        <TabPanel :value="CustomerUpsertTabSteps.CustomerOrders">
          <CustomerOrders :customerId="customerId" />
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Tab,
  TabList,
  TabPanel,
  TabPanels,
  Tabs,
  ProgressSpinner,
} from 'primevue'
import CustomerForm from '@/modules/Customer/components/CustomerForm/CustomerForm.vue'
import {
  CustomerUpsertTabSteps,
  type CustomerForm as form,
} from '@/modules/Customer/types/model'
import type { CustomerCreatePayload } from '@/modules/Customer/types/api'
import { useCreateCustomerMutation } from '@/modules/Customer/requests/mutations'
import { useCustomerByIdDataQuery } from '@/modules/Customer/requests/queries'
import CustomerDocument from '@/modules/Customer/components/CustomerDocument/CustomerDocument.vue'
import CustomerOrders from '@/modules/Customer/components/CustomerOrders/CustomerOrders.vue'
import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types.ts'
import { useUpdateCustomerMutation } from '@/modules/Customer/requests/mutations/useUpdateCustomerMutation.ts'
import useAuthStore from '@/modules/Auth/store'

const activeIndex = ref<CustomerUpsertTabSteps>(
  CustomerUpsertTabSteps.CustomerForm
)

const route = useRoute()
const customerId = computed(() => Number(route.params.id))
const { data: customerByIdData, isPending: isCustomerByIdDataPending } =
  useCustomerByIdDataQuery(customerId, {
    enabled: () => !!customerId.value,
  })

const authStore = useAuthStore()

const initialValues = computed<form>(() => {
  return {
    fullName: customerByIdData.value?.fullName ?? '',
    motherName: customerByIdData.value?.motherName ?? '',
    nationalCode: customerByIdData.value?.nationalCode ?? '',
    birthDate: customerByIdData.value?.birthDate || null,
    whatsAppPhoneNumber: customerByIdData.value?.whatsAppPhoneNumber ?? '',
    phoneNumber: customerByIdData.value?.phoneNumber ?? '',
    branchId: authStore.selectedBranch?.id ?? 0,
    business: {
      name: customerByIdData.value?.business.name ?? '',
      address: customerByIdData.value?.business.address ?? '',
      nearestKnownLocation:
        customerByIdData.value?.business.nearestKnownLocation ?? '',
    },
  }
})

const router = useRouter()
const {
  mutateAsync: createMutateAsync,
  isPending: isCreatePending,
  data: createData,
} = useCreateCustomerMutation()
const { mutateAsync: updateMutateAsync, isPending: isUpdatePending } =
  useUpdateCustomerMutation()

async function handleSubmit(values: CustomerCreatePayload) {
  if (!customerId.value) {
    await createMutateAsync(values)
    router.push({
      name: 'EditCustomerView',
      params: { id: createData.value?.data.id },
    })
    activeIndex.value = CustomerUpsertTabSteps.CustomerDocuments
  } else {
    await updateMutateAsync({ id: customerId.value, payload: values })
    activeIndex.value = CustomerUpsertTabSteps.CustomerDocuments
  }
}
</script>

<style>
.p-tablist-active-bar {
  background-color: #6f797a !important;
  height: 3px;
}
</style>
