<template>
  <div>
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <AutoComplete
      :suggestions="filteredCustomers"
      @complete="search"
      @select="onCustomerSelect"
      @clear="onClear"
      v-model="selectedCustomer"
      optionLabel="fullName"
      :unstyled="false"
      optionValue="id"
      class="!block"
      :forceSelection="true"
      :autoHighlight="true"
      :completeOnFocus="true"
      :required
      :disabled="readonly || disabled"
      :placeholder="$t(placeholder)"
      :loading="isLoading"
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs py-2"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AutoComplete, Message } from 'primevue'
import type { Props, Emits } from './CustomerSelectInput.types'
import { useCustomerDataQuery } from '@/modules/Customer/requests/queries'
import type { Customer } from '@/modules/Customer/types/model'
import useAuthStore from '@/modules/Auth/store'

defineProps<Props>()
const emit = defineEmits<Emits>()

const customerId = defineModel<Props['modelValue']>('modelValue')

const filteredCustomers = ref<Array<Customer>>([])
const selectedCustomer = ref<Customer | string | null>(null)

const pageIndex = ref(1)
const pageSize = ref(100)
const authStore = useAuthStore()
const searchTerm = ref<string | null>(null)

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const customers = ref<Array<Customer>>([])

const { data, isLoading } = useCustomerDataQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm
)

watch(
  data,
  (newData) => {
    customers.value = newData?.items || []
    
    // If user is currently typing (selectedCustomer is a string), preserve it
    // and only update filteredCustomers based on current search
    if (typeof selectedCustomer.value === 'string') {
      // User is typing, don't reset selectedCustomer
      // filteredCustomers will be updated in search() function
      return
    }
    
    // Update filteredCustomers if user is not typing
    filteredCustomers.value = [...customers.value]

    // Only update selectedCustomer if we have a valid customerId (positive number)
    if (customerId.value && customerId.value > 0) {
      const foundCustomer = customers.value.find(
        (customer) => customer.id === customerId.value
      )
      if (foundCustomer) {
        selectedCustomer.value = foundCustomer
      }
    } else if (customerId.value === null || customerId.value === undefined) {
      // Only reset if customerId is explicitly null/undefined
      selectedCustomer.value = null
    }
  },
  { immediate: true }
)

async function search(event: { query: string }) {
  const query = event.query.trim()

  // Emit the typed name for new customer creation
  emit('update:customerName', query)

  // Update searchTerm to trigger API call
  searchTerm.value = query.length > 0 ? query : null

  // Filter customers locally while typing
  if (customers.value.length > 0) {
    if (!query.length) {
      filteredCustomers.value = [...customers.value]
    } else {
      filteredCustomers.value = customers.value.filter((customer) => {
        return customer.fullName
          ?.toLowerCase()
          .includes(query.toLowerCase())
      })
    }
  }
}

watch(selectedCustomer, (newCustomer) => {
  if (typeof newCustomer === 'object' && newCustomer?.id) {
    // Existing customer selected
    customerId.value = newCustomer.id
  } else if (typeof newCustomer === 'string') {
    // New customer name typed
    customerId.value = -1
    emit('update:customerName', newCustomer)
  } else {
    // Cleared
    customerId.value = -1
    emit('update:customerName', '')
  }
})

function onCustomerSelect(event: { value: Customer }) {
  selectedCustomer.value = event.value
}

function onClear() {
  selectedCustomer.value = null
  customerId.value = -1
  emit('update:customerName', '')
}
</script>
