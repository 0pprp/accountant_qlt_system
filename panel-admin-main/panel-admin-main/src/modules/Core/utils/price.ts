/**
 * الدينار العراقي بلا كسور — لا فاصلة عشرية في أي شاشة.
 * يُدوَّر الرقم إلى دينار صحيح ثم تُوضع فواصل الآلاف على الجزء الصحيح وحده.
 */
export function formattedPrice(price: number | string | null | undefined): string {
  const raw = typeof price === 'number' ? price : Number(String(price ?? '').replace(/,/g, ''))
  if (!Number.isFinite(raw)) return '0'

  const rounded = Math.round(raw)
  const sign = rounded < 0 ? '-' : ''

  return sign + Math.abs(rounded).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',')
}
