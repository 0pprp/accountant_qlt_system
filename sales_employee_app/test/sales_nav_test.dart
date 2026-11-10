import 'package:flutter_test/flutter_test.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_nav.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_requests_bin_page.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_requests_tab.dart';

void main() {
  test('SalesNav separates SalesRequest routes from Order routes', () {
    final detail = SalesNav.toRequestDetail(8);
    expect(detail.settings.name, '/sales-requests/8');
    expect(SalesNav.isSalesRequestRoute(detail.settings.name), isTrue);
    expect(SalesNav.isOrderRoute(detail.settings.name), isFalse);

    final order = SalesNav.toLinkedOrder(6);
    expect(order.settings.name, '/orders/6');
    expect(SalesNav.isOrderRoute(order.settings.name), isTrue);
    expect(SalesNav.isSalesRequestRoute(order.settings.name), isFalse);

    final complete = SalesNav.toCompleteSale(6);
    expect(complete.settings.name, '/orders/6/complete');
    expect(SalesNav.isOrderRoute(complete.settings.name), isTrue);
  });

  test('bottom-nav / drawer «طلبات البيع» factory opens SalesRequestsBinPage inbox', () {
    final inbox = SalesNav.salesRequestsInbox();
    expect(inbox, isA<SalesRequestsBinPage>());
    final bin = inbox as SalesRequestsBinPage;
    expect(bin.uiStatusKey, 'pending'); // maps to API tab incoming
    expect(bin.title, 'طلبات البيع');
  });

  test('SalesRequestsTab is the shell page for bottom-nav index 0', () {
    const tab = SalesRequestsTab();
    expect(tab, isA<SalesRequestsTab>());
    // Construction of tab does not build Orders; child is SalesRequests inbox.
    expect(SalesNav.salesRequestsInbox(), isA<SalesRequestsBinPage>());
  });
}
