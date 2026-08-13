<template>
  <main
    class="relative flex items-center justify-center min-h-screen min-w-screen h-screen overflow-hidden"
  >
    <img
      src="@/assets/svg/auth/auth-main.svg?url"
      alt="background"
      class="absolute top-0 left-0 w-full object-cover"
      loading="lazy"
      decoding="async"
    />

    <div class="z-1 h-full w-full px-60 xl:pt-72 pt-50">
      <slot />
    </div>
  </main>
</template>

<script setup lang="ts">
defineOptions({
  name: 'TheAuthLayout',
})
</script>
