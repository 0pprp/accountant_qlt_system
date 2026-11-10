<template>
  <slot v-if="decision === 'allowed'" />
  <slot v-else-if="decision === 'loading'" name="loading">
    <div
      v-if="!silent"
      class="access-gate-loading rounded-xl bg-white/70 animate-pulse min-h-24"
      aria-busy="true"
      aria-live="polite"
    />
  </slot>
  <template v-else-if="silent" />
  <slot v-else name="fallback">
    <AccessDeniedState />
  </slot>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import AccessDeniedState from './AccessDeniedState.vue'
import type { AccessRequirement } from '@/modules/Core/access/access.types'
import { usePermission } from '@/modules/Core/composable/usePermission'

const props = withDefaults(
  defineProps<{
    requirement: AccessRequirement
    /** عند true لا تُعرض حالة المنع الافتراضية (مناسب لأقسام اختيارية داخل صفحة مسموحة) */
    silent?: boolean
  }>(),
  { silent: false }
)

const { decide } = usePermission()

const decision = computed(() => decide(props.requirement))
</script>
