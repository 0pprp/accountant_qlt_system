export const FollowerWorkEndpoints = {
  followers: '/admin/FollowerWork/followers',
  salesReview: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/sales-review`,
  remainingCustomers: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/remaining-customers`,
  declining: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/declining`,
  notes: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/notes`,
  oldCustomers: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/old-customers`,
  delegateNotes: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/delegate-notes`,
  collection: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/collection`,
  followerCollection: (listId: number) =>
    `/admin/OrderLists/${listId}/follower-collection`,
  liveLocations: '/admin/FollowerTracking/latest',
  route: (followerId: number) =>
    `/admin/FollowerTracking/${followerId}/route`,
} as const
