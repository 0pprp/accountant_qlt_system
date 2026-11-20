import 'package:field_promoter_app/features/orders/order_draft.dart';
import 'package:field_promoter_app/features/orders/order_stage.dart';
import 'package:field_promoter_app/features/profile/profile_page.dart';
import 'package:field_promoter_app/features/promoter/models.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('مراحل الطلب', () {
    test('جديد ← في المبيعات ← قيد الكشف ← تم البيع ← مكتمل', () {
      expect(OrderStages.stepIndex('inProgress', 'جديد'), 0);
      expect(OrderStages.stepIndex('inProgress', 'مسند'), 1);
      expect(OrderStages.stepIndex('inProgress', 'تمت المشاهدة'), 1);
      expect(OrderStages.stepIndex('inProgress', 'معلّق'), 1);
      expect(OrderStages.stepIndex('inProgress', 'قيد التنفيذ'), 2);
      expect(OrderStages.stepIndex('inProgress', 'تم الكشف'), 2);
      expect(OrderStages.stepIndex('inProgress', 'مُجهّز للبيع'), 2);
      expect(OrderStages.stepIndex('sold', 'تم البيع — قيد التسديد'), 3);
      expect(OrderStages.stepIndex('finished', 'مكتمل التسديد'), 4);
    });

    test('المرفوض والملغى خارج المراحل', () {
      expect(OrderStages.stepIndex('rejected', 'لم يتم البيع'), -1);
      expect(OrderStages.isRejected('rejected'), isTrue);
      expect(OrderStages.isRejected('sold'), isFalse);
    });

    test('تحليل الطلب وتقدم التسديد من JSON', () {
      final o = PromoterOrder.fromJson({
        'id': 12,
        'customerName': 'علي حسين كاظم',
        'customerPhone': '07701234567',
        'product': 'ايفون',
        'address': 'بغداد',
        'note': null,
        'branchName': 'الكرادة',
        'stage': 'sold',
        'statusLabel': 'تم البيع — قيد التسديد',
        'reason': null,
        'createdAt': '2026-11-20T09:15:00+00:00',
        'canCancel': false,
        'sale': {
          'total': 1500000.0,
          'paid': 500000.0,
          'remaining': 1000000.0,
          'paymentsCount': 2,
          'lastPaymentDate': '2026-11-19',
          'percent': 33,
        },
      });
      expect(o.sale!.percent, 33);
      expect(o.sale!.remaining, 1000000);
      expect(o.createdAt!.isUtc, isTrue);
      expect(OrderStages.stepIndex(o.stage, o.statusLabel), 3);
    });
  });

  group('مفتاح الإرسال', () {
    test('يبقى نفسه عند إعادة المحاولة ويتجدد عند تغيير المحتوى أو بعد النجاح', () {
      var n = 0;
      final keeper = SubmissionKeyKeeper(generate: () => 'k${++n}');
      const a = OrderDraft(customerName: 'علي حسين كاظم', customerPhone: '07701234567');
      const b = OrderDraft(customerName: 'علي حسين جاسم', customerPhone: '07701234567');
      final first = keeper.keyFor(a.fingerprint);
      expect(keeper.keyFor(a.fingerprint), first);
      final changed = keeper.keyFor(b.fingerprint);
      expect(changed, isNot(first));
      keeper.renew();
      expect(keeper.keyFor(b.fingerprint), isNot(changed));
    });

    test('المفتاح المحفوظ في المسودة يُستعاد لنفس المحتوى', () {
      const d = OrderDraft(customerName: 'علي حسين كاظم', address: 'بغداد');
      final restored = OrderDraft.fromJson(
        OrderDraft(
          customerName: d.customerName,
          address: d.address,
          submissionKey: 'saved-key',
          keyFingerprint: d.fingerprint,
        ).toJson(),
      );
      final keeper = SubmissionKeyKeeper(
        key: restored.submissionKey,
        fingerprint: restored.keyFingerprint,
      );
      expect(keeper.keyFor(restored.fingerprint), 'saved-key');
    });
  });

  test('رقم واتساب: الصفر البادئ يصبح 964', () {
    expect(whatsappDigits('07701234567'), '9647701234567');
    expect(whatsappDigits('+964 770 123 4567'), '9647701234567');
    expect(whatsappDigits('009647701234567'), '9647701234567');
    expect(whatsappDigits(''), isNull);
  });
}
