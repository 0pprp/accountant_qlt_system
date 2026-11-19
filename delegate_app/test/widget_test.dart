import 'package:flutter_test/flutter_test.dart';

import 'package:delegate_app/features/payments/domain/payment_collection_rules.dart';

void main() {
  test('Collection amount min 2000 and multiples of 1000', () {
    expect(PaymentCollectionRules.validateAmount(100), isNotNull);
    expect(PaymentCollectionRules.validateAmount(1000), isNotNull);
    expect(PaymentCollectionRules.validateAmount(2000), isNull);
  });
}
