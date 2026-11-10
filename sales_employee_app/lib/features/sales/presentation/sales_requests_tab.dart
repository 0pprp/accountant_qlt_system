import 'package:flutter/material.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_nav.dart';

/// تبويب التنقل السفلي «طلبات البيع» → صناديق SalesRequest (ليس Orders).
class SalesRequestsTab extends StatelessWidget {
  const SalesRequestsTab({super.key});

  @override
  Widget build(BuildContext context) {
    return SalesNav.salesRequestsInbox();
  }
}
