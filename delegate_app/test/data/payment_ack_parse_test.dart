import 'package:delegate_app/features/payments/data/payments_remote_datasource.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  const key = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
  const date = '2026-11-10';

  PaymentUploadResult parse(Map<String, dynamic> data, {String? expectedDate}) {
    return PaymentsRemoteDataSource.parseServerAck(
      data,
      expectedClientPaymentId: key,
      expectedOrderId: 1,
      expectedAmount: 1000,
      expectedDate: expectedDate ?? date,
      httpOk: true,
    );
  }

  test('accepts matching fingerprint with valid paymentId', () {
    final r = parse({
      'paymentId': 42,
      'orderId': 1,
      'amount': 1000,
      'date': date,
      'clientPaymentId': key,
      'alreadyApplied': true,
    });
    expect(r.success, isTrue);
    expect(r.paymentId, 42);
    expect(r.duplicateAck, isTrue);
  });

  test('rejects missing or non-positive paymentId', () {
    expect(
      parse({
        'orderId': 1,
        'amount': 1000,
        'date': date,
        'clientPaymentId': key,
      }).success,
      isFalse,
    );
    expect(
      parse({
        'paymentId': 0,
        'orderId': 1,
        'amount': 1000,
        'date': date,
        'clientPaymentId': key,
      }).success,
      isFalse,
    );
  });

  test('rejects date mismatch', () {
    final r = parse({
      'paymentId': 7,
      'orderId': 1,
      'amount': 1000,
      'date': '2026-11-09',
      'clientPaymentId': key,
    });
    expect(r.success, isFalse);
  });

  test('rejects amount or order mismatch', () {
    expect(
      parse({
        'paymentId': 7,
        'orderId': 2,
        'amount': 1000,
        'date': date,
        'clientPaymentId': key,
      }).success,
      isFalse,
    );
    expect(
      parse({
        'paymentId': 7,
        'orderId': 1,
        'amount': 2000,
        'date': date,
        'clientPaymentId': key,
      }).success,
      isFalse,
    );
  });
  final invalidFields = <String, Map<String, dynamic>>{
    'missing key': {'clientPaymentId': null},
    'different key': {
      'clientPaymentId': 'ffffffff-bbbb-cccc-dddd-eeeeeeeeeeee',
    },
    'missing date': {'date': null},
    'timestamp instead of DateOnly': {'date': '${date}T23:00:00Z'},
    'negative id': {'paymentId': -1},
    'fractional id': {'paymentId': 1.5},
    'fractional order id': {'orderId': 1.5},
  };
  for (final entry in invalidFields.entries) {
    test('retains payment when acknowledgement has ${entry.key}', () {
      expect(
        parse({
          'paymentId': 42,
          'orderId': 1,
          'amount': 1000,
          'date': date,
          'clientPaymentId': key,
          ...entry.value,
        }).success,
        isFalse,
      );
    });
  }
  test('HTTP failure never acknowledges even a matching body', () {
    final result = PaymentsRemoteDataSource.parseServerAck(
      {
        'paymentId': 42,
        'orderId': 1,
        'amount': 1000,
        'date': date,
        'clientPaymentId': key,
      },
      expectedClientPaymentId: key,
      expectedOrderId: 1,
      expectedAmount: 1000,
      expectedDate: date,
      httpOk: false,
    );
    expect(result.success, isFalse);
  });
}
