<template>
  <component
    :is="tag"
    :type="tag === 'button' ? type : undefined"
    :to="to"
    :href="href"
    class="q-btn"
    :class="[
      `q-btn--${variant}`,
      size === 'sm' ? 'q-btn--sm' : '',
      block ? 'q-btn--block' : '',
      iconOnly ? 'q-btn--icon' : '',
    ]"
    :disabled="disabled || loading"
    :aria-busy="loading || undefined"
    @click="$emit('click', $event)"
  >
    <i v-if="loading" class="pi pi-spin pi-spinner" aria-hidden="true" />
    <i v-else-if="icon" :class="icon" aria-hidden="true" />
    <span v-if="!iconOnly"><slot /></span>
  </component>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { RouteLocationRaw } from 'vue-router'

const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
    size?: 'md' | 'sm'
    type?: 'button' | 'submit' | 'reset'
    disabled?: boolean
    loading?: boolean
    block?: boolean
    icon?: string
    iconOnly?: boolean
    to?: RouteLocationRaw
    href?: string
  }>(),
  {
    variant: 'primary',
    size: 'md',
    type: 'button',
    disabled: false,
    loading: false,
    block: false,
    iconOnly: false,
  },
)

defineEmits<{ click: [MouseEvent] }>()

const tag = computed(() => {
  if (props.to) return 'router-link'
  if (props.href) return 'a'
  return 'button'
})
</script>
