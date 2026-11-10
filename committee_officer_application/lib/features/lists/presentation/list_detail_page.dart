import 'package:committee_officer_application/core/time/baghdad_time.dart';
import 'package:committee_officer_application/core/widgets/centered_dialog.dart';
import 'package:committee_officer_application/core/widgets/date_dialog.dart';
import 'package:committee_officer_application/core/widgets/ui/data_field.dart';
import 'package:committee_officer_application/core/widgets/ui/entity_card.dart';
import 'package:committee_officer_application/features/lists/domain/daily_customer.dart';
import 'package:committee_officer_application/core/network/dio_client.dart';
import 'package:committee_officer_application/core/theme/app_colors.dart';
import 'package:committee_officer_application/core/widgets/animated_waves.dart';
import 'package:committee_officer_application/injection.dart';
import 'package:flutter/material.dart';

class ListDetailPage extends StatefulWidget {
  const ListDetailPage({
    super.key,
    required this.listId,
    required this.listName,
  });

  final int listId;
  final String listName;

  @override
  State<ListDetailPage> createState() => _ListDetailPageState();
}

class _ListDetailPageState extends State<ListDetailPage>
    with SingleTickerProviderStateMixin {
  late final TabController _tabs;
  List<Map<String, dynamic>> _paid = const [];
  List<Map<String, dynamic>> _unpaid = const [];
  DateTime _date = baghdadNow();
  String _search = '';
  PaymentFilter _paymentFilter = PaymentFilter.all;
  int _request = 0;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _tabs = TabController(length: 4, vsync: this);
    _load();
  }

  @override
  void dispose() {
    _tabs.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    final request = ++_request;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final results = await Future.wait([
        Injection.committee.daily(
          listId: widget.listId,
          showType: 'المسددين',
          date: baghdadDateKey(_date),
        ),
        Injection.committee.daily(
          listId: widget.listId,
          showType: 'الغير مسددين',
          date: baghdadDateKey(_date),
        ),
      ]);
      if (!mounted || request != _request) return;
      final rows = results.expand(_customersOf).toList();
      setState(() {
        _paid = rows
            .where((r) => DailyCustomer.fromJson(r).hasPayment)
            .toList();
        _unpaid = rows
            .where((r) => !DailyCustomer.fromJson(r).hasPayment)
            .toList();
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

  List<Map<String, dynamic>> _customersOf(Map<String, dynamic> res) {
    final raw = res['customers'] ?? res['Customers'];
    if (raw is List) {
      return raw
          .whereType<Map>()
          .map((e) => Map<String, dynamic>.from(e))
          .toList();
    }
    return const [];
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.listName),
        actions: [
          IconButton(
            tooltip: 'تحديث',
            onPressed: _loading ? null : _load,
            icon: const Icon(Icons.refresh),
          ),
        ],
        bottom: TabBar(
          controller: _tabs,
          isScrollable: true,
          labelColor: Colors.white,
          unselectedLabelColor: const Color(0xCCFFFFFF),
          indicatorColor: Colors.white,
          indicatorWeight: 3,
          dividerColor: Colors.transparent,
          labelStyle: const TextStyle(fontWeight: FontWeight.w800),
          unselectedLabelStyle: const TextStyle(fontWeight: FontWeight.w600),
          tabs: [
            Tab(text: 'مسددين (${_paid.length})'),
            Tab(text: 'غير مسددين (${_unpaid.length})'),
            const Tab(text: 'الزبائن'),
            const Tab(text: 'الإحصائيات'),
          ],
        ),
      ),
      body: AnimatedWavesBackground(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(12, 10, 12, 0),
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      onChanged: (value) => setState(() => _search = value),
                      decoration: const InputDecoration(
                        hintText: 'اسم الزبون أو رقم الهاتف',
                        prefixIcon: Icon(Icons.search),
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  OutlinedButton.icon(
                    icon: const Icon(Icons.calendar_month_outlined),
                    label: Text(baghdadDateKey(_date)),
                    onPressed: () async {
                      final date = await showCommitteeDatePicker(
                        context,
                        initialDate: _date,
                        lastDate: baghdadNow(),
                      );
                      if (!mounted || date == null) return;
                      setState(() => _date = date);
                      await _load();
                    },
                  ),
                ],
              ),
            ),
            Expanded(
              child: _loading
                  ? const Center(child: CircularProgressIndicator())
                  : _error != null
                  ? Center(
                      child: Text(
                        _error!,
                        style: const TextStyle(color: AppColors.danger),
                      ),
                    )
                  : TabBarView(
                      controller: _tabs,
                      children: [
                        Column(
                          children: [
                            SingleChildScrollView(
                              scrollDirection: Axis.horizontal,
                              padding: const EdgeInsets.all(12),
                              child: Row(
                                spacing: 8,
                                children: [
                                  for (final filter in PaymentFilter.values)
                                    ChoiceChip(
                                      label: Text(switch (filter) {
                                        PaymentFilter.all => 'الكل',
                                        PaymentFilter.belowInstallment =>
                                          'أقل من القسط',
                                        PaymentFilter.fullInstallment =>
                                          'القسط فأكثر',
                                      }),
                                      selected: _paymentFilter == filter,
                                      onSelected: (_) => setState(
                                        () => _paymentFilter = filter,
                                      ),
                                    ),
                                ],
                              ),
                            ),
                            Expanded(
                              child: _list(
                                _paid
                                    .where(
                                      (r) =>
                                          DailyCustomer.fromJson(r)
                                              .matchesPayment(_paymentFilter),
                                    )
                                    .toList(),
                                empty: 'لا توجد تسديدات مطابقة',
                                highlight: AppColors.success,
                              ),
                            ),
                          ],
                        ),
                        _list(
                          _unpaid,
                          empty: 'لا يوجد غير مسددين في التاريخ المحدد',
                          highlight: AppColors.danger,
                        ),
                        _list(
                          [..._paid, ..._unpaid],
                          empty: 'لا زبائن',
                          highlight: AppColors.primary,
                        ),
                        Padding(
                          padding: EdgeInsets.fromLTRB(16, 16, 16, 16 + bottom),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              _statCard(
                                'مسددون في التاريخ المحدد',
                                '${_paid.length}',
                                AppColors.success,
                              ),
                              const SizedBox(height: 10),
                              _statCard(
                                'غير مسددين',
                                '${_unpaid.length}',
                                AppColors.danger,
                              ),
                              const SizedBox(height: 10),
                              _statCard(
                                'إجمالي الزبائن المعروضين',
                                '${_paid.length + _unpaid.length}',
                                AppColors.primary,
                              ),
                              const SizedBox(height: 16),
                              const Text(
                                'هذه الأعداد تخص التاريخ المحدد. التسديد الجزئي محسوب ضمن المسددين.',
                                style: TextStyle(
                                  color: AppColors.textSecondary,
                                  height: 1.4,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _statCard(String title, String value, Color color) {
    return Material(
      color: Colors.white.withValues(alpha: 0.92),
      borderRadius: BorderRadius.circular(14),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Row(
          children: [
            Expanded(
              child: Text(
                title,
                style: const TextStyle(fontWeight: FontWeight.w700),
              ),
            ),
            Text(
              value,
              style: TextStyle(
                color: color,
                fontWeight: FontWeight.w800,
                fontSize: 18,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _moveToField(DailyCustomer c) async {
    try {
      await Injection.committee.addFieldFollowup(
        listId: widget.listId,
        listName: widget.listName,
        customerId: c.id,
        customerName: c.name,
        phoneNumber: c.phone,
      );
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: 'تم الإرسال إلى المتابعة الميدانية',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تعذر الإرسال',
        message: DioClient.mapError(e),
        icon: Icons.error_outline,
        color: AppColors.danger,
      );
    }
  }

  Future<void> _recordComplete(DailyCustomer c) async {
    var draft = '';
    final ok = await showCenteredDialog<bool>(
      context: context,
      builder: (dialogContext) => Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text(
            'تمت المتابعة',
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 8),
          Text(
            c.name,
            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 12),
          TextField(
            maxLines: 3,
            onChanged: (v) => draft = v,
            decoration: const InputDecoration(
              labelText: 'ملاحظة إجبارية',
              filled: true,
              fillColor: Color(0xFFF8F8F8),
            ),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => Navigator.of(dialogContext).pop(false),
                  child: const Text('إلغاء'),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: ElevatedButton(
                  onPressed: () => Navigator.of(dialogContext).pop(true),
                  child: const Text('اعتماد'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
    if (ok != true) return;
    final note = draft.trim();
    if (note.isEmpty) {
      await showAppMessage(
        context,
        title: 'تنبيه',
        message: 'الملاحظة إجبارية',
        icon: Icons.warning_amber_rounded,
        color: AppColors.danger,
      );
      return;
    }
    try {
      await Injection.committee.recordCompleteFieldFollowup(
        listId: widget.listId,
        listName: widget.listName,
        customerId: c.id,
        customerName: c.name,
        phoneNumber: c.phone,
        note: note,
      );
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: 'تم اعتماد المتابعة الميدانية',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تعذر الاعتماد',
        message: DioClient.mapError(e),
        icon: Icons.error_outline,
        color: AppColors.danger,
      );
    }
  }

  Future<void> _addCustomerNote(DailyCustomer c) async {
    var draft = '';
    final ok = await showCenteredDialog<bool>(
      context: context,
      builder: (dialogContext) => Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const Text(
            'ملاحظة زبون',
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800),
          ),
          const SizedBox(height: 8),
          Text(c.name, style: const TextStyle(fontWeight: FontWeight.w700)),
          const SizedBox(height: 12),
          TextField(
            maxLines: 3,
            onChanged: (v) => draft = v,
            decoration: const InputDecoration(
              labelText: 'نص الملاحظة',
              filled: true,
              fillColor: Color(0xFFF8F8F8),
            ),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => Navigator.of(dialogContext).pop(false),
                  child: const Text('إلغاء'),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: ElevatedButton(
                  onPressed: () => Navigator.of(dialogContext).pop(true),
                  child: const Text('حفظ'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
    if (ok != true) return;
    final note = draft.trim();
    if (note.isEmpty) return;
    try {
      await Injection.committee.addCustomerNote(
        customerId: c.id,
        listId: widget.listId,
        noteText: note,
      );
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: 'حُفظت ملاحظة الزبون',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تعذر الحفظ',
        message: DioClient.mapError(e),
        icon: Icons.error_outline,
        color: AppColors.danger,
      );
    }
  }

  Widget _list(
    List<Map<String, dynamic>> rows, {
    required String empty,
    required Color highlight,
  }) {
    final customers = rows
        .map(DailyCustomer.fromJson)
        .where((c) => c.matches(_search))
        .toList();
    final bottom = MediaQuery.paddingOf(context).bottom;
    if (customers.isEmpty) {
      return Center(
        child: Text(empty, style: const TextStyle(color: AppColors.muted)),
      );
    }
    return ListView.separated(
      padding: EdgeInsets.fromLTRB(12, 12, 12, 24 + bottom),
      itemCount: customers.length,
      separatorBuilder: (_, _) => const SizedBox(height: 12),
      itemBuilder: (_, i) {
        final c = customers[i];
        return EntityCard(
          name: c.name.isEmpty ? 'زبون #${c.id}' : c.name,
          eyebrow: c.hasNote ? 'لديه ملاحظة زبون' : 'قائمة ${widget.listName}',
          statusLabel: c.hasPayment ? 'مسدد' : 'غير مسدد',
          fields: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (c.phone.isNotEmpty)
                DataField(
                  label: 'الهاتف',
                  value: c.phone,
                  requiredField: true,
                  isPhone: true,
                ),
              DataField(
                label: 'القسط المستحق',
                value: '${c.installment}',
                requiredField: true,
              ),
              DataField(
                label: 'المدفوع اليوم',
                value: '${c.paid}',
                requiredField: true,
              ),
              if (c.latestFieldFollowupNote != null)
                DataField(
                  label: 'آخر متابعة ميدانية',
                  value: c.latestFieldFollowupNote,
                  requiredField: true,
                ),
            ],
          ),
          actions: Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              OutlinedButton(
                onPressed: () => _moveToField(c),
                child: const Text('إرسال للمتابعة الميدانية'),
              ),
              ElevatedButton(
                style: ElevatedButton.styleFrom(backgroundColor: highlight),
                onPressed: () => _recordComplete(c),
                child: const Text('تمت المتابعة'),
              ),
              OutlinedButton(
                onPressed: () => _addCustomerNote(c),
                child: Text(c.hasNote ? 'إضافة ملاحظة زبون' : 'ملاحظة زبون'),
              ),
            ],
          ),
        );
      },
    );
  }
}
