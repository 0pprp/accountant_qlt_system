<template>
  <component :is="iconComponent" v-bind="$attrs" :key="props.name" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Props } from './SvgIcon.types'

const props = withDefaults(defineProps<Props>(), {
  name: '',
})

const icons = import.meta.glob('@/assets/svg/icons/*.svg', {
  eager: true,
  import: 'default',
})

const iconComponent = computed(() => {
  const path = `/src/assets/svg/icons/${props.name}.svg`
  return icons[path] || null
})
</script>
