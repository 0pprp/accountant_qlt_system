import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/app_dialog.dart';
import '../../../core/widgets/brand_wave_painter.dart';
import '../../../core/widgets/empty_state.dart';
import '../../../core/widgets/loading_view.dart';
import '../../../injection.dart';
import '../../customers/domain/entities/customer.dart';
import '../../receipts/data/customer_no_receipt_report.dart';
import '../../receipts/data/customer_receipt_report.dart';
import '../data/delegate_business_date_service.dart';
import '../domain/entities/today_payment_item.dart';

class TodayPaymentsPage extends StatefulWidget {
  const TodayPaymentsPage({super.key});

  @override
  State<TodayPaymentsPage> createState() => _TodayPaymentsPageState();
}

class _TodayPaymentsPageState extends State<TodayPaymentsPage> {
  List<TodayPaymentItem> _items = [];
  bool _loading = true;
  bool _sending = false;
  bool _exporting = false;
  int _pendingCount = 0;
  int _paidCount = 0;
  double _paidTotal = 0;

  @override
  void initState() {
    super.initState();
    DelegateBusinessDateService.dayRevision.addListener(_onDayRollover);
    Injection.paymentSyncService.queueChanged.addListener(_onDayRollover);
    _load();
  }

  void _onDayRollover() {
    if (!mounted) return;
    _load();
  }

  @override
  void dispose() {
    DelegateBusinessDateService.dayRevision.removeListener(_onDayRollover);
    Injection.paymentSyncService.queueChanged.removeListener(_onDayRollover);
    super.dispose();
  }

  /// نفس فلتر زر «المسددين»: مبلغ > 0 ومعرّف محلي أو صف سيرفر.
  static List<TodayPaymentItem> _paidWalletItems(List<TodayPaymentItem> items) {
    return items.where((p) {
      if (p.amount <= 0) return false;
      final hasUuid =
          (p.clientPaymentId != null && p.clientPaymentId!.trim().isNotEmpty) ||
              !p.isLocal;
      return hasUuid;
    }).toList();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    final items = await Injection.paymentsRepository.listTodayMerged();
    final pending = await Injection.paymentsRepository.listPending();
    final paid = _paidWalletItems(items);
    if (!mounted) return;
    setState(() {
      _items = items;
      _pendingCount = pending.length;
      _paidCount = paid.length;
      _paidTotal = paid.fold<double>(0, (sum, p) => sum + p.amount);
      _loading = false;
    });
  }

  Future<(String listName, String collector)> _sessionNames() async {
    final session = await Injection.authRepository.currentSession();
    final listName = (session?.cityName ?? '').trim().isEmpty
        ? '—'
        : session!.cityName!.trim();
    final collectorRaw =
        (session?.delegateName ?? session?.username ?? '').trim();
    final collector = collectorRaw.isEmpty ? '—' : collectorRaw;
    return (listName, collector);
  }

  Future<void> _exportPaid() async {
    if (_exporting) return;
    setState(() => _exporting = true);
    try {
      // أعد الجلب مباشرة من المستودع حتى لا نعتمد على حالة شاشة قديمة
      final items = await Injection.paymentsRepository.listTodayMerged();
      final customers = await Injection.getLocalCustomers();
      final byName = <String, Customer>{
        for (final c in customers) c.name.trim(): c,
      };

      final receiptData = <Map<String, dynamic>>[];
      for (final p in _paidWalletItems(items)) {
        var id = p.customerId ?? 0;
        if (id <= 0) {
          final match = byName[p.customerName.trim()];
          if (match != null) id = match.customerId;
        }
        receiptData.add({
          'CustomerId': id,
          'CustomerName': p.customerName,
          'Amount': p.amount,
        });
      }

      if (receiptData.isEmpty) {
        if (!mounted) return;
        await AppDialog.message(
          context,
          title: 'المسددين',
          body: 'لا يوجد مسددون اليوم في المخزن المحلي.',
        );
        return;
      }

      if (mounted) setState(() => _items = items);

      final names = await _sessionNames();
      if (!mounted) return;
      final report = CustomerReceiptReport.fromToday(
        delegateName: names.$1,
        receiptName: names.$2,
        receiptData: receiptData,
        customers: customers,
      );
      await report.printReceipt(context);
    } catch (e) {
      if (!mounted) return;
      await AppDialog.message(
        context,
        title: 'تعذر التصدير',
        body: '$e'.replaceFirst(RegExp(r'^[^:]+:\s*'), ''),
      );
    } finally {
      if (mounted) setState(() => _exporting = false);
    }
  }

