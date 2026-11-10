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
          >
            [زبون] {{ doc.originalFileName }}
            <span class="opacity-60">({{ doc.type || '—' }})</span>
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
          <li v-for="doc in documents.saleDocuments" :key="`o-${doc.id}`">
            [بيع {{ doc.ownerId }}] {{ doc.originalFileName }}
            <span class="opacity-60">({{ doc.type || '—' }})</span>
          </li>
          <li v-if="!documents.saleDocuments.length" class="opacity-70">
            لا مستندات
          </li>
        </ul>
      </div>
    </template>
  </section>
</template>

<script setup lang="ts">
import type { CustomerAuditProfile } from '../../requests/queries/useCustomerAuditProfileQuery'

defineProps<{
  documents: CustomerAuditProfile['documents']
  loading?: boolean
}>()
</script>
