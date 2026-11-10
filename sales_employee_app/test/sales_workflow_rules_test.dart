import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';

void main() {
  test('reason required for ready/hold/reject', () {
    expect(SalesWorkflowRules.requiresReason('ready'), isTrue);
    expect(SalesWorkflowRules.requiresReason('on_hold'), isTrue);
    expect(SalesWorkflowRules.requiresReason('rejected'), isTrue);
    expect(SalesWorkflowRules.requiresReason('sold'), isFalse);
  });

  test('terminal statuses have no actions', () {
    expect(SalesWorkflowRules.showCardActions('sold'), isFalse);
    expect(SalesWorkflowRules.showCardActions('rejected'), isFalse);
    expect(SalesWorkflowRules.canStartSale('rejected'), isFalse);
    expect(SalesWorkflowRules.canStartSale('pending'), isTrue);
    expect(SalesWorkflowRules.canStartSale('on_hold'), isTrue);
    expect(SalesWorkflowRules.canStartSale('disclosure'), isTrue);
  });

  test('pending transfers exclude sold/disclosure direct', () {
    final t = SalesWorkflowRules.allowedTransferTargets('pending');
    expect(t, containsAll(['ready', 'on_hold', 'rejected']));
    expect(t.contains('sold'), isFalse);
  });
}
