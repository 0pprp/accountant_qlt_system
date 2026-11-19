<template>
  <section class="space-y-3">
    <h3 class="text-base font-semibold">المستندات</h3>
    <p v-if="loading" class="text-sm opacity-70">جارٍ التحميل...</p>
    <template v-else-if="documents">
      <div>
        <h4 class="mb-2 text-sm font-medium">مستندات الزبون</h4>
        <ul class="space-y-1 text-sm">
          <li
            v-for="doc in documents.customerDocuments"
            :key="`c-${doc.id}`"
            class="doc-row"
          >
            <span class="doc-owner">[زبون]</span>
            <bdi class="doc-name">{{ doc.originalFileName }}</bdi>
            <span class="doc-type">(<bdi>{{ doc.type || '—' }}</bdi>)</span>
            <button
              v-if="canOpen(doc)"
              type="button"
              class="doc-open"
              @click="openDoc(doc.id, doc.originalFileName)"
            >فتح</button>
          </li>
          <li
            v-if="!documents.customerDocuments.length"
            class="opacity-70"
          >
            لا مستندات
          </li>
        </ul>
      </div>
      <div>
        <h4 class="mb-2 text-sm font-medium">مستندات البيوع</h4>
        <ul class="space-y-1 text-sm">
          <li
            v-for="doc in documents.saleDocuments"
            :key="`o-${doc.id}`"
            class="doc-row"
          >
            <span class="doc-owner">[بيع {{ doc.ownerId }}]</span>
            <bdi class="doc-name">{{ doc.originalFileName }}</bdi>
            <span class="doc-type">(<bdi>{{ doc.type || '—' }}</bdi>)</span>
            <button
              v-if="canOpen(doc)"
              type="button"
              class="doc-open"
              @click="openDoc(doc.id, doc.originalFileName)"
            >فتح</button>
          </li>
          <li v-if="!documents.saleDocuments.length" class="opacity-70">
            لا مستندات
          </li>
        </ul>
      </div>
    </template>

    <MediaViewerDialog
      v-model="viewerOpen"
      :path="viewerPath"
      :title="viewerTitle"
    />
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { CustomerAuditProfile } from '../../requests/queries/useCustomerAuditProfileQuery'
import MediaViewerDialog from '@/modules/Core/components/shared/MediaViewerDialog/MediaViewerDialog.vue'

const viewerOpen = ref(false)
const viewerPath = ref<string | null>(null)
const viewerTitle = ref('')

function openDoc(id: number, fileName: string) {
  viewerPath.value = `admin/CustomerMedia/documents/${id}`
  viewerTitle.value = fileName
  viewerOpen.value = true
}

function canOpen(doc: { relativePath?: string | null }) {
  return !!doc.relativePath
}

defineProps<{
  documents: CustomerAuditProfile['documents']
  loading?: boolean
}>()
</script>

<style scoped>
.doc-row {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 8px;
  padding: 4px 0;
}

.doc-owner {
  color: #50666a;
  white-space: nowrap;
}

.doc-name {
  overflow-wrap: anywhere;
}

.doc-type {
  opacity: 0.6;
  white-space: nowrap;
}

.doc-open {
  padding: 2px 12px;
  border-radius: 999px;
  border: 1px solid #006a64;
  color: #006a64;
  background: #fff;
  font-size: 12px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  flex-shrink: 0;
}

.doc-open:hover { background: #006a64; color: #fff; }
</style>
