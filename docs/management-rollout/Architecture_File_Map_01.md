# خريطة ملفات المرحلة 01 (بديل تحديث Word الجزئي)

| مسار | مسؤولية |
|------|---------|
| `panel-admin-main/.../modules/Management/domain/sections.ts` | سجل مركزي للأقسام/التبويبات/الصلاحيات/حالة التنفيذ |
| `panel-admin-main/.../modules/Management/router.ts` | عقود HTTP للوحة فقط عبر routes؛ يعيد استخدام views المصدر |
| `panel-admin-main/.../modules/Management/views/ManagementWorkspace.vue` | صدفة الأقسام + التبويبات + مفتاح الفرع |
| `panel-admin-main/.../modules/Management/views/ManagementEntryRedirect.vue` | توجيه لأول تبويب مسموح |
| `panel-admin-main/.../modules/Management/views/embeds/ManagementSalesRequestsEmbed.vue` | تضمين مساحة الطلبات بدون نسخ منطق |
| `panel-admin-main/.../modules/Management/composables/useManagementAccess.ts` | قرار ظهور/قفل التبويب |
| `panel-admin-main/.../modules/Core/components/layout/TheSideBar/TheSideBar.vue` | توسعة عامة `expandedBySectionId[section.id]` + `aria-controls` |
| `panel-admin-main/.../modules/Core/navigation/navigation.registry.ts` | إدخال سايدبار الإدارة |
| `panel-admin-main/.../modules/SalesRequests/components/RequestFilters.vue` | أيقونة المكبرة مع خلفية `#F8F8F8` |
| `api-main/.../SalesRequestExceptionDecider.cs` | منع موافقة المقدم (Existing source) |

ملف Word الأصلي `Management_Architecture.docx` يبقى مرجعًا تصميميًا؛ هذا الملف يوثّق ما نُفّذ في الكود.
