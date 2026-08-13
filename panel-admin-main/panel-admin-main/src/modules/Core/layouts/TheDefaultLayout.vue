<template>
  <main
    class="p-2 flex justify-between h-screen overflow-auto w-full bg-background"
  >
    <TheSideBar @rail-changed="onRailChanged" />
    <main class="p-2 flex-grow main" :style="mainStyles">
      <slot />
    </main>
  </main>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import TheSideBar from '@/modules/Core/components/layout/TheSideBar/TheSideBar.vue'

defineOptions({
  name: 'TheDefaultLayout',
})

const isRailExpanded = ref(true)

const mainStyles = computed(() => ({
  maxWidth: `calc(100% - ${isRailExpanded.value ? '260px' : '113px'})`,
}))

function onRailChanged(railState: boolean) {
  isRailExpanded.value = railState
}
</script>
