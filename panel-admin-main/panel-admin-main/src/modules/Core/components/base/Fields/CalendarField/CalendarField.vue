<template>
  <div class="flex flex-col">
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <DatePicker
      v-if="timeOnly"
      v-model="localDate"
      :placeholder="$t(placeholder)"
      name="name"
      :required
      :id="uniqueId"
      hourFormat="24"
      fluid
      timeOnly
      :unstyled="false"
      :readonly
      class="!text-black"
      @show="setCurrentTime"
    />

    <DatePicker
      v-else
      v-model="localDate"
      :placeholder="$t(placeholder)"
      name="name"
      :required
      :id="uniqueId"
      dateFormat="yy/mm/dd"
      :unstyled="false"
      :readonly
      @date-select="onDateChange"
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { DatePicker, Message } from 'primevue'
import type { Props, Emits } from './CalendarField.types'
import useUid from '@/modules/Core/composable/useCurrentInstance'

const props = defineProps<Props>()
defineEmits<Emits>()

const formattedDate = defineModel<string>('modelValue')

const uniqueId = computed(() =>
  props.timeOnly ? `time-${useUid()}` : `date-${useUid()}`
)

const localDate = ref<Date | null>(null)

if (formattedDate.value) {
  if (props.timeOnly) {
    const [hours, minutes] = formattedDate.value.split(':').map(Number)
    const today = new Date()
    localDate.value = new Date(
      today.getFullYear(),
      today.getMonth(),
      today.getDate(),
      hours,
      minutes
    )
  } else {
    const [year, month, day] = formattedDate.value.split('-').map(Number)
    localDate.value = new Date(year, month - 1, day)
  }
}
watch(formattedDate, (newValue) => {
  if (newValue) {
    if (props.timeOnly) {
      const [hours, minutes] = newValue.split(':').map(Number)
      const today = new Date()
      localDate.value = new Date(
        today.getFullYear(),
        today.getMonth(),
        today.getDate(),
        hours,
        minutes
      )
    } else {
      const [year, month, day] = newValue.split('-').map(Number)
      localDate.value = new Date(year, month - 1, day)
    }
  } else {
    localDate.value = null
  }
})

watch(localDate, (newValue) => {
  if (newValue) {
    if (props.timeOnly) {
      const hours = String(newValue.getHours()).padStart(2, '0')
      const minutes = String(newValue.getMinutes()).padStart(2, '0')
      formattedDate.value = `${hours}:${minutes}`
    } else {
      const year = newValue.getFullYear()
      const month = String(newValue.getMonth() + 1).padStart(2, '0')
      const day = String(newValue.getDate()).padStart(2, '0')
      formattedDate.value = `${year}-${month}-${day}`
    }
  } else {
    formattedDate.value = ''
  }
})

function onDateChange(value: Date) {
  if (value) {
    const year = value.getFullYear()
    const month = String(value.getMonth() + 1).padStart(2, '0')
    const day = String(value.getDate()).padStart(2, '0')

    formattedDate.value = `${year}-${month}-${day}`
  } else {
    formattedDate.value = ''
  }
}
function setCurrentTime() {
  if (!localDate.value) {
    const now = new Date()
    localDate.value = new Date(
      now.getFullYear(),
      now.getMonth(),
      now.getDate(),
      now.getHours(),
      now.getMinutes()
    )
  }
}
</script>
