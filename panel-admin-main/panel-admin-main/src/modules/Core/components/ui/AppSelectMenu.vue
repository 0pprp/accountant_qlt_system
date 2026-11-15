<template>
  <AppField :label="label" :hint="hint" :error="error" :required="required" :grow="grow">
    <div ref="rootRef" class="q-select-menu" :class="{ 'is-open': open, 'is-disabled': disabled }">
      <button
        type="button"
        class="q-select-menu__trigger"
        :disabled="disabled"
        :aria-expanded="open"
        :aria-haspopup="listbox"
        :aria-controls="listId"
        @click="toggle"
      >
        <span class="q-select-menu__value" :class="{ 'is-placeholder': !selectedLabel }">
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
          <li v-if="!filtered.length" class="q-select-menu__empty">{{ emptyMessage }}</li>
          <li
            v-for="opt in filtered"
            :key="String(opt.value)"
            role="option"
            class="q-select-menu__option"
            :class="{ 'is-selected': isSelected(opt.value) }"
            :aria-selected="isSelected(opt.value)"
            @click="pick(opt.value)"
          >
            <span>{{ opt.label }}</span>
            <i v-if="isSelected(opt.value)" class="pi pi-check" aria-hidden="true" />
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

const emit = defineEmits<{ 'update:modelValue': [string | number | null | boolean] }>()

const open = ref(false)
const query = ref('')
const rootRef = ref<HTMLElement | null>(null)
const searchRef = ref<HTMLInputElement | null>(null)
const listId = `q-select-menu-${Math.random().toString(36).slice(2, 9)}`
const listbox = 'listbox'

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
  open.value = !open.value
}

function close() {
  open.value = false
  query.value = ''
}

function pick(v: AppSelectMenuOption['value']) {
  emit('update:modelValue', v)
  close()
}

watch(open, async (v) => {
  if (v && props.filterable) {
    await nextTick()
    searchRef.value?.focus()
  }
})

function onDocPointer(e: MouseEvent) {
  if (!open.value || !rootRef.value) return
  if (!rootRef.value.contains(e.target as Node)) close()
}

onMounted(() => document.addEventListener('mousedown', onDocPointer))
onBeforeUnmount(() => document.removeEventListener('mousedown', onDocPointer))
</script>

<style scoped>
.q-select-menu {
  position: relative;
  width: 100%;
}
.q-select-menu__trigger {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  width: 100%;
  min-height: 48px;
  padding: 10px 14px;
  border: 1px solid #cddbd7;
  border-radius: 12px;
  background: #fff;
  color: #00201e;
  font: inherit;
  font-size: 16px;
  cursor: pointer;
  text-align: start;
  transition: border-color 0.15s, box-shadow 0.15s, background 0.15s;
}
.q-select-menu.is-open .q-select-menu__trigger,
.q-select-menu__trigger:focus-visible {
  outline: none;
  border-color: #006a64;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(0, 106, 100, 0.18);
}
.q-select-menu__trigger:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.q-select-menu__value {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.q-select-menu__value.is-placeholder {
  color: #8a9b99;
}
.q-select-menu__chevron {
  color: #006a64;
  font-size: 12px;
  transition: transform 0.15s;
}
.q-select-menu.is-open .q-select-menu__chevron {
  transform: rotate(180deg);
}
.q-select-menu__panel {
  position: absolute;
  z-index: 40;
  inset-inline: 0;
  top: calc(100% + 6px);
  background: #fff;
  border: 1px solid #d5e2e0;
  border-radius: 16px;
  box-shadow: 0 16px 42px rgba(0, 32, 30, 0.12), 0 3px 8px rgba(0, 32, 30, 0.05);
  overflow: hidden;
}
.q-select-menu__search {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 10px 10px 6px;
  padding: 0 12px;
  min-height: 44px;
  border-radius: 10px;
  background: #f8f8f8;
  border: 1px solid transparent;
}
.q-select-menu__search:focus-within {
  border-color: #006a64;
  background: #fff;
}
.q-select-menu__search .pi-search {
  color: #006a64;
  font-size: 13px;
}
.q-select-menu__search-input {
  flex: 1;
  min-width: 0;
  border: 0 !important;
  background: transparent !important;
  box-shadow: none !important;
  outline: none !important;
  min-height: 42px !important;
  padding: 0 !important;
  font: inherit;
  font-size: 15px;
  color: #00201e;
}
.q-select-menu__list {
  list-style: none;
  margin: 0;
  padding: 6px;
  max-height: min(280px, 50vh);
  overflow: auto;
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.q-select-menu__option {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  min-height: 44px;
  padding: 10px 12px;
  border-radius: 10px;
  border: 0;
  background: transparent;
  color: #00201e;
  cursor: pointer;
  font-size: 16px;
  line-height: 1.45;
}
.q-select-menu__option:hover {
  background: #eef6f4;
}
.q-select-menu__option.is-selected {
  background: #e7f3f1;
  color: #006a64;
  font-weight: 700;
}
.q-select-menu__option .pi-check {
  color: #006a64;
  font-size: 13px;
}
.q-select-menu__empty {
  padding: 18px 12px;
  text-align: center;
  color: #5a6b6a;
}
@media (prefers-reduced-motion: reduce) {
  .q-select-menu__trigger,
  .q-select-menu__chevron {
    transition: none;
  }
}
</style>
