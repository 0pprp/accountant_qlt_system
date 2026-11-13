import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/domain/sales_request_actions.dart';
import 'package:sales_employee_app/features/sales/domain/sales_request_tab_map.dart';

void main() {
  test('incoming allows prepare/pend/reject and startSale (bins are storage)', () {
    final actions = SalesRequestActions.forTab(
      SalesRequestTabMap.incoming,
      hasLinkedOrder: false,
    );
    expect(actions, contains(SalesRequestEmployeeAction.prepare));
    expect(actions, contains(SalesRequestEmployeeAction.pend));
    expect(actions, contains(SalesRequestEmployeeAction.reject));
    expect(actions, contains(SalesRequestEmployeeAction.startSale));
  });

  test('prepared allows inspect and startSale', () {
    final actions = SalesRequestActions.forTab(
      SalesRequestTabMap.prepared,
      hasLinkedOrder: false,
    );
    expect(actions, contains(SalesRequestEmployeeAction.inspect));
    expect(actions, contains(SalesRequestEmployeeAction.startSale));
  });

  test('pending allows startSale without matching gate', () {
    final actions = SalesRequestActions.forTab(
      SalesRequestTabMap.pending,
      hasLinkedOrder: false,
    );
    expect(actions, contains(SalesRequestEmployeeAction.startSale));
  });

  test('sold/rejected have no mutating actions without linked order', () {
    expect(
      SalesRequestActions.forTab(SalesRequestTabMap.sold, hasLinkedOrder: false),
      isEmpty,
    );
    expect(
      SalesRequestActions.forTab(SalesRequestTabMap.rejected, hasLinkedOrder: false),
      isEmpty,
    );
  });

  test('prepare, pend and reject require note', () {
    expect(SalesRequestActions.requiresNote(SalesRequestEmployeeAction.prepare), isTrue);
    expect(SalesRequestActions.requiresNote(SalesRequestEmployeeAction.pend), isTrue);
    expect(SalesRequestActions.requiresNote(SalesRequestEmployeeAction.reject), isTrue);
    expect(SalesRequestActions.requiresNote(SalesRequestEmployeeAction.inspect), isFalse);
  });
}
