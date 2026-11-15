<template>
  <AppField :label="label" :hint="hint" :error="error" :required="required" :grow="grow">
    <div class="q-search">
      <i class="pi pi-search q-search__icon" aria-hidden="true" />
      <input
        class="q-input q-search__input"
        type="search"
        :value="modelValue ?? ''"
        :placeholder="placeholder"
        :disabled="disabled"
        :name="name"
        :autocomplete="autocomplete"
        :aria-invalid="error ? true : undefined"
        @input="onInput"
      />
    </div>
  </AppField>
</template>

<script setup lang="ts">
import AppField from './AppField.vue'

withDefaults(
  defineProps<{
    modelValue?: string | null
    label?: string
    hint?: string
    error?: string
    required?: boolean
    grow?: boolean
    placeholder?: string
    disabled?: boolean
    name?: string
    autocomplete?: string
  }>(),
  {
    modelValue: '',
    required: false,
    grow: true,
    disabled: false,
    autocomplete: undefined,
  },
)

const emit = defineEmits<{ 'update:modelValue': [string] }>()

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).value)
}
</script>

<style scoped>
.q-search {
  position: relative;
}
.q-search__icon {
  position: absolute;
  inset-inline-start: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: #006a64;
  pointer-events: none;
  font-size: 14px;
}
.q-search__input {
  padding-inline-start: 40px;
  background: #fff;
  border: 1px solid #cddbd7;
  box-shadow: none;
}
</style>
