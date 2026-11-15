<template>
  <label class="q-field" :class="grow ? 'q-field--grow' : undefined">
    <span v-if="label" class="q-field__label" :class="{ 'is-required': required }">
      {{ label }}
    </span>
    <slot />
    <span v-if="hint && !error" class="q-field__hint">{{ hint }}</span>
    <span v-if="error" class="q-field__error" role="alert">{{ error }}</span>
  </label>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    label?: string
    hint?: string
    error?: string
    required?: boolean
    grow?: boolean
  }>(),
  { required: false, grow: false },
)
</script>

<style scoped>
.q-field--grow {
  grid-column: 1 / -1;
}
@media (min-width: 768px) {
  .q-field--grow {
    grid-column: span 2;
  }
}
</style>
