<template>
  <AppPage :title="$t('salesFollowup.title')" subtitle="متابعة موظفي المبيعات ضمن الفروع المسموحة">
    <SalesFollowupBoard />
  </AppPage>
</template>

<script setup lang="ts">
import { AppPage } from '@/modules/Core/components/ui'
import SalesFollowupBoard from './SalesFollowupBoard.vue'
</script>
