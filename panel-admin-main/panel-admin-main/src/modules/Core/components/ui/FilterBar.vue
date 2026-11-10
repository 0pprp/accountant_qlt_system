<template>
  <div class="q-filter-bar" role="search">
    <slot />
  </div>
</template>

<style scoped>
.q-filter-bar {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  margin-bottom: 16px;
  padding: 12px;
  background: #f8f8f8;
  border-radius: 12px;
}
.q-filter-bar :deep(label) {
  display: grid;
  gap: 4px;
  font-size: 14px;
  color: #00201e;
}
.q-filter-bar :deep(input),
.q-filter-bar :deep(select),
.q-filter-bar :deep(textarea) {
  min-height: 44px;
  background: #fff;
  border: 1px solid #cfd8d7;
  border-radius: 8px;
  padding: 8px 12px;
  font: inherit;
  color: #00201e;
}
</style>
