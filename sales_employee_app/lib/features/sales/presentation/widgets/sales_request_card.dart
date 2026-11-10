import 'package:flutter/material.dart';

import '../../../../core/widgets/ui/action_row.dart';
import '../../../../core/widgets/ui/data_field.dart';
import '../../../../core/widgets/ui/entity_card.dart';
import '../../../../core/widgets/ui/status_badge.dart';
import '../../domain/entities/sales_request_item.dart';

/// Full request summary. Opening it preserves the existing workflow/detail route.
class SalesRequestCard extends StatelessWidget {
  const SalesRequestCard({super.key, required this.item, required this.onOpen});
  final SalesRequestItem item;
  final VoidCallback onOpen;

  String date(DateTime value) {
    final d = value.toUtc().add(const Duration(hours: 3));
    return '${d.day}/${d.month}/${d.year}  ${d.hour.toString().padLeft(2, '0')}:${d.minute.toString().padLeft(2, '0')} (بغداد)';
  }

  StatusBadgeTone tone() {
    final s = item.statusLabel;
    if (s.contains('رفض') || s.contains('حذف')) return StatusBadgeTone.danger;
    if (s.contains('مكتمل') || s.contains('بيع')) return StatusBadgeTone.success;
    if (s.contains('انتظار') || s.contains('قيد')) return StatusBadgeTone.warning;
    return StatusBadgeTone.neutral;
  }

  @override
  Widget build(BuildContext context) => Directionality(
        textDirection: TextDirection.rtl,
        child: Padding(
          padding: const EdgeInsets.only(bottom: 12),
          child: EntityCard(
            eyebrow: 'طلب #${item.requestId}',
            name: item.customerName,
            statusLabel: item.statusLabel,
            statusTone: tone(),
            identityExtra: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                if (item.isReturned)
                  const Padding(
                    padding: EdgeInsets.only(top: 8),
                    child: StatusBadge(label: 'مُعاد', tone: StatusBadgeTone.info),
                  ),
                DataField(
                  label: 'الهاتف',
                  value: item.customerPhone,
                  isPhone: true,
                  requiredField: true,
                ),
              ],
            ),
            fields: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                DataField(label: 'العنوان', value: item.address, requiredField: true),
                DataField(
                  label: 'المنتج المطلوب',
                  value: item.requestedProduct,
                  requiredField: true,
                ),
                DataField(label: 'الفرع', value: item.branchName),
                DataField(label: 'مصدر الطلب', value: item.sourceTypeLabel),
                if (item.hasLinkedOrder)
                  DataField(label: 'رقم البيع المرتبط', value: '${item.linkedOrderId}'),
                if (item.createdAt != null)
                  DataField(label: 'تاريخ الإضافة', value: date(item.createdAt!)),
                if (item.statusChangedAtUtc != null)
                  DataField(
                    label: 'آخر تغيير للحالة',
                    value: date(item.statusChangedAtUtc!),
                  ),
              ],
            ),
            notes: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                DataField(label: 'ملاحظة المُرسل', value: item.submissionNote),
                DataField(label: 'آخر ملاحظة على الطلب', value: item.lastStatusNote),
              ],
            ),
            actions: ActionRow(
              primaryLabel: 'فتح الطلب والإجراءات',
              onPrimary: onOpen,
            ),
          ),
        ),
      );
}
