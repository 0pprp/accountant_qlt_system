<template>
  <AppPage
    title="معاينة مكوّنات العرض"
    subtitle="كل زر وحقل يجب أن يمر عبر مكوّنات Qalaat الموحّدة. هذه الصفحة للتطوير فقط."
  >
    <AppPanel title="الحقول والأزرار">
      <div class="q-filters">
        <AppInput v-model="q" label="بحث" type="search" placeholder="اسم طويل للتحقق من الالتفاف" />
        <AppSelect v-model="tab" label="حالة" :options="selectOptions" />
        <AppTextarea v-model="note" label="ملاحظة" placeholder="نص متعدد الأسطر" />
      </div>
      <ActionRow
        :primary="{ label: 'إجراء أساسي' }"
        :secondary="[{ id: 'sec', label: 'ثانوي' }]"
        @primary="toast = 'primary'"
        @secondary="(id) => (toast = id)"
      />
      <div class="kit-row">
        <AppButton variant="primary">Primary</AppButton>
        <AppButton variant="secondary">Secondary</AppButton>
        <AppButton variant="ghost">Ghost</AppButton>
        <AppButton variant="danger">Danger</AppButton>
      </div>
    </AppPanel>

    <AppPanel title="FilterBar + StatusTabs">
      <FilterBar>
        <AppInput v-model="q" label="بحث" />
        <AppSelect v-model="tab" label="حالة" :options="selectOptions" />
      </FilterBar>
      <StatusTabs v-model="tab" :tabs="tabs" aria-label="حالات العرض" />
    </AppPanel>

    <AppPanel title="EntityCard">
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
        </template>
        <template #actions>
          <ActionRow
            :primary="{ label: 'تدقيق الطلب' }"
            :secondary="[{ id: 'audit', label: 'سجل الحركة' }]"
            @primary="toast = 'primary'"
            @secondary="(id) => (toast = id)"
          />
        </template>
      </EntityCard>
    </AppPanel>

    <AppPanel title="الحالات">
      <div class="grid-states">
        <DataState kind="loading" title="جاري التحميل" message="لا تُعرض أصفار وهمية." />
        <DataState kind="empty" title="لا توجد نتائج" message="القائمة فارغة فعلياً." />
        <DataState kind="error" title="فشل التحميل" message="تعذّر الاتصال بالخادم.">
          <template #actions>
            <AppButton variant="secondary" @click="toast = 'retry'">إعادة المحاولة</AppButton>
          </template>
        </DataState>
        <DataState kind="forbidden" title="غير مصرّح" message="صلاحية القراءة مطلوبة." />
      </div>
    </AppPanel>

    <AppPanel title="Pagination + ConfirmReasonDialog">
      <Pagination :page="page" :has-prev="page > 1" :has-next="true" @prev="page--" @next="page++" />
      <ActionRow class="mt" :primary="{ label: 'فتح حوار سبب' }" @primary="reasonOpen = true" />
      <ConfirmReasonDialog
        v-model="reasonOpen"
        title="تأكيد الإجراء"
        message="أدخل السبب كاملاً. الإلغاء لا يرسل طلباً."
        @confirm="(r) => ((toast = r), (reasonOpen = false))"
      />
    </AppPanel>

    <p v-if="toast" class="toast" role="status">آخر حدث: {{ toast }}</p>
  </AppPage>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import {
  ActionRow,
  AppButton,
  AppInput,
  AppPage,
  AppPanel,
  AppSelect,
  AppTextarea,
  ConfirmReasonDialog,
  DataField,
  DataState,
  EntityCard,
  FilterBar,
  Pagination,
  StatusTabs,
} from '@/modules/Core/components/ui'

const q = ref('')
const tab = ref('all')
const note = ref('')
const page = ref(1)
const reasonOpen = ref(false)
const toast = ref('')
const tabs = [
  { id: 'all', label: 'الكل', count: 12 },
  { id: 'open', label: 'مفتوح', count: 4 },
  { id: 'done', label: 'منجز', count: 8 },
]
const selectOptions = computed(() => tabs.map((t) => ({ label: t.label, value: t.id })))
</script>

<style scoped>
.kit-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 12px;
}
.grid-states {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}
.mt {
  margin-top: 12px;
}
.toast {
  margin-top: 16px;
  padding: 12px 14px;
  border-radius: 12px;
  background: #e7f3f1;
  color: #006a64;
  font-weight: 600;
}
</style>
