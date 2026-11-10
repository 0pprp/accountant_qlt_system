import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/injection.dart';

class PriceBoardPage extends StatefulWidget {
  const PriceBoardPage({super.key});

  @override
  State<PriceBoardPage> createState() => _PriceBoardPageState();
}

class _PriceBoardPageState extends State<PriceBoardPage> {
  late Future<List<_Ad>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<_Ad>> _load() async {
    final DioClient dio = Injection.dioClient;
    final res = await dio.dio.get('api/v1/app/PriceBoardAds');
    final data = res.data;
    List raw;
    if (data is Map && data['items'] is List) {
      raw = data['items'] as List;
    } else if (data is List) {
      raw = data;
    } else {
      raw = const [];
    }
    return raw.map((e) {
      final m = Map<String, dynamic>.from(e as Map);
      return _Ad(
        productName: '${m['productName'] ?? m['ProductName'] ?? ''}',
        province: '${m['targetProvinceName'] ?? m['TargetProvinceName'] ?? ''}',
        salePrice: _num(m['salePrice'] ?? m['SalePrice']),
        daily: _num(m['dailyInstallment'] ?? m['DailyInstallment']),
        down: _num(m['downPayment'] ?? m['DownPayment']),
      );
    }).toList();
  }

  double _num(Object? v) => double.tryParse('$v') ?? 0;

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        appBar: AppBar(title: const Text('لوحة الأسعار')),
        body: FutureBuilder<List<_Ad>>(
          future: _future,
          builder: (context, snap) {
            if (snap.connectionState != ConnectionState.done) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snap.hasError) {
              return Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text('تعذر تحميل لوحة الأسعار'),
                    const SizedBox(height: 12),
                    FilledButton(
                      onPressed: () => setState(() => _future = _load()),
                      child: const Text('إعادة المحاولة'),
                    ),
                  ],
                ),
              );
            }
            final items = snap.data ?? const [];
            if (items.isEmpty) {
              return const Center(child: Text('لا إعلانات أسعار لمحافظتك حالياً'));
            }
            return ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: items.length,
              separatorBuilder: (_, __) => const SizedBox(height: 10),
              itemBuilder: (context, i) {
                final a = items[i];
                return Card(
                  child: Padding(
                    padding: const EdgeInsets.all(14),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(a.productName,
                            style: const TextStyle(
                                fontSize: 18, fontWeight: FontWeight.bold)),
                        Text(a.province, style: const TextStyle(color: Colors.black54)),
                        const SizedBox(height: 8),
                        Text('سعر البيع: ${a.salePrice}'),
                        Text('القسط اليومي: ${a.daily}'),
                        Text('المقدمة: ${a.down}'),
                      ],
                    ),
                  ),
                );
              },
            );
          },
        ),
      ),
    );
  }
}

class _Ad {
  const _Ad({
    required this.productName,
    required this.province,
    required this.salePrice,
    required this.daily,
    required this.down,
  });
  final String productName;
  final String province;
  final double salePrice;
  final double daily;
  final double down;
}
