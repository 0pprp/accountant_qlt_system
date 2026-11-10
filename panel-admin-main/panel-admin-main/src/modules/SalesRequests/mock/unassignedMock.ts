import type {
  DuplicateRequestHit,
  MatchCard,
  MatchHit,
  UnassignedSalesRequest,
} from '../types'

/**
 * بيانات معاينة تصميم صريحة فقط — لا تُفعَّل تلقائيًا عند فشل API.
 * استيرادها يدويًا في وضع Development للمعاينة المعزولة عن العمليات الحية.
 */
export const MOCK_DESIGN_MODE = false

export const MOCK_UNASSIGNED: UnassignedSalesRequest[] = [
  {
    id: 1001,
    customerName: 'أحمد محمد علي',
    customerPhone: '07801234567',
    sourceType: 0,
    sourceTypeLabel: 'إدخال يدوي',
    branchId: 1,
    branchName: 'فرع المراجعة',
    provinceId: 1,
    provinceName: 'بغداد',
    createdAt: new Date().toISOString(),
    createdByName: 'مراجع النظام',
    statusLabel: 'غير مسند',
    status: 0, tabKey: 'unassigned', concurrencyStamp: 0,
  },
  {
    id: 1002,
    customerName: 'حسن',
    customerPhone: null,
    sourceType: 4,
    sourceTypeLabel: 'استيراد Excel',
    branchId: 1,
    branchName: 'فرع المراجعة',
    createdAt: new Date().toISOString(),
    statusLabel: 'غير مسند',
    status: 0, tabKey: 'unassigned', concurrencyStamp: 0,
  },
]

export const MOCK_CARDS: MatchCard[] = [
  {
    key: 'tripleName',
    label: 'تشابه الاسم الثلاثي',
    state: 'results',
    count: 2,
  },
  {
    key: 'phone',
    label: 'تشابه رقم الهاتف',
    state: 'noResults',
    count: 0,
  },
  {
    key: 'fatherGrandfather',
    label: 'تشابه اسم الأب والجد',
    state: 'results',
    count: 1,
  },
  {
    key: 'sonFather',
    label: 'تطابق ابن مع أب',
    state: 'insufficientData',
    count: null,
  },
]

export const MOCK_HITS: MatchHit[] = [
  {
    customerId: 501,
    customerName: 'أحمد محمد علي حسين',
    matchReason: 'تطابق الاسم الثلاثي: أحمد محمد علي',
    highlightedParts: ['أحمد', 'محمد', 'علي'],
    phone: '07801234567',
    branchName: 'فرع المراجعة',
    ratingLabel: 'جيد',
    sellTotal: 1500000,
    paidTotal: 400000,
    remaining: 1100000,
    paymentCount: 4,
    paymentDays: 45,
    saleDate: '2025-06-01',
    canOpenProfile: true,
  },
]

export const MOCK_DUPLICATES: DuplicateRequestHit[] = [
  {
    id: 900,
    sourceType: 5,
    sourceTypeLabel: 'CRM خارجي',
    statusLabel: 'مسند',
    createdAt: new Date().toISOString(),
    assignedEmployeeName: 'موظف تجريبي',
  },
]
