import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/presentation/sale_details_page.dart';
import 'package:sales_employee_app/injection.dart';

class PendingSalesPage extends StatefulWidget {
  const PendingSalesPage({super.key});

  @override
  State<PendingSalesPage> createState() => _PendingSalesPageState();
}

class _PendingSalesPageState extends State<PendingSalesPage> {
  List<SalesBinSummary> _bins = const [];
  String? _selected;
  List<SaleOrder> _orders = const [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadBins();
  }

  Future<void> _loadBins() async {
    setState(() => _loading = true);
    try {
      final bins = await Injection.salesRemote.getBins();
      if (!mounted) return;
      setState(() {
        _bins = bins;
        _loading = false;
      });
      if (bins.isNotEmpty) await _openBin(bins.first.key);
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'صناديق الحالات', message: DioClient.mapError(e));
    }
  }

  Future<void> _openBin(String key) async {
    setState(() {
      _selected = key;
      _loading = true;
    });
    try {
      final orders = await Injection.salesRemote.getByStatus(key);
      if (!mounted) return;
      setState(() {
        _orders = orders;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'خطأ', message: DioClient.mapError(e));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('صناديق الحالات')),
      body: Column(
        children: [
          SizedBox(
            height: 92,
            child: ListView.separated(
              padding: const EdgeInsets.fromLTRB(12, 12, 12, 8),
              scrollDirection: Axis.horizontal,
              itemCount: _bins.length,
              separatorBuilder: (_, __) => const SizedBox(width: 8),
              itemBuilder: (_, i) {
                final b = _bins[i];
                final selected = b.key == _selected;
                return ChoiceChip(
                  selected: selected,
                  label: Text('${b.label} (${b.count})'),
                  selectedColor: AppColors.primary.withValues(alpha: 0.15),
                  onSelected: (_) => _openBin(b.key),
                );
              },
            ),
          ),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : _orders.isEmpty
                    ? const Center(child: Text('لا توجد عناصر في هذا الصندوق'))
                    : ListView.builder(
                        padding: const EdgeInsets.fromLTRB(12, 0, 12, 20),
                        itemCount: _orders.length,
                        itemBuilder: (_, i) {
                          final o = _orders[i];
                          return Card(
                            child: ListTile(
                              title: Text(o.customerName),
                              subtitle: Text('#${o.id} • ${o.status ?? '-'}'),
                              trailing: const Icon(Icons.chevron_left),
                              onTap: () => Navigator.of(context).push(
                                MaterialPageRoute(builder: (_) => SaleDetailsPage(orderId: o.id)),
                              ),
                            ),
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }
}
