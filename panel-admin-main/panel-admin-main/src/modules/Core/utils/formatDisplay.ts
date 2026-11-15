/** Panel display numbers: always Western digits 0–9 (never Arabic-Indic). */

const LATN = 'en-US' as const

const moneyFmt = new Intl.NumberFormat(LATN, { maximumFractionDigits: 0 })
const numFmt = new Intl.NumberFormat(LATN, { maximumFractionDigits: 2 })
const intFmt = new Intl.NumberFormat(LATN, { maximumFractionDigits: 0 })

const ARABIC_INDIC = /[٠-٩۰-۹]/g

/** Force any Arabic-Indic digits in a string to 0–9. */
export function toLatinDigits(value: string): string {
  return value.replace(ARABIC_INDIC, (ch) => {
    const code = ch.charCodeAt(0)
    if (code >= 0x0660 && code <= 0x0669) return String(code - 0x0660)
    if (code >= 0x06f0 && code <= 0x06f9) return String(code - 0x06f0)
    return ch
  })
}

export function formatNumber(
  value: number | null | undefined,
  opts?: { maxFractionDigits?: number },
): string {
  if (value == null || Number.isNaN(Number(value))) return '—'
  const n = Number(value)
  if (opts?.maxFractionDigits != null) {
    return new Intl.NumberFormat(LATN, {
      maximumFractionDigits: opts.maxFractionDigits,
    }).format(n)
  }
  return Number.isInteger(n) ? intFmt.format(n) : numFmt.format(n)
}

export function formatMoney(value: number | null | undefined, suffix = 'د.ع'): string {
  if (value == null || Number.isNaN(Number(value))) return '—'
  const amount = moneyFmt.format(Math.round(Number(value)))
  return suffix ? `${amount} ${suffix}` : amount
}

export function formatMoneyPlain(value: number | null | undefined): string {
  return formatMoney(value, '')
}

export function formatDateTime(
  value: string | Date | null | undefined,
  opts?: Intl.DateTimeFormatOptions,
): string {
  if (value == null || value === '') return '—'
  try {
    const d = value instanceof Date ? value : new Date(value)
    if (Number.isNaN(d.getTime())) return toLatinDigits(String(value))
    return new Intl.DateTimeFormat(LATN, {
      dateStyle: 'medium',
      timeStyle: 'short',
      timeZone: 'Asia/Baghdad',
      ...opts,
    }).format(d)
  } catch {
    return toLatinDigits(String(value))
  }
}

export function formatDate(
  value: string | Date | null | undefined,
  opts?: Intl.DateTimeFormatOptions,
): string {
  if (value == null || value === '') return '—'
  try {
    const d = value instanceof Date ? value : new Date(value)
    if (Number.isNaN(d.getTime())) return toLatinDigits(String(value))
    return new Intl.DateTimeFormat(LATN, {
      dateStyle: 'medium',
      timeZone: 'Asia/Baghdad',
      ...opts,
    }).format(d)
  } catch {
    return toLatinDigits(String(value))
  }
}
