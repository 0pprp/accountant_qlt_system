import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/network/dio_client.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:sales_employee_app/core/widgets/animated_waves.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';
import 'package:sales_employee_app/features/sales/presentation/widgets/status_transfer_sheet.dart';
import 'package:sales_employee_app/injection.dart';

class SaleDetailsPage extends StatefulWidget {
  const SaleDetailsPage({super.key, required this.orderId});

  final int orderId;

  @override
  State<SaleDetailsPage> createState() => _SaleDetailsPageState();
}

class _SaleDetailsPageState extends State<SaleDetailsPage> {
  SaleOrder? _order;
  bool _loading = true;
  bool _busy = false;

  bool get _isDemo => widget.orderId < 0;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      if (_isDemo) {
        final demo = await Injection.demoOrders.all();
        SaleOrder? o;
        for (final e in demo) {
          if (e.id == widget.orderId) {
            o = e;
            break;
          }
        }
        if (!mounted) return;
        setState(() {
          _order = o;
          _loading = false;
        });
        return;
      }
      final o = await Injection.salesRemote.getOrder(widget.orderId);
      if (!mounted) return;
      setState(() {
        _order = o;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() => _loading = false);
      await showAppMessage(context, title: 'تفاصيل المبيع', message: DioClient.mapError(e));
    }
  }

  Future<void> _transferTo(String target, {String? reason}) async {
    setState(() => _busy = true);
    try {
      if (_isDemo) {
        await Injection.demoOrders.transfer(widget.orderId, target, reason: reason);
      } else {
        await Injection.salesRemote.transfer(
          id: widget.orderId,
          targetStatus: target,
          reason: reason,
        );
      }
      if (!mounted) return;
      await showAppMessage(
        context,
        title: 'تم النقل',
        message: 'تم تحديث حالة الطلب',
        icon: Icons.check_circle_outline,
        color: AppColors.success,
      );
      await _load();
    } catch (e) {
      if (!mounted) return;
      await showAppMessage(context, title: 'فشل النقل', message: DioClient.mapError(e));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _pickAndTransfer() async {
    final current = SalesWorkflowRules.normalize(_order?.status);
    if (SalesWorkflowRules.isTerminal(current)) {
      await showAppMessage(context, title: 'مغلق', message: 'لا إجراءات بعد الرفض أو تم البيع');
      return;
    }
    final result = await showStatusTransferSheet(context, currentStatus: current);
    if (result == null) return;
    await _transferTo(result.target, reason: result.reason);
  }

  Future<void> _chipTransfer(String target) async {
    final current = SalesWorkflowRules.normalize(_order?.status);
    final dest = SalesWorkflowRules.normalize(target);
    if (dest == current) return;
    if (!SalesWorkflowRules.allowedTransferTargets(current).contains(dest)) {
      await showAppMessage(context, title: 'غير مسموح', message: 'هذا النقل غير متاح من الحالة الحالية');
      return;
    }

    String? reason;
    if (SalesWorkflowRules.requiresReason(dest)) {
      final ctrl = TextEditingController();
      final title = dest == SalesWorkflowRules.rejected
          ? 'سبب الرفض (إلزامي)'
          : dest == SalesWorkflowRules.ready
              ? 'ملاحظة الجاهزية (إلزامية)'
              : 'ملاحظة التعليق (إلزامية)';
      final ok = await showCenteredDialog<bool>(
        context: context,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(title, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
            const SizedBox(height: 12),
            TextField(controller: ctrl, maxLines: 3, decoration: const InputDecoration(hintText: 'اكتب...')),
            const SizedBox(height: 14),
            Row(
              children: [
                Expanded(child: OutlinedButton(onPressed: () => Navigator.pop(context, false), child: const Text('إلغاء'))),
                const SizedBox(width: 8),
                Expanded(child: ElevatedButton(onPressed: () => Navigator.pop(context, true), child: const Text('تأكيد النقل'))),
              ],
            ),
          ],
        ),
      );
      if (ok != true) return;
      reason = ctrl.text.trim();
      if (reason.isEmpty) {
        await showAppMessage(
          context,
          title: 'سبب مطلوب',
          message: 'يجب إدخال ملاحظة/سبب قبل النقل',
          icon: Icons.warning_amber_rounded,
          color: AppColors.warning,
        );
        return;
      }
    } else {
      final label = kSalesWorkflowStatuses.firstWhere((e) => e.$1 == dest).$2;
      final ok = await showConfirmDialog(context, title: 'نقل الحالة', message: 'هل تريد نقل الطلب إلى «$label»؟');
      if (!ok) return;
    }
    await _transferTo(dest, reason: reason);
  }

  @override
  Widget build(BuildContext context) {
    final status = (_order?.status ?? 'pending').toLowerCase();
    final parts = splitBaghdadDateTime(_order?.createdAt);
    final bottomInset = MediaQuery.paddingOf(context).bottom;

    return Scaffold(
      appBar: AppBar(title: Text(_isDemo ? 'طلب تجريبي' : 'طلب #${widget.orderId}')),
      body: AnimatedWavesBackground(
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : _order == null
                ? const Center(child: Text('غير موجود'))
                : ListView(
                    padding: EdgeInsets.fromLTRB(16, 16, 16, 140 + bottomInset),
                    children: [
                      Text(
                        _order!.customerName,
                        style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w700),
                      ),
                      const SizedBox(height: 6),
                      Text('الحالة الحالية: ${_labelOf(status)}'),
                      Text('الإجمالي: ${_order!.total?.toStringAsFixed(0) ?? '-'}'),
                      Text('التاريخ: ${parts.date}'),
                      if (parts.time.isNotEmpty) Text('الوقت: ${parts.time}'),
                      if (_order!.customerPhone != null) Text('هاتف الزبون: ${_order!.customerPhone}'),
                      if (_order!.businessName != null) Text('اسم المحل: ${_order!.businessName}'),
                      if (_order!.customerAddress != null) Text('عنوان المحل: ${_order!.customerAddress}'),
                      if (_order!.nearestKnownLocation != null)
                        Text('أقرب نقطة دالة: ${_order!.nearestKnownLocation}'),
                      if (_order!.creationAddress != null) Text('عنوان المبيع: ${_order!.creationAddress}'),
                      if (_order!.branchName != null) Text('المحافظة / الفرع: ${_order!.branchName}'),
                      if (_order!.orderListName != null) Text('القائمة: ${_order!.orderListName}'),
                      if (_order!.sellerName != null) Text('البائع: ${_order!.sellerName}'),
                      if (_order!.productNames.isNotEmpty)
                        Text('المنتجات: ${_order!.productNames.join('، ')}'),
                      if (_order!.workflowReason != null) Text('سبب آخر نقل: ${_order!.workflowReason}'),
                      if (_busy) ...[
                        const SizedBox(height: 16),
                        const Center(child: CircularProgressIndicator()),
                      ],
                    ],
                  ),
      ),
      bottomNavigationBar: _order == null ||
              _loading ||
              SalesWorkflowRules.isTerminal(status)
          ? null
          : Material(
              elevation: 12,
              color: Colors.white,
              child: SafeArea(
                top: false,
                minimum: const EdgeInsets.only(bottom: 8),
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(12, 10, 12, 8),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      SizedBox(
                        width: double.infinity,
                        child: ElevatedButton.icon(
                          onPressed: _busy ? null : _pickAndTransfer,
                          icon: const Icon(Icons.swap_horiz_rounded),
                          label: const Text('نقل الحالة'),
                        ),
                      ),
                      const SizedBox(height: 8),
                      Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        alignment: WrapAlignment.center,
                        children: [
                          for (final s in kSalesWorkflowStatuses)
                            if (s.$1 == status ||
                                SalesWorkflowRules.allowedTransferTargets(status).contains(s.$1))
                              ChoiceChip(
                                selected: status == s.$1,
                                label: Text(s.$2),
                                selectedColor: AppColors.primary.withValues(alpha: 0.18),
                                onSelected: _busy || status == s.$1 ? null : (_) => _chipTransfer(s.$1),
                              ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            ),
    );
  }

  String _labelOf(String key) =>
      kSalesWorkflowStatuses.firstWhere((e) => e.$1 == key, orElse: () => (key, key)).$2;
}
