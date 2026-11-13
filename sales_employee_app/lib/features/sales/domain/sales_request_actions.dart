import 'package:sales_employee_app/features/sales/domain/sales_request_tab_map.dart';

/// إجراءات موظف مسموحة حسب تبويب العرض — ليست transfer مفتوحًا لأي enum.
enum SalesRequestEmployeeAction {
  prepare,
  pend,
  reject,
  inspect,
  transferName,
  startSale,
  openLinkedSale,
}

abstract final class SalesRequestActions {
  static List<SalesRequestEmployeeAction> forTab(String tabKey, {required bool hasLinkedOrder}) {
    final tab = tabKey.trim().toLowerCase();
    switch (tab) {
      case SalesRequestTabMap.incoming:
        return [
          SalesRequestEmployeeAction.prepare,
          SalesRequestEmployeeAction.pend,
          SalesRequestEmployeeAction.reject,
          SalesRequestEmployeeAction.transferName,
          SalesRequestEmployeeAction.startSale,
          if (hasLinkedOrder) SalesRequestEmployeeAction.openLinkedSale,
        ];
      case SalesRequestTabMap.prepared:
        return [
          SalesRequestEmployeeAction.inspect,
          SalesRequestEmployeeAction.pend,
          SalesRequestEmployeeAction.reject,
          SalesRequestEmployeeAction.transferName,
          SalesRequestEmployeeAction.startSale,
          if (hasLinkedOrder) SalesRequestEmployeeAction.openLinkedSale,
        ];
      case SalesRequestTabMap.inspected:
        return [
          SalesRequestEmployeeAction.startSale,
          SalesRequestEmployeeAction.pend,
          SalesRequestEmployeeAction.reject,
          SalesRequestEmployeeAction.transferName,
          if (hasLinkedOrder) SalesRequestEmployeeAction.openLinkedSale,
        ];
      case SalesRequestTabMap.pending:
        return [
          SalesRequestEmployeeAction.prepare,
          SalesRequestEmployeeAction.reject,
          SalesRequestEmployeeAction.transferName,
          SalesRequestEmployeeAction.startSale,
          if (hasLinkedOrder) SalesRequestEmployeeAction.openLinkedSale,
        ];
      case SalesRequestTabMap.rejected:
      case SalesRequestTabMap.sold:
        return [
          if (hasLinkedOrder) SalesRequestEmployeeAction.openLinkedSale,
        ];
      default:
        return const [];
    }
  }

  static bool requiresNote(SalesRequestEmployeeAction a) =>
      a == SalesRequestEmployeeAction.prepare ||
      a == SalesRequestEmployeeAction.pend ||
      a == SalesRequestEmployeeAction.reject;

  static String label(SalesRequestEmployeeAction a) {
    switch (a) {
      case SalesRequestEmployeeAction.prepare:
        return 'تجهيز للبيع';
      case SalesRequestEmployeeAction.pend:
        return 'تعليق';
      case SalesRequestEmployeeAction.reject:
        return 'رفض';
      case SalesRequestEmployeeAction.inspect:
        return 'كشف';
      case SalesRequestEmployeeAction.transferName:
        return 'نقل الاسم لزميل';
      case SalesRequestEmployeeAction.startSale:
        return 'بدء / استكمال البيع';
      case SalesRequestEmployeeAction.openLinkedSale:
        return 'فتح البيع المرتبط';
    }
  }
}
