export const ManagementSalesMapEndpoints = {
  provinces: '/admin/ManagementSalesMap/provinces',
  lists: (provinceId: number) =>
    `/admin/ManagementSalesMap/provinces/${provinceId}/lists`,
  points: (provinceId: number) =>
    `/admin/ManagementSalesMap/provinces/${provinceId}/points`,
  orderDetail: (orderId: number) =>
    `/admin/ManagementSalesMap/orders/${orderId}`,
} as const
