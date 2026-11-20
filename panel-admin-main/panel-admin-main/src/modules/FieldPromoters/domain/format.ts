import type { AttendanceStatus, OrderStage, PromoterStatus } from '../requests/fieldPromoters'

export const INTEREST_COLORS: Record<number, string> = {
  1: '#C62828',
  2: '#EF6C00',
  3: '#F9A825',
  4: '#7CB342',
  5: '#2E7D32',
}

export const INTEREST_LABELS: Record<number, string> = {
  1: 'غير مهتم إطلاقاً',
  2: 'قليل الاهتمام',
  3: 'مهتم نوعاً ما',
  4: 'مهتم',
  5: 'مهتم جداً',
}

export const STATUS_META: Record<PromoterStatus, { label: string; color: string; tone: string }> = {
  working: { label: 'في الدوام', color: '#2E7D32', tone: 'good' },
  stationary: { label: 'واقف', color: '#C62828', tone: 'danger' },
  notStarted: { label: 'لم يبدأ', color: '#8A9694', tone: 'muted' },
  ended: { label: 'أنهى الدوام', color: '#2f6fb0', tone: 'info' },
  suspended: { label: 'موقوف', color: '#6d675b', tone: 'dark' },
}

export const STAGE_LABELS: Record<OrderStage, string> = {
  inProgress: 'قيد المتابعة',
  sold: 'قيد التسديد',
  finished: 'مكتملة',
  rejected: 'مرفوضة',
}

export const ATTENDANCE_META: Record<AttendanceStatus, { label: string; tone: string }> = {
  Met: { label: 'حقّق', tone: 'good' },
  Missed: { label: 'لم يحقّق', tone: 'danger' },
  Off: { label: 'غائب', tone: 'muted' },
  Today: { label: 'اليوم', tone: 'info' },
  NoTarget: { label: 'بلا هدف', tone: 'plain' },
}

const numberFmt = new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 })

export function num(v: number | null | undefined): string {
  return v == null ? '—' : numberFmt.format(v)
}

export function money(v: number | null | undefined): string {
  return v == null ? '—' : `${numberFmt.format(v)} د.ع`
}

export function hm(minutes: number | null | undefined): string {
  if (minutes == null) return '—'
  const m = Math.max(0, Math.round(minutes))
  return `${Math.floor(m / 60)}:${String(m % 60).padStart(2, '0')}`
}

export function percent(part: number, total: number): number {
  return total > 0 ? Math.round((part / total) * 100) : 0
}

export function time(v: string | null | undefined): string {
  if (!v) return '—'
  const d = new Date(v)
  return Number.isNaN(d.getTime()) ? '—' : d.toLocaleTimeString('ar-IQ', { hour: 'numeric', minute: '2-digit', timeZone: 'Asia/Baghdad' })
}

export function dateLabel(v: string | null | undefined): string {
  if (!v) return '—'
  const d = new Date(v.length === 10 ? `${v}T12:00:00Z` : v)
  return Number.isNaN(d.getTime()) ? v : d.toLocaleDateString('en-GB', { timeZone: 'Asia/Baghdad' })
}

export function dateTime(v: string | null | undefined): string {
  if (!v) return '—'
  return `${dateLabel(v)} ${time(v)}`
}

export function dayHeader(v: string): { weekday: string; day: string } {
  const d = new Date(`${v}T12:00:00Z`)
  return {
    weekday: d.toLocaleDateString('ar-IQ', { weekday: 'short', timeZone: 'UTC' }),
    day: d.toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', timeZone: 'UTC' }),
  }
}

export function relative(v: string | null | undefined, now = Date.now()): string {
  if (!v) return '—'
  const t = new Date(v).getTime()
  if (Number.isNaN(t)) return '—'
  const mins = Math.floor((now - t) / 60000)
  if (mins < 1) return 'الآن'
  if (mins < 60) return `قبل ${mins} دقيقة`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `قبل ${hours} ساعة`
  const days = Math.floor(hours / 24)
  if (days < 30) return `قبل ${days} يوم`
  return dateLabel(v)
}

export function iraqToday(offsetDays = 0): string {
  const d = new Date(Date.now() + 3 * 3600_000 + offsetDays * 86400_000)
  return d.toISOString().slice(0, 10)
}

export function iraqDayOf(v: string): string {
  return new Date(Date.parse(v) + 3 * 3600_000).toISOString().slice(0, 10)
}

export function daysBetween(from: string, to: string): number {
  return Math.round((Date.parse(`${to}T00:00:00Z`) - Date.parse(`${from}T00:00:00Z`)) / 86400_000) + 1
}

export function escapeHtml(s: string | null | undefined): string {
  return String(s ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;')
}

export function routeId(raw: unknown): number | null {
  const n = Number(Array.isArray(raw) ? raw[0] : raw)
  return Number.isInteger(n) && n > 0 ? n : null
}

export function routeDate(raw: unknown): string | null {
  const v = String(Array.isArray(raw) ? raw[0] : raw ?? '')
  return /^\d{4}-\d{2}-\d{2}$/.test(v) ? v : null
}
