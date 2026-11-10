import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../payments/presentation/collect_payment_sheet.dart';
import '../../payments/presentation/payment_success_feedback.dart';
import '../../receipts/data/receipt_build_helper.dart';
import '../../receipts/presentation/receipt_preview_page.dart';
import '../domain/entities/customer.dart';

class CustomerDetailPage extends StatelessWidget {
  const CustomerDetailPage({super.key, required this.customer});

  final Customer customer;

  @override
  Widget build(BuildContext context) {
    final money = NumberFormat('#,##0.##', 'en');
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: Text(customer.name)),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.card,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.border),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _row('الرقم', '${customer.customerId}'),
                _row('الهاتف', customer.phone ?? '—'),
                _row('العنوان', customer.address ?? '—'),
                _row('المحل', customer.shopName ?? '—'),
                _row(
                  'المتبقي',
                  customer.remaining == null
                      ? '—'
                      : money.format(customer.remaining),
                ),
                _row(
                  'القسط اليومي',
                  customer.dailyInstallment == null
                      ? '—'
                      : money.format(customer.dailyInstallment),
                ),
                _row(
                  'سعر البيع',
                  customer.salePrice == null
                      ? '—'
                      : money.format(customer.salePrice),
                ),
                _row(
                  'آخر تسديد',
                  customer.lastPaymentDate ?? '—',
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          ElevatedButton.icon(
            onPressed: () async {
              final result =
                  await showCollectPaymentSheet(context, customer);
              if (result == null || !context.mounted) return;
              await PaymentSuccessFeedback.playLocalSaveSuccess(context);
              if (!context.mounted) return;
              try {
                final bytes = await buildCustomerReceiptPdf(
                  customer: customer,
                  amountPaid: result.amount,
                );
                if (!context.mounted) return;
                await Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => ReceiptPreviewPage(
                      bytes: bytes,
                      title: receiptPreviewTitle(customer.name),
                    ),
                  ),
                );
              } catch (_) {
                if (!context.mounted) return;
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('تم التسديد — تعذر تجهيز الوصل'),
                  ),
                );
              }
            },
            icon: const Icon(Icons.payments_outlined),
            label: Text('تحصيل تسديد', style: AppTextStyles.button),
          ),
        ],
      ),
    );
  }

  Widget _row(String k, String v) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        children: [
          SizedBox(
            width: 110,
            child: Text(k, style: AppTextStyles.muted),
          ),
          Expanded(
            child: Text(
              v,
              style: AppTextStyles.body.copyWith(fontWeight: FontWeight.w600),
            ),
          ),
        ],
      ),
    );
  }
}
