export const CommitteeTasksEndpoints = {
  list: '/admin/CommitteeTasks',
  create: '/admin/CommitteeTasks',
  update: (id: number) => `/admin/CommitteeTasks/${id}`,
  remove: (id: number) => `/admin/CommitteeTasks/${id}`,
} as const
