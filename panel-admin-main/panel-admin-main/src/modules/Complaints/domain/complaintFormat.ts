import { formatDate } from '@/modules/Core/utils/formatDisplay'

const ROLE_LABELS: Readonly<Record<string, string>> = {
  admin: 'المدير',
  chiefaccountant: 'مسؤول المحاسبين',
  mainaccountant: 'المحاسب الرئيسي',
  branchaccountant: 'المحاسب الفرعي',
  branchmanager: 'مدير الفرع',
  ceo: 'المدير التنفيذي',
  mandob: 'مندوب',
  motaba: 'متابع',
  director: 'المدير',
  monitor: 'المراقب',
  salesmanager: 'مدير مبيعات',
  archiveofficer: 'مسؤول الأرشيف',
}

export function complaintSourceLabel(sourceApp: string | null | undefined, senderRole?: string | null): string {
  const app = (sourceApp ?? '').trim().toLowerCase()
  if (app.includes('delegate')) return 'مندوب'
  if (app.includes('follower')) return 'متابع'
  if (app.includes('sales') || app.includes('employee')) return 'موظف مبيعات'
  if (app.includes('filter')) return 'موظف فلترة'
  if (app.includes('committee')) {
    const role = (senderRole ?? '').trim().toLowerCase()
    if (role === 'mandob') return 'مندوب'
    if (role === 'motaba') return 'متابع'
    return 'تطبيق اللجنة'
  }
  if (app === 'complaint') return 'شكوى'
  if (app === 'message') return 'رسالة'
  return 'غير متوفر'
}

export function complaintRoleLabel(role: string | null | undefined): string {
  const raw = (role ?? '').trim()
  if (!raw) return 'غير متوفر'
  return ROLE_LABELS[raw.toLowerCase()] ?? raw
}

export function complaintRelativeTime(value: string | null | undefined, now: Date = new Date()): string {
  if (!value) return '—'
  const at = new Date(value)
  if (Number.isNaN(at.getTime())) return '—'
  const minutes = Math.floor((now.getTime() - at.getTime()) / 60000)
  if (minutes < 1) return 'الآن'
  if (minutes < 60) return `قبل ${minutes} دقيقة`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `قبل ${hours} ساعة`
  const days = Math.floor(hours / 24)
  if (days < 30) return `قبل ${days} يوم`
  return formatDate(at)
}
