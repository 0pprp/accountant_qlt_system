import type { PermissionModule } from '@/modules/Auth/types/model'

export interface StepItem {
  title: string
  icon: string
  value: string | number
  enTitle?: string
  isLastStep: boolean
  permission?: PermissionModule
}

export interface Props {
  steps: StepItem[]
  modelValue?: number | string
  activeStep?: number
}

export interface Emits {
  (event: 'update:activeStep', value: number | string): void
}
