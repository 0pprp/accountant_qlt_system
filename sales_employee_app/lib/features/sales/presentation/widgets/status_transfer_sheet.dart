import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/widgets/centered_dialog.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';

const kSalesWorkflowStatuses = <(String key, String label)>[
  ('pending', 'طلبات البيع'),
  ('ready', 'جاهز للبيع'),
  ('disclosure', 'تم الكشف'),
  ('on_hold', 'معلقة'),
  ('rejected', 'مرفوض'),
  ('sold', 'تم البيع'),
];

Future<({String target, String? reason})?> showStatusTransferSheet(
  BuildContext context, {
  required String currentStatus,
}) async {
  final current = SalesWorkflowRules.normalize(currentStatus);
  if (SalesWorkflowRules.isTerminal(current)) {
    await showAppMessage(context, title: 'مغلق', message: 'لا إجراءات على طلب مرفوض أو تم بيعه');
    return null;
  }

  final allowed = SalesWorkflowRules.allowedTransferTargets(current);
  final picked = await showModalBottomSheet<String>(
    context: context,
    backgroundColor: Colors.white,
    shape: const RoundedRectangleBorder(
      borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
    ),
    builder: (ctx) {
      final bottom = MediaQuery.paddingOf(ctx).bottom;
      return Padding(
        padding: EdgeInsets.fromLTRB(16, 12, 16, 16 + bottom),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('نقل إلى حالة', style: TextStyle(fontSize: 17, fontWeight: FontWeight.w700)),
            const SizedBox(height: 12),
            for (final s in kSalesWorkflowStatuses)
              if (allowed.contains(s.$1))
                ListTile(
                  title: Text(s.$2),
                  leading: const Icon(Icons.radio_button_unchecked),
                  onTap: () => Navigator.pop(ctx, s.$1),
                ),
          ],
        ),
      );
    },
  );
  if (picked == null) return null;

  String? reason;
  if (SalesWorkflowRules.requiresReason(picked)) {
    final ctrl = TextEditingController();
    final ok = await showCenteredDialog<bool>(
      context: context,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            picked == 'ready'
                ? 'ملاحظة الجاهزية (إلزامية)'
                : picked == 'rejected'
                    ? 'سبب الرفض (إلزامي)'
                    : 'ملاحظة التعليق (إلزامية)',
            style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16),
          ),
          const SizedBox(height: 12),
          TextField(controller: ctrl, maxLines: 3, decoration: const InputDecoration(hintText: 'اكتب...')),
          const SizedBox(height: 14),
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
    reason = ctrl.text.trim();
    if (reason.isEmpty) {
      await showAppMessage(context, title: 'مطلوب', message: 'الملاحظة/السبب إلزامي');
      return null;
    }
  } else {
    final label = kSalesWorkflowStatuses.firstWhere((e) => e.$1 == picked).$2;
    final ok = await showConfirmDialog(context, title: 'نقل الحالة', message: 'نقل إلى «$label»؟');
    if (!ok) return null;
  }
  return (target: picked, reason: reason);
}
