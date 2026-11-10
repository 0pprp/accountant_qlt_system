import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/domain/sales_request_tab_map.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';

void main() {
  test('UI pending (طلبات البيع) maps to incoming not SalesRequest.Pending hold', () {
    expect(SalesRequestTabMap.apiTabForUiKey('pending'), SalesRequestTabMap.incoming);
    expect(SalesRequestTabMap.apiTabForUiKey('on_hold'), SalesRequestTabMap.pending);
  });

  test('requestId and orderId are distinct concepts', () {
    expect(SalesWorkflowRules.hasLinkedRequest(requestId: 9), isTrue);
    expect(SalesWorkflowRules.hasLinkedRequest(orderId: 9), isFalse);
    expect(SalesWorkflowRules.hasLinkedOrder(9), isTrue);
  });
}
