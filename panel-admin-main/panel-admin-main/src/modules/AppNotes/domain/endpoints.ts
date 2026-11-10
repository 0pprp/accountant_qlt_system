export const AppNotesEndpoints = {
  list: '/admin/AppNotes',
  unreadCount: '/admin/AppNotes/unread-count',
  byId: (id: number) => `/admin/AppNotes/${id}`,
  markRead: (id: number) => `/admin/AppNotes/${id}/read`,
  markAllRead: '/admin/AppNotes/read-all',
} as const
