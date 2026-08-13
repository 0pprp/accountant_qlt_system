<template>
  <DatePicker
    ref="datePickerRef"
    v-model="tempDateRange"
    :unstyled="false"
    showIcon
    fluid
    selectionMode="range"
    :showOnFocus="false"
    :inputId="uniqueId"
    class="HeaderDatePicker"
    @show="onShow"
    @hide="onHide"
  >
    <template #footer>
      <div class="flex justify-between gap-2 p-3 border-t">
        <Button
          @click="onAccept"
          :label="$t('words.save')"
          class="btn-primary w-2/3"
        />

        <Button
          @click="onCancel"
          class="bg-white shadow-2xl !text-primary w-1/3"
        >
          {{ $t('words.cancel') }}
        </Button>
      </div>
    </template>
  </DatePicker>
</template>

<script setup lang="ts">
import { Button, DatePicker } from 'primevue'
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import useUid from '@/modules/Core/composable/useCurrentInstance'

const route = useRoute()
const router = useRouter()

const datePickerRef = ref()

const dateRange = ref<Date[] | null>(null)
const tempDateRange = ref<Date[] | null>(null)
const isUpdatingFromQuery = ref(false)
const isPickerOpen = ref(false)

const uniqueId = computed(() => `date-range-${useUid()}`)

function initializeDatesFromQuery() {
  const startDate = route.query.startDate as string
  const endDate = route.query.endDate as string

  if (startDate && endDate) {
    const start = new Date(startDate)
    const end = new Date(endDate)

    if (!isNaN(start.getTime()) && !isNaN(end.getTime())) {
      isUpdatingFromQuery.value = true
      dateRange.value = [start, end]
      tempDateRange.value = [start, end]
      isUpdatingFromQuery.value = false
    }
  } else {
    isUpdatingFromQuery.value = true
    dateRange.value = null
    tempDateRange.value = null
    isUpdatingFromQuery.value = false
  }
}

watch(
  () => [route.query.startDate, route.query.endDate],
  () => {
    initializeDatesFromQuery()
  },
  { immediate: true }
)

function formatDateToString(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function onShow() {
  isPickerOpen.value = true
  tempDateRange.value = dateRange.value ? [...dateRange.value] : null
}

function onHide() {
  isPickerOpen.value = false
}

function onAccept() {
  if (tempDateRange.value && tempDateRange.value.length > 0) {
    let startDate: Date
    let endDate: Date

    if (
      tempDateRange.value.length === 1 ||
      (tempDateRange.value.length === 2 && tempDateRange.value[1] === null)
    ) {
      startDate = tempDateRange.value[0]
      endDate = tempDateRange.value[0]
    } else if (
      tempDateRange.value.length === 2 &&
      tempDateRange.value[0] &&
      tempDateRange.value[1]
    ) {
      startDate = tempDateRange.value[0]
      endDate = tempDateRange.value[1]
    } else {
      onCancel()
      return
    }

    const startDateString = formatDateToString(startDate)
    const endDateString = formatDateToString(endDate)

    dateRange.value = [startDate, endDate]
    tempDateRange.value = [startDate, endDate]

    const newQuery = {
      ...route.query,
      startDate: startDateString,
      endDate: endDateString,
    }

    router.push({ query: newQuery })
  } else {
    dateRange.value = null

    const newQuery = { ...route.query }
    delete newQuery.startDate
    delete newQuery.endDate

    router.push({ query: newQuery })
  }

  closePicker()
}

function onCancel() {
  tempDateRange.value = null

  const newQuery = { ...route.query }
  delete newQuery.startDate
  delete newQuery.endDate

  dateRange.value = null
  tempDateRange.value = null

  router.push({ query: newQuery })

  closePicker()
}

function closePicker() {
  if (datePickerRef.value && typeof datePickerRef.value.hide === 'function') {
    datePickerRef.value.hide()
    return
  }

  if (datePickerRef.value && datePickerRef.value.overlayVisible !== undefined) {
    datePickerRef.value.overlayVisible = false
    return
  }

  const inputElement = document.querySelector(
    `#${uniqueId.value} input`
  ) as HTMLInputElement
  if (inputElement) {
    inputElement.blur()
    setTimeout(() => {
      document.body.click()
    }, 10)
  }
}

initializeDatesFromQuery()
</script>
