<template>
  <div class="q-data-field" :class="{ phone: isPhone }">
    <dt class="q-data-field__label">{{ label }}</dt>
    <dd class="q-data-field__value">
      <a
        v-if="isPhone && display !== emptyLabel"
        class="q-data-field__phone"
        :href="`tel:${raw}`"
        dir="ltr"
        >{{ display }}</a
      >
      <span v-else :dir="isPhone ? 'ltr' : undefined">{{ display }}</span>
    </dd>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    label: string
    value?: string | number | null
    isPhone?: boolean
    emptyLabel?: string
  }>(),
  { emptyLabel: 'غير مسجل', isPhone: false },
)

const raw = computed(() => (props.value == null ? '' : String(props.value).trim()))
const display = computed(() => (raw.value ? raw.value : props.emptyLabel))
</script>

<style scoped>
.q-data-field {
  min-width: 0;
}
.q-data-field__label {
  margin: 0 0 5px;
  color: #61756f;
  font-size: 14px;
}
.q-data-field__value {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  line-height: 1.75;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
  color: #00201e;
}
.q-data-field__phone {
  color: #006a64;
  text-decoration: none;
  font-family: system-ui, sans-serif;
  font-size: 18px;
  padding-block: 4px;
  display: inline-block;
  min-height: 44px;
  line-height: 36px;
}
.q-data-field__phone:focus-visible {
  outline: 3px solid #006a64;
  outline-offset: 2px;
}
</style>
