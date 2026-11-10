import 'package:flutter/material.dart';
import 'package:sales_employee_app/core/theme/app_colors.dart';
import 'package:sales_employee_app/core/time/baghdad_time.dart';
import 'package:sales_employee_app/features/sales/domain/entities/sale_order.dart';
import 'package:sales_employee_app/features/sales/domain/sales_workflow_rules.dart';

class CustomerStatusCard extends StatelessWidget {
  const CustomerStatusCard({
    super.key,
    required this.order,
    this.onTap,
    this.onActions,
    this.onCompleteSale,
  });

  final SaleOrder order;
  final VoidCallback? onTap;
  final VoidCallback? onActions;
  final VoidCallback? onCompleteSale;

  @override
  Widget build(BuildContext context) {
    final parts = splitBaghdadDateTime(order.createdAt);
    final phone = (order.customerPhone != null && order.customerPhone!.trim().isNotEmpty)
        ? order.customerPhone!.trim()
        : '—';
    final status = SalesWorkflowRules.normalize(order.status);
    final canSale = onCompleteSale != null && SalesWorkflowRules.canStartSale(status);
    final saleLabel = status == SalesWorkflowRules.disclosure ? 'متابعة البيع' : 'إتمام البيع';

    return Material(
      color: Colors.transparent,
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Ink(
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            color: Colors.white.withValues(alpha: 0.9),
            border: Border.all(color: Colors.white),
            boxShadow: [
              BoxShadow(
                color: AppColors.primary.withValues(alpha: 0.07),
                blurRadius: 14,
                offset: const Offset(0, 6),
              ),
            ],
          ),
          padding: const EdgeInsets.fromLTRB(14, 14, 10, 14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 44,
                    height: 44,
                    decoration: BoxDecoration(
                      color: AppColors.primary.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Icon(Icons.person_rounded, color: AppColors.primary),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(order.customerName, style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700)),
                        const SizedBox(height: 4),
                        Text(phone, style: const TextStyle(fontSize: 13.5, color: AppColors.textSecondary, fontWeight: FontWeight.w600)),
                        if (order.branchName != null) ...[
                          const SizedBox(height: 4),
                          Text('المحافظة: ${order.branchName}', style: const TextStyle(fontSize: 12.5, color: AppColors.muted)),
                        ],
                        const SizedBox(height: 6),
                        Text('الحالة: ${SalesWorkflowRules.label(status)}', style: const TextStyle(fontSize: 12.5, fontWeight: FontWeight.w600)),
                        const SizedBox(height: 6),
                        Text(parts.date, style: const TextStyle(fontSize: 13, color: AppColors.muted)),
                        if (parts.time.isNotEmpty)
                          Text(parts.time, style: const TextStyle(fontSize: 13, color: AppColors.muted, fontWeight: FontWeight.w600)),
                        if (order.workflowReason != null && order.workflowReason!.trim().isNotEmpty) ...[
                          const SizedBox(height: 8),
                          Text(
                            'ملاحظة: ${order.workflowReason}',
                            style: const TextStyle(fontSize: 12.5, color: AppColors.warning, height: 1.3),
                          ),
                        ],
                      ],
                    ),
                  ),
                  if (onActions != null)
                    IconButton(
                      tooltip: 'إجراءات',
                      onPressed: onActions,
                      icon: const Icon(Icons.more_vert_rounded, color: AppColors.primary),
                    ),
                ],
              ),
              if (canSale) ...[
                const SizedBox(height: 12),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: onCompleteSale,
                    icon: const Icon(Icons.point_of_sale_rounded),
                    label: Text(saleLabel),
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
