# Priority change list (01)

## P0 — أماكن التغيير
1. `committee_officer_application/lib/features/field/presentation/field_followups_page.dart` — استبدال ComingSoon بمسار FieldFollowups الحقيقي (عقد API جديد يُوثَّق كجديد إن لزم).
2. `sales_employee_app/lib/features/sales/presentation/bin_orders_page.dart` (107–120) — ربط نقل الاسم الحي بواجهة الزملاء/الأمر؛ الإبقاء على الشرط الأمني.
3. `sales_employee_app/lib/features/home/presentation/widgets/home_drawer.dart` (164–199) — أربع صفحات: أرشيف، فيديوهات، شكوى، ملاحظة مدير (إعادة استخدام APIs الموجودة حيث تتوفر).

## P1
4. `committee_officer_application/.../app_drawer.dart:104` — إحصائيات.
5. `sales_employee_app/.../demo_orders_store.dart` + injection + استدعاءات المسار الحي — عزل demo عن الإنتاج.
6. إحصائيات مندوبي FE — تقرير لوحة أو N/A منتج.

## P2
7. قرار منتج لـ main_follower_application.
8. `Management/domain/sections.ts` committee-ratings.
9. FE.daily-followup.

لا تبدأ تغييرات حسابية/مخطط في دفعة 01.
