import 'package:delegate_app/features/payments/domain/payment_collection_rules.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('PaymentCollectionRules', () {
    test('rejects non-positive amount', () {
      expect(PaymentCollectionRules.validateAmount(0), isNotNull);
      expect(PaymentCollectionRules.validateAmount(-1), isNotNull);
    });

    test('rejects below minimum 2000', () {
      expect(PaymentCollectionRules.validateAmount(1000), isNotNull);
      expect(PaymentCollectionRules.validateAmount(1999), isNotNull);
    });

    test('accepts minimum 2000 when divisible by 1000', () {
      expect(PaymentCollectionRules.validateAmount(2000), isNull);
      expect(PaymentCollectionRules.validateAmount(10000), isNull);
    });

    test('rejects amount above remaining', () {
      expect(
        PaymentCollectionRules.validateAmount(3000, remaining: 2000),
        isNotNull,
      );
      expect(
        PaymentCollectionRules.validateAmount(2001, remaining: 2000),
        isNotNull,
      );
    });

    test('accepts amount equal to remaining', () {
      expect(
        PaymentCollectionRules.validateAmount(2000, remaining: 2000),
        isNull,
      );
      expect(
        PaymentCollectionRules.validateAmount(5500, remaining: 5500),
        isNull,
      );
    });

    test('accepts remaining below minimum when settling in full', () {
      expect(
        PaymentCollectionRules.validateAmount(1500, remaining: 1500),
        isNull,
      );
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
