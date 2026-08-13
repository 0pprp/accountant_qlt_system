import type { ComposerTranslation } from 'vue-i18n'
import {
  ACTIVITY_TYPE_KEYS,
  TARGET_ENTITY_TYPE_KEYS,
  DEVICE_TYPE_KEYS,
  type ActivityType,
  type TargetEntityType,
  type DeviceType,
} from '../constants/enums'

type TranslateFn = ComposerTranslation

export function getActivityTypeLabel(
  value: ActivityType | number,
  t: TranslateFn
): string {
  const labelKey = ACTIVITY_TYPE_KEYS[value as ActivityType]
  return labelKey ? t(labelKey) : '-'
}

export function getTargetEntityTypeLabel(
  value: TargetEntityType | number,
  t: TranslateFn
): string {
  const labelKey = TARGET_ENTITY_TYPE_KEYS[value as TargetEntityType]
  return labelKey ? t(labelKey) : '-'
}

export function getDeviceTypeLabel(
  value: DeviceType | number,
  t: TranslateFn
): string {
  const labelKey = DEVICE_TYPE_KEYS[value as DeviceType]
  return labelKey ? t(labelKey) : '-'
}

export function buildEnumSelectOptions(
  keys: Record<number, string>,
  t: TranslateFn,
  allLabelKey: string
): Array<{ value: number | null; label: string }> {
  const options: Array<{ value: number | null; label: string }> = [
    {
      value: null,
      label: t(allLabelKey),
    },
  ]

  Object.entries(keys).forEach(([enumValue, labelKey]) => {
    options.push({
      value: Number(enumValue),
      label: t(labelKey),
    })
  })

  return options
}
