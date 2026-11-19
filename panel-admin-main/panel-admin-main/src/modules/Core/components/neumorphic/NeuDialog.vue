<template>
  <Dialog
    :visible="visible"
    modal
    :show-header="false"
    :dismissable-mask="!busy"
    :close-on-escape="!busy"
    :pt="pt"
    :pt-options="{ mergeSections: true, mergeProps: true }"
    :style="{ width: widths[size] }"
    @update:visible="!$event && !busy && emit('close')"
  >
    <div class="dfb-dialog-shell" dir="rtl">
      <header class="dfb-dialog-head">
        <div class="dfb-dialog-titles">
          <h2 class="dfb-dialog-title">{{ title }}</h2>
          <p v-if="subtitle" class="dfb-dialog-sub">{{ subtitle }}</p>
        </div>
        <button type="button" class="dfb-icon-button" aria-label="إغلاق" :disabled="busy" @click="emit('close')">
          <svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true"><path d="M6 6l12 12M18 6L6 18" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" /></svg>
        </button>
      </header>
      <div v-if="$slots.summary" class="dfb-kpis">
        <slot name="summary" />
      </div>
      <div v-if="$slots.toolbar" class="dfb-dialog-toolbar">
        <slot name="toolbar" />
      </div>
      <div class="dfb-dialog-body">
        <slot />
      </div>
      <footer v-if="$slots.footer" class="dfb-dialog-foot">
        <slot name="footer" />
      </footer>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import './neumorphic.css'

withDefaults(defineProps<{
  visible: boolean
  title: string
  subtitle?: string
  size?: 'sm' | 'md' | 'lg' | 'xl'
  busy?: boolean
}>(), { size: 'md', subtitle: undefined, busy: false })

const emit = defineEmits<{ close: [] }>()

const widths = {
  sm: 'min(520px, 96vw)',
  md: 'min(680px, 96vw)',
  lg: 'min(1000px, 96vw)',
  xl: 'min(1320px, 98vw)',
}

const pt = {
  root: { class: 'dfb-dialog' },
  mask: { class: 'dfb-dialog-mask' },
  content: { class: 'dfb-dialog-content' },
}
</script>
