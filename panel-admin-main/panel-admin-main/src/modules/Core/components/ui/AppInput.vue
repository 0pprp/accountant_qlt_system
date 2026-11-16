<template>
  <AppField :label="label" :hint="hint" :error="error" :required="required" :grow="grow">
    <div
      class="q-control-shell"
      :class="{ 'q-control-shell--password': isPassword }"
    >
      <input
        class="q-input"
        :class="{ 'q-input--has-action': isPassword }"
        :type="inputType"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :name="name"
        :maxlength="maxlength"
        :autocomplete="autocomplete"
        :min="min"
        :max="max"
        :dir="dir"
        :aria-invalid="error ? true : undefined"
        @input="onInput"
      >
      <button
        v-if="isPassword"
        type="button"
        class="q-pw-toggle"
        :aria-label="showPassword ? 'إخفاء كلمة السر' : 'إظهار كلمة السر'"
        :aria-pressed="showPassword"
        tabindex="0"
        @click="showPassword = !showPassword"
      >
        <svg
          v-if="!showPassword"
          viewBox="0 0 24 24"
          width="22"
          height="22"
          fill="none"
          aria-hidden="true"
        >
          <path
            d="M2.5 12s3.5-7 9.5-7 9.5 7 9.5 7-3.5 7-9.5 7-9.5-7-9.5-7Z"
            stroke="currentColor"
            stroke-width="1.7"
            stroke-linejoin="round"
          />
          <circle
            cx="12"
            cy="12"
            r="3.2"
            stroke="currentColor"
            stroke-width="1.7"
          />
        </svg>
        <svg
          v-else
          viewBox="0 0 24 24"
          width="22"
          height="22"
          fill="none"
          aria-hidden="true"
        >
          <path
            d="M3 3l18 18"
            stroke="currentColor"
            stroke-width="1.7"
            stroke-linecap="round"
          />
          <path
            d="M10.6 6.3A10.4 10.4 0 0 1 12 5c6 0 9.5 7 9.5 7a16.6 16.6 0 0 1-3.1 3.9M7.1 7.2A15.7 15.7 0 0 0 2.5 12S6 19 12 19c1.4 0 2.7-.3 3.9-.8"
            stroke="currentColor"
            stroke-width="1.7"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
          <path
            d="M9.9 9.9a3.2 3.2 0 0 0 4.2 4.2"
            stroke="currentColor"
            stroke-width="1.7"
            stroke-linecap="round"
          />
        </svg>
      </button>
    </div>
  </AppField>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import AppField from './AppField.vue'

const props = withDefaults(
  defineProps<{
    modelValue?: string | number | null
    label?: string
    hint?: string
    error?: string
    required?: boolean
    grow?: boolean
    type?: string
    placeholder?: string
    disabled?: boolean
    readonly?: boolean
    name?: string
    maxlength?: number | string
    autocomplete?: string
    min?: number | string
    max?: number | string
    dir?: 'ltr' | 'rtl' | 'auto'
  }>(),
  {
    modelValue: '',
    type: 'text',
    required: false,
    grow: false,
    disabled: false,
    readonly: false,
    min: undefined,
    max: undefined,
    dir: undefined,
  },
)

const emit = defineEmits<{ 'update:modelValue': [string] }>()

const showPassword = ref(false)
const isPassword = computed(() => props.type === 'password')
const inputType = computed(() => {
  if (!isPassword.value) return props.type
  return showPassword.value ? 'text' : 'password'
})

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).value)
}
</script>

<style scoped>
.q-control-shell {
  position: relative;
  width: 100%;
  display: block;
  background: transparent;
  border: 0;
  padding: 0;
  margin: 0;
  box-shadow: none;
  min-height: 0;
}
.q-control-shell--password .q-input--has-action {
  padding-inline-end: 52px;
}
.q-pw-toggle {
  position: absolute;
  top: 50%;
  inset-inline-end: 8px;
  transform: translateY(-50%);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  margin: 0;
  padding: 0;
  border: 0;
  border-radius: 10px;
  background: transparent;
  box-shadow: none;
  color: #50666a;
  cursor: pointer;
}
.q-pw-toggle:hover {
  color: #006a64;
  background: rgba(0, 106, 100, 0.08);
}
.q-pw-toggle:focus-visible {
  outline: 2px solid #006a64;
  outline-offset: 1px;
}
</style>
