import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sales_request_item.dart';

void main() {
  test('parses assigned request without Order', () {
    final item = SalesRequestItem.fromJson({
      'requestId': 42,
      'customerName': 'زبون تجريبي',
      'customerPhone': '0700000000',
      'status': 2,
      'statusLabel': 'مسند',
      'tabKey': 'incoming',
      'linkedOrderId': null,
      'isReturned': false,
    });
    expect(item.requestId, 42);
    expect(item.hasLinkedOrder, isFalse);
    expect(item.requestId, isNot(item.linkedOrderId));
  });

  test('returned flag and note preserved', () {
    final item = SalesRequestItem.fromJson({
      'requestId': 7,
      'customerName': 'عائد',
      'status': 4,
      'statusLabel': 'مُعاد',
      'tabKey': 'incoming',
      'isReturned': true,
      'lastStatusNote': 'أرجع للإكمال',
      'linkedOrderId': 99,
    });
    expect(item.isReturned, isTrue);
    expect(item.lastStatusNote, 'أرجع للإكمال');
    expect(item.hasLinkedOrder, isTrue);
    expect(item.linkedOrderId, 99);
  });
}