  Future<void> _exportUnpaid() async {
    if (_exporting) return;
    setState(() => _exporting = true);
    try {
      final items = await Injection.paymentsRepository.listTodayMerged();
      final customers = await Injection.getLocalCustomers();
      final paidIds = <int>{};
      final paidNames = <String>{};
      for (final p in items) {
        final id = p.customerId;
        if (id != null && id > 0) paidIds.add(id);
        final n = p.customerName.trim();
        if (n.isNotEmpty) paidNames.add(n);
      }
      if (mounted) setState(() => _items = items);

      // غير المسددين = المستمرين الظاهرين بلا دفع اليوم (بدون قانونية/مصفرين)
      final unpaid = customers
          .where(
            (c) =>
                c.isContinuous &&
                !paidIds.contains(c.customerId) &&
                !paidNames.contains(c.name.trim()),
          )
          .toList()
        ..sort((a, b) => a.name.compareTo(b.name));

      if (unpaid.isEmpty) {
        if (!mounted) return;
        await AppDialog.message(
          context,
          title: 'الغير مسددين',
          body: 'لا يوجد غير مسددين (مستمرين بلا تسديد اليوم).',
        );
        return;
      }

      final names = await _sessionNames();
      if (!mounted) return;
      final report = CustomerNoReceiptReport(
        delegateName: names.$1,
        receiptName: names.$2,
        customerData: unpaid,
      );
      await report.printReport(context);
    } catch (e) {
      if (!mounted) return;
      await AppDialog.message(
        context,
        title: 'تعذر التصدير',
        body: '$e'.replaceFirst(RegExp(r'^[^:]+:\s*'), ''),
      );
    } finally {
      if (mounted) setState(() => _exporting = false);
    }
  }

