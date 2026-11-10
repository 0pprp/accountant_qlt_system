<template>
  <SalesRequestsWorkspace :default-tab="defaultTab" :fixed-tab="defaultTab === 'exceptions' ? defaultTab : undefined" />
</template>

<script setup lang="ts">
import SalesRequestsWorkspace from '@/modules/SalesRequests/views/SalesRequestsWorkspace.vue'
import type { SalesRequestTabKey } from '@/modules/SalesRequests/domain/tabs'

withDefaults(
  defineProps<{
    defaultTab?: SalesRequestTabKey
  }>(),
  { defaultTab: undefined }
)
</script>
