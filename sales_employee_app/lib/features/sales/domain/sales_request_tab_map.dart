/// تبويبات طلبات SalesRequest — لا تخلط مع SalesWorkflowRules.pending (Order).
abstract final class SalesRequestTabMap {
  static const incoming = 'incoming'; // طلبات البيع
  static const prepared = 'prepared';
  static const inspected = 'inspected';
  static const pending = 'pending'; // معلقة = SalesRequest.Pending
  static const rejected = 'rejected';
  static const sold = 'sold';

  /// مفاتيح واجهة الموظف → تبويب API
  static String apiTabForUiKey(String uiKey) {
    switch (uiKey.trim().toLowerCase().replaceAll('-', '_')) {
      case 'pending':
      case 'incoming':
      case 'requests':
        // UI القديم سمّى «طلبات البيع» pending على Order — هنا نوجّه إلى incoming
        return incoming;
      case 'ready':
      case 'prepared':
        return prepared;
      case 'disclosure':
      case 'inspected':
        return inspected;
      case 'on_hold':
      case 'onhold':
      case 'hold':
        return pending; // معلقة
      case 'rejected':
        return rejected;
      case 'sold':
      case 'completed':
        return sold;
      default:
        return incoming;
    }
  }

  static String label(String tab) {
    switch (tab) {
      case incoming:
        return 'طلبات البيع';
      case prepared:
        return 'جاهز للبيع';
      case inspected:
        return 'تم الكشف';
      case pending:
        return 'معلقة';
      case rejected:
        return 'مرفوض';
      case sold:
        return 'تم البيع';
      default:
        return tab;
    }
  }
}
