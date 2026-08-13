// src/modules/Sale/requests/mutations/useSaleExcelReportMutation.ts
import { type UseMutationOptions, useMutation } from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export interface SaleExcelReportParams {
  branchId: number
  searchTerm?: Array<string> | null
}

const ENDPOINT = '/admin/Orders/excel-report'

export function downloadSaleExcelReport(
  params: SaleExcelReportParams
): Promise<AxiosResponse<Blob>> {
  return axiosInstance.get<Blob>(ENDPOINT, {
    params: {
      branchId: params.branchId,
      searchTerm: params.searchTerm,
    },
    responseType: 'blob',
    paramsSerializer: (params) => {
      const searchParams = new URLSearchParams()

      Object.entries(params).forEach(([key, value]) => {
        if (Array.isArray(value)) {
          value.forEach((item) => {
            if (item !== null && item !== undefined) {
              searchParams.append(key, String(item))
            }
          })
        } else if (value !== null && value !== undefined) {
          searchParams.append(key, String(value))
        }
      })

      return searchParams.toString()
    },
  })
}

export function useSaleExcelReportMutation(
  options?: UseMutationOptions<
    AxiosResponse<Blob>,
    AxiosCustomError,
    SaleExcelReportParams,
    unknown
  >
) {
  return useMutation({
    mutationFn: downloadSaleExcelReport,
    meta: {
      success: {
        title: 'sale.exportTitleSuccess',
        description: 'sale.exportDescriptionSuccess',
      },
    },
    ...options,
  })
}
