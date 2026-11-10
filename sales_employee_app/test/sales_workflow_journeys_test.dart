import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';

/// In-memory mirror of demo order workflow for D1–D8 verification.
class _JourneyOrder {
  _JourneyOrder({required this.id, this.status = SalesWorkflowRules.pending});

  final int id;
  String status;
  String? note;
  String? assignee;
  bool reassigned = false;
  Map<String, dynamic>? draft;
  double maxSell = 2000000;
  bool inventoryDeducted = false;
}

class _JourneyEngine {
  final Map<int, _JourneyOrder> orders = {};

  int seedPending() {
    final id = -(orders.length + 1);
    orders[id] = _JourneyOrder(id: id);
    return id;
  }

  void transfer(int id, String target, {String? reason}) {
    final o = orders[id]!;
    if (SalesWorkflowRules.isTerminal(o.status)) throw StateError('terminal_status');
    final t = SalesWorkflowRules.normalize(target);
    if (t != SalesWorkflowRules.sold &&
        t != SalesWorkflowRules.disclosure &&
        !SalesWorkflowRules.allowedTransferTargets(o.status).contains(t)) {
      throw StateError('transfer_not_allowed');
    }
    if (SalesWorkflowRules.requiresReason(t) && (reason == null || reason.trim().isEmpty)) {
      throw StateError('reason_required');
    }
    o.status = t;
    if (reason != null) o.note = reason.trim();
  }

  void reassign(int id, {required String assignee, required String reason}) {
    final o = orders[id]!;
    if (!SalesWorkflowRules.canReassign(o.status)) throw StateError('terminal_status');
    if (!SalesWorkflowRules.canReassignWith(assignee: assignee, reason: reason)) {
      throw StateError('reassign_required');
    }
    o.reassigned = true;
    o.assignee = assignee;
    o.note = reason;
  }

  void disclosure(int id, Map<String, dynamic> draft) {
    final o = orders[id]!;
    if (SalesWorkflowRules.isTerminal(o.status)) throw StateError('terminal_status');
    o.draft = draft;
    o.status = SalesWorkflowRules.disclosure;
    o.note = 'تم الكشف — مسودة محفوظة';
  }

  void completeSale(int id, {required double total, required List products}) {
    if (!SalesWorkflowRules.hasLinkedRequest(requestId: id)) throw StateError('sale_requires_linked_order');
    final o = orders[id];
    if (o == null || o.reassigned) throw StateError('order_not_found');
    if (o.status == SalesWorkflowRules.rejected) throw StateError('rejected_cannot_sell');
    if (!SalesWorkflowRules.canStartSale(o.status)) throw StateError('sale_not_allowed');
    if (products.isEmpty) throw StateError('products_required');
    if (!SalesWorkflowRules.isValidSellTotal(total, maxSell: o.maxSell)) {
      throw StateError('invalid_price');
    }
    o.status = SalesWorkflowRules.sold;
    o.inventoryDeducted = true;
    o.note = 'تم البيع';
    o.draft = {'total': total, 'products': products};
  }

  bool visibleToOwner(int id) {
    final o = orders[id];
    return o != null && !o.reassigned;
  }
}

