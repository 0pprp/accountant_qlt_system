/**
 * إلغاء طلبات HTTP الجارية عند تبديل فرع الحساب.
 * يُدمج مع signal الجلسة وsignal الـquery في axios.
 */
let branchAbortController = new AbortController()

export function getBranchAbortSignal(): AbortSignal {
  return branchAbortController.signal
}

export function abortBranchRequests() {
  branchAbortController.abort()
  branchAbortController = new AbortController()
}
