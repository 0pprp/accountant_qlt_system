<template>
  <Button
    :loading="isLoading"
    :disabled="isLoading"
    :unstyled="false"
    class="!bg-background !text-gray-700 !border !border-line !rounded-lg"
    severity="gray-700"
    @click="handleDownload"
  >
    {{ label || $t('words.export') }}
    <i class="pi pi-upload"> </i>
  </Button>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Button } from 'primevue'
import type { Props } from './ExcelReport.types'
import { useExcelReport } from '@/modules/Core/composable/useExcelReport'

const props = defineProps<Props>()

const { downloadExcelReport, isLoading } = useExcelReport()

const reportOptions = computed(() => ({
  endpoint: props.endpoint,
  filename: props.filename,
  params: props.params,
}))

async function handleDownload() {
  await downloadExcelReport(reportOptions.value)
}
</script>
