import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';

enum OrderCardAction {
  ready,
  hold,
  reject,
  reassign,
  startSale,
  resumeSale,
}

Future<({OrderCardAction action, String? reason, String? assignee})?> showOrderActionsSheet(
  BuildContext context, {
  required String currentStatus,
}) async {
  final status = SalesWorkflowRules.normalize(currentStatus);
  if (SalesWorkflowRules.isTerminal(status)) return null;

  final canSale = SalesWorkflowRules.canStartSale(status);
  final canReassign = SalesWorkflowRules.canReassign(status);
  final saleLabel = status == SalesWorkflowRules.disclosure ? 'متابعة البيع' : 'إنشاء / إتمام البيع';

  final picked = await showModalBottomSheet<OrderCardAction>(
    context: context,
    backgroundColor: Colors.white,
    shape: const RoundedRectangleBorder(
      borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
    ),
    builder: (ctx) {
      final bottom = MediaQuery.paddingOf(ctx).bottom;
      Widget tile(OrderCardAction a, IconData icon, String title, {Color? color}) {
        return ListTile(
          leading: Icon(icon, color: color ?? AppColors.primary),
          title: Text(title, style: TextStyle(fontWeight: FontWeight.w600, color: color)),
          onTap: () => Navigator.pop(ctx, a),
        );
      }

      return Padding(
        padding: EdgeInsets.fromLTRB(8, 12, 8, 12 + bottom),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('إجراءات الطلب', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w700)),
            const SizedBox(height: 8),
            if (canSale) tile(OrderCardAction.startSale, Icons.point_of_sale_rounded, saleLabel),
            if (status != SalesWorkflowRules.ready)
              tile(OrderCardAction.ready, Icons.task_alt_rounded, 'جاهز للبيع'),
            if (status != SalesWorkflowRules.onHold)
              tile(OrderCardAction.hold, Icons.pause_circle_outline, 'تعليق (معلقة)'),
            tile(OrderCardAction.reject, Icons.cancel_outlined, 'رفض', color: AppColors.danger),
            if (canReassign)
              tile(OrderCardAction.reassign, Icons.person_search_rounded, 'نقل الاسم لزميل'),
          ],
        ),
      );
    },
  );

  if (picked == null) return null;

  if (picked == OrderCardAction.startSale || picked == OrderCardAction.resumeSale) {
    return (action: picked == OrderCardAction.resumeSale ? OrderCardAction.resumeSale : OrderCardAction.startSale, reason: null, assignee: null);
  }

  if (picked == OrderCardAction.reassign) {
    final nameCtrl = TextEditingController();
    final reasonCtrl = TextEditingController();
    final ok = await showCenteredDialog<bool>(
      context: context,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Text('نقل الاسم', style: TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
          const SizedBox(height: 10),
          TextField(controller: nameCtrl, decoration: const InputDecoration(labelText: 'اسم الزميل المستلم (إلزامي)')),
          const SizedBox(height: 8),
          TextField(controller: reasonCtrl, maxLines: 2, decoration: const InputDecoration(labelText: 'سبب النقل (إلزامي)')),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(child: OutlinedButton(onPressed: () => Navigator.pop(context, false), child: const Text('إلغاء'))),
              const SizedBox(width: 8),
              Expanded(child: ElevatedButton(onPressed: () => Navigator.pop(context, true), child: const Text('تأكيد'))),
            ],
          ),
        ],
      ),
    );
    if (ok != true) return null;
    if (nameCtrl.text.trim().isEmpty || reasonCtrl.text.trim().isEmpty) {
      await showAppMessage(context, title: 'نقل الاسم', message: 'المستلم والسبب إلزاميان');
      return null;
    }
    return (action: OrderCardAction.reassign, reason: reasonCtrl.text.trim(), assignee: nameCtrl.text.trim());
  }

  final needsReason = picked == OrderCardAction.ready ||
      picked == OrderCardAction.hold ||
      picked == OrderCardAction.reject;
  if (!needsReason) return (action: picked, reason: null, assignee: null);

  final title = switch (picked) {
    OrderCardAction.ready => 'ملاحظة الجاهزية (إلزامية)',
    OrderCardAction.hold => 'ملاحظة التعليق (إلزامية)',
    _ => 'سبب الرفض (إلزامي)',
  };
  final ctrl = TextEditingController();
  final ok = await showCenteredDialog<bool>(
    context: context,
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(title, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
        const SizedBox(height: 10),
        TextField(controller: ctrl, maxLines: 3, decoration: const InputDecoration(hintText: 'اكتب بوضوح...')),
        const SizedBox(height: 12),
        Row(
          children: [
            Expanded(child: OutlinedButton(onPressed: () => Navigator.pop(context, false), child: const Text('إلغاء'))),
            const SizedBox(width: 8),
            Expanded(child: ElevatedButton(onPressed: () => Navigator.pop(context, true), child: const Text('تأكيد'))),
          ],
        ),
      ],
    ),
  );
  if (ok != true) return null;
  if (ctrl.text.trim().isEmpty) {
    await showAppMessage(context, title: 'مطلوب', message: 'الملاحظة/السبب إلزامي لهذا الإجراء');
    return null;
  }
  return (action: picked, reason: ctrl.text.trim(), assignee: null);
}
