<template>
  <component :is="layout">
    <RouterView v-slot="{ Component }">
      <Transition
        enterFromClass="opacity-0"
        enterActiveClass="transition duration-300"
      >
        <component :is="Component" />
      </Transition>
    </RouterView>
    <BaseStatementDialog v-if="toastStore.messages !== null" />
  </component>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import BaseStatementDialog from '../components/base/BaseStatementDialog/BaseStatementDialog.vue'
import { useToastStore } from '../store'

defineOptions({
  name: 'TheAppLayout',
})

const toastStore = useToastStore()

const route = useRoute()

const layout = computed(() => {
  const layout = route?.meta?.layout as AppLayouts

  if (layout) {
    return `The${layout}Layout`
  }
  return 'div'
})
</script>
