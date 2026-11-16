<template>
  <div class="tab-switch-wrap">
    <button
      type="button"
      role="switch"
      class="tab-switch"
      :class="{
        'tab-switch--open': enabled,
        'tab-switch--closed': !enabled,
        'tab-switch--disabled': disabled,
      }"
      :aria-checked="enabled"
      :aria-label="label"
      :disabled="disabled"
      @click="onToggle"
      @keydown.space.prevent="onToggle"
    >
      <span class="tab-switch__track" aria-hidden="true">
        <span class="tab-switch__thumb">
          <i
            class="pi"
            :class="enabled ? 'pi-check' : 'pi-lock'"
            aria-hidden="true"
          />
        </span>
      </span>
      <span class="tab-switch__text">{{ enabled ? 'مفتوح' : 'مقفول' }}</span>
    </button>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  label: string
  enabled: boolean
  disabled?: boolean
}>()

const emit = defineEmits<{
  toggle: [next: boolean]
}>()

function onToggle() {
  if (props.disabled) return
  emit('toggle', !props.enabled)
}
</script>

<style scoped>
.tab-switch-wrap {
  display: inline-flex;
  align-items: center;
}

.tab-switch {
  --track-w: 48px;
  --track-h: 28px;
  --thumb: 22px;
  display: inline-flex;
  align-items: center;
  gap: 0.65rem;
  min-width: 48px;
  min-height: 44px;
  padding: 0.35rem 0.55rem;
  border: none;
  border-radius: 12px;
  background: transparent;
  color: #00201e;
  font: inherit;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}

.tab-switch:focus-visible {
  outline: 2px solid #006a64;
  outline-offset: 3px;
}

.tab-switch--disabled {
  opacity: 0.55;
  cursor: default;
}

.tab-switch__track {
  position: relative;
  width: var(--track-w);
  height: var(--track-h);
  border-radius: 999px;
  flex-shrink: 0;
  box-shadow: inset 2px 2px 4px rgba(0, 32, 30, 0.12),
    inset -1px -1px 3px rgba(255, 255, 255, 0.7);
  transition: background-color 0.16s ease;
}

.tab-switch--closed .tab-switch__track {
  background: #e4e9e8;
  border: 1px solid #9aa8a5;
}

.tab-switch--open .tab-switch__track {
  background: #006a64;
  border: 1px solid #006a64;
}

.tab-switch__thumb {
  position: absolute;
  top: 2px;
  /* RTL: المفتوح يدفع المقبض لليسار (بداية السطر)، المقفول لليمين */
  inset-inline-start: 2px;
  width: var(--thumb);
  height: var(--thumb);
  border-radius: 50%;
  display: grid;
  place-items: center;
  background: #f8f8f8;
  color: #526b67;
  box-shadow: 2px 2px 5px rgba(0, 32, 30, 0.18),
    -1px -1px 3px rgba(255, 255, 255, 0.85);
  transition: inset-inline-start 0.16s ease, color 0.16s ease;
  font-size: 0.65rem;
}

.tab-switch--open .tab-switch__thumb {
  inset-inline-start: calc(var(--track-w) - var(--thumb) - 2px);
  color: #006a64;
}

.tab-switch--open .tab-switch__text {
  color: #006a64;
}

.tab-switch--closed .tab-switch__text {
  color: #526b67;
}

@media (prefers-reduced-motion: reduce) {
  .tab-switch__track,
  .tab-switch__thumb {
    transition: none;
  }
}
</style>
