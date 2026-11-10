<template>
  <div class="sr-tabs-wrap">
    <div
      class="sr-tabs"
      role="tablist"
      aria-label="تبويبات سير العمل"
    >
      <button
        v-for="t in [...workflowTabs, ...extraViews]"
        :key="t.key"
        type="button"
        role="tab"
        class="sr-tab"
        :class="{
          active: modelValue === t.key,
          locked: isLocked(t),
        }"
        :aria-selected="modelValue === t.key"
        :disabled="isLocked(t) || t.implementationStatus === 'not_implemented'"
        :title="isLocked(t) ? 'لا صلاحية لهذا التبويب' : t.label"
        @click="!isLocked(t) && $emit('update:modelValue', t.key)"
      >
        <span
          class="sr-dot"
          :style="{ background: t.color }"
          aria-hidden="true"
        />
        <span class="sr-tab-label">{{ t.label }}</span><span v-if="t.implementationStatus === 'not_implemented'" class="sr-ni">غير متاح</span>
        <span
          v-if="countFor(t.key) != null"
          class="sr-count"
        >
          {{ countFor(t.key) }}
        </span>
      </button>
    </div>


  </div>
</template>

<script setup lang="ts">
import {
  EXTRA_VIEWS,
  WORKFLOW_TABS,
  isOverlappingView,
  type SalesRequestTabKey,
  type TabDefinition,
} from '../domain/tabs'

const props = defineProps<{
  modelValue: SalesRequestTabKey
  counts: Record<string, number>
  isLocked: (tab: TabDefinition) => boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [SalesRequestTabKey]
}>()

const workflowTabs = WORKFLOW_TABS
const extraViews = EXTRA_VIEWS

function isOverlapping(key: string): boolean {
  return isOverlappingView(key)
}

function countFor(key: string): number | null {
  if (!(key in props.counts)) return null
  return props.counts[key]
}

function onExtra(t: TabDefinition) {
  if (t.implementationStatus === 'not_implemented' || props.isLocked(t)) return
  emit('update:modelValue', t.key)
}
</script>

<style scoped>
.sr-tabs-wrap {
  display: flex;
  flex-wrap: wrap;
  align-items: stretch;
  gap: 0.5rem;
  max-width: 100%;
}

.sr-tabs {
  display: flex;
  gap: 0.35rem;
  flex-wrap: wrap;
  overflow-x: auto;
  overscroll-behavior-x: contain;
  max-width: 100%;
  padding-bottom: 0.15rem;
  scrollbar-width: thin;
  -webkit-overflow-scrolling: touch;
}

.sr-tab,
.sr-more-btn,
.sr-more-item {
  min-height: 44px;
  min-width: 44px;
  font-family: inherit;
  border-radius: 0.85rem;
  border: 1px solid #d7e2e0;
  background: #fff;
  color: #00201e;
  padding: 0.45rem 0.75rem;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  white-space: nowrap;
  transition: background 180ms ease, border-color 180ms ease, color 180ms ease;
}

.sr-tab:focus-visible,
.sr-more-btn:focus-visible,
.sr-more-item:focus-visible {
  outline: 2px solid #006a64;
  outline-offset: 2px;
}

.sr-tab.active {
  background: #006a64;
  border-color: #006a64;
  color: #fff;
}

.sr-tab.locked,
.sr-more-item.locked,
.sr-more-item.ni {
  opacity: 0.55;
  cursor: not-allowed;
}

.sr-dot {
  width: 0.55rem;
  height: 0.55rem;
  border-radius: 50%;
  flex-shrink: 0;
}

.sr-tab.active .sr-dot {
  box-shadow: 0 0 0 2px rgba(255, 255, 255, 0.45);
}

.sr-count {
  font-size: 0.75rem;
  opacity: 0.85;
  font-variant-numeric: tabular-nums;
}

.sr-extra {
  margin-inline-start: auto;
}

.sr-more {
  position: relative;
}

.sr-more-list {
  position: absolute;
  inset-inline-end: 0;
  top: calc(100% + 0.25rem);
  z-index: 20;
  min-width: 12rem;
  margin: 0;
  padding: 0.35rem;
  list-style: none;
  background: #fff;
  border: 1px solid #d7e2e0;
  border-radius: 0.85rem;
  box-shadow: 0 8px 24px rgba(0, 32, 30, 0.08);
}

.sr-more-item {
  width: 100%;
  justify-content: flex-start;
  border: none;
  border-radius: 0.65rem;
}

.sr-more-item.active {
  background: #e6f2f1;
}

.sr-ni {
  margin-inline-start: auto;
  font-size: 0.7rem;
  color: #7d6608;
}

@media (prefers-reduced-motion: reduce) {
  .sr-tab,
  .sr-more-btn,
  .sr-more-item {
    transition: none;
  }
}

@media (max-width: 767px) {
  .sr-tabs-wrap {
    flex-direction: column;
  }
  .sr-extra {
    margin-inline-start: 0;
  }
  .sr-tab-label {
    font-size: 0.85rem;
  }
}
</style>
