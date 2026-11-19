import 'package:flutter/material.dart';
import '../../../injection.dart';
import '../../payments/presentation/collect_payment_sheet.dart';
import '../../customers/domain/entities/customer.dart';

class NewSalesForRatingPage extends StatefulWidget {
  const NewSalesForRatingPage({super.key});
  @override
  State<NewSalesForRatingPage> createState() => _NewSalesForRatingState();
}

class _NewSalesForRatingState extends State<NewSalesForRatingPage> {
  final _rows = <Map<String, dynamic>>[];
  bool _busy = false, _more = false;
  String? _error;
  static const _path = 'api/v1/app/SalesRequests/delegate/new-sales';

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load({bool append = false}) async {
    if (_busy) return;
    setState(() { _busy = true; _error = null; });
    try {
      final res = await Injection.dioClient.dio.get(_path, queryParameters: {
        'afterId': append && _rows.isNotEmpty ? _rows.last['orderId'] : 0,
      });
      final page = (res.data as List).cast<Map<String, dynamic>>();
      if (!mounted) return;
      setState(() {
        if (!append) _rows.clear();
        _rows.addAll(page);
        _more = page.length == 50;
      });
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر تحميل المبيعات');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _collect(Map<String, dynamic> row) async {
    if (_busy) return;
    setState(() { _busy = true; _error = null; });
    try {
      final customer = Customer(
        customerId: row['customerId'] as int, name: row['customer'] as String,
        delegateId: row['delegateId'] as int, activeOrderId: row['orderId'] as int,
        dailyInstallment: (row['dailyInstallment'] as num).toDouble(),
      );
      final payment = await showCollectPaymentSheet(context, customer, submitSaleRating: (rating) async {
        await Injection.dioClient.dio.post('$_path/${row['orderId']}/evaluate', data: {
          'orderId': row['orderId'], 'rating': rating.rating, 'note': rating.note,
          'shopStockEstimatedValue': rating.shopStockEstimatedValue,
          'estimatedDailyRevenue': rating.estimatedDailyRevenue,
        });
      });
      if (mounted && payment != null) setState(() => _rows.remove(row));
    } catch (_) {
      if (mounted) setState(() => _error = 'تعذر فتح التسديد. حدّث القائمة وأعد المحاولة.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('المبيعات الجديدة'), actions: [
      IconButton(onPressed: _busy ? null : () => _load(), icon: const Icon(Icons.refresh), tooltip: 'تحديث'),
    ]),
    body: SafeArea(child: ListView(padding: const EdgeInsets.all(16), children: [
      if (_busy) const LinearProgressIndicator(),
      if (_error != null) Text(_error!),
      if (!_busy && _error == null && _rows.isEmpty) const Center(child: Text('لا توجد مبيعات جديدة مستحقة للتسديد')),
      for (final sale in _rows) Card(child: ListTile(
        title: Text('${sale['customer']}'), subtitle: Text("${sale['listName']} · ${sale['saleDate']}\nمبيع #${sale['orderId']} · ${sale['amount']} د.ع"),
        trailing: TextButton(onPressed: _busy ? null : () => _collect(sale), child: const Text('تسديد')),
      )),
      if (_more) TextButton(onPressed: _busy ? null : () => _load(append:true), child: const Text('عرض المزيد')),
    ])),
  );
}
