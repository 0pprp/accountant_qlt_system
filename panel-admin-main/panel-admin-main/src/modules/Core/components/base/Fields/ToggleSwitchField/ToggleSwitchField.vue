<template>
  <div
    class="flex flex-row justify-between rounded-2xl p-2 gap-4 items-center switch-box"
  >
    <div class="flex flex-row gap-1 items-center">
      <SvgIcon v-if="iconName" :name="iconName" :class="`text-${color}`" />

      <label v-if="label" class="text-gray-700 font-bold">
        {{ label }}
      </label>
    </div>

    <ToggleSwitch
      v-model="localValue"
      :unstyled="false"
      :disabled="readonly || disabled"
    />
  </div>
</template>

<script setup lang="ts">
import { ToggleSwitch } from 'primevue'
import type { Props, Emits } from './ToggleSwitchField.types'
import SvgIcon from '../../../SvgIcon/SvgIcon.vue'

defineProps<Props>()
defineEmits<Emits>()

const localValue = defineModel<Props['modelValue']>('modelValue')
</script>

<style scoped>
.switch-box {
  border: 1px solid var(--color-line);
  background: #fff !important;
}
</style>
