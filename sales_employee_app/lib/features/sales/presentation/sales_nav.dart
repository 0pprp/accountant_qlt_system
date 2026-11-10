import 'package:flutter/material.dart';
import 'package:sales_employee_app/features/sales/presentation/complete_sale_wizard_page.dart';
import 'package:sales_employee_app/features/sales/presentation/sale_details_page.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_request_detail_page.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_requests_bin_page.dart';

/// توجيه طلبات البيع vs سجلات Order — requestId ≠ linkedOrderId/orderId.
abstract final class SalesNav {
  /// صندوق «طلبات البيع» (SalesRequest incoming) — ليس قائمة Orders.
  static Widget salesRequestsInbox({Key? key}) => SalesRequestsBinPage(
        key: key,
        uiStatusKey: 'pending',
        title: 'طلبات البيع',
      );

  static MaterialPageRoute<void> toRequestDetail(int requestId) {
    assert(requestId > 0, 'requestId must be a SalesRequest id');
    return MaterialPageRoute<void>(
      settings: RouteSettings(name: '/sales-requests/$requestId'),
      builder: (_) => SalesRequestDetailPage(requestId: requestId),
    );
  }

  /// فتح سجل بيع مرتبط — orderId فقط، لا تمرّر requestId هنا.
  static MaterialPageRoute<void> toLinkedOrder(int orderId) {
    assert(orderId > 0, 'orderId must be a linked Order id, not requestId');
    return MaterialPageRoute<void>(
      settings: RouteSettings(name: '/orders/$orderId'),
      builder: (_) => SaleDetailsPage(orderId: orderId),
    );
  }

  static MaterialPageRoute<void> toCompleteSale(int orderId) {
    assert(orderId > 0, 'complete-sale uses linked Order id');
    return MaterialPageRoute<void>(
      settings: RouteSettings(name: '/orders/$orderId/complete'),
      builder: (_) => CompleteSaleWizardPage(orderId: orderId),
    );
  }

  /// يميّز مسار الطلب عن مسار البيع في الاختبارات والتوجيه.
  static bool isSalesRequestRoute(String? name) =>
      name != null && name.startsWith('/sales-requests/');

  static bool isOrderRoute(String? name) =>
      name != null && (name.startsWith('/orders/') || name == '/orders');
}
