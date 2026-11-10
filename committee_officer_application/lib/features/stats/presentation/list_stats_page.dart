import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/core/widgets/ui/data_field.dart';
import 'package:committee_officer_application/core/widgets/ui/entity_card.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

/// إحصائيات قوائم المتابع — ListStats المرجعية؛ ليست ملخص مبيعات عام.
class ListStatsPage extends StatefulWidget {
  const ListStatsPage({super.key});

  @override
  State<ListStatsPage> createState() => _ListStatsPageState();
}

class _ListStatsPageState extends State<ListStatsPage> {
  int _request = 0;
  bool _loading = true;
  String? _error;
  List<Map<String, dynamic>> _items = const [];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final request = ++_request;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final rows = await Injection.committee.listStats();
      if (!mounted || request != _request) return;
      setState(() {
        _items = rows;
        _loading = false;
      });
    } catch (e) {
      if (!mounted || request != _request) return;
      setState(() {
        _loading = false;
        _error = DioClient.mapError(e);
      });
    }
  }

  String _s(Map<String, dynamic> j, String a, String b) =>
      '${j[a] ?? j[b] ?? ''}';

  String _num(Map<String, dynamic> j, String a, String b) {
    final raw = j[a] ?? j[b];
    if (raw == null) return '—';
    return '$raw';
  }

  bool _prevAvailable(Map<String, dynamic> j) =>
      j['previousMonthAvailable'] == true || j['PreviousMonthAvailable'] == true;

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(
        title: const Text('الإحصائيات'),
        actions: [
          IconButton(
            tooltip: 'تحديث',
            onPressed: _loading ? null : _load,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: AnimatedWavesBackground(
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : _error != null
            ? Center(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Text(
                    _error!,
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: AppColors.danger),
                  ),
                ),
              )
            : _items.isEmpty
            ? const Center(
                child: Text(
                  'لا توجد إحصائيات للقوائم المخصصة',
                  style: TextStyle(color: AppColors.muted),
                ),
              )
            : ListView.separated(
                padding: EdgeInsets.fromLTRB(16, 16, 16, 24 + bottom),
                itemCount: _items.length,
                separatorBuilder: (_, _) => const SizedBox(height: 12),
                itemBuilder: (_, i) {
                  final j = _items[i];
                  final name = _s(j, 'listName', 'ListName');
                  final label = _s(j, 'finalRatingLabel', 'FinalRatingLabel');
                  final prevOk = _prevAvailable(j);
                  return EntityCard(
                    name: name.isEmpty ? 'قائمة' : name,
                    eyebrow: 'شهر ${_num(j, 'yearMonth', 'YearMonth')}',
                    statusLabel: label.isEmpty ? null : label,
                    fields: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        DataField(
                          label: 'زبائن نشطون',
                          value: _num(j, 'activeCustomersCount', 'ActiveCustomersCount'),
                          requiredField: true,
                        ),
                        DataField(
                          label: 'جدد هذا الشهر',
                          value: _num(j, 'newCustomersCount', 'NewCustomersCount'),
                          requiredField: true,
                        ),
                        DataField(
                          label: 'مصفّرون',
                          value: _num(j, 'zeroedCustomersCount', 'ZeroedCustomersCount'),
                          requiredField: true,
                        ),
                        DataField(
                          label: 'صافي العدد',
                          value: _num(j, 'netCustomersCount', 'NetCustomersCount'),
                          requiredField: true,
                        ),
                        DataField(
                          label: 'متوسط التقييم',
                          value: _num(j, 'averageScore', 'AverageScore'),
                          requiredField: true,
                        ),
                        DataField(
                          label: 'ممتاز / جيد / ضعيف',
                          value:
                              '${_num(j, 'excellentCount', 'ExcellentCount')} / ${_num(j, 'goodCount', 'GoodCount')} / ${_num(j, 'weakCount', 'WeakCount')}',
                          requiredField: true,
                        ),
                        DataField(
                          label: 'إجمالي المبيعات الحالي',
                          value: _num(j, 'currentTotalSales', 'CurrentTotalSales'),
                          requiredField: true,
                        ),
                        DataField(
                          label: 'مبيعات الشهر السابق',
                          value: prevOk
                              ? _num(j, 'previousTotalSales', 'PreviousTotalSales')
                              : 'غير متوفر (لا لقطة مجمّدة)',
                          requiredField: true,
                        ),
                        DataField(
                          label: 'مقيمون',
                          value: _num(j, 'ratedCustomerCount', 'RatedCustomerCount'),
                          requiredField: true,
                        ),
                      ],
                    ),
                  );
                },
              ),
      ),
    );
  }
}