  Future<void> _sendAll() async {
    if (_sending) return;
    setState(() => _sending = true);
    try {
      await Injection.paymentSyncService.syncNow();
      if (!mounted) return;
      await _load();
      if (!mounted || _pendingCount == 0) return;
      await AppDialog.message(context, title: 'تعذر تأكيد بعض التسديدات',
        body: Injection.paymentsRepository.lastSyncError ??
            Injection.paymentSyncService.lastError ?? 'تحقق من الاتصال وأعد المحاولة.');
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final money = NumberFormat('#,##0.##', 'en');
    final bottomPad = 120.0 + MediaQuery.paddingOf(context).bottom;

    return SafeArea(
      bottom: false,
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 10, 16, 6),
            child: Row(
              children: [
                Expanded(
                  child: Text(
                    'تسديدات اليوم',
                    style: AppTextStyles.title.copyWith(fontSize: 18),
                  ),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 8),
            child: Row(
              children: [
                Expanded(
                  child: FilledButton.icon(
                    onPressed: _exporting ? null : _exportPaid,
                    icon: const Icon(Icons.print, size: 18),
                    label: const Text('المسددين'),
                    style: FilledButton.styleFrom(
                      backgroundColor: AppColors.secondary,
                      minimumSize: const Size.fromHeight(44),
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: FilledButton.icon(
                    onPressed: _exporting ? null : _exportUnpaid,
                    icon: const Icon(Icons.print_disabled, size: 18),
                    label: const Text('الغير مسددين'),
                    style: FilledButton.styleFrom(
                      backgroundColor: AppColors.danger,
                      minimumSize: const Size.fromHeight(44),
                    ),
                  ),
                ),
              ],
            ),
          ),
          if (_pendingCount > 0 || _sending)
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 8),
              child: FilledButton.icon(
                onPressed: _sending ? null : _sendAll,
                icon: _sending
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: AppColors.onPrimary,
                        ),
                      )
                    : const Icon(Icons.cloud_upload_outlined),
                label: const Text('إرسال التسديدات'),
                style: FilledButton.styleFrom(
                  minimumSize: const Size.fromHeight(46),
                ),
              ),
            ),
          if (_paidCount > 0)
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 8),
              child: Container(
                width: double.infinity,
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: AppColors.surface,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: AppColors.border),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.04),
                      blurRadius: 12,
                      offset: const Offset(0, 4),
                    ),
                  ],
                ),
                child: Row(
                  children: [
                    Expanded(
                      child: _PendingMetricCard(
                        title: 'عدد التسديدات',
                        value: '$_paidCount',
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      child: _PendingMetricCard(
                        title: 'المبلغ الكلي',
                        value: money.format(_paidTotal),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          Expanded(
            child: RefreshIndicator(
              color: AppColors.primary,
              onRefresh: _load,
              child: _loading
                  ? const LoadingView()
                  : _items.isEmpty
                      ? ListView(
                          physics: const AlwaysScrollableScrollPhysics(),
                          children: const [
                            SizedBox(height: 120),
                            EmptyState(message: 'لا توجد تسديدات اليوم بعد'),
                          ],
                        )
                      : ListView.separated(
                          physics: const AlwaysScrollableScrollPhysics(),
                          padding: EdgeInsets.fromLTRB(16, 8, 16, bottomPad),
                          itemCount: _items.length,
                          separatorBuilder: (_, _) =>
                              const SizedBox(height: 10),
                          itemBuilder: (context, i) {
                            final p = _items[i];
                            final failed = p.statusLabel.contains('فشل');
                            return ClipRRect(
                              borderRadius: BorderRadius.circular(14),
                              child: Stack(
                                children: [
                                  Positioned.fill(
                                    child: CustomPaint(
                                      painter:
                                          const BrandWavePainter(opacity: 0.08),
                                    ),
                                  ),
                                  Material(
                                    color:
                                        AppColors.card.withValues(alpha: 0.92),
                                    child: ListTile(
                                      contentPadding:
                                          const EdgeInsets.symmetric(
                                        horizontal: 14,
                                        vertical: 6,
                                      ),
                                      title: Text(
                                        '${p.customerName} — ${money.format(p.amount)}',
                                        style: AppTextStyles.body.copyWith(
                                          fontWeight: FontWeight.w700,
                                        ),
                                      ),
                                      subtitle: Text(
                                        [
                                          p.isLocal
                                              ? 'محلي · ${p.statusLabel}'
                                              : 'سيرفر · ${p.statusLabel}',
                                          if (p.syncError != null &&
                                              p.syncError!.isNotEmpty)
                                            p.syncError!,
                                          if (p.orderId != null)
                                            'طلب #${p.orderId}',
                                          if (p.dateLabel != null)
                                            p.dateLabel!,
                                        ].join('\n'),
                                        style: AppTextStyles.muted.copyWith(
                                          color: failed
                                              ? AppColors.danger
                                              : AppColors.muted,
                                          height: 1.35,
                                        ),
                                      ),
                                      trailing: Icon(
                                        p.isLocal
                                            ? Icons.schedule_send_outlined
                                            : Icons.cloud_done_outlined,
                                        color: p.isLocal
                                            ? AppColors.warning
                                            : AppColors.success,
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            );
                          },
                        ),
            ),
          ),
        ],
      ),
    );
  }
}

class _PendingMetricCard extends StatelessWidget {
  const _PendingMetricCard({
    required this.title,
    required this.value,
  });

  final String title;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
      decoration: BoxDecoration(
        color: AppColors.card,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.03),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          Text(title, style: AppTextStyles.muted),
          const SizedBox(height: 6),
          Text(
            value,
            style: AppTextStyles.amount.copyWith(
              color: AppColors.primary,
              fontSize: 20,
            ),
          ),
        ],
      ),
    );
  }
}
