import { onBeforeUnmount, ref } from 'vue'
import useAuthStore from '@/modules/Auth/store'

export interface AuthedBlob {
  url: string
  type: string
}

/**
 * وسائط محميّة بتوكن — الرابط المباشر يفشل بـ401 لأن المتصفّح لا يرسل الترويسة.
 * نجلب البايتات بالتوكن ونصنع عنوان blob، ونحرّره عند إغلاق الشاشة.
 */
export function useAuthedMedia() {
  const authStore = useAuthStore()
  const base = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? '/api/v1/'
  const objectUrls = ref<string[]>([])

  async function fetchBlob(path: string): Promise<AuthedBlob | null> {
    try {
      const response = await fetch(`${base}${path}`, {
        headers: { Authorization: `Bearer ${authStore.accessToken}` },
      })
      if (!response.ok) return null

      const blob = await response.blob()
      const url = URL.createObjectURL(blob)
      objectUrls.value.push(url)
      return { url, type: blob.type }
    } catch {
      return null
    }
  }

  async function fetchBlobUrl(path: string): Promise<string | null> {
    return (await fetchBlob(path))?.url ?? null
  }

  async function openInNewTab(path: string) {
    const url = await fetchBlobUrl(path)
    if (url) window.open(url, '_blank', 'noopener')
  }

  function revoke(url: string) {
    URL.revokeObjectURL(url)
    objectUrls.value = objectUrls.value.filter((x) => x !== url)
  }

  function release() {
    objectUrls.value.forEach((url) => URL.revokeObjectURL(url))
    objectUrls.value = []
  }

  onBeforeUnmount(release)

  return { fetchBlob, fetchBlobUrl, openInNewTab, revoke, release }
}
