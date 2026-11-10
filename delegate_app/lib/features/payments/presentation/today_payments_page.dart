import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/widgets/app_dialog.dart';
import '../../../core/widgets/brand_wave_painter.dart';
import '../../../core/widgets/empty_state.dart';
import '../../../core/widgets/loading_view.dart';
import '../../../injection.dart';
import '../../sync/presentation/sync_status_banner.dart';
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
  int _pendingCount = 0;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    final items = await Injection.paymentsRepository.listTodayMerged();
    final pending = await Injection.paymentsRepository.countPending();
    if (!mounted) return;
    setState(() {
      _items = items;
      _pendingCount = pending;
      _loading = false;
    });
  }

  Future<void> _sendAll() async {
    if (_sending) return;
    final pending = await Injection.paymentsRepository.countPending();
    if (!mounted) return;
    if (pending <= 0) {
      await AppDialog.message(
        context,
        title: 'إرسال التسديدات',
        body: 'لا توجد تسديدات معلّقة للإرسال.',
      );
      await _load();
      return;
    }

    setState(() => _sending = true);
    final progress = ValueNotifier(SyncProgress(done: 0, total: pending));
    // ignore: unawaited_futures
    AppDialog.showProgress(context, progress: progress);

    try {
      final n = await Injection.paymentSyncService.syncNow(
        onProgress: (done, total) {
          progress.value = SyncProgress(done: done, total: total);
        },
      );
      if (!mounted) return;
      if (Navigator.of(context).canPop()) {
        Navigator.of(context).pop();
      }
      final detail = Injection.paymentsRepository.lastSyncError ??
          Injection.paymentSyncService.lastError;
      await AppDialog.message(
        context,
        title: 'نتيجة الإرسال',
        body: n > 0
            ? 'تم إرسال $n من $pending تسديد بنجاح.${detail != null && detail.isNotEmpty && n < pending ? '\n\nتنبيه: $detail' : ''}'
            : (detail != null && detail.isNotEmpty
                ? detail
                : 'لم يُرسل شيء — راجع التسديدات المحلية.'),
      );
      await _load();
    } catch (e) {
      if (!mounted) return;
      if (Navigator.of(context).canPop()) {
        Navigator.of(context).pop();
      }
      await AppDialog.message(
        context,
        title: 'فشل الإرسال',
        body: '$e'.replaceFirst(RegExp(r'^[^:]+:\s*'), ''),
      );
    } finally {
      progress.dispose();
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
          const SyncStatusBanner(),
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
                if (_pendingCount > 0)
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: AppColors.warning.withValues(alpha: 0.18),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text(
                      'معلّق: $_pendingCount',
                      style: AppTextStyles.muted.copyWith(
                        color: AppColors.foreground,
                        fontWeight: FontWeight.w700,
                        fontSize: 12,
                      ),
                    ),
                  ),
              ],
            ),
          ),
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
                          padding:
                              EdgeInsets.fromLTRB(16, 8, 16, bottomPad),
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
                                    color: AppColors.card.withValues(alpha: 0.92),
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
