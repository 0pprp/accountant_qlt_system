import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/core/widgets/qalaat_pattern_background.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/presentation/sale_details_page.dart';
import 'package:sales_employee_app/injection.dart';

class OrdersTab extends StatefulWidget {
  const OrdersTab({super.key});

  @override
  State<OrdersTab> createState() => _OrdersTabState();
}

class _OrdersTabState extends State<OrdersTab> {
  List<SaleOrder> _orders = const [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final list = await Injection.salesRemote.getOrders();
      if (!mounted) return;
      setState(() {
        _orders = list;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'سجلات البيع', message: DioClient.mapError(e));
    }
  }

  @override
  Widget build(BuildContext context) {
    return QalaatPatternBackground(
      child: SafeArea(
        child: Column(
          children: [
            const Padding(
              padding: EdgeInsets.fromLTRB(20, 16, 20, 8),
              child: Align(
                alignment: Alignment.centerRight,
                child: Text(
                  'سجلات البيع',
                  style: TextStyle(fontSize: 22, fontWeight: FontWeight.w700, color: AppColors.primary),
                ),
              ),
            ),
            Expanded(
              child: RefreshIndicator(
                onRefresh: _load,
                child: _loading
                    ? ListView(children: const [SizedBox(height: 120), Center(child: CircularProgressIndicator())])
                    : _orders.isEmpty
                        ? ListView(
                            children: const [
                              SizedBox(height: 120),
                              Center(child: Text('لا توجد سجلات بيع')),
                            ],
                          )
                        : ListView.builder(
                            padding: const EdgeInsets.fromLTRB(12, 0, 12, 100),
                            itemCount: _orders.length,
                            itemBuilder: (_, i) {
                              final o = _orders[i];
                              return Card(
                                child: ListTile(
                                  title: Text(o.customerName),
                                  subtitle: Text('#${o.id} • ${o.createdAt ?? o.status ?? ''}'),
                                  trailing: Text(o.total?.toStringAsFixed(0) ?? ''),
                                  onTap: () => Navigator.of(context).push(
                                    MaterialPageRoute(builder: (_) => SaleDetailsPage(orderId: o.id)),
                                  ),
                                ),
                              );
                            },
                          ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
