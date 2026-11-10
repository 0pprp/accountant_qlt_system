import { computed, ref, type Ref } from 'vue'
import type { SalesRequestListItem } from '../types'
import { isOverlappingView, tabByKey } from '../domain/tabs'

/**
 * اختيار الطلب + مسودة الإجراءات + انتقال خارج الفلتر دون مسح المسودة.
 */
export function useSalesRequestSelection(
  selectedId: Ref<number | null>,
  currentTab: Ref<string>,
  canReadTab: (key: string) => boolean
) {
  const draftNotes = ref<Record<number, { reject?: string; exception?: string; assignReason?: string }>>(
    {}
  )
  const outsideFilterMessage = ref<string | null>(null)
  const outsideFilterTargetTab = ref<string | null>(null)
  /** تفاصيل معلّقة حتى لو خرجت من قائمة التبويب الحالي */
  const pinnedDetail = ref<SalesRequestListItem | null>(null)

  function select(item: SalesRequestListItem | null) {
    selectedId.value = item?.id ?? null
    outsideFilterMessage.value = null
    outsideFilterTargetTab.value = null
    pinnedDetail.value = item
  }

  function getDraft(id: number) {
    if (!draftNotes.value[id]) {
      draftNotes.value[id] = {}
    }
    return draftNotes.value[id]
  }

  function clearDraft(id: number) {
    delete draftNotes.value[id]
  }

  /**
   * بعد تحديث الطلب: إن انتقل خارج التبويب الحالي أبقِ التفاصيل مع رسالة.
   */
  function onRemoteUpdate(updated: SalesRequestListItem) {
    pinnedDetail.value = {
      ...pinnedDetail.value,
      ...updated,
    }
    // لا تمسح المسودة
    if (
      updated.tabKey &&
      updated.tabKey !== currentTab.value &&
      currentTab.value !== 'all' &&
      !isOverlappingView(currentTab.value)
    ) {
      const label = tabByKey(updated.tabKey)?.label ?? updated.tabKey
      outsideFilterMessage.value = `انتقل إلى ${label}`
      outsideFilterTargetTab.value = canReadTab(updated.tabKey)
        ? updated.tabKey
        : null
    } else {
      outsideFilterMessage.value = null
      outsideFilterTargetTab.value = null
    }
  }

  const hasOpenDraft = computed(() => {
    const id = selectedId.value
    if (!id) return false
    const d = draftNotes.value[id]
    return Boolean(d?.reject || d?.exception || d?.assignReason)
  })

  return {
    draftNotes,
    outsideFilterMessage,
    outsideFilterTargetTab,
    pinnedDetail,
    select,
    getDraft,
    clearDraft,
    onRemoteUpdate,
    hasOpenDraft,
  }
}
