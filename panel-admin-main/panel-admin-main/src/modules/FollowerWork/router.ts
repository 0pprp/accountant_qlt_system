import type { Router } from 'vue-router'

/**
 * المسار canonical يُسجَّل عبر Management (reuse FollowerWorkPage).
 * هذا الملف يوثّق aliases ولا يكرّر سجل التنقل الرئيسي.
 *
 * Canonical: /management/follower-work
 * Alias Management tab: /management/oversight/follower-work
 * Section paths (06–13): /management/follower-work/{segment} — تُفعَّل عند تنفيذ دفعتها.
 */
export default function registerFollowerWorkRouter(_router: Router) {
  // aliases تُضاف في Management/router.ts لتجنب مصدرَي صلاحية متعارضين
}
