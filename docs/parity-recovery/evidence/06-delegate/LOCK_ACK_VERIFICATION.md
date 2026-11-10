
## تحقق مفاتيح الأقفال والإقرار — 2026-11-10

تمت مراجعة التعديلات الموجودة دون إعادة بنائها: namespaces ثنائية ثابتة order=791902 وcash=791903، منفصلة عن ensure-draft=791901 والقفل أحادي المفتاح. لا GetHashCode في كاتب التحصيل؛ ترتيب الطلب ثم حامل النقد محفوظ.

أُعيد الإثبات عبر عمليتي API مستقلتين من publish محلي على 4202/4203 ضد qalaat_22_23_iso فقط. الاختبار الجديد يمسك القفل مؤقتًا ويتأكد عبر pg_locks أن جلستي العمليتين تنتظران نفس القفل قبل تحريره؛ بالتالي Create وSync متداخلان فعلًا، وليس إرسالًا متتابعًا كما في أول سيناريو السكربت السابق.

- concurrent Create/Sync: نفس PaymentId، KEY_COUNT=1، إقرار واحد alreadyApplied.
- مفتاحان يتجاوزان المتبقي: 200 + 400، زيادة المقبوض 1000 فقط.
- طلبان مختلفان لحامل نقد واحد: كلاهما 200، cash delta=2000.
- دليل آلي مع assertions: evidence/06-delegate/dual-process-lock-barrier.json.
- سكربت إعادة الإنتاج: evidence/06-delegate/verify-payment-lock-barrier.py. يتطلب متغيرات PAYMENT_VERIFY_DSN/USER/PASSWORD/OUTPUT؛ يتحقق من اسم قاعدة iso والعنوان الحلقي، ولا يطبع tokens. يستخدم ويعدّل fixture الطلبين 1 و3 في iso؛ لا يُشغّل على الإنتاج.

Flutter: الإقرار يشترط المفتاح والطلب والمبلغ والتاريخ بصيغة DateOnly المطابقة وPaymentId موجبًا صحيحًا. أُضيف رفض المعرف الكسري والتاريخ الذي يحمل لاحقة timestamp بدل عقد DateOnly؛ لا تحويل إلى رقم صحيح يخفي بيانات غير صالحة. الرد غير الناجح أو الناقص لا يسمح بحذف الطابور.

النتائج: ClientPaymentIdSafetyTests 6/6؛ Flutter delegate full suite 24/24 (اختبارات الإقرار 12 حالة). publish API ناجح مع تحذيرات موجودة في ملفات أخرى. evidence/06-delegate/flutter-lock-ack-tests.log يحفظ النتيجة. لا تعديل مخطط، لا إنتاج، لا تغيير تصميم، ولا ادعاء Device جديد.
