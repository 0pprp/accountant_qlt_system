import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/network/json_map.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/animated_waves.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/catalog/domain/entities/product_item.dart';
import 'package:sales_employee_app/injection.dart';

class WarehousePage extends StatefulWidget {
  const WarehousePage({super.key});

  @override
  State<WarehousePage> createState() => _WarehousePageState();
}

class _WarehousePageState extends State<WarehousePage> {
  List<ProductItem> _items = const [];
  String _branchLabel = 'مخزن فرعك / محافظتك';
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      try {
        final profile = await Injection.dioClient.dio.get('api/v1/app/Users/profile');
        if (profile.data is Map) {
          final m = Map<String, dynamic>.from(profile.data as Map);
          final branches = JsonMap.pick(m, ['branches', 'Branches']);
          if (branches is List && branches.isNotEmpty && branches.first is Map) {
            final b = Map<String, dynamic>.from(branches.first as Map);
            final name = JsonMap.str(b, ['name', 'Name', 'governorate', 'Governorate']);
            if (name.isNotEmpty) _branchLabel = 'مخزن: $name';
          }
        }
      } catch (_) {}

      final list = await Injection.catalogRemote.getInventory();
      if (!mounted) return;
      setState(() {
        _items = list;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'المخزن', message: DioClient.mapError(e));
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(title: const Text('المخزن')),
      body: AnimatedWavesBackground(
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : RefreshIndicator(
                onRefresh: _load,
                child: ListView(
                  padding: EdgeInsets.fromLTRB(16, 12, 16, 24 + bottom),
                  children: [
                    Container(
                      padding: const EdgeInsets.all(14),
                      decoration: BoxDecoration(
                        color: AppColors.primary.withValues(alpha: 0.08),
                        borderRadius: BorderRadius.circular(14),
                        border: Border.all(color: AppColors.border),
                      ),
                      child: Text(
                        '$_branchLabel\nيُعرض مخزون فرع موظف المبيعات فقط.',
                        style: const TextStyle(height: 1.35),
                      ),
                    ),
                    const SizedBox(height: 12),
                    if (_items.isEmpty)
                      const Padding(
                        padding: EdgeInsets.only(top: 48),
                        child: Center(child: Text('لا توجد أصناف في مخزن فرعك')),
                      )
                    else
                      ..._items.map((p) => _ProductStockCard(product: p)),
                  ],
                ),
              ),
      ),
    );
  }
}

class _ProductStockCard extends StatelessWidget {
  const _ProductStockCard({required this.product});
  final ProductItem product;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(16),
        color: Colors.white.withValues(alpha: 0.9),
        border: Border.all(color: AppColors.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            product.name,
            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 10),
          _row('العدد المتوفر', product.quantity?.toStringAsFixed(0) ?? '—'),
          _row('سعر القسط', product.dailyInstallment?.toStringAsFixed(0) ?? '—'),
          _row('سعر البيع', product.price?.toStringAsFixed(0) ?? '—'),
        ],
      ),
    );
  }

  Widget _row(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Row(
        children: [
          Expanded(
            child: Text(label, style: const TextStyle(color: AppColors.textSecondary, fontSize: 13.5)),
          ),
          Text(value, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 14)),
        ],
      ),
    );
  }
}
