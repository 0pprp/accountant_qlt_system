<template>
  <!-- غلاف شفاف — شاشة الدخول/الانترو كاملة داخل LoginPage بنفس تطبيق الفريق -->
  <div class="auth-layout">
    <slot />
  </div>
</template>

<script setup lang="ts">
defineOptions({
  name: 'TheAuthLayout',
})
</script>

<style scoped>
.auth-layout {
  min-height: 100dvh;
  width: 100%;
  overflow: auto;
  background: #f8f8f8;
}
</style>
