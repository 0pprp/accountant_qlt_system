<template>
  <div v-if="activeStep !== undefined">
    <Stepper v-model:value="activeStep" class="pt-5" :unstyled="false">
      <StepList
        class="flex flex-row flex-auto gap-2 !justify-center"
        :unstyled="false"
      >
        <Step
          v-for="(step, index) in steps"
          :key="index"
          v-slot="{ value, a11yAttrs }"
          asChild
          :unstyled="false"
          :value="step.value"
          class="flex justify-center rrrr"
        >
          <div v-bind="a11yAttrs.root">
            <button
              class="bg-transparent border-0 inline-flex !justify-center items-center flex-col gap-2"
              v-bind="a11yAttrs.header"
              :disabled="!isStepClickable(value)"
              :class="{
                'cursor-pointer': isStepClickable(value),
                'cursor-not-allowed': !isStepClickable(value),
              }"
            >
              <span
                :class="[
                  'rounded-full border-2 w-12 h-12 inline-flex items-center justify-center',
                  {
                    'bg-primary text-primary-contrast border-primary':
                      value <= activeStep,
                    'border-surface-3 bg-surface-3': value > activeStep,
                  },
                ]"
              >
                <SvgIcon
                  :name="step.icon"
                  :class="[
                    'w-5.5',
                    {
                      'text-white': value <= activeStep,
                      'text-gray-500': value > activeStep,
                    },
                  ]"
                />
              </span>
              <span
                :class="[
                  'w-12 h-12',
                  {
                    'text-gray-900': value <= activeStep,
                    'text-gray-700': value > activeStep,
                  },
                ]"
                >{{ step.title }}</span
              >
            </button>
          </div>
          <Divider
            v-if="!step.isLastStep"
            :unstyled="false"
            class="!mb-[5rem] before:!bg-primary before:h-[4px] before:rounded !w-[100px]"
          />
        </Step>
      </StepList>
    </Stepper>
    <slot name="content" />
  </div>
</template>

<script setup lang="ts">
import { Divider, Step, StepList, Stepper } from 'primevue'
import SvgIcon from '../../SvgIcon/SvgIcon.vue'
import type { Props } from './CustomStepper.types'

defineProps<Props>()

const activeStep = defineModel<number | string>('modelValue')

function isStepClickable(stepValue: number | string): boolean {
  return stepValue <= activeStep.value!
}
</script>
