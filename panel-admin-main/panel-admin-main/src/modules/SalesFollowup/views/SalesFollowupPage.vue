<template>
  <div class="sf-page" dir="rtl">
    <header class="sf-page-head">
      <h1 class="sf-page-title">{{ $t('salesFollowup.title') }}</h1>
    </header>
    <SalesFollowupBoard />
  </div>
</template>

<script setup lang="ts">
import SalesFollowupBoard from './SalesFollowupBoard.vue'
</script>

<style scoped>
.sf-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 0.25rem 0 1.5rem;
}
.sf-page-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}
.sf-page-title {
  margin: 0;
  font-size: 1.55rem;
  font-weight: 700;
  color: #00201e;
  line-height: 1.4;
}
</style>
