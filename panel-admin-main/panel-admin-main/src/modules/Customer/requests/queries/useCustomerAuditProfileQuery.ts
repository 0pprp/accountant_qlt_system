import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export const CUSTOMER_AUDIT_PROFILE_KEY = 'customer-audit-profile'

export interface CustomerAuditProfile {
  header: {
    id: number
    fullName: string
    phone?: string | null
    branchName?: string | null
    statusLabel?: string | null
  }
  availableSections: string[]
  schemaRequiredSections: string[]
  notImplementedSections?: string[]
  partialSections?: string[]
  overview?: {
    motherName?: string | null
    nationalCode?: string | null
    birthDate?: string | null
    address?: string | null
    businessType?: string | null
    ratingLabel?: string | null
    sellTotal?: number | null
    paidTotal?: number | null
    remaining?: number | null
  } | null
  accounts?: {
    rows: Array<{
      orderId: number
      sellAmount?: number | null
      paidAmount?: number | null
      remaining?: number | null
      payments: Array<{
        id: number
        date: string
        amount: number
        receiptNumber?: string | null
        note?: string | null
      }>
    }>
  } | null
  sales?: Array<{
    id: number
    createdAt: string
    sellerName?: string | null
    orderListName?: string | null
    sellAmount?: number | null
    prepaymentAmount?: number | null
    products: string[]
  }> | null
  documents?: {
    customerDocuments: Array<{
      id: number
      ownerType: string
      ownerId?: number | null
      originalFileName: string
      relativePath?: string | null
      type?: string | null
    }>
    saleDocuments: Array<{
      id: number
      ownerType: string
      ownerId?: number | null
      originalFileName: string
      relativePath?: string | null
      type?: string | null
    }>
  } | null
  ratingsAndLocation?: CustomerAuditRatingsAndLocation | null
}

export interface CustomerAuditStaffRating {
  role: string
  title: string
  score?: number | null
  note?: string | null
  ratedByName?: string | null
  ratedAtUtc?: string | null
  orderId?: number | null
  shopStockEstimatedValue?: number | null
  estimatedDailyRevenue?: number | null
  contextAmount?: number | null
  occasion?: string | null
  occasionLabel?: string | null
  isWrongSale?: boolean | null
}

export interface CustomerAuditShopSurvey {
  title: string
  shopName?: string | null
  businessType?: string | null
  stockEstimatedValue?: number | null
  estimatedDailyRevenue?: number | null
  shopLength?: number | null
  shopWidth?: number | null
  shopArea?: number | null
  latitude?: number | null
  longitude?: number | null
  photoId?: number | null
  createdAtUtc?: string | null
}

export interface CustomerAuditRatingsAndLocation {
  delegateRating?: CustomerAuditStaffRating | null
  committeeFieldRating?: CustomerAuditStaffRating | null
  shopSurvey?: CustomerAuditShopSurvey | null
  gpsLatitude?: number | null
  gpsLongitude?: number | null
  hasGps: boolean
  latestOrderLatitude?: number | null
  latestOrderLongitude?: number | null
  snapshots?: CustomerAuditRatingSnapshot[] | null
  locations?: CustomerAuditLocation[] | null
  photos?: CustomerAuditPhoto[] | null
}

export interface CustomerAuditRatingSnapshot {
  origin: string
  ratingLabel?: string | null
  score?: number | null
  captureDate: string
}

export interface CustomerAuditLocation {
  source: string
  sourceLabel: string
  latitude: number
  longitude: number
  accuracyMeters?: number | null
  confidence?: number | null
  capturedAtUtc?: string | null
}

export interface CustomerAuditPhoto {
  id: number
  contentType?: string | null
  capturedAtUtc?: string | null
}

export function fetchCustomerAuditProfile(
  customerId: number,
  sections: string
) {
  return axiosInstance
    .get<CustomerAuditProfile>(
      `/admin/Customers/${customerId}/audit-profile`,
      { params: { sections } }
    )
    .then((r) => r.data)
}

export function useCustomerAuditProfileQuery(
  customerId: Ref<number>,
  sections: Ref<string>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [CUSTOMER_AUDIT_PROFILE_KEY, customerId, sections],
    queryFn: () =>
      fetchCustomerAuditProfile(customerId.value, sections.value),
    enabled,
  })
}
