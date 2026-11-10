/// Operational rules for sales request bins (business logic only).
abstract final class SalesWorkflowRules {
  static const pending = 'pending';
  static const ready = 'ready';
  static const disclosure = 'disclosure';
  static const onHold = 'on_hold';
  static const rejected = 'rejected';
  static const sold = 'sold';

  static String normalize(String? status) => (status ?? pending).trim().toLowerCase().replaceAll('-', '_');

  static bool isTerminal(String? status) {
    final s = normalize(status);
    return s == rejected || s == sold;
  }

  static bool requiresReason(String targetStatus) {
    final t = normalize(targetStatus);
    return t == rejected || t == onHold || t == ready;
  }

  /// Status transitions allowed from [current] via explicit transfer (not via wizard).
  static List<String> allowedTransferTargets(String? current) {
    final s = normalize(current);
    if (isTerminal(s)) return const [];
    switch (s) {
      case pending:
        return const [ready, onHold, rejected];
      case ready:
        return const [onHold, rejected, pending];
      case onHold:
        return const [ready, rejected, pending];
      case disclosure:
        return const [onHold, rejected, ready];
      default:
        return const [ready, onHold, rejected];
    }
  }

  static bool canStartSale(String? status) {
    final s = normalize(status);
    return s == pending || s == ready || s == onHold || s == disclosure;
  }

  static bool canReassign(String? status) => !isTerminal(status);

  static bool showCardActions(String? status) => !isTerminal(status);

  /// إثبات وجود طلب عمل — requestId فقط. orderId وحده لا يكفي.
  static bool hasLinkedRequest({int? requestId, int? orderId}) {
    if (requestId != null && requestId != 0) return true;
    // orderId مُتجاهل عمدًا — التباس hasLinkedRequest(orderId) القديم
    return false;
  }

  static bool hasLinkedOrder(int? linkedOrderId) =>
      linkedOrderId != null && linkedOrderId != 0;

  static bool canReassignWith({required String? assignee, required String? reason}) {
    return (assignee?.trim().isNotEmpty ?? false) && (reason?.trim().isNotEmpty ?? false);
  }

  static bool isValidSellTotal(double total, {double? maxSell}) {
    if (total <= 0) return false;
    if (maxSell != null && total > maxSell) return false;
    return true;
  }

  static String label(String key) {
    switch (normalize(key)) {
      case pending:
        return 'طلبات البيع';
      case ready:
        return 'جاهز للبيع';
      case disclosure:
        return 'تم الكشف';
      case onHold:
        return 'معلقة';
      case rejected:
        return 'مرفوض';
      case sold:
        return 'تم البيع';
      default:
        return key;
    }
  }
}
