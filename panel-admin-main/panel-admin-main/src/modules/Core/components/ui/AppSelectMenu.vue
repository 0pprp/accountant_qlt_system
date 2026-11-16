<template>
  <AppField
    tag="div"
    :label="label"
    :hint="hint"
    :error="error"
    :required="required"
    :grow="grow"
  >
    <div
      ref="rootRef"
      class="q-select-menu"
      :class="{ 'is-open': open, 'is-disabled': disabled }"
    >
      <button
        type="button"
        class="q-select-menu__trigger"
        :disabled="disabled"
        :aria-expanded="open"
        :aria-haspopup="listbox"
        :aria-controls="listId"
        @click.stop="toggle"
      >
        <span
          class="q-select-menu__value"
          :class="{ 'is-placeholder': !selectedLabel }"
        >
          {{ selectedLabel || placeholder || 'اختر…' }}
        </span>
        <i class="pi pi-chevron-down q-select-menu__chevron" aria-hidden="true" />
      </button>

      <div
        v-if="open"
        :id="listId"
        class="q-select-menu__panel"
        role="listbox"
        :aria-label="label || placeholder"
        @mousedown.stop
        @pointerdown.stop
      >
        <div v-if="filterable" class="q-select-menu__search">
          <i class="pi pi-search" aria-hidden="true" />
          <input
            ref="searchRef"
            v-model="query"
            type="search"
            class="q-select-menu__search-input"
            :placeholder="filterPlaceholder"
            autocomplete="off"
            @keydown.esc.stop="close"
          />
        </div>
        <ul class="q-select-menu__list">
          <li v-if="!filtered.length" class="q-select-menu__empty">
            {{ emptyMessage }}
          </li>
          <li
            v-for="opt in filtered"
            :key="String(opt.value)"
            role="option"
            class="q-select-menu__option"
            :class="{ 'is-selected': isSelected(opt.value) }"
            :aria-selected="isSelected(opt.value)"
            @pointerdown.prevent.stop="pick(opt.value)"
            @mousedown.prevent.stop="pick(opt.value)"
            @click.prevent.stop
          >
            <span>{{ opt.label }}</span>
            <i
              v-if="isSelected(opt.value)"
              class="pi pi-check"
              aria-hidden="true"
            />
          </li>
        </ul>
      </div>
    </div>
  </AppField>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import AppField from './AppField.vue'

export type AppSelectMenuOption = {
  label: string
  value: string | number | null | boolean
}

const props = withDefaults(
  defineProps<{
    modelValue?: string | number | null | boolean
    options: AppSelectMenuOption[]
    label?: string
    hint?: string
    error?: string
    required?: boolean
    grow?: boolean
    placeholder?: string
    disabled?: boolean
    filterable?: boolean
    filterPlaceholder?: string
    emptyMessage?: string
  }>(),
  {
    required: false,
    grow: false,
    disabled: false,
    filterable: true,
    filterPlaceholder: 'ابحث…',
    emptyMessage: 'لا نتائج',
  },
)

const emit = defineEmits<{
  'update:modelValue': [string | number | null | boolean]
}>()

const open = ref(false)
const query = ref('')
const rootRef = ref<HTMLElement | null>(null)
const searchRef = ref<HTMLInputElement | null>(null)
const listId = `q-select-menu-${Math.random().toString(36).slice(2, 9)}`
const listbox = 'listbox'
/** يمنع إعادة فتح القائمة بسبب نفس نقرة الاختيار. */
let ignoreToggleUntil = 0

const selectedLabel = computed(() => {
  const hit = props.options.find((o) => o.value === props.modelValue)
  return hit?.label ?? ''
})

const filtered = computed(() => {
  const q = query.value.trim().toLowerCase()
  if (!q) return props.options
  return props.options.filter((o) => o.label.toLowerCase().includes(q))
})

function isSelected(v: AppSelectMenuOption['value']) {
  return v === props.modelValue
}

function toggle() {
  if (props.disabled) return
  if (Date.now() < ignoreToggleUntil) return
  open.value = !open.value
  if (!open.value) query.value = ''
}

function close() {
  open.value = false
  query.value = ''
}

function pick(v: AppSelectMenuOption['value']) {
  if (props.disabled) return
  emit('update:modelValue', v)
  ignoreToggleUntil = Date.now() + 400
  close()
}

watch(open, async (v) => {
  if (v && props.filterable) {
    await nextTick()
    searchRef.value?.focus()
  }
})

function onDocPointer(e: Event) {
  if (!open.value || !rootRef.value) return
  if (Date.now() < ignoreToggleUntil) return
  if (!rootRef.value.contains(e.target as Node)) close()
}

onMounted(() => {
  document.addEventListener('pointerdown', onDocPointer, true)
  document.addEventListener('mousedown', onDocPointer, true)
})
onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', onDocPointer, true)
  document.removeEventListener('mousedown', onDocPointer, true)
})
</script>