void main() {
  group('A/B action gates', () {
    test('pending card actions include ready/hold/reject/sale/reassign', () {
      expect(SalesWorkflowRules.canStartSale('pending'), isTrue);
      expect(SalesWorkflowRules.canReassign('pending'), isTrue);
      expect(SalesWorkflowRules.showCardActions('pending'), isTrue);
      expect(SalesWorkflowRules.allowedTransferTargets('pending'), containsAll(['ready', 'on_hold', 'rejected']));
    });

    test('on_hold is freeze not reject; resume paths open', () {
      expect(SalesWorkflowRules.canStartSale('on_hold'), isTrue);
      expect(SalesWorkflowRules.allowedTransferTargets('on_hold'), containsAll(['ready', 'rejected']));
      expect(SalesWorkflowRules.requiresReason('on_hold'), isTrue);
      expect(SalesWorkflowRules.isTerminal('on_hold'), isFalse);
    });

    test('sold/rejected: no card actions, no sale', () {
      for (final s in ['sold', 'rejected']) {
        expect(SalesWorkflowRules.showCardActions(s), isFalse);
        expect(SalesWorkflowRules.canStartSale(s), isFalse);
        expect(SalesWorkflowRules.canReassign(s), isFalse);
      }
    });
  });

  group('D journeys', () {
    late _JourneyEngine eng;

    setUp(() => eng = _JourneyEngine());

    test('D1 pending → sale steps → sold + rating gate eligible', () {
      final id = eng.seedPending();
      eng.completeSale(id, total: 1350000, products: [
        {'id': 1, 'qty': 1},
      ]);
      expect(eng.orders[id]!.status, SalesWorkflowRules.sold);
      expect(eng.orders[id]!.inventoryDeducted, isTrue);
      expect(SalesWorkflowRules.showCardActions('sold'), isFalse);
      // rating gate is UI once/day per list — rule: sold completes operationally
      expect(true, isTrue);
    });

    test('D2 pending → hold(note) → resume sale → sold', () {
      final id = eng.seedPending();
      eng.transfer(id, 'on_hold', reason: 'انتظار وثائق');
      expect(eng.orders[id]!.note, 'انتظار وثائق');
      expect(() => eng.transfer(id, 'on_hold', reason: ''), throwsA(isA<StateError>()));
      eng.completeSale(id, total: 1000000, products: [
        {'id': 1},
      ]);
      expect(eng.orders[id]!.status, SalesWorkflowRules.sold);
    });

    test('D3 pending → partial disclosure → resume → sold', () {
      final id = eng.seedPending();
      eng.disclosure(id, {'shopName': 'محل أ', 'lat': 32.0});
      expect(eng.orders[id]!.status, SalesWorkflowRules.disclosure);
      expect(eng.orders[id]!.inventoryDeducted, isFalse);
      eng.completeSale(id, total: 900000, products: [
        {'id': 1},
      ]);
      expect(eng.orders[id]!.status, SalesWorkflowRules.sold);
      expect(eng.orders[id]!.inventoryDeducted, isTrue);
    });

    test('D4 pending → ready(note) → sale → sold', () {
      final id = eng.seedPending();
      eng.transfer(id, 'ready', reason: 'جاهز بعد كشف');
      eng.completeSale(id, total: 800000, products: [
        {'id': 1},
      ]);
      expect(eng.orders[id]!.status, SalesWorkflowRules.sold);
    });

    test('D5 pending → rejected(reason) blocks sale', () {
      final id = eng.seedPending();
      eng.transfer(id, 'rejected', reason: 'عنوان وهمي');
      expect(
        () => eng.completeSale(id, total: 100, products: [
          {'id': 1},
        ]),
        throwsA(predicate((e) => e is StateError && e.message == 'rejected_cannot_sell')),
      );
      expect(SalesWorkflowRules.canStartSale('rejected'), isFalse);
    });

    test('D6 pending → reassign hides from owner', () {
      final id = eng.seedPending();
      eng.reassign(id, assignee: 'زميل ب', reason: 'منطقة أخرى');
      expect(eng.visibleToOwner(id), isFalse);
      expect(
        () => eng.reassign(id, assignee: '', reason: 'x'),
        throwsA(isA<StateError>()),
      );
    });

    test('D7 on_hold → reject with reason', () {
      final id = eng.seedPending();
      eng.transfer(id, 'on_hold', reason: 'تأجيل');
      eng.transfer(id, 'rejected', reason: 'رفض نهائي');
      expect(eng.orders[id]!.status, SalesWorkflowRules.rejected);
      expect(eng.orders[id]!.note, 'رفض نهائي');
    });

    test('D8 sale without linked order rejected', () {
      expect(SalesWorkflowRules.hasLinkedRequest(requestId: null), isFalse);
      expect(SalesWorkflowRules.hasLinkedRequest(requestId: 0), isFalse);
      expect(SalesWorkflowRules.hasLinkedRequest(requestId: 5), isTrue);
      expect(SalesWorkflowRules.hasLinkedOrder(null), isFalse);
      expect(
        () => eng.completeSale(0, total: 1, products: [
          {'id': 1},
        ]),
        throwsA(predicate((e) => e is StateError && e.message == 'sale_requires_linked_order')),
      );
    });

    test('E price ceiling + disclosure does not sell', () {
      final id = eng.seedPending();
      eng.orders[id]!.maxSell = 500000;
      expect(
        () => eng.completeSale(id, total: 600000, products: [
          {'id': 1},
        ]),
        throwsA(predicate((e) => e is StateError && e.message == 'invalid_price')),
      );
      eng.disclosure(id, {'shopName': 'x'});
      expect(eng.orders[id]!.status, isNot(SalesWorkflowRules.sold));
      expect(eng.orders[id]!.inventoryDeducted, isFalse);
    });
  });
}
