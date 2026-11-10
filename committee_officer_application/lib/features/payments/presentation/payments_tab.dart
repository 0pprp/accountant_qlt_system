import 'package:committee_officer_application/core/time/baghdad_time.dart';
import 'package:committee_officer_application/core/widgets/date_dialog.dart';
import 'package:committee_officer_application/features/lists/domain/daily_customer.dart';
import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

/// تسديدات اليوم لقوائم مسؤول اللجنة فقط (قد تكون أكثر من قائمة).
class PaymentsTab extends StatefulWidget {
  const PaymentsTab({super.key});

  @override
  State<PaymentsTab> createState() => _PaymentsTabState();
}

class _PaymentsTabState extends State<PaymentsTab> {
  final List<_PaymentGroup> _groups = [];
  List<Map<String, dynamic>> _lists = [];
  int? _listId;
  DateTime _from = DateUtils.dateOnly(baghdadNow());
  DateTime _to = DateUtils.dateOnly(baghdadNow());
  String _search = '';
  int _request = 0;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final request = ++_request;
    final from = _from;
    final to = _to;
    final selected = _listId;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final lists = _lists.isEmpty ? await Injection.committee.lists() : _lists;
      final groups = <_PaymentGroup>[];
      for (final list in lists) {
        final id = int.tryParse('${list['id'] ?? list['Id'] ?? 0}') ?? 0;
        if (id <= 0 || (selected != null && selected != id)) continue;
        final name = '${list['name'] ?? list['Name'] ?? ''}';
        final rows = <Map<String, dynamic>>[];
        for (
          var date = from;
          !date.isAfter(to);
          date = date.add(const Duration(days: 1))
        ) {
          if (!mounted || request != _request) return;
          final results = await Future.wait([
            Injection.committee.daily(
              listId: id,
              showType: 'المسددين',
              date: baghdadDateKey(date),
            ),
            Injection.committee.daily(
              listId: id,
              showType: 'الغير مسددين',
              date: baghdadDateKey(date),
            ),
          ]);
          for (final result in results) {
            final customers = result['customers'] ?? result['Customers'];
            if (customers is List) {
              for (final value in customers.whereType<Map>()) {
                final row = Map<String, dynamic>.from(value);
                if (DailyCustomer.fromJson(row).hasPayment) {
                  rows.add({...row, 'paymentDate': baghdadDateKey(date)});
                }
              }
            }
          }
        }
        groups.add(_PaymentGroup(listId: id, listName: name, rows: rows));
      }
      if (!mounted || request != _request) return;
      setState(() {
        _lists = lists;
        _groups
          ..clear()
          ..addAll(groups);
        _loading = false;
      });
    } catch (error) {
      if (!mounted || request != _request) return;
      setState(() {
        _loading = false;
        _error = DioClient.mapError(error);
      });
    }
  }

  Future<void> _pickDate(bool start) async {
    final date = await showCommitteeDatePicker(
      context,
      initialDate: start ? _from : _to,
      lastDate: baghdadNow(),
      title: start ? 'من تاريخ' : 'إلى تاريخ',
    );
    if (!mounted || date == null) return;
    setState(() {
      if (start) {
        _from = date;
        if (_to.isBefore(date)) _to = date;
      } else {
        _to = date;
        if (_from.isAfter(date)) _from = date;
      }
    });
    await _load();
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    final visible = _groups
        .map(
          (g) => _PaymentGroup(
            listId: g.listId,
            listName: g.listName,
            rows: g.rows
                .where((r) => DailyCustomer.fromJson(r).matches(_search))
                .toList(),
          ),
        )
        .toList();
    final totalPaid = visible.fold<int>(0, (n, g) => n + g.rows.length);
    final totalAmount = visible
        .expand((g) => g.rows)
        .fold<double>(0, (sum, row) => sum + DailyCustomer.fromJson(row).paid);
    return Scaffold(
      body: AnimatedWavesBackground(
        child: SafeArea(
          bottom: false,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'التسديدات',
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                    Text(
                      _groups.isEmpty
                          ? 'تسديدات قوائمك حسب الفترة المحددة'
                          : 'تسديدات يومية: $totalPaid • المجموع: ${totalAmount.toStringAsFixed(0)} د.ع',
                      style: const TextStyle(
                        color: AppColors.textSecondary,
                        fontSize: 12.5,
                      ),
                    ),
                  ],
                ),
              ),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                child: Column(
                  children: [
                    DropdownButtonFormField<int>(
                      initialValue: _listId ?? 0,
                      decoration: const InputDecoration(labelText: 'القائمة'),
                      items: [
                        const DropdownMenuItem(
                          value: 0,
                          child: Text('كل القوائم'),
                        ),
                        for (final list in _lists)
                          DropdownMenuItem(
                            value:
                                int.tryParse(
                                  '${list['id'] ?? list['Id'] ?? 0}',
                                ) ??
                                0,
                            child: Text(
                              '${list['name'] ?? list['Name'] ?? ''}',
                            ),
                          ),
                      ],
                      onChanged: (id) {
                        setState(() => _listId = id == 0 ? null : id);
                        _load();
                      },
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton(
                            onPressed: () => _pickDate(true),
                            child: Text('من ${baghdadDateKey(_from)}'),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Expanded(
                          child: OutlinedButton(
                            onPressed: () => _pickDate(false),
                            child: Text('إلى ${baghdadDateKey(_to)}'),
                          ),
                        ),
                      ],
                    ),
                    TextField(
                      onChanged: (value) => setState(() => _search = value),
                      decoration: const InputDecoration(
                        hintText: 'بحث بالاسم أو الهاتف',
                        prefixIcon: Icon(Icons.search),
                      ),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: RefreshIndicator(
                  onRefresh: _load,
                  child: _loading
                      ? ListView(
                          children: const [
                            SizedBox(height: 120),
                            Center(child: CircularProgressIndicator()),
                          ],
                        )
                      : _error != null
                      ? ListView(
                          children: [
                            Padding(
                              padding: const EdgeInsets.all(24),
                              child: Text(_error!),
                            ),
                          ],
                        )
                      : _groups.isEmpty
                      ? ListView(
                          children: const [
                            SizedBox(height: 120),
                            Center(
                              child: Text('لا توجد قوائم مخصصة لعرض تسديداتها'),
                            ),
                          ],
                        )
                      : ListView.builder(
                          padding: EdgeInsets.fromLTRB(16, 8, 16, 24 + bottom),
                          itemCount: visible.length,
                          itemBuilder: (_, i) {
                            final g = visible[i];
                            return Padding(
                              padding: const EdgeInsets.only(bottom: 14),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.stretch,
                                children: [
                                  Text(
                                    g.listName,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.w800,
                                      fontSize: 15,
                                    ),
                                  ),
                                  const SizedBox(height: 6),
                                  if (g.rows.isEmpty)
                                    const Padding(
                                      padding: EdgeInsets.symmetric(
                                        vertical: 10,
                                      ),
                                      child: Text(
                                        'لا تسديدات مطابقة لهذه القائمة',
                                        style: TextStyle(
                                          color: AppColors.muted,
                                        ),
                                      ),
                                    )
                                  else
                                    ...g.rows.map((r) {
                                      return Padding(
                                        padding: const EdgeInsets.only(
                                          bottom: 8,
                                        ),
                                        child: Material(
                                          color: Colors.white.withValues(
                                            alpha: 0.92,
                                          ),
                                          borderRadius: BorderRadius.circular(
                                            14,
                                          ),
                                          child: ListTile(
                                            title: Text(
                                              '${r['fullName'] ?? r['FullName'] ?? ''}',
                                              style: const TextStyle(
                                                fontWeight: FontWeight.w700,
                                              ),
                                            ),
                                            subtitle: Text(
                                              "${r['paymentDate']} • قسط: ${r['dailyInstallment'] ?? r['DailyInstallment'] ?? 0}",
                                              style: const TextStyle(
                                                color: AppColors.textSecondary,
                                              ),
                                            ),
                                            trailing: Text(
                                              '${r['paidToday'] ?? r['PaidToday'] ?? 0}',
                                              style: const TextStyle(
                                                color: AppColors.success,
                                                fontWeight: FontWeight.w700,
                                              ),
                                            ),
                                          ),
                                        ),
                                      );
                                    }),
                                ],
                              ),
                            );
                          },
                        ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _PaymentGroup {
  _PaymentGroup({
    required this.listId,
    required this.listName,
    required this.rows,
  });
  final int listId;
  final String listName;
  final List<Map<String, dynamic>> rows;
}
