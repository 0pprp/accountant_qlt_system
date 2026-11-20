import 'dart:async';

import 'package:flutter/material.dart';

import '../../core/formatting/iraq_time.dart';
import '../../core/formatting/thousands_separator_input_formatter.dart';
import '../../core/network/api_errors.dart';
import '../../core/theme/app_theme.dart';
import '../../core/validation/validators.dart';
import '../../core/widgets/feedback.dart';
import '../../core/widgets/neu.dart';
import '../../injection.dart';
import '../promoter/models.dart';
import 'order_stage.dart';
import 'orders_page.dart';

class OrderDetailPage extends StatefulWidget {
  const OrderDetailPage({super.key, required this.orderId, this.initial});
  final int orderId;
  final PromoterOrder? initial;

  @override
  State<OrderDetailPage> createState() => _OrderDetailPageState();
}

class _OrderDetailPageState extends State<OrderDetailPage> {
  PromoterOrder? _order;
  String? _error;
  bool _loading = false;
  bool _cancelling = false;
  bool _changed = false;

  @override
  void initState() {
    super.initState();
    _order = widget.initial;
    unawaited(_load());
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final o = await Injection.api.order(widget.orderId);
      if (mounted) {
        setState(() {
          _order = o;
          _error = null;
        });
      }
    } catch (e) {
      if (mounted) setState(() => _error = ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _cancel() async {
    final reason = TextEditingController();
    final formKey = GlobalKey<FormState>();
    final ok = await AppFeedback.dialog<bool>(
      context,
      title: 'إلغاء الطلب',
      message: 'سيتم إلغاء الطلب ولن يصل إلى قسم المبيعات.',
      icon: Icons.cancel_outlined,
      color: AppColors.danger,
      body: Form(
        key: formKey,
        child: NeuTextField(
          controller: reason,
          hint: 'سبب الإلغاء (اختياري)',
          maxLines: 2,
          maxLength: Validators.maxCancelReasonLength,
          validator: Validators.cancelReason,
        ),
      ),
      actions: const [
        DialogAction(label: 'إلغاء الطلب', value: true, danger: true),
        DialogAction(label: 'رجوع', value: false, primary: false),
      ],
    );
    final text = reason.text;
    if (ok != true || !mounted) return;
    if (Validators.cancelReason(text) != null) {
      AppFeedback.snack(context, 'السبب أطول من المسموح', error: true);
      return;
    }
    setState(() => _cancelling = true);
    try {
      await Injection.api.cancelOrder(widget.orderId, text);
      _changed = true;
      unawaited(Injection.store.afterActivity());
      if (!mounted) return;
      AppFeedback.snack(context, 'تم إلغاء الطلب');
      await _load();
    } catch (e) {
      if (mounted) await AppFeedback.error(context, ApiErrors.message(e));
    } finally {
      if (mounted) setState(() => _cancelling = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final o = _order;
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (!didPop) Navigator.of(context).pop(_changed);
      },
      child: Scaffold(
        appBar: AppBar(title: Text(o == null ? 'تفاصيل الطلب' : 'طلب #${o.id}')),
        body: o == null
            ? (_loading
                  ? const Center(child: CircularProgressIndicator())
                  : StateView(
                      icon: Icons.cloud_off_rounded,
                      message: _error ?? ApiErrors.generic,
                      actionLabel: 'إعادة المحاولة',
                      onAction: _load,
                    ))
            : RefreshIndicator(
                onRefresh: _load,
                child: ListView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: EdgeInsets.fromLTRB(
                    18,
                    8,
                    18,
                    24 + MediaQuery.paddingOf(context).bottom,
                  ),
                  children: [
                    _header(o),
                    if (OrderStages.isRejected(o.stage) && o.reason != null) ...[
                      const SizedBox(height: 14),
                      _reasonBanner(o.reason!),
                    ],
                    const SizedBox(height: 16),
                    _stepper(o),
                    if (o.sale != null) ...[
                      const SizedBox(height: 16),
                      _sale(o.sale!, stageColor(o.stage)),
                    ],
                    const SizedBox(height: 16),
                    _details(o),
                    if (o.canCancel) ...[
                      const SizedBox(height: 22),
                      NeuButton(
                        label: 'إلغاء الطلب',
                        icon: Icons.cancel_outlined,
                        primary: false,
                        danger: true,
                        busy: _cancelling,
                        onPressed: _cancel,
                      ),
                    ],
                  ],
                ),
              ),
      ),
    );
  }

  Widget _header(PromoterOrder o) => NeuCard(
    child: Row(
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                o.customerName,
                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
              ),
              Text(
                IraqTime.dateTime(o.createdAt),
                style: const TextStyle(color: AppColors.muted, fontSize: 13),
              ),
            ],
          ),
        ),
        Flexible(child: StatusPill(o.statusLabel, color: stageColor(o.stage))),
      ],
    ),
  );

  Widget _reasonBanner(String reason) => Container(
    padding: const EdgeInsets.all(14),
    decoration: BoxDecoration(
      color: AppColors.danger.withValues(alpha: 0.08),
      borderRadius: BorderRadius.circular(16),
      border: Border.all(color: AppColors.danger.withValues(alpha: 0.3)),
    ),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Icon(Icons.info_outline_rounded, color: AppColors.danger),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            'السبب: $reason',
            style: const TextStyle(color: AppColors.danger, fontWeight: FontWeight.w600),
          ),
        ),
      ],
    ),
  );

  Widget _stepper(PromoterOrder o) {
    final current = OrderStages.stepIndex(o.stage, o.statusLabel);
    final rejected = current < 0;
    return NeuCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('مراحل الطلب', style: TextStyle(fontWeight: FontWeight.w700)),
          const SizedBox(height: 14),
          Row(
            children: [
              for (var i = 0; i < OrderStages.steps.length; i++) ...[
                if (i > 0)
                  Expanded(
                    child: Container(
                      height: 3,
                      margin: const EdgeInsets.only(bottom: 22),
                      color: !rejected && i <= current
                          ? AppColors.primary
                          : AppColors.line,
                    ),
                  ),
                _StepDot(
                  label: OrderStages.steps[i],
                  done: !rejected && i < current,
                  active: !rejected && i == current,
                ),
              ],
            ],
          ),
          if (rejected) ...[
            const SizedBox(height: 8),
            StatusPill(o.statusLabel, color: AppColors.danger, icon: Icons.block_rounded),
          ],
        ],
      ),
    );
  }

  Widget _sale(SaleProgress s, Color color) {
    String money(double v) => ThousandsSeparatorInputFormatter.format(v);
    return NeuCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'تسديد المبيعة ${s.percent}%',
            style: const TextStyle(fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 10),
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: LinearProgressIndicator(
              value: s.percent / 100,
              minHeight: 10,
              backgroundColor: AppColors.primarySoft,
              color: color,
            ),
          ),
          const SizedBox(height: 14),
          _row('قيمة المبيعة', money(s.total)),
          _row('المدفوع', money(s.paid)),
          _row('المتبقي', money(s.remaining)),
          _row('عدد الدفعات', '${s.paymentsCount}'),
          _row('آخر دفعة', IraqTime.dateOnly(s.lastPaymentDate)),
        ],
      ),
    );
  }

  Widget _details(PromoterOrder o) => NeuCard(
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        _row('المنتج', o.product),
        _row('الهاتف', o.customerPhone, ltr: true),
        _row('الفرع', o.branchName),
        _row('العنوان', o.address),
        if (o.note != null) _row('ملاحظة', o.note!),
      ],
    ),
  );

  Widget _row(String label, String value, {bool ltr = false}) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 6),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 100,
          child: Text(label, style: const TextStyle(color: AppColors.muted)),
        ),
        Expanded(
          child: Text(
            value.isEmpty ? '—' : value,
            textDirection: ltr ? TextDirection.ltr : null,
            textAlign: TextAlign.start,
            style: const TextStyle(fontWeight: FontWeight.w600),
          ),
        ),
      ],
    ),
  );
}

class _StepDot extends StatelessWidget {
  const _StepDot({required this.label, required this.done, required this.active});
  final String label;
  final bool done;
  final bool active;

  @override
  Widget build(BuildContext context) {
    final on = done || active;
    return SizedBox(
      width: 52,
      child: Column(
        children: [
          Container(
            width: 24,
            height: 24,
            decoration: BoxDecoration(
              color: on ? AppColors.primary : AppColors.surface,
              shape: BoxShape.circle,
              border: Border.all(
                color: on ? AppColors.primary : AppColors.line,
                width: 2,
              ),
              boxShadow: active
                  ? [BoxShadow(color: AppColors.primary.withValues(alpha: 0.35), blurRadius: 8)]
                  : null,
            ),
            child: done
                ? const Icon(Icons.check_rounded, size: 15, color: Colors.white)
                : null,
          ),
          const SizedBox(height: 4),
          Text(
            label,
            textAlign: TextAlign.center,
            maxLines: 2,
            style: TextStyle(
              fontSize: 10.5,
              height: 1.1,
              color: on ? AppColors.ink : AppColors.muted,
              fontWeight: active ? FontWeight.w700 : FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}
