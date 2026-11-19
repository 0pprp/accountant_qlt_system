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
  sectionCounts: (followerId: number) =>
    `/admin/FollowerWork/${followerId}/section-counts`,
  customerProfile: (followerId: number, customerId: number) =>
    `/admin/FollowerWork/${followerId}/customers/${customerId}/profile`,
  shopCapture: '/admin/FollowerWork/shop-capture',
  shopPhoto: (photoId: number) => `/admin/FollowerWork/shop-capture/photos/${photoId}`,
  manageFollowers: '/admin/FollowerWork/followers',
  manageFollower: (followerId: number) => `/admin/FollowerWork/followers/${followerId}`,
} as const
