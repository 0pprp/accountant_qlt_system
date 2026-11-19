import { toLatinDigits } from '@/modules/Core/utils/formatDisplay'

export function waiverAmountDigits(raw: string): string {
  return toLatinDigits(raw ?? '').replace(/\D/g, '').replace(/^0+(?=\d)/, '')
}

export function formatWaiverAmountInput(raw: string): string {
  const digits = waiverAmountDigits(raw)
  if (!digits) return ''
  return digits.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
}

export function parseWaiverAmount(raw: string): number | null {
  const digits = waiverAmountDigits(raw)
  if (!digits) return null
  const n = Number(digits)
  return Number.isSafeInteger(n) && n > 0 ? n : null
}
