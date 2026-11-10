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
