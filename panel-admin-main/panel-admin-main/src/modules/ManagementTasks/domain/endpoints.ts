export const ManagementTaskEndpoints = {
  list: '/admin/ManagementTasks',
  detail: (id: number) => `/admin/ManagementTasks/${id}`,
  assigneeOptions: '/admin/ManagementTasks/assignee-options',
  accept: (id: number) => `/admin/ManagementTasks/${id}/accept`,
  reject: (id: number) => `/admin/ManagementTasks/${id}/reject`,
  transfer: (id: number) => `/admin/ManagementTasks/${id}/transfer`,
  complete: (id: number) => `/admin/ManagementTasks/${id}/complete`,
  confirm: (id: number) => `/admin/ManagementTasks/${id}/confirm`,
  resend: (id: number) => `/admin/ManagementTasks/${id}/resend`,
  attachments: (id: number) => `/admin/ManagementTasks/${id}/attachments`,
  attachment: (id: number, fileId: number) =>
    `/admin/ManagementTasks/${id}/attachments/${fileId}`,
} as const
