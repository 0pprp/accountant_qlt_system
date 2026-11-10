import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class PendingSalesPage extends StatefulWidget {
  const PendingSalesPage({super.key});

  @override
  State<PendingSalesPage> createState() => _PendingSalesPageState();
}

class _PendingSalesPageState extends State<PendingSalesPage> {
  List<Map<String, dynamic>> _rows = const [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final rows = await Injection.committee.pendingSales();
      if (!mounted) return;
      setState(() {
        _rows = rows;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'فشل التحميل', message: DioClient.mapError(e), color: AppColors.danger);
    }
  }

  Future<void> _authenticate(Map<String, dynamic> row) async {
    final orderId = int.tryParse('${row['orderId'] ?? row['OrderId'] ?? 0}') ?? 0;
    final name = '${row['customerName'] ?? row['CustomerName'] ?? ''}';
    if (orderId <= 0) return;

    final ratingCtrl = TextEditingController();
    final noteCtrl = TextEditingController();
    final stockCtrl = TextEditingController();
    final dailyCtrl = TextEditingController();
    String? localError;

    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setLocal) => AlertDialog(
          title: Text('مصادقة البيع — $name'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: ratingCtrl,
                  keyboardType: TextInputType.number,
                  inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                  decoration: const InputDecoration(labelText: 'التقييم (1–10)'),
                ),
                TextField(
                  controller: noteCtrl,
                  maxLines: 2,
                  decoration: const InputDecoration(labelText: 'ملاحظة (إلزامية إن التقييم أقل من 7)'),
                ),
                TextField(
                  controller: stockCtrl,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(labelText: 'قيمة بضاعة المحل'),
                ),
                TextField(
                  controller: dailyCtrl,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(labelText: 'قيمة الوارد اليومي'),
                ),
                if (localError != null)
                  Padding(
                    padding: const EdgeInsets.only(top: 8),
                    child: Text(localError!, style: const TextStyle(color: AppColors.danger)),
                  ),
              ],
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('إلغاء')),
            TextButton(
              onPressed: () {
                final rating = int.tryParse(ratingCtrl.text.trim()) ?? 0;
                final stock = double.tryParse(stockCtrl.text.trim().replaceAll(',', '')) ?? 0;
                final daily = double.tryParse(dailyCtrl.text.trim().replaceAll(',', '')) ?? 0;
                if (rating < 1 || rating > 10) {
                  setLocal(() => localError = 'التقييم يجب أن يكون من 1 إلى 10');
                  return;
                }
                if (rating < 7 && noteCtrl.text.trim().isEmpty) {
                  setLocal(() => localError = 'الملاحظة إجبارية عندما يكون التقييم أقل من 7');
                  return;
                }
                if (stock <= 0 || daily <= 0) {
                  setLocal(() => localError = 'قيم بضاعة المحل والوارد اليومي مطلوبة وأكبر من صفر');
                  return;
                }
                Navigator.pop(ctx, true);
              },
              child: const Text('اعتماد'),
            ),
          ],
        ),
      ),
    );

    final rating = int.tryParse(ratingCtrl.text.trim()) ?? 0;
    final note = noteCtrl.text.trim();
    final stock = double.tryParse(stockCtrl.text.trim().replaceAll(',', '')) ?? 0;
    final daily = double.tryParse(dailyCtrl.text.trim().replaceAll(',', '')) ?? 0;
    ratingCtrl.dispose();
    noteCtrl.dispose();
    stockCtrl.dispose();
    dailyCtrl.dispose();
    if (ok != true) return;

    try {
      await Injection.committee.authenticatePendingSale(
        orderId,
        rating: rating,
        note: note.isEmpty ? null : note,
        shopStockEstimatedValue: stock,
        estimatedDailyRevenue: daily,
      );
      if (!mounted) return;
      await showAppMessage(context, title: 'تم', message: 'تمت المصادقة بنجاح', color: AppColors.success);
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(context, title: 'فشل التصديق', message: DioClient.mapError(e), color: AppColors.danger);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(title: const Text('المبيعات / تصديق')),
      body: AnimatedWavesBackground(
        child: RefreshIndicator(
          onRefresh: _load,
          child: _loading
              ? ListView(children: const [SizedBox(height: 160), Center(child: CircularProgressIndicator())])
              : _rows.isEmpty
                  ? ListView(children: const [SizedBox(height: 160), Center(child: Text('لا مبيعات معلّقة للمصادقة'))])
                  : ListView.separated(
                      padding: EdgeInsets.fromLTRB(16, 12, 16, 24 + bottom),
                      itemCount: _rows.length,
                      separatorBuilder: (_, _) => const SizedBox(height: 8),
                      itemBuilder: (_, i) {
                        final r = _rows[i];
                        return Material(
                          color: Colors.white.withValues(alpha: 0.92),
                          borderRadius: BorderRadius.circular(14),
                          child: ListTile(
                            title: Text(
                              '${r['customerName'] ?? r['CustomerName'] ?? ''}',
                              style: const TextStyle(fontWeight: FontWeight.w700),
                            ),
                            subtitle: Text('طلب #${r['orderId'] ?? r['OrderId'] ?? ''}'),
                            trailing: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Text(
                                  '${r['sellAmount'] ?? r['SellAmount'] ?? 0}',
                                  style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.w700),
                                ),
                                const SizedBox(width: 8),
                                ElevatedButton(
                                  onPressed: () => _authenticate(r),
                                  style: ElevatedButton.styleFrom(
                                    minimumSize: const Size(72, 36),
                                    padding: const EdgeInsets.symmetric(horizontal: 10),
                                  ),
                                  child: const Text('تصديق'),
                                ),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
        ),
      ),
    );
  }
}
