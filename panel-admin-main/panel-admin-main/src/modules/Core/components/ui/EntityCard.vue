<template>
  <article class="q-entity-card" :class="{ selected }" :aria-selected="selected || undefined">
    <header class="q-entity-card__top">
      <div class="q-entity-card__identity">
        <span v-if="eyebrow" class="q-entity-card__eyebrow">{{ eyebrow }}</span>
        <h3 class="q-entity-card__name">{{ name }}</h3>
        <slot name="identity-extra" />
      </div>
      <div v-if="$slots.badge || statusLabel" class="q-entity-card__badge">
        <slot name="badge">
          <StatusBadge v-if="statusLabel" :label="statusLabel" :tone="statusTone" />
        </slot>
      </div>
    </header>
    <div v-if="$slots.fields" class="q-entity-card__fields">
      <slot name="fields" />
    </div>
    <div v-if="$slots.notes" class="q-entity-card__notes">
      <slot name="notes" />
    </div>
    <div v-if="$slots.actions" class="q-entity-card__actions">
      <slot name="actions" />
    </div>
    <slot />
  </article>
</template>

<script setup lang="ts">
import StatusBadge from './StatusBadge.vue'

withDefaults(
  defineProps<{
    name: string
    eyebrow?: string
    statusLabel?: string
    statusTone?: 'neutral' | 'warning' | 'danger' | 'success' | 'info'
    selected?: boolean
  }>(),
  { statusTone: 'neutral', selected: false },
)
</script>

<style scoped>
.q-entity-card {
  background: #fff;
  border: 1px solid #dce7e5;
  border-radius: 18px;
  padding: 24px;
  color: #00201e;
  border-inline-start: 4px solid #dce7e5;
  overflow-wrap: anywhere;
}
.q-entity-card.selected {
  border-color: #006a64;
  border-inline-start-color: #006a64;
}
.q-entity-card__top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}
.q-entity-card__eyebrow {
  color: #526b67;
  font-size: 14px;
}
.q-entity-card__name {
  margin: 6px 0;
  font-size: 20px;
  line-height: 1.65;
  font-weight: 700;
}
.q-entity-card__fields {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px 24px;
  margin-top: 16px;
}
.q-entity-card__notes {
  margin-top: 14px;
}
.q-entity-card__actions {
  margin-top: 16px;
}
@media (max-width: 640px) {
  .q-entity-card {
    padding: 16px;
  }
  .q-entity-card__fields {
    grid-template-columns: 1fr;
  }
  .q-entity-card__top {
    flex-wrap: wrap;
  }
}
</style>
