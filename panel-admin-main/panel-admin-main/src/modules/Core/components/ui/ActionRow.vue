<template>
  <div class="q-action-row" role="group">
    <AppButton
      v-if="primary"
      variant="primary"
      :disabled="primary.disabled || busy"
      @click="$emit('primary')"
    >
      {{ primary.label }}
    </AppButton>
    <AppButton
      v-for="(action, i) in secondary"
      :key="`${action.label}-${i}`"
      variant="secondary"
      :disabled="action.disabled || busy"
      @click="$emit('secondary', action.id ?? action.label)"
    >
      {{ action.label }}
    </AppButton>
    <slot />
  </div>
</template>

<script setup lang="ts">
import AppButton from './AppButton.vue'

export type UiAction = { id?: string; label: string; disabled?: boolean }

defineProps<{
  primary?: UiAction
  secondary?: UiAction[]
  busy?: boolean
}>()

defineEmits<{ primary: []; secondary: [id: string] }>()
</script>

<style scoped>
.q-action-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 16px;
}
</style>
