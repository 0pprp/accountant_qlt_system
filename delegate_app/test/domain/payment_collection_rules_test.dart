import 'package:delegate_app/features/payments/domain/payment_collection_rules.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('PaymentCollectionRules', () {
    test('rejects non-positive amount', () {
      expect(PaymentCollectionRules.validateAmount(0), isNotNull);
      expect(PaymentCollectionRules.validateAmount(-1), isNotNull);
    });

    test('accepts positive amount', () {
      expect(PaymentCollectionRules.validateAmount(10000), isNull);
    });

    test('blocks when already collected on business day', () {
      expect(
        PaymentCollectionRules.blocksNewCollection(
          alreadyCollectedToday: true,
          hasUnsyncedPending: false,
        ),
        isTrue,
      );
    });

    test('blocks when unsynced pending exists', () {
      expect(
        PaymentCollectionRules.blocksNewCollection(
          alreadyCollectedToday: false,
          hasUnsyncedPending: true,
        ),
        isTrue,
      );
    });

    test('allows when clear', () {
      expect(
        PaymentCollectionRules.blocksNewCollection(
          alreadyCollectedToday: false,
          hasUnsyncedPending: false,
        ),
        isFalse,
      );
    });
  });
}
