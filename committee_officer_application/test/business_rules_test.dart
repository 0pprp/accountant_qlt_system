import 'package:committee_officer_application/features/lists/domain/daily_customer.dart';
import 'package:committee_officer_application/features/shift/data/shift_remote.dart';
import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:flutter_test/flutter_test.dart';

import 'session_network_test.dart' show MemorySession, Adapter, json;

void main() {
  DailyCustomer customer(double paid) => DailyCustomer(
    id: 1,
    name: 'أحمد علي',
    phone: '07701234567',
    paid: paid,
    installment: 10000,
  );
  test('partial payments belong to paid and below-installment filters', () {
    expect(customer(5000).hasPayment, isTrue);
    expect(
      customer(5000).matchesPayment(PaymentFilter.belowInstallment),
      isTrue,
    );
    expect(
      customer(10000).matchesPayment(PaymentFilter.fullInstallment),
      isTrue,
    );
    expect(
      customer(15000).matchesPayment(PaymentFilter.fullInstallment),
      isTrue,
    );
    expect(customer(0).matchesPayment(PaymentFilter.all), isFalse);
    expect(customer(0).matchesPayment(PaymentFilter.belowInstallment), isFalse);
  });
  test('search accepts name and telephone and trims whitespace', () {
    expect(customer(0).matches(' أحمد '), isTrue);
    expect(customer(0).matches('0770'), isTrue);
    expect(customer(0).matches('غير موجود'), isFalse);
  });
  test('daily payload accepts both API casing styles', () {
    final row = DailyCustomer.fromJson({
      'CustomerId': 4,
      'FullName': 'علي',
      'PaidToday': '5000',
      'DailyInstallment': 10000,
    });
    expect(row.id, 4);
    expect(row.belowInstallment, isTrue);
  });
  test(
    'shift network errors cannot grant entry or silently end the shift',
    () async {
      final session = MemorySession();
      final client = DioClient(
        sessionStore: session,
        onRefresh: () async => false,
      );
      client.dio.httpClientAdapter = Adapter((_) => json('{}', 503));
      final shift = ShiftRemote(client, session);
      await expectLater(shift.currentActive(), throwsException);
      await expectLater(shift.end(), throwsException);
    },
  );
  test(
    'server shift state is authoritative and malformed state fails closed',
    () async {
      final session = MemorySession();
      final client = DioClient(
        sessionStore: session,
        onRefresh: () async => false,
      );
      client.dio.httpClientAdapter = Adapter(
        (_) => json('{"hasActiveShift":false}', 200),
      );
      expect(await ShiftRemote(client, session).currentActive(), isFalse);
      client.dio.httpClientAdapter = Adapter((_) => json('{}', 200));
      await expectLater(
        ShiftRemote(client, session).currentActive(),
        throwsException,
      );
    },
  );
}
