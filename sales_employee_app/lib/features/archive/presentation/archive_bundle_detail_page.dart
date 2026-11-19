import 'package:dio/dio.dart';
import 'package:flutter/material.dart';

import '../../../core/network/dio_client.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/widgets/centered_dialog.dart';
import '../../../injection.dart';
import '../data/archive_remote.dart';
import '../domain/archive_models.dart';

/// تفصيل الحزمة: غير المستلمة تتصدّر، وتأشير متعدد ثم «استلمت المعاملات المحددة».
class ArchiveBundleDetailPage extends StatefulWidget {
  const ArchiveBundleDetailPage({
    super.key,
    required this.bundleId,
    required this.stage,
    required this.title,
  });

  final int bundleId;
  final int stage;
  final String title;

  @override
  State<ArchiveBundleDetailPage> createState() => _ArchiveBundleDetailPageState();
}

class _ArchiveBundleDetailPageState extends State<ArchiveBundleDetailPage> {
  late final ArchiveRemote _remote = ArchiveRemote(Injection.dioClient);

  ArchiveBundleDetail? _detail;
  final Set<int> _selected = <int>{};
  bool _loading = true;
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
      _selected.clear();
    });
    try {
      final detail = await _remote.bundle(widget.bundleId, stage: widget.stage);
      if (!mounted) return;
      setState(() {
        _detail = detail;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _error = e is DioException ? DioClient.mapError(e) : 'تعذر تحميل الحزمة';
      });
    }
  }

  List<ArchiveTransactionRow> get _selectable =>
      (_detail?.transactions ?? const <ArchiveTransactionRow>[])
          .where((t) => !t.isVoided)
          .toList();

  bool get _allChecked =>
      _selectable.isNotEmpty && _selectable.every((t) => _selected.contains(t.id));

  Future<void> _run(Future<ArchiveActionResult> Function() action) async {
    setState(() => _busy = true);
    try {
      final result = await action();
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم',
        message: result.message,
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      if (!mounted) return;
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تعذر الإجراء',
        message: e is DioException ? DioClient.mapError(e) : 'حدث خطأ',
        icon: Icons.error_outline,
        color: AppColors.danger,
      );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final detail = _detail;

    return Scaffold(
      appBar: AppBar(title: Text(widget.title)),
      bottomNavigationBar: detail == null ? null : _bottomBar(detail),
      body: RefreshIndicator(onRefresh: _load, child: _body()),
    );
  }

  Widget _body() {
    if (_loading) return const Center(child: CircularProgressIndicator());

    if (_error != null) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(24),
        children: [
          Text(_error!, textAlign: TextAlign.center),
          const SizedBox(height: 12),
          Center(
            child: OutlinedButton(onPressed: _load, child: const Text('إعادة المحاولة')),
          ),
        ],
      );
    }

    final detail = _detail;
    if (detail == null || detail.transactions.isEmpty) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(24),
        children: const [Text('لا معاملات في هذه الحزمة', textAlign: TextAlign.center)],
      );
    }

    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.fromLTRB(12, 12, 12, 24),
      children: [
        _summary(detail),
        if (_selectable.isNotEmpty)
          CheckboxListTile(
            value: _allChecked,
            dense: true,
            title: Text('تحديد الكل (${_selectable.length})'),
            onChanged: (_) => setState(() {
              if (_allChecked) {
                _selected.clear();
              } else {
                _selected
                  ..clear()
                  ..addAll(_selectable.map((t) => t.id));
              }
            }),
          ),
        const Divider(height: 1),
        ...detail.transactions.map(_row),
      ],
    );
  }

  Widget _summary(ArchiveBundleDetail detail) {
    final b = detail.bundle;
    final isSource = widget.stage == ArchiveStage.salesEmployee;

    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            isSource
                ? 'تم استلام ${b.receivedOnwardCount} من ${b.itemCount}'
                : 'استلمتُ ${b.receivedByMeCount} من ${b.itemCount}',
            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w800),
          ),
          if (!b.isPeriodClosed)
            Padding(
              padding: const EdgeInsets.only(top: 4),
              child: Text(
                'العشرة ما زالت جارية حتى ${b.periodEnd} — الإرسال مقفل حتى تنتهي.',
                style: TextStyle(fontSize: 12, color: AppColors.warning),
              ),
            ),
        ],
      ),
    );
  }

  Widget _row(ArchiveTransactionRow t) {
    return CheckboxListTile(
      value: _selected.contains(t.id),
      enabled: !t.isVoided,
      controlAffinity: ListTileControlAffinity.leading,
      onChanged: (_) => setState(() {
        if (_selected.contains(t.id)) {
          _selected.remove(t.id);
        } else {
          _selected.add(t.id);
        }
      }),
      title: Text(
        t.customerName,
        style: TextStyle(
          fontWeight: t.awaitsMyReceipt ? FontWeight.w700 : FontWeight.w400,
          decoration: t.isVoided ? TextDecoration.lineThrough : null,
        ),
      ),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('#${t.orderId} · ${t.statusLabel}',
              style: const TextStyle(fontSize: 12)),
          if (t.hasOpenReturn)
            Text(
              'مسترجَعة — ${t.lastReturnReason ?? ''}',
              style: TextStyle(fontSize: 12, color: AppColors.danger),
            ),
        ],
      ),
      tileColor: t.awaitsMyReceipt
          ? AppColors.primary.withValues(alpha: 0.06)
          : null,
    );
  }

  Widget _bottomBar(ArchiveBundleDetail detail) {
    final stage = detail.myStage ?? widget.stage;
    final undoable = detail.transactions
        .where((t) => t.isInTransit && _selected.contains(t.id))
        .map((t) => t.id)
        .toList();

    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(12, 8, 12, 12),
        child: Row(
          children: [
            if (detail.canReceive)
              Expanded(
                child: FilledButton(
                  onPressed: _busy || _selected.isEmpty
                      ? null
                      : () => _run(() => _remote.receive(_selected.toList(), stage)),
                  child: const Text('استلمت المعاملات المحددة'),
                ),
              ),
            if (detail.canUndoSend && undoable.isNotEmpty) ...[
              const SizedBox(width: 8),
              OutlinedButton(
                onPressed: _busy
                    ? null
                    : () => _run(() => _remote.undoSend(undoable, stage)),
                child: const Text('إلغاء الإرسال'),
              ),
            ],
            if (detail.canSend) ...[
              const SizedBox(width: 8),
              Expanded(
                child: FilledButton(
                  onPressed: _busy
                      ? null
                      : () => _run(() => _remote.send(widget.bundleId, stage)),
                  child: Text('إرسال الحزمة (${detail.bundle.pendingSendCount})'),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
