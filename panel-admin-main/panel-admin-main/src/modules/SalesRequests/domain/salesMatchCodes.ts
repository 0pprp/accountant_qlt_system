import type { MatchCriterion } from '../types'

const DIACRITICS = new Set(
  [...'ًٌٍَُِّْٰٕٔ'].map((c) => c.codePointAt(0)!)
)

/** تطبيع الاسم للمقارنة — يطابق SalesRequestNameSimilarity في الـAPI. */
export function normalizeName(value?: string | null): string {
  if (!value?.trim()) return ''
  const mapped = [...value.trim()]
    .map((c) => {
      const cp = c.codePointAt(0)!
      if (DIACRITICS.has(cp) || c === 'ـ') return ''
      if (/[!"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~،؛؟«»]/.test(c)) return ''
      if ('أإآٱ'.includes(c)) return 'ا'
      if (c === 'ى') return 'ي'
      if (c === 'ة') return 'ه'
      return c
    })
    .join('')
  return mapped.split(/\s+/).filter(Boolean).join(' ')
}

export function matchingTokens(value?: string | null): string[] {
  const normalized = normalizeName(value)
  if (!normalized) return []
  const parts = normalized.split(' ').filter(Boolean)
  while (parts.length && /^\d+$/.test(parts[parts.length - 1]!)) parts.pop()
  return parts
}

export function tripleParts(value?: string | null): {
  first: string
  father: string
  grandfather: string
} {
  const t = matchingTokens(value)
  return {
    first: t[0] ?? '',
    father: t[1] ?? '',
    grandfather: t[2] ?? '',
  }
}

/** تطبيع الهاتف للمقارنة — يطابق SalesPhoneNormalizer.ForMatch. */
export function phoneForMatch(value?: string | null): string {
  let digits = digitsOnly(value)
  if (!digits) return ''
  if (digits.startsWith('00964') && digits.length >= 15) digits = digits.slice(5)
  else if (digits.startsWith('964') && digits.length >= 13) digits = digits.slice(3)
  if (digits.startsWith('0') && digits.length === 11) digits = digits.slice(1)
  if (digits.length === 10 && digits.startsWith('7')) return digits
  return ''
}

function digitsOnly(value?: string | null): string {
  if (!value?.trim()) return ''
  const trimmed = value.trim()
  if (/e/i.test(trimmed) && trimmed.includes('.')) {
    const n = Number(trimmed)
    if (Number.isFinite(n) && n >= 7_000_000_000 && n <= 7_999_999_999) {
      return String(Math.trunc(n))
    }
  }
  return trimmed.replace(/\D/g, '')
}

export interface ComparisonCode {
  criterion: MatchCriterion
  label: string
  code: string
  ready: boolean
  hint: string
}

/** أكواد المقارنة القابلة للنسخ/اللصق من بيانات الطلب في النظام. */
export function buildComparisonCodes(
  customerName?: string | null,
  customerPhone?: string | null
): ComparisonCode[] {
  const { first, father, grandfather } = tripleParts(customerName)
  const phone = phoneForMatch(customerPhone)
  const triple = [first, father, grandfather].filter(Boolean).join(' · ')
  const fg = [father, grandfather].filter(Boolean).join(' · ')
  const sonFather = [father, grandfather].filter(Boolean).join(' · ')

  return [
    {
      criterion: 'tripleName',
      label: 'تشابه الاسم الثلاثي',
      code: triple,
      ready: matchingTokens(customerName).length >= 3,
      hint: 'الاسم · الأب · الجد',
    },
    {
      criterion: 'phone',
      label: 'تشابه رقم الهاتف',
      code: phone,
      ready: phone.length === 10,
      hint: '10 أرقام تبدأ بـ 7',
    },
    {
      criterion: 'fatherGrandfather',
      label: 'تشابه اسم الأب والجد',
      code: fg,
      ready: father.length > 0 && grandfather.length > 0,
      hint: 'الأب · الجد',
    },
    {
      criterion: 'sonFather',
      label: 'تطابق ابن مع أب',
      code: sonFather,
      ready: father.length > 0 && grandfather.length > 0,
      hint: 'يُقارن أب/جد الطلب مع اسم/أب المرشّح',
    },
  ]
}

export async function copyText(value: string): Promise<boolean> {
  const text = value.trim()
  if (!text) return false
  try {
    if (navigator.clipboard?.writeText) {
      await navigator.clipboard.writeText(text)
      return true
    }
  } catch {
    /* fall through */
  }
  try {
    const el = document.createElement('textarea')
    el.value = text
    el.setAttribute('readonly', '')
    el.style.position = 'fixed'
    el.style.insetInlineStart = '-9999px'
    document.body.appendChild(el)
    el.select()
    const ok = document.execCommand('copy')
    document.body.removeChild(el)
    return ok
  } catch {
    return false
  }
}
