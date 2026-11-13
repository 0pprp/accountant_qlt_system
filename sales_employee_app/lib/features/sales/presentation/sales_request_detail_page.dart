import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sales_request_item.dart';
import 'package:sales_employee_app/features/sales/domain/sales_request_actions.dart';
import 'package:sales_employee_app/features/sales/domain/sales_request_tab_map.dart';
import 'package:sales_employee_app/features/sales/presentation/sales_nav.dart';
import 'package:sales_employee_app/injection.dart';

/// تفاصيل طلب SalesRequest للموظف — ملكية + إجراءات مقيّدة بعقود السيرفر.
class SalesRequestDetailPage extends StatefulWidget {
  const SalesRequestDetailPage({super.key, required this.requestId});

  final int requestId;

  @override
  State<SalesRequestDetailPage> createState() => _SalesRequestDetailPageState();
}

class _SalesRequestDetailPageState extends State<SalesRequestDetailPage> {
  SalesRequestItem? _item;
  bool _loading = true;
  bool _busy = false;
  bool _lockedOut = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _bootstrap();
  }

  Future<void> _bootstrap() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      // اطلاع idempotent قبل التفاصيل
      try {
        await Injection.salesRemote.viewSalesRequest(widget.requestId);
      } catch (_) {}
      final item = await Injection.salesRemote.getSalesRequest(
        widget.requestId,
      );
      if (!mounted) return;
      if (item == null) {
        setState(() {
          _lockedOut = true;
          _loading = false;
          _error = 'الطلب غير متاح';
        });
        return;
      }
      setState(() {
        _item = item;
        _loading = false;
        _lockedOut = false;
      });
    } catch (e) {
      if (!mounted) return;
      final msg = DioClient.mapError(e);
      final lost =
          msg.contains('Not_Owner') ||
          msg.contains('غير مملوك') ||
          msg.contains('403') ||
          msg.contains('404');
      setState(() {
        _loading = false;
        _lockedOut = lost;
        _error = msg;
        if (lost) _item = null;
      });
      // لا demo_orders عند فشل الشبكة
    }
  }

  Future<String?> _askNote(String title) async {
    var draft = '';
    final ok = await showCenteredDialog<bool>(
      context: context,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            title,
            style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16),
          ),
          const SizedBox(height: 12),
          TextField(
            onChanged: (value) => draft = value,
            maxLines: 3,
            decoration: const InputDecoration(hintText: 'اكتب...'),
          ),
          const SizedBox(height: 14),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => Navigator.pop(context, false),
                  child: const Text('إلغاء'),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: ElevatedButton(
                  onPressed: () => Navigator.pop(context, true),
                  child: const Text('تأكيد'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
    final note = draft.trim();
    if (ok != true || !mounted) return null;
    if (note.isEmpty) {
      await showAppMessage(
        context,
        title: 'مطلوب',
        message: 'الملاحظة/السبب إلزامي',
      );
      return null;
    }
    return note;
  }

  Future<void> _runAction(SalesRequestEmployeeAction action) async {
    final item = _item;
    if (item == null || _busy || _lockedOut) return;

    if (action == SalesRequestEmployeeAction.openLinkedSale) {
      if (!item.hasLinkedOrder) return;
      // linkedOrderId فقط — لا تستخدم requestId كـ orderId.
      await Navigator.of(context)
          .push(SalesNav.toLinkedOrder(item.linkedOrderId!));
      if (mounted) await _bootstrap();
      return;
    }

    if (action == SalesRequestEmployeeAction.startSale) {
      await _startOrContinueSale(item);
      return;
    }

    if (action == SalesRequestEmployeeAction.transferName) {
      await _transferName(item);
      return;
    }

    String? note;
    if (SalesRequestActions.requiresNote(action)) {
      note = await _askNote(SalesRequestActions.label(action));
      if (note == null) return;
    }

    setState(() => _busy = true);
    try {
      switch (action) {
        case SalesRequestEmployeeAction.prepare:
          await Injection.salesRemote.prepareSalesRequest(
            item.requestId,
            note: note,
          );
          break;
        case SalesRequestEmployeeAction.pend:
          await Injection.salesRemote.pendSalesRequest(
            item.requestId,
            note: note!,
          );
          break;
        case SalesRequestEmployeeAction.reject:
          await Injection.salesRemote.rejectSalesRequest(
            item.requestId,
            note: note!,
          );
          break;
        case SalesRequestEmployeeAction.inspect:
          await Injection.salesRemote.inspectSalesRequest(item.requestId);
          break;
        case SalesRequestEmployeeAction.transferName:
        case SalesRequestEmployeeAction.startSale:
        case SalesRequestEmployeeAction.openLinkedSale:
          break;
      }
      if (mounted) await _bootstrap();
    } catch (e) {
      if (!mounted) return;
      final msg = DioClient.mapError(e);
      if (msg.contains('Not_Owner') || msg.contains('غير مملوك')) {
        setState(() {
          _lockedOut = true;
          _item = null;
        });
      }
      await showAppMessage(
        context,
        title: SalesRequestActions.label(action),
        message: msg,
      );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _transferName(SalesRequestItem item) async {
    setState(() => _busy = true);
    List<({int id, String name})> peers = const [];
    try {
      peers = await Injection.salesRemote.transferPeers(item.requestId);
    } catch (e) {
      if (!mounted) return;
      setState(() => _busy = false);
      await showAppMessage(
        context,
        title: 'نقل الاسم',
        message: DioClient.mapError(e),
      );
      return;
    }
    if (!mounted) return;
    setState(() => _busy = false);
    if (peers.isEmpty) {
      await showAppMessage(
        context,
        title: 'نقل الاسم',
        message: 'لا يوجد زميل مؤهل في فرع هذا الطلب.',
      );
      return;
    }

    ({int id, String name})? selected = peers.first;
    final reasonCtrl = TextEditingController();
    final ok = await showCenteredDialog<bool>(
      context: context,
      child: StatefulBuilder(
        builder: (ctx, setLocal) {
          return Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'نقل الاسم لزميل',
                style: TextStyle(fontWeight: FontWeight.w700, fontSize: 16),
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<int>(
                value: selected?.id,
                items: [
                  for (final p in peers)
                    DropdownMenuItem(value: p.id, child: Text(p.name)),
                ],
                onChanged: (v) {
                  selected = peers.firstWhere((p) => p.id == v);
                  setLocal(() {});
                },
                decoration: const InputDecoration(labelText: 'الزميل المستلم'),
              ),
              const SizedBox(height: 8),
              TextField(
                controller: reasonCtrl,
                maxLines: 3,
                decoration: const InputDecoration(labelText: 'سبب النقل (إلزامي)'),
              ),
              const SizedBox(height: 14),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton(
                      onPressed: () => Navigator.pop(ctx, false),
                      child: const Text('إلغاء'),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: ElevatedButton(
                      onPressed: () => Navigator.pop(ctx, true),
                      child: const Text('تأكيد'),
                    ),
                  ),
                ],
              ),
            ],
          );
        },
      ),
    );
    final reason = reasonCtrl.text.trim();
    reasonCtrl.dispose();
    if (ok != true || selected == null) return;
    if (reason.length < 3) {
      await showAppMessage(
        context,
        title: 'نقل الاسم',
        message: 'السبب إلزامي (٣ أحرف على الأقل)',
      );
      return;
    }

    setState(() => _busy = true);
    try {
      await Injection.salesRemote.transferName(
        requestId: item.requestId,
        toEmployeeId: selected!.id,
        reason: reason,
      );
      if (!mounted) return;
      setState(() {
        _lockedOut = true;
        _item = null;
        _error = 'تم نقل الاسم — لم تعد تملك هذا الطلب';
      });
      await showAppMessage(
        context,
        title: 'تم',
        message: 'نُقل الطلب إلى ${selected!.name}',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      if (mounted) Navigator.of(context).pop();
    } catch (e) {
      if (!mounted) return;
      final msg = DioClient.mapError(e);
      if (msg.contains('Not_Owner') || msg.contains('غير مملوك')) {
        setState(() {
          _lockedOut = true;
          _item = null;
        });
      }
      await showAppMessage(context, title: 'نقل الاسم', message: msg);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  /// يضمن المسودة من السيرفر (idempotent) ثم يفتح معالج الإكمال — بلا نجاح وهمي عند انقطاع الشبكة.
  Future<void> _startOrContinueSale(SalesRequestItem item) async {
    setState(() => _busy = true);
    try {
      var linked = item.linkedOrderId;
      if (linked == null || linked == 0) {
        final ensured = await Injection.salesRemote.ensureDraft(item.requestId);
        linked = ensured?.linkedOrderId;
        if (linked == null || linked == 0) {
          if (!mounted) return;
          await showAppMessage(
            context,
            title: 'مسودة البيع',
            message: 'تعذر إنشاء مسودة البيع. أعد المحاولة أو تحقق من اتصال الشبكة.',
          );
          if (mounted) await _bootstrap();
          return;
        }
      }
      if (!mounted) return;
      await Navigator.of(context).push(SalesNav.toCompleteSale(linked));
      if (mounted) await _bootstrap();
    } catch (e) {
      if (!mounted) return;
      final msg = DioClient.mapError(e);
      if (msg.contains('Not_Owner') || msg.contains('غير مملوك')) {
        setState(() {
          _lockedOut = true;
          _item = null;
        });
      }
      await showAppMessage(context, title: 'البيع', message: msg);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final bottom = MediaQuery.paddingOf(context).bottom;
    final item = _item;
    final actions = item == null || _lockedOut
        ? const <SalesRequestEmployeeAction>[]
        : SalesRequestActions.forTab(
            item.tabKey,
            hasLinkedOrder: item.hasLinkedOrder,
          );

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(item?.customerName ?? 'تفاصيل الطلب'),
        backgroundColor: AppColors.primary,
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _lockedOut
          ? Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text(
                  _error ?? 'لم يعد هذا الطلب مملوكًا لك',
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: AppColors.foreground,
                    fontSize: 16,
                  ),
                ),
              ),
            )
          : item == null
          ? Center(child: Text(_error ?? 'تعذر التحميل'))
          : ListView(
              padding: EdgeInsets.fromLTRB(16, 12, 16, 120 + bottom),
              children: [
                if (item.isReturned)
                  Container(
                    margin: const EdgeInsets.only(bottom: 10),
                    padding: const EdgeInsets.symmetric(
                      horizontal: 12,
                      vertical: 10,
                    ),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFFF3CD),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: const Color(0xFFE0C36A)),
                    ),
                    child: Text(
                      'مُعاد للموظف'
                      '${item.lastStatusNote != null ? ' — ${item.lastStatusNote}' : ''}',
                      style: const TextStyle(fontWeight: FontWeight.w600),
                    ),
                  ),
                _kv('رقم الطلب', '#${item.requestId}'),
                _kv('الحالة', item.statusLabel),
                _kv('التبويب', SalesRequestTabMap.label(item.tabKey)),
                _kv('الهاتف', item.customerPhone ?? '—'),
                _kv('العنوان', item.address ?? 'غير مسجل'),
                _kv('المنتج المطلوب', item.requestedProduct ?? 'غير مسجل'),
                _kv('مصدر الطلب', item.sourceTypeLabel ?? 'غير مسجل'),
                if (item.submissionNote != null)
                  _kv('ملاحظة المُرسل', item.submissionNote!),
                _kv('الفرع', item.branchName ?? '${item.branchId ?? '—'}'),
                _kv(
                  'آخر تغيير',
                  item.statusChangedAtUtc?.toLocal().toString() ?? '—',
                ),
                if (item.lastStatusNote != null)
                  _kv('ملاحظة', item.lastStatusNote!),
                _kv(
                  'البيع المرتبط',
                  item.hasLinkedOrder
                      ? '#${item.linkedOrderId}'
                      : 'لا يوجد بعد',
                ),
                if (_error != null && !_lockedOut)
                  Padding(
                    padding: const EdgeInsets.only(top: 12),
                    child: Text(
                      _error!,
                      style: const TextStyle(color: Colors.red),
                    ),
                  ),
              ],
            ),
      bottomNavigationBar: actions.isEmpty
          ? null
          : SafeArea(
              child: Container(
                padding: const EdgeInsets.fromLTRB(12, 8, 12, 8),
                decoration: BoxDecoration(
                  color: AppColors.surface,
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.06),
                      blurRadius: 8,
                      offset: const Offset(0, -2),
                    ),
                  ],
                ),
                child: Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    for (final a in actions)
                      ElevatedButton(
                        onPressed: _busy ? null : () => _runAction(a),
                        style: ElevatedButton.styleFrom(
                          minimumSize: const Size(44, 44),
                          backgroundColor: AppColors.primary,
                          foregroundColor: Colors.white,
                        ),
                        child: Text(SalesRequestActions.label(a)),
                      ),
                  ],
                ),
              ),
            ),
    );
  }

  Widget _kv(String k, String v) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 110,
            child: Text(
              k,
              style: TextStyle(
                color: AppColors.foreground.withValues(alpha: 0.65),
              ),
            ),
          ),
          Expanded(
            child: Text(
              v,
              style: const TextStyle(
                fontWeight: FontWeight.w600,
                color: AppColors.foreground,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
