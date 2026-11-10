import 'package:flutter_test/flutter_test.dart';
import 'package:delegate_app/features/sales/data/sales_request_submission.dart';

void main() {
  test('Arabic and Persian phone digits normalize to the same Iraqi number', () {
    expect(SalesRequestSubmission.phone('٠٧٨٠١٢٣٤٥٦٧'), '07801234567');
    expect(SalesRequestSubmission.phone('۰۷۸۰۱۲۳۴۵۶۷'), '07801234567');
  });
  test('Phone must start 07 and contain exactly eleven digits', () {
    expect(SalesRequestSubmission.validPhone('07801234567'), isTrue);
    for (final phone in ['0780123456','078012345678','+9647801234567','08801234567','07801 34567']) {
      expect(SalesRequestSubmission.validPhone(phone), isFalse);
    }
  });
}
