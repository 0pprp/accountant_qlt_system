<template>
  <div class="q-ui ui-kit-preview" dir="rtl">
    <PageHeader
      title="معاينة مكوّنات العرض"
      subtitle="صفحة تطوير محمية — لا تستدعي endpoints ولا تمنح صلاحيات. الحاويات تمرّر البيانات والأكشنات."
    />

    <section class="block">
      <h2>FilterBar + StatusTabs</h2>
      <FilterBar>
        <label
          >بحث
          <input v-model="q" type="search" placeholder="اسم طويل جداً للتحقق من الالتفاف" />
        </label>
        <label
          >حالة
          <select v-model="tab">
            <option v-for="t in tabs" :key="t.id" :value="t.id">{{ t.label }}</option>
          </select>
        </label>
      </FilterBar>
      <StatusTabs v-model="tab" :tabs="tabs" aria-label="حالات العرض" />
    </section>

    <section class="block">
      <h2>EntityCard / DataField / StatusBadge / ActionRow</h2>
      <EntityCard
        eyebrow="طلب #9001"
        name="زبون تجريبي باسم طويل جداً للتأكد من عدم القصّ بثلاث نقاط"
        status-label="قيد المتابعة"
        status-tone="warning"
        selected
      >
        <template #identity-extra>
          <DataField label="الهاتف" value="07801234567" is-phone />
        </template>
        <template #fields>
          <DataField label="المنتج المطلوب" value="مكيف 1.5 طن مع تركيب وضمان" />
          <DataField
            label="العنوان"
            value="بغداد / الكرادة داخل / زقاق 12 / دار 4 — وصف طويل جداً يجب أن يلتف"
          />
          <DataField label="موظف المبيعات" value="أحمد علي" />
          <DataField label="أُرسل بواسطة" value="مندوب الفرع" />
        </template>
        <template #notes>
          <div class="note">
            <strong>ملاحظة المُرسل</strong>
            <p>
              ملاحظة متعددة الأسطر&#10;سطر ثانٍ&#10;سطر ثالث للتأكد من pre-wrap وعدم إخفاء المحتوى.
            </p>
          </div>
        </template>
        <template #actions>
          <ActionRow
            :primary="{ label: 'تدقيق الطلب' }"
            :secondary="[{ id: 'audit', label: 'سجل الحركة' }]"
            @primary="toast = 'primary'"
            @secondary="id => (toast = id)"
          />
        </template>
      </EntityCard>
    </section>

    <section class="block">
      <h2>Matching cards (2×2 / 4)</h2>
      <MatchingSummary :cards="matchCards" :selected-key="matchKey" @select="k => (matchKey = k)" />
    </section>

    <section class="block grid-states">
      <DataState kind="loading" title="جاري التحميل" message="لا تُعرض أصفار وهمية." />
      <DataState kind="empty" title="لا توجد نتائج" message="القائمة فارغة فعلياً." />
      <DataState kind="error" title="فشل التحميل" message="تعذّر الاتصال بالخادم.">
        <template #actions>
          <button type="button" class="retry" @click="toast = 'retry'">إعادة المحاولة</button>
        </template>
      </DataState>
      <DataState kind="forbidden" title="غير مصرّح" message="صلاحية القراءة مطلوبة لفتح التبويب." />
    </section>

    <section class="block">
      <h2>Pagination + ConfirmReasonDialog</h2>
      <Pagination :page="page" :has-prev="page > 1" :has-next="true" @prev="page--" @next="page++" />
      <ActionRow
        class="mt"
        :primary="{ label: 'فتح حوار سبب' }"
        @primary="reasonOpen = true"
      />
      <ConfirmReasonDialog
        v-model="reasonOpen"
        title="تأكيد الإجراء"
        message="أدخل السبب كاملاً. الإلغاء لا يرسل طلباً."
        @confirm="r => ((toast = r), (reasonOpen = false))"
      />
    </section>

    <p v-if="toast" class="toast" role="status">آخر حدث: {{ toast }}</p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import PageHeader from '@/modules/Core/components/ui/PageHeader.vue'
import FilterBar from '@/modules/Core/components/ui/FilterBar.vue'
import StatusTabs from '@/modules/Core/components/ui/StatusTabs.vue'
import EntityCard from '@/modules/Core/components/ui/EntityCard.vue'
import DataField from '@/modules/Core/components/ui/DataField.vue'
import ActionRow from '@/modules/Core/components/ui/ActionRow.vue'
import DataState from '@/modules/Core/components/ui/DataState.vue'
import Pagination from '@/modules/Core/components/ui/Pagination.vue'
import ConfirmReasonDialog from '@/modules/Core/components/ui/ConfirmReasonDialog.vue'
import MatchingSummary from '@/modules/SalesRequests/components/MatchingSummary.vue'
import type { MatchCard } from '@/modules/SalesRequests/types'
import '@/modules/Core/components/ui/ui.tokens.css'

const q = ref('')
const tab = ref('all')
const tabs = [
  { id: 'all', label: 'الكل', count: 12 },
  { id: 'inProgress', label: 'قيد التنفيذ', count: 3 },
  { id: 'rejected', label: 'مرفوض', count: 1 },
  { id: 'completed', label: 'مكتمل', count: 8 },
]
const page = ref(1)
const reasonOpen = ref(false)
const toast = ref('')
const matchKey = ref<string | null>('phone')
const matchCards: MatchCard[] = [
  { key: 'phone', label: 'تطابق الهاتف', state: 'results', count: 2 },
  { key: 'tripleName', label: 'تطابق الاسم الثلاثي', state: 'loading', count: null },
  { key: 'fatherGrandfather', label: 'الأب والجد', state: 'failed', count: null },
  { key: 'sonFather', label: 'الابن والأب', state: 'insufficientData', count: null },
]
</script>

<style scoped>
.ui-kit-preview {
  max-width: 1100px;
  margin: 0 auto;
  padding: 16px 16px 48px;
  min-height: 100vh;
  background: #f4f7f7;
  color: #00201e;
}
.block {
  margin-bottom: 24px;
}
.block h2 {
  margin: 0 0 12px;
  font-size: 18px;
  color: #006a64;
}
.note {
  border-inline-start: 3px solid #b5cec8;
  padding: 10px 14px;
  background: #fafcfb;
  font-size: 15px;
  line-height: 1.8;
}
.note p {
  margin: 4px 0 0;
  white-space: pre-wrap;
}
.grid-states {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}
.retry,
.mt :deep(.q-action-row__primary) {
  min-height: 44px;
}
.retry {
  background: #006a64;
  color: #fff;
  border: 0;
  border-radius: 10px;
  padding: 10px 16px;
  font: inherit;
  font-weight: 600;
  cursor: pointer;
}
.toast {
  position: sticky;
  bottom: 12px;
  background: #00201e;
  color: #fff;
  padding: 12px 16px;
  border-radius: 12px;
}
</style>
