import 'package:field_promoter_app/core/validation/validators.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('الاسم الثلاثي', () {
    test('ثلاثة أسماء عربية مقبولة', () {
      expect(Validators.normalizeTripleName('علي حسين كاظم'), 'علي حسين كاظم');
      expect(Validators.customerName('علي حسين كاظم'), isNull);
    });

    test('«عبد» و«ابو» تُدمج مع الكلمة التالية', () {
      expect(Validators.normalizeTripleName('علي عبد الله'), isNull);
      expect(Validators.normalizeTripleName('ابو علي حسين'), isNull);
      expect(Validators.normalizeTripleName('محمد عبد الله حسن'), 'محمد عبد الله حسن');
      expect(Validators.normalizeTripleName('أبو بكر عبد الرحمن جاسم'), isNotNull);
      expect(Validators.normalizeTripleName('حسن أبي طالب علي'), isNotNull);
    });

    test('اسمان فقط مرفوض', () {
      expect(Validators.normalizeTripleName('علي حسين'), isNull);
      expect(Validators.customerName('علي حسين'), 'اكتب الاسم الثلاثي بالحروف العربية');
    });

    test('الحروف اللاتينية والأرقام مرفوضة', () {
      expect(Validators.normalizeTripleName('Ali Hussein Kadhim'), isNull);
      expect(Validators.normalizeTripleName('علي حسين 3'), isNull);
    });

    test('كلمة من حرف واحد مرفوضة كما في الخادم', () {
      expect(Validators.normalizeTripleName('علي و حسين'), isNull);
    });

    test('التشكيل والتطويل لا يؤثران', () {
      expect(Validators.normalizeTripleName('عَلِيّ حـسين كاظم'), 'علي حسين كاظم');
    });

    test('المسافات الزائدة تُتجاهل والفارغ مرفوض', () {
      expect(Validators.normalizeTripleName('  علي   حسين  كاظم '), 'علي حسين كاظم');
      expect(Validators.customerName('  '), 'اكتب اسم الزبون الثلاثي');
    });
  });

  group('الهاتف العراقي', () {
    test('الصيغ المقبولة تُطبَّع إلى 07XXXXXXXXX', () {
      expect(Validators.normalizePhone('07701234567'), '07701234567');
      expect(Validators.normalizePhone('+9647701234567'), '07701234567');
      expect(Validators.normalizePhone('009647801234567'), '07801234567');
      expect(Validators.normalizePhone('9647501234567'), '07501234567');
      expect(Validators.normalizePhone('7901234567'), '07901234567');
      expect(Validators.normalizePhone('0770 123 4567'), '07701234567');
      expect(Validators.normalizePhone('٠٧٧٠١٢٣٤٥٦٧'), '07701234567');
      expect(Validators.normalizePhone('۰۷۷۰۱۲۳۴۵۶۷'), '07701234567');
    });

    test('الأرقام غير الصحيحة مرفوضة', () {
      expect(Validators.normalizePhone('0770123456'), isNull);
      expect(Validators.normalizePhone('077012345678'), isNull);
      expect(Validators.normalizePhone('07201234567'), isNull);
      expect(Validators.normalizePhone('06701234567'), isNull);
      expect(Validators.normalizePhone(''), isNull);
      expect(Validators.customerPhone('123'), isNotNull);
      expect(Validators.customerPhone('07701234567'), isNull);
    });

    test('هاتف المحل اختياري لكن يُتحقق منه إن كُتب', () {
      expect(Validators.optionalPhone(''), isNull);
      expect(Validators.optionalPhone('0770'), isNotNull);
      expect(Validators.optionalPhone('07801234567'), isNull);
    });
  });

  group('حقول أخرى', () {
    test('العنوان من 3 إلى 400 حرف', () {
      expect(Validators.address('ab'), isNotNull);
      expect(Validators.address('حي'), isNotNull);
      expect(Validators.address('حي الجامعة'), isNull);
      expect(Validators.address('أ' * 400), isNull);
      expect(Validators.address('أ' * 401), isNotNull);
    });

    test('الملاحظة 500 حرف كحد أقصى', () {
      expect(Validators.note(''), isNull);
      expect(Validators.note('a' * 500), isNull);
      expect(Validators.note('a' * 501), isNotNull);
    });

    test('اسم المحل من 2 إلى 150', () {
      expect(Validators.pinName('a'), isNotNull);
      expect(Validators.pinName('ab'), isNull);
      expect(Validators.pinName('a' * 150), isNull);
      expect(Validators.pinName('a' * 151), isNotNull);
    });

    test('الاهتمام من 1 إلى 5', () {
      expect(Validators.interest(null), isNotNull);
      expect(Validators.interest(0), isNotNull);
      expect(Validators.interest(1), isNull);
      expect(Validators.interest(5), isNull);
      expect(Validators.interest(6), isNotNull);
    });

    test('المنتج الحر من 2 إلى 100', () {
      expect(Validators.normalizeProduct(' ثلاجة   كبيرة '), 'ثلاجة كبيرة');
      expect(Validators.product('a'), isNotNull);
      expect(Validators.product('a' * 101), isNotNull);
    });

    test('حدود العراق', () {
      expect(Validators.insideIraq(33.3, 44.4), isTrue);
      expect(Validators.insideIraq(40, 44), isFalse);
      expect(Validators.insideIraq(33, 50), isFalse);
    });
  });
}
